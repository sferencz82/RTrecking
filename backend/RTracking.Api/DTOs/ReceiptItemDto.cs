namespace RTracking.Api.DTOs;

public class ReceiptItemDto
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string RawName { get; set; } = string.Empty;
    public string? NormalizedName { get; set; }
    public int CategoryId { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal PriceTotal { get; set; }
    public decimal EdibleFraction { get; set; }
    public string? Notes { get; set; }
}
