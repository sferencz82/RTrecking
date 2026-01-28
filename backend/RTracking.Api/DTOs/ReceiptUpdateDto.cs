namespace RTracking.Api.DTOs;

public class ReceiptUpdateDto
{
    public DateTime? PurchasedAt { get; set; }
    public string? MerchantName { get; set; }
    public string? LocationName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public decimal? Total { get; set; }
}
