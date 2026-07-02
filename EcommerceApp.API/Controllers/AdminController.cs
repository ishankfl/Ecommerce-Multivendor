using EcommerceApp.Application.DTOs.Shared;
using EcommerceApp.Application.DTOs.Vendor;
using EcommerceApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Moderator")]
[Route("api/admin/vendors")]
public class AdminController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public AdminController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingVendors()
    {
        var vendors = await _vendorService.GetPendingVendorsAsync();
        return Ok(ApiResponse<IReadOnlyList<VendorResponse>>.Ok(vendors));
    }

    [HttpGet("approved")]
    public async Task<IActionResult> GetApprovedVendors()
    {
        var vendors = await _vendorService.GetApprovedVendorsAsync();
        return Ok(ApiResponse<IReadOnlyList<VendorResponse>>.Ok(vendors));
    }

    [HttpPost("approve")]
    public async Task<IActionResult> ApproveVendor([FromBody] VendorApprovalRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return await HandleAsync(async () =>
        {
            var vendor = await _vendorService.ApproveVendorAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<VendorResponse>.Ok(vendor, "Vendor approved successfully"));
        });
    }

    [HttpPost("reject")]
    public async Task<IActionResult> RejectVendor([FromBody] VendorRejectionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return await HandleAsync(async () =>
        {
            var vendor = await _vendorService.RejectVendorAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<VendorResponse>.Ok(vendor, "Vendor rejected successfully"));
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
