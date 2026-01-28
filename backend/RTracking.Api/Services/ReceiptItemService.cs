using Microsoft.EntityFrameworkCore;
using RTracking.Api.Data;
using RTracking.Api.DTOs;
using RTracking.Api.Models;

namespace RTracking.Api.Services;

public class ReceiptItemService : IReceiptItemService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReceiptItemService> _logger;

    public ReceiptItemService(
        ApplicationDbContext context,
        ILogger<ReceiptItemService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReceiptItemDto> CreateItemAsync(Guid receiptId, ReceiptItemCreateDto dto)
    {
        // Validate receipt exists
        var receipt = await _context.Receipts.FindAsync(receiptId);
        if (receipt == null)
        {
            throw new KeyNotFoundException($"Receipt with id {receiptId} not found");
        }

        // Validate PriceTotal
        if (dto.PriceTotal < 0)
        {
            throw new ArgumentException($"PriceTotal must be >= 0, got {dto.PriceTotal}");
        }

        // Validate EdibleFraction
        if (dto.EdibleFraction < 0.1m || dto.EdibleFraction > 1.0m)
        {
            throw new ArgumentException($"EdibleFraction must be between 0.1 and 1.0, got {dto.EdibleFraction}");
        }

        // Validate CategoryId exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            throw new ArgumentException($"Category with id {dto.CategoryId} not found");
        }

        var item = new ReceiptItem
        {
            Id = Guid.NewGuid(),
            ReceiptId = receiptId,
            RawName = dto.RawName,
            NormalizedName = dto.NormalizedName ?? dto.RawName,
            CategoryId = dto.CategoryId,
            Quantity = dto.Quantity ?? 1,
            Unit = dto.Unit,
            WeightKg = dto.WeightKg,
            PriceTotal = dto.PriceTotal,
            EdibleFraction = dto.EdibleFraction,
            Notes = dto.Notes
        };

        _context.ReceiptItems.Add(item);
        await _context.SaveChangesAsync();

        // Recalculate receipt total
        await RecalculateReceiptTotalAsync(receiptId);

        _logger.LogInformation("Item created: {ItemId} for receipt {ReceiptId}", item.Id, receiptId);

        return MapToDto(item);
    }

    public async Task<ReceiptItemDto?> GetItemByIdAsync(Guid itemId)
    {
        var item = await _context.ReceiptItems.FindAsync(itemId);
        
        if (item == null)
        {
            return null;
        }

        return MapToDto(item);
    }

    public async Task<IEnumerable<ReceiptItemDto>> GetItemsByReceiptIdAsync(Guid receiptId)
    {
        var items = await _context.ReceiptItems
            .Where(i => i.ReceiptId == receiptId)
            .OrderBy(i => i.RawName)
            .ToListAsync();

        return items.Select(MapToDto);
    }

    public async Task<ReceiptItemDto?> UpdateItemAsync(Guid receiptId, Guid itemId, ReceiptItemUpdateDto dto)
    {
        var item = await _context.ReceiptItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ReceiptId == receiptId);

        if (item == null)
        {
            return null;
        }

        // Update only provided fields
        if (dto.RawName != null)
        {
            item.RawName = dto.RawName;
        }

        if (dto.NormalizedName != null)
        {
            item.NormalizedName = dto.NormalizedName;
        }

        if (dto.CategoryId.HasValue)
        {
            // Validate CategoryId exists
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
            if (!categoryExists)
            {
                throw new ArgumentException($"Category with id {dto.CategoryId.Value} not found");
            }
            item.CategoryId = dto.CategoryId.Value;
        }

        if (dto.Quantity.HasValue)
        {
            item.Quantity = dto.Quantity.Value;
        }

        if (dto.Unit != null)
        {
            item.Unit = dto.Unit;
        }

        if (dto.WeightKg.HasValue)
        {
            item.WeightKg = dto.WeightKg.Value;
        }

        if (dto.PriceTotal.HasValue)
        {
            // Validate PriceTotal
            if (dto.PriceTotal.Value < 0)
            {
                throw new ArgumentException($"PriceTotal must be >= 0, got {dto.PriceTotal.Value}");
            }
            item.PriceTotal = dto.PriceTotal.Value;
        }

        if (dto.EdibleFraction.HasValue)
        {
            // Validate EdibleFraction
            if (dto.EdibleFraction.Value < 0.1m || dto.EdibleFraction.Value > 1.0m)
            {
                throw new ArgumentException($"EdibleFraction must be between 0.1 and 1.0, got {dto.EdibleFraction.Value}");
            }
            item.EdibleFraction = dto.EdibleFraction.Value;
        }

        if (dto.Notes != null)
        {
            item.Notes = dto.Notes;
        }

        await _context.SaveChangesAsync();

        // Recalculate receipt total
        await RecalculateReceiptTotalAsync(receiptId);

        _logger.LogInformation("Item updated: {ItemId} for receipt {ReceiptId}", itemId, receiptId);

        return MapToDto(item);
    }

    public async Task<bool> DeleteItemAsync(Guid receiptId, Guid itemId)
    {
        var item = await _context.ReceiptItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ReceiptId == receiptId);

        if (item == null)
        {
            return false;
        }

        _context.ReceiptItems.Remove(item);
        await _context.SaveChangesAsync();

        // Recalculate receipt total
        await RecalculateReceiptTotalAsync(receiptId);

        _logger.LogInformation("Item deleted: {ItemId} from receipt {ReceiptId}", itemId, receiptId);

        return true;
    }

    public async Task RecalculateReceiptTotalAsync(Guid receiptId)
    {
        var total = await _context.ReceiptItems
            .Where(i => i.ReceiptId == receiptId)
            .SumAsync(i => i.PriceTotal);

        var receipt = await _context.Receipts.FindAsync(receiptId);
        if (receipt != null)
        {
            receipt.Total = total;
            receipt.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private static ReceiptItemDto MapToDto(ReceiptItem item)
    {
        return new ReceiptItemDto
        {
            Id = item.Id,
            ReceiptId = item.ReceiptId,
            RawName = item.RawName,
            NormalizedName = item.NormalizedName,
            CategoryId = item.CategoryId,
            Quantity = item.Quantity,
            Unit = item.Unit,
            WeightKg = item.WeightKg,
            PriceTotal = item.PriceTotal,
            EdibleFraction = item.EdibleFraction,
            Notes = item.Notes
        };
    }
}
