using EcommerceApp.Application.DTOs.Vendor;
using EcommerceApp.Application.Interfaces.Repositories;
using EcommerceApp.Application.Interfaces.Services;
using EcommerceApp.Domain.Entities.Identity;
using EcommerceApp.Domain.Entities.Vendor;
using EcommerceApp.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EcommerceApp.Application.Services;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepo;
    private readonly IVendorDocumentRepository _documentRepo;
    private readonly IUserRepository _userRepo;
    private readonly IFileStorageService _fileStorage;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;

    public VendorService(
        IVendorRepository vendorRepo,
        IVendorDocumentRepository documentRepo,
        IUserRepository userRepo,
        IFileStorageService fileStorage,
        IConfiguration config,
        IEmailService emailService)
    {
        _vendorRepo = vendorRepo;
        _documentRepo = documentRepo;
        _userRepo = userRepo;
        _fileStorage = fileStorage;
        _config = config;
        _emailService = emailService;
    }


    public async Task<VendorResponse> RegisterVendorAsync(VendorRegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var shopName = request.ShopName.Trim();

        if (await _userRepo.ExistsByEmailAsync(email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        if (!await _vendorRepo.IsShopNameUniqueAsync(shopName))
        {
            throw new InvalidOperationException("Shop name is already taken");
        }

        var verificationToken = GenerateRandomToken();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone?.Trim() ?? string.Empty,
            Role = UserRole.Seller,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,

        };

        var vendor = new Vendor
        {
            UserId = user.Id,
            ShopName = shopName,
            ShopSlug = await GenerateUniqueSlugAsync(shopName),
            Description = request.Description.Trim(),
            PhoneNumber = request.Phone?.Trim(),
            Email = email,
            PANNumber = request.PANNumber,
            VATNumber = request.VATNumber,
            RegistrationNumber = request.RegistrationNumber,
            Status = VendorStatus.Pending,
            IsApproved = false
        };

        await _userRepo.AddAsync(user);
        await _vendorRepo.AddAsync(vendor);
        await _vendorRepo.SaveChangesAsync();

        SendVerificationEmails(user, verificationToken);

        return MapToResponse(vendor);
    }
    public async Task<VendorResponse> ApplyVendorAsync(Guid userId, VendorApplicationRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");
        var existingVendor = await _vendorRepo.GetVendorByUserIdAsync(userId);
        if (existingVendor != null)
        {
            throw new InvalidOperationException("You already have a vendor application");
        }

        var shopSlug = await GenerateUniqueSlugAsync(request.ShopName);
        var vendor = new Vendor
        {
            UserId = userId,
            ShopName = request.ShopName.Trim(),
            ShopSlug = shopSlug,
            Description = request.Description.Trim(),
            PhoneNumber = request.PhoneNumber,
            Email = request.Email ?? user.Email,
            PANNumber = request.PANNumber,
            VATNumber = request.VATNumber,
            RegistrationNumber = request.RegistrationNumber,
            Status = VendorStatus.Pending,
            IsApproved = false
        };

        await _vendorRepo.AddAsync(vendor);
        await _vendorRepo.SaveChangesAsync();

        return MapToResponse(vendor);
    }

    public async Task<VendorResponse> UploadDocumentAsync(Guid userId, VendorDocumentRequest request)
    {
        var vendor = await _vendorRepo.GetVendorWithDocumentsByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Vendor not found");

        if (vendor.Documents.Count >= 5)
        {
            throw new ValidationException("Maximum 5 documents allowed");
        }

        var fileUrl = await _fileStorage.UploadFileAsync(
            request.File,
            $"vendors/{vendor.Id}/documents",
            request.DocumentType.ToString());

        var document = new VendorDocument
        {
            VendorId = vendor.Id,
            DocumentType = request.DocumentType,
            FileName = request.File.FileName,
            FileUrl = fileUrl,
            FileSize = request.File.Length,
            FileExtension = Path.GetExtension(request.File.FileName)
        };

        await _documentRepo.AddAsync(document);
        await _documentRepo.SaveChangesAsync();
        vendor.Documents.Add(document);

        return MapToResponse(vendor);
    }

    public async Task<VendorDocumentResponse> GetDocumentAsync(Guid userId, Guid documentId)
    {
        var vendor = await _vendorRepo.GetVendorByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Vendor not found");
        var document = await _documentRepo.GetDocumentForVendorAsync(vendor.Id, documentId)
            ?? throw new KeyNotFoundException("Document not found");

        return MapDocumentToResponse(document);
    }

    public async Task<IReadOnlyList<VendorDocumentResponse>> GetDocumentsAsync(Guid userId)
    {
        var vendor = await _vendorRepo.GetVendorByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Vendor not found");
        var documents = await _documentRepo.GetDocumentsForVendorAsync(vendor.Id);

        return documents.Select(MapDocumentToResponse).ToList();
    }

    public async Task<bool> DeleteDocumentAsync(Guid userId, Guid documentId)
    {
        var vendor = await _vendorRepo.GetVendorByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Vendor not found");
        var document = await _documentRepo.GetDocumentForVendorAsync(vendor.Id, documentId)
            ?? throw new KeyNotFoundException("Document not found");

        await _fileStorage.DeleteFileAsync(document.FileUrl);
        _documentRepo.Delete(document);
        await _documentRepo.SaveChangesAsync();
        return true;
    }

    public async Task<VendorResponse> GetVendorProfileAsync(Guid userId)
    {
        var vendor = await _vendorRepo.GetVendorWithDocumentsByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Vendor not found");
        return MapToResponse(vendor);
    }

    public async Task<bool> UpdateVendorProfileAsync(Guid userId, UpdateVendorRequest request)
    {
        var vendor = await _vendorRepo.GetVendorByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Vendor not found");

        if (!string.Equals(vendor.ShopName, request.ShopName, StringComparison.OrdinalIgnoreCase))
        {
            if (!await _vendorRepo.IsShopNameUniqueAsync(request.ShopName, vendor.Id))
            {
                throw new InvalidOperationException("Shop name is already taken");
            }

            vendor.ShopSlug = await GenerateUniqueSlugAsync(request.ShopName, vendor.Id);
        }

        vendor.ShopName = request.ShopName.Trim();
        vendor.Description = request.Description.Trim();
        vendor.PhoneNumber = request.PhoneNumber;
        vendor.Email = request.Email;
        vendor.PANNumber = request.PANNumber;
        vendor.VATNumber = request.VATNumber;
        vendor.RegistrationNumber = request.RegistrationNumber;

        _vendorRepo.Update(vendor);
        await _vendorRepo.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<VendorResponse>> GetAllVendorsAsync()
    {
        var vendors = await _vendorRepo.GetAllVendorsAsync();
        return vendors.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<VendorResponse>> GetPendingVendorsAsync()
    {
        var vendors = await _vendorRepo.GetPendingVendorsAsync();
        return vendors.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<VendorResponse>> GetApprovedVendorsAsync()
    {
        var vendors = await _vendorRepo.GetApprovedVendorsAsync();
        return vendors.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<VendorResponse>> GetRejectedVendorsAsync()
    {
        var vendors = await _vendorRepo.GetRejectedVendorsAsync();
        return vendors.Select(MapToResponse).ToList();
    }

    public async Task<VendorStatsResponse> GetVendorStatsAsync()
    {
        var approved = await _vendorRepo.CountByStatusAsync(VendorStatus.Approved);
        var pending  = await _vendorRepo.CountByStatusAsync(VendorStatus.Pending)
                     + await _vendorRepo.CountByStatusAsync(VendorStatus.UnderReview);
        var rejected = await _vendorRepo.CountByStatusAsync(VendorStatus.Rejected);

        return new VendorStatsResponse
        {
            All      = approved + pending + rejected,
            Approved = approved,
            Pending  = pending,
            Rejected = rejected
        };
    }

    public async Task<VendorResponse> ApproveVendorAsync(Guid adminId, VendorApprovalRequest request)
    {
        var vendor = await _vendorRepo.GetVendorWithDocumentsAsync(request.VendorId)
            ?? throw new KeyNotFoundException("Vendor not found");

        if (vendor.Status is not (VendorStatus.Pending or VendorStatus.UnderReview))
        {
            throw new InvalidOperationException("Vendor cannot be approved");
        }

        vendor.Status = VendorStatus.Approved;
        vendor.IsApproved = true;
        vendor.ApprovedAt = DateTime.UtcNow;
        vendor.RejectedAt = null;
        vendor.RejectionReason = null;

        var user = await _userRepo.GetByIdAsync(vendor.UserId);
        if (user != null)
        {
            user.Role = UserRole.Seller;
            _userRepo.Update(user);
        }

        _vendorRepo.Update(vendor);
        await _vendorRepo.SaveChangesAsync();
        return MapToResponse(vendor);
    }

    public async Task<VendorResponse> RejectVendorAsync(Guid adminId, VendorRejectionRequest request)
    {
        var vendor = await _vendorRepo.GetVendorWithDocumentsAsync(request.VendorId)
            ?? throw new KeyNotFoundException("Vendor not found");

        if (vendor.Status == VendorStatus.Rejected)
        {
            throw new InvalidOperationException("Vendor is already rejected");
        }

        vendor.Status = VendorStatus.Rejected;
        vendor.IsApproved = false;
        vendor.RejectedAt = DateTime.UtcNow;
        vendor.RejectionReason = request.ResolvedReason;

        _vendorRepo.Update(vendor);
        await _vendorRepo.SaveChangesAsync();
        return MapToResponse(vendor);
    }

    private async Task<string> GenerateUniqueSlugAsync(string shopName, Guid? excludeVendorId = null)
    {
        var baseSlug = GenerateSlug(shopName);
        var slug = baseSlug;
        var suffix = 1;

        while (!await _vendorRepo.IsShopSlugUniqueAsync(slug, excludeVendorId))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static string GenerateSlug(string text)
    {
        var slug = text.Trim().ToLowerInvariant().Replace("&", "and");
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }


    private void SendVerificationEmails(User user, string verificationToken)
    {
        var verificationLink = EmailLinkBuilder.BuildVerificationLink(
            _config["AppUrl"],
            user.Email,
            verificationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
                await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, verificationLink);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        });
    }

    private static string GenerateRandomToken() => Guid.NewGuid().ToString("N");
    private static VendorResponse MapToResponse(Vendor vendor) => new()
    {
        Id = vendor.Id,
        UserId = vendor.UserId,
        ShopName = vendor.ShopName,
        ShopSlug = vendor.ShopSlug,
        Description = vendor.Description,
        LogoUrl = vendor.LogoUrl,
        Status = vendor.Status,
        IsApproved = vendor.IsApproved,
        RejectionReason = vendor.RejectionReason,
        ApprovedAt = vendor.ApprovedAt,
        CreatedAt = vendor.CreatedAt,
        Documents = vendor.Documents.Select(MapDocumentToResponse).ToList()
    };

    private static VendorDocumentResponse MapDocumentToResponse(VendorDocument document) => new()
    {
        Id = document.Id,
        DocumentType = document.DocumentType,
        DocumentTypeName = document.DocumentType.ToString(),
        FileName = document.FileName,
        FileUrl = document.FileUrl,
        FileSize = document.FileSize,
        IsVerified = document.IsVerified,
        UploadedAt = document.CreatedAt
    };
}

