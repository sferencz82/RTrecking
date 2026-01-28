using RTracking.Api.DTOs;

namespace RTracking.Api.Services;

public interface IReceiptService
{
    Task<ReceiptDto> CreateReceiptAsync(ReceiptCreateDto dto);
    Task<ReceiptDto?> GetReceiptByIdAsync(Guid id);
    Task<IEnumerable<ReceiptDto>> GetReceiptsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<ReceiptDto?> UpdateReceiptAsync(Guid id, ReceiptUpdateDto dto);
    Task<bool> DeleteReceiptAsync(Guid id);
    Task<ReceiptDto> ApplyDraftParseAsync(Guid id, DraftReceiptParseDto draft);
}
