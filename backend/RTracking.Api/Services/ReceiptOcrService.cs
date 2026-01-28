using System.Globalization;
using System.Text.RegularExpressions;
using RTracking.Api.Data;
using RTracking.Api.DTOs;
using Tesseract;

namespace RTracking.Api.Services;

public class ReceiptOcrService : IReceiptOcrService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ReceiptOcrService> _logger;
    private readonly string _tessdataPath;

    public ReceiptOcrService(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<ReceiptOcrService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;

        // Determine tessdata path - check in project root or wwwroot
        var contentRoot = _environment.ContentRootPath;
        var tessdataInRoot = Path.Combine(contentRoot, "tessdata");
        var tessdataInWwwRoot = Path.Combine(_environment.WebRootPath ?? contentRoot, "tessdata");

        if (Directory.Exists(tessdataInRoot))
        {
            _tessdataPath = tessdataInRoot;
        }
        else if (Directory.Exists(tessdataInWwwRoot))
        {
            _tessdataPath = tessdataInWwwRoot;
        }
        else
        {
            _tessdataPath = tessdataInRoot; // Default location
            _logger.LogWarning("Tessdata directory not found. Expected at: {Path}", _tessdataPath);
        }
    }

    public async Task<DraftReceiptParseDto> ProcessReceiptImageAsync(Guid receiptId, string imagePath)
    {
        var rawText = await GetRawOcrTextAsync(imagePath);

        // Save raw OCR text to receipt
        var receipt = await _context.Receipts.FindAsync(receiptId);
        if (receipt != null)
        {
            receipt.OcrRawText = rawText;
            receipt.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Parse the OCR text
        var parseResult = ParseOcrText(rawText);

        return parseResult;
    }

    public async Task<string> GetRawOcrTextAsync(string imagePath)
    {
        try
        {
            // Resolve full image path
            var fullImagePath = ResolveImagePath(imagePath);
            
            if (!File.Exists(fullImagePath))
            {
                throw new FileNotFoundException($"Image not found: {fullImagePath}");
            }

            // Configure Tesseract with multiple languages
            // Try eng+deu first, fallback to eng only if deu not available
            var languages = "eng+deu";
            if (!Directory.Exists(Path.Combine(_tessdataPath, "deu.traineddata")))
            {
                _logger.LogWarning("German language pack not found, using English only");
                languages = "eng";
            }

            using var engine = new TesseractEngine(_tessdataPath, languages, EngineMode.Default);
            
            using var img = Pix.LoadFromFile(fullImagePath);
            using var page = engine.Process(img);

            var text = page.GetText();
            
            _logger.LogInformation("OCR completed. Text length: {Length}", text?.Length ?? 0);

            return text ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OCR processing");
            throw;
        }
    }

    private string ResolveImagePath(string imagePath)
    {
        if (Path.IsPathRooted(imagePath))
        {
            return imagePath;
        }

        // Try wwwroot first, then content root
        var wwwRootPath = Path.Combine(_environment.WebRootPath ?? string.Empty, imagePath.TrimStart('/', '\\'));
        if (File.Exists(wwwRootPath))
        {
            return wwwRootPath;
        }

        var contentRootPath = Path.Combine(_environment.ContentRootPath, imagePath.TrimStart('/', '\\'));
        return contentRootPath;
    }

    private DraftReceiptParseDto ParseOcrText(string ocrText)
    {
        if (string.IsNullOrWhiteSpace(ocrText))
        {
            return new DraftReceiptParseDto();
        }

        var lines = ocrText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var result = new DraftReceiptParseDto();

        // Try to extract merchant name (usually first few lines)
        result.MerchantName = ExtractMerchantName(lines);

        // Try to extract date/time
        result.PurchasedAt = ExtractDate(lines);

        // Extract line items
        result.Items = ExtractLineItems(lines);

        return result;
    }

    private string? ExtractMerchantName(List<string> lines)
    {
        // Merchant name is usually in the first 3-5 lines
        // Look for lines that don't look like prices or dates
        var candidateLines = lines.Take(5).ToList();
        
        foreach (var line in candidateLines)
        {
            // Skip lines that are clearly dates, prices, or common receipt headers
            if (IsDateLine(line) || IsPriceLine(line) || 
                line.Contains("RECEIPT", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("BON", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("QUITTUNG", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // If line has reasonable length and doesn't look like a price, it might be merchant name
            if (line.Length > 3 && line.Length < 100 && !ContainsOnlyNumbersAndSymbols(line))
            {
                return line;
            }
        }

        return null;
    }

    private DateTime? ExtractDate(List<string> lines)
    {
        // Swiss date formats: DD.MM.YYYY, DD/MM/YYYY, DD-MM-YYYY
        var datePatterns = new[]
        {
            @"\b(\d{1,2})[./-](\d{1,2})[./-](\d{2,4})\b", // DD.MM.YYYY or DD/MM/YYYY
            @"\b(\d{4})[./-](\d{1,2})[./-](\d{1,2})\b"  // YYYY.MM.DD
        };

        foreach (var line in lines.Take(10))
        {
            foreach (var pattern in datePatterns)
            {
                var match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    try
                    {
                        var day = int.Parse(match.Groups[1].Value);
                        var month = int.Parse(match.Groups[2].Value);
                        var year = int.Parse(match.Groups[3].Value);

                        // Handle 2-digit years
                        if (year < 100)
                        {
                            year += 2000;
                        }

                        // Validate date
                        if (month >= 1 && month <= 12 && day >= 1 && day <= 31)
                        {
                            var date = new DateTime(year, month, day);
                            
                            // Don't accept dates too far in future or past
                            if (date <= DateTime.Now.AddDays(1) && date >= DateTime.Now.AddYears(-10))
                            {
                                // Try to extract time from the same or next line
                                var time = ExtractTime(line);
                                if (!time.HasValue && lines.IndexOf(line) < lines.Count - 1)
                                {
                                    time = ExtractTime(lines[lines.IndexOf(line) + 1]);
                                }
                                if (time.HasValue)
                                {
                                    return date.Date.Add(time.Value);
                                }
                                return date;
                            }
                        }
                    }
                    catch
                    {
                        // Continue to next pattern
                    }
                }
            }
        }

        return null;
    }

    private TimeSpan? ExtractTime(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        // Time patterns: HH:MM, HH.MM
        var timePattern = @"\b(\d{1,2})[:.](\d{2})\b";
        var match = Regex.Match(line, timePattern);
        
        if (match.Success)
        {
            try
            {
                var hour = int.Parse(match.Groups[1].Value);
                var minute = int.Parse(match.Groups[2].Value);
                
                if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60)
                {
                    return new TimeSpan(hour, minute, 0);
                }
            }
            catch
            {
                // Invalid time
            }
        }

        return null;
    }

    private List<DraftReceiptItemDto> ExtractLineItems(List<string> lines)
    {
        var items = new List<DraftReceiptItemDto>();

        // CHF amount pattern: matches 12.50, 12,50, 12.5, etc.
        var chfPattern = @"\b(\d{1,3}(?:[.,]\d{2,3})?)\s*(?:CHF|Fr\.?|SFr\.?)?\b";
        var chfPatternEnd = @"\b(\d{1,3}(?:[.,]\d{2,3})?)\s*(?:CHF|Fr\.?|SFr\.?)?\s*$";

        foreach (var line in lines)
        {
            // Skip header lines, totals, dates
            if (IsHeaderLine(line) || IsTotalLine(line) || IsDateLine(line))
            {
                continue;
            }

            // Look for lines ending with a price
            var priceMatch = Regex.Match(line, chfPatternEnd);
            if (!priceMatch.Success)
            {
                // Try pattern without end anchor
                priceMatch = Regex.Match(line, chfPattern);
            }

            if (priceMatch.Success)
            {
                try
                {
                    var priceStr = priceMatch.Groups[1].Value.Replace(',', '.');
                    var price = decimal.Parse(priceStr, CultureInfo.InvariantCulture);

                    // Extract item name (everything before the price)
                    var priceIndex = priceMatch.Index;
                    var itemName = line.Substring(0, priceIndex).Trim();

                    // Skip if item name is too short or looks like a header
                    if (itemName.Length < 2 || IsHeaderLine(itemName))
                    {
                        continue;
                    }

                    var item = new DraftReceiptItemDto
                    {
                        RawName = itemName,
                        PriceTotal = price
                    };

                    // Try to extract quantity, unit, weight from the line
                    ExtractQuantityAndUnit(line, item);
                    ExtractWeight(line, item);

                    items.Add(item);
                }
                catch
                {
                    // Skip this line if parsing fails
                }
            }
        }

        return items;
    }

    private void ExtractQuantityAndUnit(string line, DraftReceiptItemDto item)
    {
        // Patterns: "2x", "2 x", "2 pcs", "2 Stk", "2x 500g"
        var qtyPattern = @"\b(\d+(?:[.,]\d+)?)\s*[xX×]\s*";
        var qtyMatch = Regex.Match(line, qtyPattern);
        
        if (qtyMatch.Success)
        {
            try
            {
                var qtyStr = qtyMatch.Groups[1].Value.Replace(',', '.');
                item.Quantity = decimal.Parse(qtyStr, CultureInfo.InvariantCulture);
            }
            catch
            {
                // Ignore
            }
        }

        // Extract unit: kg, g, l, ml, pcs, stk, etc.
        var unitPattern = @"\b(\d+(?:[.,]\d+)?)\s*(kg|g|l|ml|pcs|stk|st\.?|pce|piece)\b";
        var unitMatch = Regex.Match(line, unitPattern, RegexOptions.IgnoreCase);
        
        if (unitMatch.Success)
        {
            item.Unit = unitMatch.Groups[2].Value.ToLowerInvariant();
            
            // If quantity not set yet, try to get it from unit match
            if (!item.Quantity.HasValue)
            {
                try
                {
                    var qtyStr = unitMatch.Groups[1].Value.Replace(',', '.');
                    item.Quantity = decimal.Parse(qtyStr, CultureInfo.InvariantCulture);
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }

    private void ExtractWeight(string line, DraftReceiptItemDto item)
    {
        // Weight patterns: "0.532 kg", "532 g", "0,532 kg"
        var weightPattern = @"\b(\d+(?:[.,]\d+)?)\s*(kg|g)\b";
        var weightMatch = Regex.Match(line, weightPattern, RegexOptions.IgnoreCase);
        
        if (weightMatch.Success)
        {
            try
            {
                var weightStr = weightMatch.Groups[1].Value.Replace(',', '.');
                var weight = decimal.Parse(weightStr, CultureInfo.InvariantCulture);
                var unit = weightMatch.Groups[2].Value.ToLowerInvariant();

                // Convert to kg
                if (unit == "g")
                {
                    item.WeightKg = weight / 1000m;
                }
                else
                {
                    item.WeightKg = weight;
                }
            }
            catch
            {
                // Ignore
            }
        }
    }

    private bool IsDateLine(string line)
    {
        return Regex.IsMatch(line, @"\b\d{1,2}[./-]\d{1,2}[./-]\d{2,4}\b") ||
               Regex.IsMatch(line, @"\b\d{4}[./-]\d{1,2}[./-]\d{1,2}\b");
    }

    private bool IsPriceLine(string line)
    {
        // Line that is mostly just a price
        var priceOnlyPattern = @"^\s*(\d{1,3}(?:[.,]\d{2,3})?)\s*(?:CHF|Fr\.?|SFr\.?)?\s*$";
        return Regex.IsMatch(line, priceOnlyPattern, RegexOptions.IgnoreCase);
    }

    private bool IsTotalLine(string line)
    {
        var totalKeywords = new[] { "total", "summe", "total:", "summe:", "total:", "gesamt", "TOTAL", "SUMME" };
        return totalKeywords.Any(keyword => line.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsHeaderLine(string line)
    {
        var headerKeywords = new[] { "receipt", "bon", "quittung", "rechnung", "invoice", "kasse", "caisse" };
        return headerKeywords.Any(keyword => line.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
               line.All(c => char.IsUpper(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c));
    }

    private bool ContainsOnlyNumbersAndSymbols(string line)
    {
        return line.All(c => char.IsDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c));
    }
}
