using RTracking.Api.DTOs;

namespace RTracking.Api.Services;

public interface IReportService
{
    Task<ReportSummaryDto> GetSummaryAsync(DateTime fromDate, DateTime toDate);
    Task<ReportItemsDto> GetItemsAsync(DateTime? fromDate, DateTime? toDate, int? categoryId);
}
