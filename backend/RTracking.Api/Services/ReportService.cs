using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using RTracking.Api.Data;
using RTracking.Api.DTOs;

namespace RTracking.Api.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;
    private const int MaxReportDays = 184; // Approximately 6 months

    // Switzerland timezone (CET/CEST)
    private static readonly TimeZoneInfo SwissTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
            ? "W. Europe Standard Time" 
            : "Europe/Zurich");

    public ReportService(
        ApplicationDbContext context,
        ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReportSummaryDto> GetSummaryAsync(DateTime fromDate, DateTime toDate)
    {
        // Validate date range (6 months max)
        var daysDiff = (toDate.Date - fromDate.Date).Days;
        if (daysDiff > MaxReportDays)
        {
            throw new ArgumentException(
                $"Date range cannot exceed {MaxReportDays} days (approximately 6 months). " +
                $"Requested range: {daysDiff} days.");
        }

        if (toDate < fromDate)
        {
            throw new ArgumentException("To date must be after from date.");
        }

        // Convert Swiss local time to UTC for database queries
        // Input dates are treated as Swiss local time (start/end of day)
        var fromUtc = ConvertToUtc(fromDate.Date);
        var toUtc = ConvertToUtc(toDate.Date.AddDays(1).AddTicks(-1)); // End of day

        // Get receipts in date range
        var receipts = await _context.Receipts
            .Where(r => r.PurchasedAt >= fromUtc && r.PurchasedAt <= toUtc)
            .Include(r => r.ReceiptItems)
                .ThenInclude(i => i.Category)
            .ToListAsync();

        var result = new ReportSummaryDto
        {
            FromDate = fromDate.Date,
            ToDate = toDate.Date
        };

        // Calculate total spend
        result.TotalSpend = receipts
            .Where(r => r.Total.HasValue)
            .Sum(r => r.Total!.Value);

        // Calculate spend by category
        var categorySpend = receipts
            .SelectMany(r => r.ReceiptItems)
            .GroupBy(i => new { i.CategoryId, i.Category.Name })
            .Select(g => new SpendByCategoryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalSpend = g.Sum(i => i.PriceTotal)
            })
            .OrderByDescending(c => c.TotalSpend)
            .ToList();

        result.SpendByCategory = categorySpend;

        // Calculate daily spend
        var dailySpend = receipts
            .Where(r => r.Total.HasValue)
            .GroupBy(r => ConvertToSwissTime(r.PurchasedAt).Date)
            .Select(g => new DailySpendDto
            {
                Date = g.Key,
                TotalSpend = g.Sum(r => r.Total!.Value)
            })
            .OrderBy(d => d.Date)
            .ToList();

        // Fill in missing days with zero spend
        var allDays = new List<DailySpendDto>();
        var currentDate = fromDate.Date;
        var dailySpendDict = dailySpend.ToDictionary(d => d.Date, d => d.TotalSpend);

        while (currentDate <= toDate.Date)
        {
            allDays.Add(new DailySpendDto
            {
                Date = currentDate,
                TotalSpend = dailySpendDict.GetValueOrDefault(currentDate, 0)
            });
            currentDate = currentDate.AddDays(1);
        }

        result.SpendOverTimeDaily = allDays;

        return result;
    }

    public async Task<ReportItemsDto> GetItemsAsync(DateTime? fromDate, DateTime? toDate, int? categoryId)
    {
        var query = _context.ReceiptItems
            .Include(i => i.Receipt)
            .Include(i => i.Category)
            .AsQueryable();

        // Apply date filter if provided
        if (fromDate.HasValue || toDate.HasValue)
        {
            var fromUtc = fromDate.HasValue 
                ? ConvertToUtc(fromDate.Value.Date) 
                : DateTime.MinValue;
            
            var toUtc = toDate.HasValue 
                ? ConvertToUtc(toDate.Value.Date.AddDays(1).AddTicks(-1))
                : DateTime.MaxValue;

            query = query.Where(i => i.Receipt.PurchasedAt >= fromUtc && i.Receipt.PurchasedAt <= toUtc);
        }

        // Apply category filter if provided
        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        var items = await query.ToListAsync();

        // Aggregate by NormalizedName (fallback to RawName)
        var aggregated = items
            .GroupBy(i => !string.IsNullOrWhiteSpace(i.NormalizedName) ? i.NormalizedName : i.RawName)
            .Select(g => new ItemAggregateDto
            {
                ItemName = g.Key,
                TotalSpent = g.Sum(i => i.PriceTotal),
                TotalWeightKg = g.Where(i => i.WeightKg.HasValue).Sum(i => i.WeightKg!.Value),
                ItemCount = g.Count()
            })
            .OrderByDescending(i => i.TotalSpent)
            .ToList();

        return new ReportItemsDto
        {
            Items = aggregated
        };
    }

    /// <summary>
    /// Converts Swiss local time to UTC.
    /// Used when querying the database (which stores UTC).
    /// </summary>
    private static DateTime ConvertToUtc(DateTime swissLocalTime)
    {
        // If DateTimeKind is already UTC, return as-is
        if (swissLocalTime.Kind == DateTimeKind.Utc)
        {
            return swissLocalTime;
        }

        // Treat input as Swiss local time and convert to UTC
        return TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(swissLocalTime, DateTimeKind.Unspecified),
            SwissTimeZone);
    }

    /// <summary>
    /// Converts UTC time to Swiss local time.
    /// Used when returning dates to the client.
    /// </summary>
    private static DateTime ConvertToSwissTime(DateTime utcTime)
    {
        if (utcTime.Kind != DateTimeKind.Utc)
        {
            // If not UTC, assume it's already in the correct timezone
            return utcTime;
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, SwissTimeZone);
    }
}
