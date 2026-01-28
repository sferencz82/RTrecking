namespace RTracking.Api.DTOs;

public class ReportItemsDto
{
    public List<ItemAggregateDto> Items { get; set; } = new();
}

public class ItemAggregateDto
{
    public string ItemName { get; set; } = string.Empty; // NormalizedName or RawName
    public decimal TotalSpent { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public int ItemCount { get; set; } // Number of times this item appears
}
