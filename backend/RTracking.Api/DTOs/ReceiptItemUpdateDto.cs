namespace RTracking.Api.DTOs;

public class ReceiptItemUpdateDto
{
    public string? RawName { get; set; }
    public string? NormalizedName { get; set; }
    public int? CategoryId { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? PriceTotal { get; set; }
    public decimal? EdibleFraction { get; set; }
    public string? Notes { get; set; }
}
