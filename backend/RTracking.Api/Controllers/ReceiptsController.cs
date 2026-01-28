using Microsoft.AspNetCore.Mvc;
using RTracking.Api.DTOs;
using RTracking.Api.Services;

namespace RTracking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptsController : ControllerBase
{
    private readonly IReceiptService _receiptService;
    private readonly IReceiptItemService _itemService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IReceiptOcrService _ocrService;
    private readonly ILogger<ReceiptsController> _logger;

    public ReceiptsController(
        IReceiptService receiptService,
        IReceiptItemService itemService,
        IFileStorageService fileStorageService,
        IReceiptOcrService ocrService,
        ILogger<ReceiptsController> logger)
    {
        _receiptService = receiptService;
        _itemService = itemService;
        _fileStorageService = fileStorageService;
        _ocrService = ocrService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a receipt image and create a receipt record
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptDto>> UploadReceipt(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Validate file type (optional - add more validation as needed)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest($"File type not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        try
        {
            // Save file
            var imagePath = await _fileStorageService.SaveFileAsync(file);

            // Create receipt
            var receiptDto = new ReceiptCreateDto
            {
                PurchasedAt = DateTime.UtcNow, // Default to now, can be updated later
                MerchantName = string.Empty, // Will be filled by OCR later
                Currency = "CHF",
                ImagePath = imagePath
            };

            var receipt = await _receiptService.CreateReceiptAsync(receiptDto);

            return CreatedAtAction(nameof(GetReceipt), new { id = receipt.Id }, receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading receipt");
            return StatusCode(500, "An error occurred while uploading the receipt");
        }
    }

    /// <summary>
    /// Get a list of receipts with optional date filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReceiptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReceiptDto>>> GetReceipts(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var receipts = await _receiptService.GetReceiptsAsync(from, to);
        return Ok(receipts);
    }

    /// <summary>
    /// Get a receipt by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReceiptDto>> GetReceipt(Guid id)
    {
        var receipt = await _receiptService.GetReceiptByIdAsync(id);
        
        if (receipt == null)
        {
            return NotFound();
        }

        return Ok(receipt);
    }

    /// <summary>
    /// Delete a receipt and its associated file
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReceipt(Guid id)
    {
        var deleted = await _receiptService.DeleteReceiptAsync(id);
        
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Update receipt fields
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReceiptDto>> UpdateReceipt(Guid id, ReceiptUpdateDto dto)
    {
        var receipt = await _receiptService.UpdateReceiptAsync(id, dto);
        
        if (receipt == null)
        {
            return NotFound();
        }

        return Ok(receipt);
    }

    /// <summary>
    /// Run OCR on a receipt image and return draft parse results
    /// </summary>
    [HttpPost("{id}/ocr")]
    [ProducesResponseType(typeof(DraftReceiptParseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DraftReceiptParseDto>> ProcessOcr(Guid id)
    {
        var receipt = await _receiptService.GetReceiptByIdAsync(id);
        
        if (receipt == null)
        {
            return NotFound();
        }

        try
        {
            var parseResult = await _ocrService.ProcessReceiptImageAsync(id, receipt.ImagePath);
            return Ok(parseResult);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Image file not found for receipt {ReceiptId}", id);
            return NotFound("Image file not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OCR for receipt {ReceiptId}", id);
            return StatusCode(500, "An error occurred while processing OCR");
        }
    }

    /// <summary>
    /// Apply draft parse to create receipt items
    /// </summary>
    [HttpPost("{id}/apply-draft")]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptDto>> ApplyDraft(Guid id, DraftReceiptParseDto draft)
    {
        try
        {
            var receipt = await _receiptService.ApplyDraftParseAsync(id, draft);
            return Ok(receipt);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying draft to receipt {ReceiptId}", id);
            return StatusCode(500, "An error occurred while applying draft");
        }
    }

    /// <summary>
    /// Get all items for a receipt
    /// </summary>
    [HttpGet("{id}/items")]
    [ProducesResponseType(typeof(IEnumerable<ReceiptItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReceiptItemDto>>> GetReceiptItems(Guid id)
    {
        var items = await _itemService.GetItemsByReceiptIdAsync(id);
        return Ok(items);
    }

    /// <summary>
    /// Add a new item to a receipt
    /// </summary>
    [HttpPost("{id}/items")]
    [ProducesResponseType(typeof(ReceiptItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReceiptItemDto>> CreateItem(Guid id, ReceiptItemCreateDto dto)
    {
        try
        {
            var item = await _itemService.CreateItemAsync(id, dto);
            return CreatedAtAction(nameof(GetItem), new { id, itemId = item.Id }, item);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Receipt not found");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item for receipt {ReceiptId}", id);
            return StatusCode(500, "An error occurred while creating item");
        }
    }

    /// <summary>
    /// Get a single item by ID
    /// </summary>
    [HttpGet("{id}/items/{itemId}")]
    [ProducesResponseType(typeof(ReceiptItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReceiptItemDto>> GetItem(Guid id, Guid itemId)
    {
        var item = await _itemService.GetItemByIdAsync(itemId);
        
        if (item == null || item.ReceiptId != id)
        {
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// Update an item
    /// </summary>
    [HttpPut("{id}/items/{itemId}")]
    [ProducesResponseType(typeof(ReceiptItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptItemDto>> UpdateItem(Guid id, Guid itemId, ReceiptItemUpdateDto dto)
    {
        try
        {
            var item = await _itemService.UpdateItemAsync(id, itemId, dto);
            
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item {ItemId} for receipt {ReceiptId}", itemId, id);
            return StatusCode(500, "An error occurred while updating item");
        }
    }

    /// <summary>
    /// Delete an item
    /// </summary>
    [HttpDelete("{id}/items/{itemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(Guid id, Guid itemId)
    {
        var deleted = await _itemService.DeleteItemAsync(id, itemId);
        
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
