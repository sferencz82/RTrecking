using RTracking.Api.DTOs;

namespace RTracking.Api.Services;

public interface IReceiptOcrService
{
    Task<DraftReceiptParseDto> ProcessReceiptImageAsync(Guid receiptId, string imagePath);
    Task<string> GetRawOcrTextAsync(string imagePath);
}
