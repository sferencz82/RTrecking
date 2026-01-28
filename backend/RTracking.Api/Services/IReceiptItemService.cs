using RTracking.Api.DTOs;

namespace RTracking.Api.Services;

public interface IReceiptItemService
{
    Task<ReceiptItemDto> CreateItemAsync(Guid receiptId, ReceiptItemCreateDto dto);
    Task<ReceiptItemDto?> GetItemByIdAsync(Guid itemId);
    Task<IEnumerable<ReceiptItemDto>> GetItemsByReceiptIdAsync(Guid receiptId);
    Task<ReceiptItemDto?> UpdateItemAsync(Guid receiptId, Guid itemId, ReceiptItemUpdateDto dto);
    Task<bool> DeleteItemAsync(Guid receiptId, Guid itemId);
    Task RecalculateReceiptTotalAsync(Guid receiptId);
}
