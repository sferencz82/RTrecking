# Receipt Tracking Application - Architecture Plan

## Overview
A personal receipt tracking application for Switzerland with OCR capabilities, manual correction, and reporting features.

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 10 Web API
- **ORM**: Entity Framework Core
- **Database**: MySQL
- **API Documentation**: Swagger/OpenAPI
- **Authentication**: None (personal use)

### Frontend
- **Framework**: Angular (latest stable)
- **HTTP Client**: Angular HttpClient
- **UI Components**: Angular Material (recommended)
- **Image Handling**: File upload from camera roll or folder, preview, canvas manipulation

### OCR Integration
- **Options**: Tesseract.js (client-side) or cloud service (Google Vision API, Azure Computer Vision, AWS Textract)
- **Recommendation**: Start with Tesseract.js for offline capability, consider cloud for better accuracy

## Architecture Principles

1. **Separation of Concerns**: Clear separation between backend API and frontend SPA
2. **RESTful API**: Standard REST endpoints for all operations
3. **Swiss Localization**: CHF currency, decimal comma support, multi-language receipt handling
4. **Data Validation**: Both client-side and server-side validation
5. **Error Handling**: Comprehensive error handling and user feedback

## Database Schema

### Entities
We should have a BaseEntity to all Entities that is the same troughout of the project. This means we can rely on theses fields exist in all Entity, we use a specific field as EntityStateId that has 4 possible state, Draft = 2, Created = 1, Deleted = 3, ToPurg = 4. This should help us sof delete some of the items we dont want to see, but in a mistake we can reinstate the original state.

#### BaseEntity
- `Id` (int, PK, auto-increment)
- `CreatedAt` (DateTime, default: now)
- `UpdatedAt` (DateTime, default: now)
- `EntityStateId` (int, default: 1)


#### Receipt
- `Id` (int, PK, auto-increment)
- `MerchantName` (string, nullable) - Raw OCR text
- `MerchantNameNormalized` (string, nullable) - Normalized/cleaned name
- `ReceiptDate` (DateTime, nullable) - Extracted date from receipt
- `ReceiptTime` (TimeSpan, nullable) - Extracted time from receipt
- `TotalAmount` (decimal(18,2), nullable) - Total from receipt
- `Currency` (string, default: "CHF")
- `ImagePath` (string) - Path to uploaded image file
- `ImageFileName` (string) - Original filename
- `OcrRawText` (string, nullable, TEXT) - Full OCR output for debugging
- `OcrConfidence` (decimal(5,2), nullable) - Overall OCR confidence score
- `IsProcessed` (bool, default: false) - Whether OCR has been run
- `IsCorrected` (bool, default: false) - Whether user has manually corrected
- `CreatedAt` (DateTime, default: now)
- `UpdatedAt` (DateTime, default: now)
- `EntityStateId` (int, default: 1)
- `Notes` (string, nullable, TEXT)

#### LineItem
- `Id` (int, PK, auto-increment)
- `ReceiptId` (int, FK to Receipt)
- `NameRaw` (string) - Original OCR text for item name
- `NameNormalized` (string) - Normalized/cleaned item name
- `Price` (decimal(18,2)) - Unit price or total price
- `Quantity` (decimal(10,3), default: 1) - Quantity purchased
- `Unit` (string, nullable) - Unit of measurement (kg, g, l, ml, pcs, stk, etc.)
- `WeightKg` (decimal(10,3), nullable) - Actual weight in kilograms
- `Category` (string, nullable) - User-assigned category
- `EdibleFraction` (decimal(5,3), nullable) - Fraction that is edible (e.g., 0.8 for 80% edible, accounting for bones, peels, etc.)
- `LineNumber` (int, nullable) - Original line number on receipt
- `OcrConfidence` (decimal(5,2), nullable) - OCR confidence for this line
- `CreatedAt` (DateTime, default: now)
- `UpdatedAt` (DateTime, default: now)
- `EntityStateId` (int, default: 1)

#### Category (Optional - for predefined categories)
- `Id` (int, PK, auto-increment)
- `Name` (string, unique)
- `Description` (string, nullable)
- `Color` (string, nullable) - For UI display
- `CreatedAt` (DateTime, default: now)
- `UpdatedAt` (DateTime, default: now)
- `EntityStateId` (int, default: 1)
## API Endpoints

### Receipts

#### POST /api/receipts/upload
- **Description**: Upload receipt image
- **Request**: multipart/form-data with image file
- **Response**: ReceiptDto with Id, ImagePath, Status
- **Notes**: Saves file to storage, creates Receipt record

#### POST /api/receipts/{id}/ocr
- **Description**: Trigger OCR processing on uploaded receipt
- **Response**: ReceiptDto with populated OCR data
- **Notes**: Can be called automatically after upload or manually

#### GET /api/receipts
- **Description**: Get paginated list of receipts
- **Query Parameters**: 
  - `page` (int, default: 1)
  - `pageSize` (int, default: 20)
  - `startDate` (DateTime?, optional)
  - `endDate` (DateTime?, optional)
  - `merchant` (string?, optional)
  - `isProcessed` (bool?, optional)
- **Response**: PagedResult<ReceiptDto>

#### GET /api/receipts/{id}
- **Description**: Get single receipt with line items
- **Response**: ReceiptDetailDto

#### PUT /api/receipts/{id}
- **Description**: Update receipt metadata (merchant, date, etc.)
- **Request**: ReceiptUpdateDto
- **Response**: ReceiptDto

#### DELETE /api/receipts/{id}
- **Description**: Delete receipt and associated line items
- **Response**: 204 No Content

#### GET /api/receipts/{id}/image
- **Description**: Get receipt image file
- **Response**: File stream

### Line Items

#### GET /api/receipts/{receiptId}/lineitems
- **Description**: Get all line items for a receipt
- **Response**: List<LineItemDto>

#### POST /api/receipts/{receiptId}/lineitems
- **Description**: Create new line item
- **Request**: LineItemCreateDto
- **Response**: LineItemDto

#### PUT /api/lineitems/{id}
- **Description**: Update line item
- **Request**: LineItemUpdateDto
- **Response**: LineItemDto

#### DELETE /api/lineitems/{id}
- **Description**: Delete line item
- **Response**: 204 No Content

### Reports

#### GET /api/reports/summary
- **Description**: Get summary statistics
- **Query Parameters**:
  - `startDate` (DateTime, required)
  - `endDate` (DateTime, required)
- **Response**: ReportSummaryDto
  - TotalSpent (decimal)
  - TotalItems (int)
  - ReceiptCount (int)
  - CategoryBreakdown (List<CategorySummaryDto>)
  - DateRange (DateRangeDto)

#### GET /api/reports/by-category
- **Description**: Get spending breakdown by category
- **Query Parameters**:
  - `startDate` (DateTime, required)
  - `endDate` (DateTime, required)
  - `groupBy` (string: "day"|"week"|"month", default: "month")
- **Response**: List<CategoryTimeSeriesDto>

#### GET /api/reports/over-time
- **Description**: Get spending trends over time
- **Query Parameters**:
  - `startDate` (DateTime, required)
  - `endDate` (DateTime, required)
  - `groupBy` (string: "day"|"week"|"month", default: "day")
- **Response**: List<TimeSeriesDataPointDto>

#### GET /api/reports/categories
- **Description**: Get list of all categories used
- **Response**: List<CategoryDto>

### Categories (Optional)

#### GET /api/categories
- **Description**: Get all predefined categories
- **Response**: List<CategoryDto>

#### POST /api/categories
- **Description**: Create new category
- **Request**: CategoryCreateDto
- **Response**: CategoryDto

#### PUT /api/categories/{id}
- **Description**: Update category
- **Request**: CategoryUpdateDto
- **Response**: CategoryDto

#### DELETE /api/categories/{id}
- **Description**: Delete category
- **Response**: 204 No Content

## Frontend UI Screens

### 1. Receipt List Screen (`/receipts`)
- **Purpose**: Display all receipts in a table/card view
- **Features**:
  - Pagination
  - Date range filter
  - Merchant search filter
  - Status filter (processed/unprocessed)
  - Sort by date (newest/oldest)
  - Quick actions: View, Edit, Delete, Process OCR
  - Upload new receipt button (floating action button or header button)

### 2. Receipt Upload Screen (`/receipts/upload`)
- **Purpose**: Upload and process new receipt
- **Features**:
  - Drag-and-drop or file picker for image upload
  - Image preview
  - Upload progress indicator
  - Auto-trigger OCR after upload (optional)
  - Manual OCR trigger button
  - Navigation to detail/edit screen after upload

### 3. Receipt Detail/Edit Screen (`/receipts/:id`)
- **Purpose**: View and edit receipt with line items
- **Features**:
  - Receipt image display (zoomable)
  - Receipt metadata editor:
    - Merchant name (raw + normalized)
    - Date picker
    - Time picker
    - Total amount
  - Line items table/list:
    - Add new line item button
    - Edit inline or modal
    - Delete line item
    - Fields: Name (raw/normalized), Price, Quantity, Unit, Weight (kg), Category (dropdown/autocomplete), Edible Fraction
  - Save/Cancel buttons
  - OCR confidence indicators (optional visual feedback)

### 4. Reports Dashboard (`/reports`)
- **Purpose**: Display summary and analytics
- **Features**:
  - Date range selector:
    - Preset buttons: Today, This Week, This Month, Last Week, Last Month
    - Custom range picker (with 6-month max validation)
    - Quick presets dropdown
  - Summary cards:
    - Total spent (CHF)
    - Number of receipts
    - Number of items
    - Average per receipt
  - Charts:
    - Spending by category (pie/bar chart)
    - Spending over time (line chart)
    - Category trends (stacked area/bar chart)
  - Export options (optional: CSV, PDF)

### 5. Category Management Screen (`/categories`) - Optional
- **Purpose**: Manage predefined categories
- **Features**:
  - List of categories with color indicators
  - Add/Edit/Delete categories
  - Category usage statistics

## Data Transfer Objects (DTOs)

### ReceiptDto
```csharp
{
  Id: int,
  MerchantName: string?,
  MerchantNameNormalized: string?,
  ReceiptDate: DateTime?,
  ReceiptTime: TimeSpan?,
  TotalAmount: decimal?,
  Currency: string,
  ImageFileName: string,
  IsProcessed: bool,
  IsCorrected: bool,
  CreatedAt: DateTime,
  UpdatedAt: DateTime,
  LineItemsCount: int,
  EntityStateId: int
}
```

### ReceiptDetailDto (extends ReceiptDto)
```csharp
{
  ...ReceiptDto fields,
  LineItems: List<LineItemDto>,
  ImageUrl: string,
  EntityStateId: int
}
```

### ReceiptUpdateDto
```csharp
{
  MerchantName: string?,
  MerchantNameNormalized: string?,
  ReceiptDate: DateTime?,
  ReceiptTime: TimeSpan?,
  TotalAmount: decimal?,
  Notes: string?,
  EntityStateId: int
}
```

### LineItemDto
```csharp
{
  Id: int,
  ReceiptId: int,
  NameRaw: string,
  NameNormalized: string,
  Price: decimal,
  Quantity: decimal,
  Unit: string?,
  WeightKg: decimal?,
  Category: string?,
  EdibleFraction: decimal?,
  LineNumber: int?,
  CreatedAt: DateTime,
  UpdatedAt: DateTime,
  EntityStateId: int
}
```

### LineItemCreateDto / LineItemUpdateDto
```csharp
{
  NameRaw: string,
  NameNormalized: string,
  Price: decimal,
  Quantity: decimal,
  Unit: string?,
  WeightKg: decimal?,
  Category: string?,
  EdibleFraction: decimal?,
  LineNumber: int?,
  EntityStateId: int
}
```

### ReportSummaryDto
```csharp
{
  StartDate: DateTime,
  EndDate: DateTime,
  TotalSpent: decimal,
  TotalItems: int,
  ReceiptCount: int,
  CategoryBreakdown: List<CategorySummaryDto>,
  EntityStateId: int
}
```

### CategorySummaryDto
```csharp
{
  Category: string,
  TotalSpent: decimal,
  ItemCount: int,
  Percentage: decimal,
  EntityStateId: int
}
```

### CategoryTimeSeriesDto
```csharp
{
  Category: string,
  DataPoints: List<TimeSeriesDataPointDto>,
  EntityStateId: int
}
```

### TimeSeriesDataPointDto
```csharp
{
  Date: DateTime,
  Amount: decimal,
  ItemCount: int,
  EntityStateId: int
}
```

## Folder Structure

```
RTracking/
├── backend/
│   ├── RTracking.Api/
│   │   ├── Controllers/
│   │   │   ├── ReceiptsController.cs
│   │   │   ├── LineItemsController.cs
│   │   │   ├── ReportsController.cs
│   │   │   └── CategoriesController.cs
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── DbInitializer.cs (optional)
│   │   ├── DTOs/
│   │   │   ├── ReceiptDto.cs
│   │   │   ├── ReceiptDetailDto.cs
│   │   │   ├── ReceiptUpdateDto.cs
│   │   │   ├── LineItemDto.cs
│   │   │   ├── LineItemCreateDto.cs
│   │   │   ├── LineItemUpdateDto.cs
│   │   │   ├── ReportSummaryDto.cs
│   │   │   ├── CategorySummaryDto.cs
│   │   │   ├── CategoryTimeSeriesDto.cs
│   │   │   ├── TimeSeriesDataPointDto.cs
│   │   │   ├── CategoryDto.cs
│   │   │   ├── CategoryCreateDto.cs
│   │   │   ├── CategoryUpdateDto.cs
│   │   │   └── PagedResult.cs
│   │   ├── Models/
│   │   │   ├── Receipt.cs
│   │   │   ├── LineItem.cs
│   │   │   └── Category.cs
│   │   ├── Services/
│   │   │   ├── IReceiptService.cs
│   │   │   ├── ReceiptService.cs
│   │   │   ├── ILineItemService.cs
│   │   │   ├── LineItemService.cs
│   │   │   ├── IReportService.cs
│   │   │   ├── ReportService.cs
│   │   │   ├── IOcrService.cs
│   │   │   ├── OcrService.cs (or TesseractOcrService.cs)
│   │   │   ├── IFileStorageService.cs
│   │   │   └── FileStorageService.cs
│   │   ├── Mappings/
│   │   │   └── AutoMapperProfile.cs (optional, or manual mapping)
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── ApplicationBuilderExtensions.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── RTracking.Api.csproj
│   └── README.md
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/
│   │   │   │   ├── models/
│   │   │   │   │   ├── receipt.model.ts
│   │   │   │   │   ├── line-item.model.ts
│   │   │   │   │   ├── category.model.ts
│   │   │   │   │   └── report.model.ts
│   │   │   │   ├── services/
│   │   │   │   │   ├── receipt.service.ts
│   │   │   │   │   ├── line-item.service.ts
│   │   │   │   │   ├── report.service.ts
│   │   │   │   │   ├── category.service.ts
│   │   │   │   │   └── ocr.service.ts
│   │   │   │   └── interceptors/
│   │   │   │       └── http-error.interceptor.ts
│   │   │   ├── features/
│   │   │   │   ├── receipts/
│   │   │   │   │   ├── receipt-list/
│   │   │   │   │   │   ├── receipt-list.component.ts
│   │   │   │   │   │   ├── receipt-list.component.html
│   │   │   │   │   │   └── receipt-list.component.scss
│   │   │   │   │   ├── receipt-upload/
│   │   │   │   │   │   ├── receipt-upload.component.ts
│   │   │   │   │   │   ├── receipt-upload.component.html
│   │   │   │   │   │   └── receipt-upload.component.scss
│   │   │   │   │   ├── receipt-detail/
│   │   │   │   │   │   ├── receipt-detail.component.ts
│   │   │   │   │   │   ├── receipt-detail.component.html
│   │   │   │   │   │   └── receipt-detail.component.scss
│   │   │   │   │   └── receipt.module.ts
│   │   │   │   ├── reports/
│   │   │   │   │   ├── reports-dashboard/
│   │   │   │   │   │   ├── reports-dashboard.component.ts
│   │   │   │   │   │   ├── reports-dashboard.component.html
│   │   │   │   │   │   └── reports-dashboard.component.scss
│   │   │   │   │   ├── date-range-selector/
│   │   │   │   │   │   ├── date-range-selector.component.ts
│   │   │   │   │   │   ├── date-range-selector.component.html
│   │   │   │   │   │   └── date-range-selector.component.scss
│   │   │   │   │   └── reports.module.ts
│   │   │   │   └── categories/ (optional)
│   │   │   │       ├── category-management/
│   │   │   │       │   ├── category-management.component.ts
│   │   │   │       │   ├── category-management.component.html
│   │   │   │       │   └── category-management.component.scss
│   │   │   │       └── categories.module.ts
│   │   │   ├── shared/
│   │   │   │   ├── components/
│   │   │   │   │   ├── line-item-editor/
│   │   │   │   │   │   ├── line-item-editor.component.ts
│   │   │   │   │   │   ├── line-item-editor.component.html
│   │   │   │   │   │   └── line-item-editor.component.scss
│   │   │   │   │   ├── image-viewer/
│   │   │   │   │   │   ├── image-viewer.component.ts
│   │   │   │   │   │   ├── image-viewer.component.html
│   │   │   │   │   │   └── image-viewer.component.scss
│   │   │   │   │   └── loading-spinner/
│   │   │   │   │       ├── loading-spinner.component.ts
│   │   │   │   │       ├── loading-spinner.component.html
│   │   │   │   │       └── loading-spinner.component.scss
│   │   │   │   ├── pipes/
│   │   │   │   │   ├── currency.pipe.ts (CHF formatting)
│   │   │   │   │   └── date-format.pipe.ts
│   │   │   │   └── validators/
│   │   │   │       └── date-range.validator.ts
│   │   │   ├── app.component.ts
│   │   │   ├── app.component.html
│   │   │   ├── app.component.scss
│   │   │   ├── app-routing.module.ts
│   │   │   └── app.module.ts
│   │   ├── assets/
│   │   │   ├── images/
│   │   │   └── icons/
│   │   ├── environments/
│   │   │   ├── environment.ts
│   │   │   └── environment.prod.ts
│   │   ├── styles/
│   │   │   └── styles.scss
│   │   ├── index.html
│   │   └── main.ts
│   ├── angular.json
│   ├── package.json
│   ├── tsconfig.json
│   ├── tsconfig.app.json
│   └── README.md
├── .gitignore
└── README.md
```

## Key Implementation Considerations

### Swiss Localization
- **Currency**: Always display CHF, use decimal comma (,) for display, but store as decimal in database
- **Date Format**: Support DD.MM.YYYY format (Swiss standard)
- **Time Format**: 24-hour format (HH:mm)
- **Number Parsing**: Handle both comma and dot as decimal separators in OCR

### OCR Processing
- **Strategy**: 
  1. Upload image → Save to storage
  2. Trigger OCR (client-side with Tesseract.js or server-side API call)
  3. Parse OCR text to extract:
     - Merchant name (usually at top)
     - Date/time (various formats: DD.MM.YYYY, DD/MM/YYYY, etc.)
     - Line items (price patterns: CHF X.XX or X,XX)
     - Total amount
  4. Store raw OCR text for debugging
  5. Allow manual correction of all fields

### File Storage
- Store uploaded images in `wwwroot/uploads/receipts/` or configured storage path
- Generate unique filenames (GUID + extension)
- Consider image optimization/compression

### Validation Rules
- Date range in reports: Maximum 6 months
- Price: Must be positive
- Quantity: Must be positive
- Edible fraction: Between 0 and 1 (if provided)
- Weight: Must be positive (if provided)

### Error Handling
- API: Return appropriate HTTP status codes and error messages
- Frontend: Display user-friendly error messages
- Log errors for debugging

## Development Workflow

1. **Backend First**: Set up API structure, models, DbContext, basic CRUD
2. **Database Migration**: Create initial migration
3. **Frontend Setup**: Angular project, routing, basic components
4. **Integration**: Connect frontend to backend API
5. **OCR Integration**: Implement OCR service
6. **Reports**: Implement reporting endpoints and UI
7. **Polish**: Error handling, validation, UI improvements

## Next Steps

After reviewing this architecture plan, proceed with:
1. Creating the backend project structure
2. Setting up Entity Framework models and DbContext
3. Implementing API controllers
4. Creating the Angular frontend project
5. Building UI components
6. Integrating OCR functionality
7. Implementing reporting features
