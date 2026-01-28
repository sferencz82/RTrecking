namespace RTracking.Api.DTOs;

public class DraftReceiptParseDto
{
    public string? MerchantName { get; set; }
    public DateTime? PurchasedAt { get; set; }
    public List<DraftReceiptItemDto> Items { get; set; } = new();
}

public class DraftReceiptItemDto
{
    public string RawName { get; set; } = string.Empty;
    public decimal PriceTotal { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal EdibleFraction { get; set; } = 1.0m;
}
