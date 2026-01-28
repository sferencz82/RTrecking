# OCR Implementation Summary

## Overview

OCR functionality has been implemented using Tesseract.NET for offline receipt text extraction. The service extracts merchant name, purchase date/time, and line items from receipt images.

## Components

### 1. Model Updates

**Receipt.cs** - Added field:
- `OcrRawText` (string?, TEXT) - Stores the raw OCR output for debugging

### 2. DTOs

**DraftReceiptParseDto.cs**:
- `MerchantName` (string?) - Extracted merchant name
- `PurchasedAt` (DateTime?) - Extracted purchase date/time
- `Items` (List<DraftReceiptItemDto>) - Extracted line items

**DraftReceiptItemDto**:
- `RawName` (string) - Item name from OCR
- `PriceTotal` (decimal) - Item price
- `Quantity` (decimal?) - Optional quantity
- `Unit` (string?) - Optional unit (kg, g, l, pcs, etc.)
- `WeightKg` (decimal?) - Optional weight in kilograms

### 3. Services

**IReceiptOcrService** / **ReceiptOcrService**:
- `ProcessReceiptImageAsync()` - Main OCR processing method
- `GetRawOcrTextAsync()` - Extracts raw text from image

### 4. Endpoint

**POST /api/receipts/{id}/ocr**
- Processes the receipt image
- Saves raw OCR text to receipt
- Returns `DraftReceiptParseDto` with extracted data
- Does NOT auto-save items (user must review first)

## Parsing Heuristics

### Merchant Name Extraction
- Looks in first 3-5 lines
- Skips lines that look like dates, prices, or headers
- Returns first reasonable candidate

### Date/Time Extraction
- Supports Swiss formats: DD.MM.YYYY, DD/MM/YYYY, DD-MM-YYYY
- Also supports: YYYY.MM.DD
- Validates date is within reasonable range (not future, not too old)
- Extracts time if present (HH:MM or HH.MM format)

### Line Item Extraction
- Identifies lines ending with CHF amounts
- Patterns: `12.50`, `12,50`, `12.5 CHF`, etc.
- Extracts item name (text before price)
- Attempts to extract:
  - **Quantity**: Patterns like "2x", "2 x", "2 pcs"
  - **Unit**: kg, g, l, ml, pcs, stk, etc.
  - **Weight**: "0.532 kg", "532 g" (converts to kg)

### Price Detection
- Handles both decimal comma (`,`) and dot (`.`) as separators
- Recognizes CHF, Fr., SFr. currency indicators
- Supports amounts from 0.01 to 999.99

### Weight Detection
- Recognizes: "0.532 kg", "532 g", "0,532 kg"
- Automatically converts grams to kilograms
- Stores as `WeightKg` in the DTO

## Language Support

- **English (eng)**: Always available
- **German (deu)**: Primary Swiss language, used if available
- **French (fra)**: Can be added (see OCR_SETUP.md)
- **Italian (ita)**: Can be added (see OCR_SETUP.md)

Default: `eng+deu`, falls back to `eng` if German not available.

## Usage Example

```bash
# 1. Upload receipt
POST /api/receipts/upload
Response: { "id": "guid-here", ... }

# 2. Run OCR
POST /api/receipts/{guid-here}/ocr
Response: {
  "merchantName": "Migros",
  "purchasedAt": "2026-01-22T14:30:00Z",
  "items": [
    {
      "rawName": "Brot 500g",
      "priceTotal": 2.50,
      "quantity": 1,
      "unit": "g",
      "weightKg": 0.5
    },
    {
      "rawName": "Milch 1l",
      "priceTotal": 1.80,
      "quantity": 1,
      "unit": "l"
    }
  ]
}
```

## Error Handling

- Returns 404 if receipt not found
- Returns 404 if image file not found
- Returns 500 if OCR processing fails
- Logs all errors for debugging

## Limitations

- OCR accuracy depends on image quality
- Parsing heuristics are best-effort (may miss some items)
- Date/time extraction may fail on non-standard formats
- Weight/quantity extraction is pattern-based and may miss edge cases

## Future Improvements

1. Add image preprocessing (deskew, denoise, contrast adjustment)
2. Machine learning for better item extraction
3. Support for more receipt formats
4. Cloud OCR fallback option
5. Confidence scores for extracted fields
6. User feedback loop to improve parsing

## Testing

Test with various receipt formats:
- Swiss grocery stores (Migros, Coop)
- Different languages (German, French, Italian)
- Various image qualities
- Different date formats
