namespace EcommerceApp.Application.DTOs.Vendor;

public class VendorStatsResponse
{
    public int All { get; set; }
    public int Approved { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
}
