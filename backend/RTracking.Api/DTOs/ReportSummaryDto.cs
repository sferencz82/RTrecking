namespace RTracking.Api.DTOs;

public class ReportSummaryDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalSpend { get; set; }
    public List<SpendByCategoryDto> SpendByCategory { get; set; } = new();
    public List<DailySpendDto> SpendOverTimeDaily { get; set; } = new();
}

public class SpendByCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalSpend { get; set; }
}

public class DailySpendDto
{
    public DateTime Date { get; set; }
    public decimal TotalSpend { get; set; }
}
