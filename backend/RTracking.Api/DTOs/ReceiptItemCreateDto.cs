namespace RTracking.Api.DTOs;

public class ReceiptItemCreateDto
{
    public string RawName { get; set; } = string.Empty;
    public string? NormalizedName { get; set; }
    public int CategoryId { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal PriceTotal { get; set; }
    public decimal EdibleFraction { get; set; } = 1.0m;
    public string? Notes { get; set; }
}
