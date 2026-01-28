# OCR Setup Guide - Tesseract Configuration

## Overview

The application uses Tesseract OCR (via Tesseract.NET) for offline receipt text extraction. This guide explains how to set up the language data files (tessdata).

## NuGet Package

The project uses **Tesseract** version 5.2.0, which is a .NET wrapper for the Tesseract OCR engine.

## Language Support

The OCR service is configured to support:
- **English (eng)** - Always available
- **German (deu)** - Primary Swiss language
- **French (fra)** - Can be added later
- **Italian (ita)** - Can be added later

Currently, the service uses **eng+deu** by default, falling back to **eng only** if German language pack is not found.

## Tessdata Directory Location

The service looks for tessdata in the following locations (in order):
1. `{ContentRoot}/tessdata/` (project root)
2. `{WebRoot}/tessdata/` (wwwroot folder)

**Recommended location:** `backend/RTracking.Api/tessdata/`

## Downloading Language Data Files

### Option 1: Download from GitHub (Recommended)

1. Go to: https://github.com/tesseract-ocr/tessdata
2. Download the following `.traineddata` files:
   - `eng.traineddata` (English) - **Required**
   - `deu.traineddata` (German) - **Recommended**
   - `fra.traineddata` (French) - Optional
   - `ita.traineddata` (Italian) - Optional

3. Place all downloaded files in: `backend/RTracking.Api/tessdata/`

### Option 2: Using Git (if tessdata is in a submodule)

```bash
cd backend/RTracking.Api
git submodule add https://github.com/tesseract-ocr/tessdata.git tessdata
```

### Option 3: Direct Download Links

- English: https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata
- German: https://github.com/tesseract-ocr/tessdata/raw/main/deu.traineddata
- French: https://github.com/tesseract-ocr/tessdata/raw/main/fra.traineddata
- Italian: https://github.com/tesseract-ocr/tessdata/raw/main/ita.traineddata

## Directory Structure

After setup, your directory should look like:

```
backend/RTracking.Api/
├── tessdata/
│   ├── eng.traineddata
│   ├── deu.traineddata
│   ├── fra.traineddata (optional)
│   └── ita.traineddata (optional)
├── Controllers/
├── Services/
└── ...
```

## Configuration

The OCR service automatically detects the tessdata directory. No additional configuration is needed in `appsettings.json`.

### Adding More Languages

To add French and Italian support, modify `ReceiptOcrService.cs`:

```csharp
// In GetRawOcrTextAsync method, change:
var languages = "eng+deu+fra+ita";

// Add checks for language files:
if (!Directory.Exists(Path.Combine(_tessdataPath, "fra.traineddata")))
{
    languages = languages.Replace("+fra", "");
}
if (!Directory.Exists(Path.Combine(_tessdataPath, "ita.traineddata")))
{
    languages = languages.Replace("+ita", "");
}
```

## Testing OCR

1. Upload a receipt image using `POST /api/receipts/upload`
2. Call `POST /api/receipts/{id}/ocr` with the receipt ID
3. Check the response for parsed data

## Troubleshooting

### Error: "Tessdata directory not found"

- Ensure the `tessdata` folder exists in the project root
- Check that `.traineddata` files are directly in the `tessdata` folder (not in subfolders)
- Verify file names match exactly: `eng.traineddata`, `deu.traineddata`, etc.

### Error: "Failed to load language"

- Verify the language file exists and is not corrupted
- Check file permissions
- Ensure the file is not locked by another process

### Poor OCR Results

- Use high-quality images (minimum 300 DPI recommended)
- Ensure good contrast and lighting
- Try preprocessing images (deskew, denoise) before OCR
- Consider using Tesseract's image preprocessing options

## File Sizes

Approximate file sizes:
- `eng.traineddata`: ~4.5 MB
- `deu.traineddata`: ~4.5 MB
- `fra.traineddata`: ~4.5 MB
- `ita.traineddata`: ~4.5 MB

Total for all 4 languages: ~18 MB

## Git Configuration

If you want to include tessdata in your repository:

```bash
# Add to .gitignore (if you want to exclude it)
echo "tessdata/*.traineddata" >> .gitignore
```

Or use Git LFS for large files:

```bash
git lfs track "tessdata/*.traineddata"
```

## Production Deployment

For production:
1. Ensure tessdata folder is included in deployment
2. Set proper file permissions
3. Consider using environment variables for tessdata path if needed
4. Test OCR functionality after deployment

## Alternative: Cloud OCR Services

If Tesseract results are insufficient, consider:
- Google Cloud Vision API
- Azure Computer Vision
- AWS Textract

These can be added as alternative implementations of `IReceiptOcrService`.
