using Microsoft.EntityFrameworkCore;
using RTracking.Api.Data;
using RTracking.Api.DTOs;
using RTracking.Api.Models;

namespace RTracking.Api.Services;

public class ReceiptService : IReceiptService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        ILogger<ReceiptService> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<ReceiptDto> CreateReceiptAsync(ReceiptCreateDto dto)
    {
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            PurchasedAt = dto.PurchasedAt,
            MerchantName = dto.MerchantName,
            Currency = dto.Currency,
            Total = dto.Total,
            ImagePath = dto.ImagePath,
            LocationName = dto.LocationName,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Receipt created: {ReceiptId}", receipt.Id);

        return MapToDto(receipt);
    }

    public async Task<ReceiptDto?> GetReceiptByIdAsync(Guid id)
    {
        var receipt = await _context.Receipts.FindAsync(id);
        
        if (receipt == null)
        {
            return null;
        }

        return MapToDto(receipt);
    }

    public async Task<IEnumerable<ReceiptDto>> GetReceiptsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Receipts.AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.PurchasedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            // Include the entire day
            var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(r => r.PurchasedAt <= toDateEnd);
        }

        var receipts = await query
            .OrderByDescending(r => r.PurchasedAt)
            .ToListAsync();

        return receipts.Select(MapToDto);
    }

    public async Task<ReceiptDto?> UpdateReceiptAsync(Guid id, ReceiptUpdateDto dto)
    {
        var receipt = await _context.Receipts.FindAsync(id);
        
        if (receipt == null)
        {
            return null;
        }

        // Update only provided fields
        if (dto.PurchasedAt.HasValue)
        {
            receipt.PurchasedAt = dto.PurchasedAt.Value;
        }

        if (dto.MerchantName != null)
        {
            receipt.MerchantName = dto.MerchantName;
        }

        if (dto.LocationName != null)
        {
            receipt.LocationName = dto.LocationName;
        }

        if (dto.Latitude.HasValue)
        {
            receipt.Latitude = dto.Latitude.Value;
        }

        if (dto.Longitude.HasValue)
        {
            receipt.Longitude = dto.Longitude.Value;
        }

        if (dto.Total.HasValue)
        {
            receipt.Total = dto.Total.Value;
        }

        receipt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Receipt updated: {ReceiptId}", id);

        return MapToDto(receipt);
    }

    public async Task<ReceiptDto> ApplyDraftParseAsync(Guid id, DraftReceiptParseDto draft)
    {
        var receipt = await _context.Receipts
            .Include(r => r.ReceiptItems)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receipt == null)
        {
            throw new KeyNotFoundException($"Receipt with id {id} not found");
        }

        // Update receipt fields if provided
        if (draft.MerchantName != null)
        {
            receipt.MerchantName = draft.MerchantName;
        }

        if (draft.PurchasedAt.HasValue)
        {
            receipt.PurchasedAt = draft.PurchasedAt.Value;
        }

        // Delete existing items
        _context.ReceiptItems.RemoveRange(receipt.ReceiptItems);

        // Create new items from draft
        var newItems = new List<ReceiptItem>();
        decimal total = 0;

        foreach (var draftItem in draft.Items)
        {
            // Validate PriceTotal
            if (draftItem.PriceTotal < 0)
            {
                throw new ArgumentException($"PriceTotal must be >= 0, got {draftItem.PriceTotal}");
            }

            // Validate EdibleFraction
            if (draftItem.EdibleFraction < 0.1m || draftItem.EdibleFraction > 1.0m)
            {
                throw new ArgumentException($"EdibleFraction must be between 0.1 and 1.0, got {draftItem.EdibleFraction}");
            }

            var item = new ReceiptItem
            {
                Id = Guid.NewGuid(),
                ReceiptId = id,
                RawName = draftItem.RawName,
                NormalizedName = draftItem.RawName, // Default to RawName if not provided
                CategoryId = 9, // Default to "Other" category
                Quantity = draftItem.Quantity ?? 1,
                Unit = draftItem.Unit,
                WeightKg = draftItem.WeightKg,
                PriceTotal = draftItem.PriceTotal,
                EdibleFraction = draftItem.EdibleFraction,
                Notes = null
            };

            newItems.Add(item);
            total += draftItem.PriceTotal;
        }

        // Add new items
        _context.ReceiptItems.AddRange(newItems);

        // Update receipt total
        receipt.Total = total;
        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Draft parse applied to receipt {ReceiptId}, created {ItemCount} items, total: {Total}", 
            id, newItems.Count, total);

        return MapToDto(receipt);
    }

    public async Task<bool> DeleteReceiptAsync(Guid id)
    {
        var receipt = await _context.Receipts.FindAsync(id);
        
        if (receipt == null)
        {
            return false;
        }

        // Delete associated file
        if (!string.IsNullOrEmpty(receipt.ImagePath))
        {
            await _fileStorageService.DeleteFileAsync(receipt.ImagePath);
        }

        // Delete receipt from database
        _context.Receipts.Remove(receipt);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Receipt deleted: {ReceiptId}", id);

        return true;
    }

    private static ReceiptDto MapToDto(Receipt receipt)
    {
        return new ReceiptDto
        {
            Id = receipt.Id,
            PurchasedAt = receipt.PurchasedAt,
            MerchantName = receipt.MerchantName,
            Currency = receipt.Currency,
            Total = receipt.Total,
            ImagePath = receipt.ImagePath,
            LocationName = receipt.LocationName,
            Latitude = receipt.Latitude,
            Longitude = receipt.Longitude,
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt
        };
    }
}
