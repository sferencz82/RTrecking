# Architecture Summary - Quick Reference

## Entities

### Receipt
- Id (PK)
- MerchantName (raw OCR)
- MerchantNameNormalized
- ReceiptDate
- ReceiptTime
- TotalAmount
- Currency (default: CHF)
- ImagePath
- ImageFileName
- OcrRawText
- OcrConfidence
- IsProcessed
- IsCorrected
- CreatedAt
- UpdatedAt
- Notes
- EntityStateId

### LineItem
- Id (PK)
- ReceiptId (FK)
- NameRaw (OCR text)
- NameNormalized
- Price
- Quantity
- Unit (kg/g/l/pcs/etc.)
- WeightKg (nullable)
- Category
- EdibleFraction (nullable, 0-1)
- LineNumber
- OcrConfidence
- CreatedAt
- UpdatedAt
- EntityStateId

### Category (Optional)
- Id (PK)
- Name (unique)
- Description
- Color
- CreatedAt
- UpdatedAt
- EntityStateId

## API Endpoints

### Receipts
- `POST /api/receipts/upload` - Upload receipt image
- `POST /api/receipts/{id}/ocr` - Trigger OCR processing
- `GET /api/receipts` - List receipts (paginated, filtered)
- `GET /api/receipts/{id}` - Get receipt details
- `PUT /api/receipts/{id}` - Update receipt
- `DELETE /api/receipts/{id}` - Delete receipt
- `GET /api/receipts/{id}/image` - Get image file

### Line Items
- `GET /api/receipts/{receiptId}/lineitems` - Get line items
- `POST /api/receipts/{receiptId}/lineitems` - Create line item
- `PUT /api/lineitems/{id}` - Update line item
- `DELETE /api/lineitems/{id}` - Delete line item

### Reports
- `GET /api/reports/summary` - Summary statistics
- `GET /api/reports/by-category` - Category breakdown over time
- `GET /api/reports/over-time` - Spending trends
- `GET /api/reports/categories` - List all categories used

### Categories (Optional)
- `GET /api/categories` - List categories
- `POST /api/categories` - Create category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

## UI Screens

1. **Receipt List** (`/receipts`)
   - Table/card view with pagination
   - Filters: date range, merchant, status
   - Actions: view, edit, delete, process OCR
   - Upload button

2. **Receipt Upload** (`/receipts/upload`)
   - Drag-and-drop file upload
   - Image preview
   - OCR trigger (auto or manual)
   - Navigate to detail after upload

3. **Receipt Detail/Edit** (`/receipts/:id`)
   - Image viewer (zoomable)
   - Receipt metadata editor (merchant, date, time, total)
   - Line items table with inline editing
   - Add/Edit/Delete line items
   - Save/Cancel actions

4. **Reports Dashboard** (`/reports`)
   - Date range selector (presets + custom, max 6 months)
   - Summary cards (total spent, receipt count, item count)
   - Charts: by category, over time, category trends
   - Export options (optional)

5. **Category Management** (`/categories`) - Optional
   - List categories with colors
   - Add/Edit/Delete categories
   - Usage statistics

## Key Features

- **OCR Processing**: Extract merchant, date/time, line items from receipt images
- **Manual Correction**: Edit all OCR-extracted data
- **Line Item Tracking**: Name (raw + normalized), price, quantity, unit, weight, category, edible fraction
- **Reporting**: Date range selection, category summaries, time series analysis
- **Swiss Localization**: CHF currency, decimal comma support, multi-language receipts
