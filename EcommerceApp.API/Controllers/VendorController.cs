using EcommerceApp.Application.DTOs.Shared;
using EcommerceApp.Application.DTOs.Vendor;
using EcommerceApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/vendor")]
public class VendorController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] VendorRegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return await HandleAsync(async () =>
        {
            var vendor = await _vendorService.RegisterVendorAsync(request);
            return Ok(ApiResponse<VendorResponse>.Ok(vendor, "Vendor registration submitted successfully"));
        });
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] VendorApplicationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return await HandleAsync(async () =>
        {
            var vendor = await _vendorService.ApplyVendorAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<VendorResponse>.Ok(vendor, "Vendor application submitted successfully"));
        });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        return await HandleAsync(async () =>
        {
            var vendor = await _vendorService.GetVendorProfileAsync(GetCurrentUserId());
            return Ok(ApiResponse<VendorResponse>.Ok(vendor));
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateVendorRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return await HandleAsync(async () =>
        {
            await _vendorService.UpdateVendorProfileAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<bool>.Ok(true, "Vendor profile updated successfully"));
        });
    }

    [HttpPost("documents")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadDocument([FromForm] VendorDocumentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return await HandleAsync(async () =>
        {
            var vendor = await _vendorService.UploadDocumentAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<VendorResponse>.Ok(vendor, "Document uploaded successfully"));
        });
    }

    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments()
    {
        return await HandleAsync(async () =>
        {
            var documents = await _vendorService.GetDocumentsAsync(GetCurrentUserId());
            return Ok(ApiResponse<IReadOnlyList<VendorDocumentResponse>>.Ok(documents));
        });
    }

    [HttpGet("documents/{documentId:guid}")]
    public async Task<IActionResult> GetDocument(Guid documentId)
    {
        return await HandleAsync(async () =>
        {
            var document = await _vendorService.GetDocumentAsync(GetCurrentUserId(), documentId);
            return Ok(ApiResponse<VendorDocumentResponse>.Ok(document));
        });
    }

    [HttpDelete("documents/{documentId:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid documentId)
    {
        return await HandleAsync(async () =>
        {
            await _vendorService.DeleteDocumentAsync(GetCurrentUserId(), documentId);
            return Ok(ApiResponse<bool>.Ok(true, "Document deleted successfully"));
        });
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedAccessException("User not found");
        }

        return parsedUserId;
    }

    private async Task<IActionResult> HandleAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
