namespace RTracking.Api.DTOs;

public class ReceiptDto
{
    public Guid Id { get; set; }
    public DateTime PurchasedAt { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string Currency { get; set; } = "CHF";
    public decimal? Total { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? LocationName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
