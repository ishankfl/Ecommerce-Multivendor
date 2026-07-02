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

    /// <summary>GET /api/admin/vendors — all vendors regardless of status</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllVendors()
    {
        var vendors = await _vendorService.GetAllVendorsAsync();
        return Ok(ApiResponse<IReadOnlyList<VendorResponse>>.Ok(vendors));
    }

    /// <summary>GET /api/admin/vendors/stats — counts: all, approved, pending, rejected</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetVendorStats()
    {
        var stats = await _vendorService.GetVendorStatsAsync();
        return Ok(ApiResponse<VendorStatsResponse>.Ok(stats));
    }

    /// <summary>GET /api/admin/vendors/pending</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingVendors()
    {
        var vendors = await _vendorService.GetPendingVendorsAsync();
        return Ok(ApiResponse<IReadOnlyList<VendorResponse>>.Ok(vendors));
    }

    /// <summary>GET /api/admin/vendors/approved</summary>
    [HttpGet("approved")]
    public async Task<IActionResult> GetApprovedVendors()
    {
        var vendors = await _vendorService.GetApprovedVendorsAsync();
        return Ok(ApiResponse<IReadOnlyList<VendorResponse>>.Ok(vendors));
    }

    /// <summary>GET /api/admin/vendors/rejected</summary>
    [HttpGet("rejected")]
    public async Task<IActionResult> GetRejectedVendors()
    {
        var vendors = await _vendorService.GetRejectedVendorsAsync();
        return Ok(ApiResponse<IReadOnlyList<VendorResponse>>.Ok(vendors));
    }

    /// <summary>POST /api/admin/vendors/approve</summary>
    [HttpPost("approve")]
    public async Task<IActionResult> ApproveVendor([FromBody] VendorApprovalRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));

        return await HandleAsync(async () =>
        {
            var vendor = await _vendorService.ApproveVendorAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<VendorResponse>.Ok(vendor, "Vendor approved successfully"));
        });
    }

    /// <summary>POST /api/admin/vendors/reject</summary>
    [HttpPost("reject")]
    public async Task<IActionResult> RejectVendor([FromBody] VendorRejectionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));

        return await HandleAsync(async () =>
        {
            var vendor = await _vendorService.RejectVendorAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<VendorResponse>.Ok(vendor, "Vendor rejected successfully"));
        });
    }

    // -------------------------------------------------------------------------
    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
            throw new UnauthorizedAccessException("User not found");
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
