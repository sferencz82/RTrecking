using Microsoft.AspNetCore.Mvc;
using RTracking.Api.DTOs;
using RTracking.Api.Services;

namespace RTracking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Get summary report with totals, spend by category, and daily spend over time
    /// </summary>
    /// <param name="from">Start date (YYYY-MM-DD) - treated as Swiss local time</param>
    /// <param name="to">End date (YYYY-MM-DD) - treated as Swiss local time</param>
    /// <returns>Report summary with totals and breakdowns</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ReportSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        try
        {
            var summary = await _reportService.GetSummaryAsync(from, to);
            return Ok(summary);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary report");
            return StatusCode(500, "An error occurred while generating the report");
        }
    }

    /// <summary>
    /// Get aggregated items report
    /// </summary>
    /// <param name="from">Start date (YYYY-MM-DD) - optional, treated as Swiss local time</param>
    /// <param name="to">End date (YYYY-MM-DD) - optional, treated as Swiss local time</param>
    /// <param name="categoryId">Category ID filter - optional</param>
    /// <returns>Aggregated items by normalized name</returns>
    [HttpGet("items")]
    [ProducesResponseType(typeof(ReportItemsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportItemsDto>> GetItems(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? categoryId = null)
    {
        try
        {
            var items = await _reportService.GetItemsAsync(from, to, categoryId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating items report");
            return StatusCode(500, "An error occurred while generating the report");
        }
    }
}
