# Project Folder Structure

```
RTracking/
│
├── backend/
│   └── RTracking.Api/
│       ├── Controllers/
│       │   ├── ReceiptsController.cs
│       │   ├── LineItemsController.cs
│       │   ├── ReportsController.cs
│       │   └── CategoriesController.cs
│       │
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── DbInitializer.cs (optional)
│       │
│       ├── DTOs/
│       │   ├── ReceiptDto.cs
│       │   ├── ReceiptDetailDto.cs
│       │   ├── ReceiptUpdateDto.cs
│       │   ├── LineItemDto.cs
│       │   ├── LineItemCreateDto.cs
│       │   ├── LineItemUpdateDto.cs
│       │   ├── ReportSummaryDto.cs
│       │   ├── CategorySummaryDto.cs
│       │   ├── CategoryTimeSeriesDto.cs
│       │   ├── TimeSeriesDataPointDto.cs
│       │   ├── CategoryDto.cs
│       │   ├── CategoryCreateDto.cs
│       │   ├── CategoryUpdateDto.cs
│       │   └── PagedResult.cs
│       │
│       ├── Models/
│       │   ├── Receipt.cs
│       │   ├── LineItem.cs
│       │   └── Category.cs
│       │
│       ├── Services/
│       │   ├── IReceiptService.cs
│       │   ├── ReceiptService.cs
│       │   ├── ILineItemService.cs
│       │   ├── LineItemService.cs
│       │   ├── IReportService.cs
│       │   ├── ReportService.cs
│       │   ├── IOcrService.cs
│       │   ├── OcrService.cs
│       │   ├── IFileStorageService.cs
│       │   └── FileStorageService.cs
│       │
│       ├── Mappings/
│       │   └── AutoMapperProfile.cs (optional)
│       │
│       ├── Extensions/
│       │   ├── ServiceCollectionExtensions.cs
│       │   └── ApplicationBuilderExtensions.cs
│       │
│       ├── wwwroot/
│       │   └── uploads/
│       │       └── receipts/
│       │
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── RTracking.Api.csproj
│
├── frontend/
│   └── src/
│       ├── app/
│       │   ├── core/
│       │   │   ├── models/
│       │   │   │   ├── receipt.model.ts
│       │   │   │   ├── line-item.model.ts
│       │   │   │   ├── category.model.ts
│       │   │   │   └── report.model.ts
│       │   │   │
│       │   │   ├── services/
│       │   │   │   ├── receipt.service.ts
│       │   │   │   ├── line-item.service.ts
│       │   │   │   ├── report.service.ts
│       │   │   │   ├── category.service.ts
│       │   │   │   └── ocr.service.ts
│       │   │   │
│       │   │   └── interceptors/
│       │   │       └── http-error.interceptor.ts
│       │   │
│       │   ├── features/
│       │   │   ├── receipts/
│       │   │   │   ├── receipt-list/
│       │   │   │   │   ├── receipt-list.component.ts
│       │   │   │   │   ├── receipt-list.component.html
│       │   │   │   │   └── receipt-list.component.scss
│       │   │   │   │
│       │   │   │   ├── receipt-upload/
│       │   │   │   │   ├── receipt-upload.component.ts
│       │   │   │   │   ├── receipt-upload.component.html
│       │   │   │   │   └── receipt-upload.component.scss
│       │   │   │   │
│       │   │   │   ├── receipt-detail/
│       │   │   │   │   ├── receipt-detail.component.ts
│       │   │   │   │   ├── receipt-detail.component.html
│       │   │   │   │   └── receipt-detail.component.scss
│       │   │   │   │
│       │   │   │   └── receipt.module.ts
│       │   │   │
│       │   │   ├── reports/
│       │   │   │   ├── reports-dashboard/
│       │   │   │   │   ├── reports-dashboard.component.ts
│       │   │   │   │   ├── reports-dashboard.component.html
│       │   │   │   │   └── reports-dashboard.component.scss
│       │   │   │   │
│       │   │   │   ├── date-range-selector/
│       │   │   │   │   ├── date-range-selector.component.ts
│       │   │   │   │   ├── date-range-selector.component.html
│       │   │   │   │   └── date-range-selector.component.scss
│       │   │   │   │
│       │   │   │   └── reports.module.ts
│       │   │   │
│       │   │   └── categories/ (optional)
│       │   │       ├── category-management/
│       │   │       │   ├── category-management.component.ts
│       │   │       │   ├── category-management.component.html
│       │   │       │   └── category-management.component.scss
│       │   │       │
│       │   │       └── categories.module.ts
│       │   │
│       │   ├── shared/
│       │   │   ├── components/
│       │   │   │   ├── line-item-editor/
│       │   │   │   │   ├── line-item-editor.component.ts
│       │   │   │   │   ├── line-item-editor.component.html
│       │   │   │   │   └── line-item-editor.component.scss
│       │   │   │   │
│       │   │   │   ├── image-viewer/
│       │   │   │   │   ├── image-viewer.component.ts
│       │   │   │   │   ├── image-viewer.component.html
│       │   │   │   │   └── image-viewer.component.scss
│       │   │   │   │
│       │   │   │   └── loading-spinner/
│       │   │   │       ├── loading-spinner.component.ts
│       │   │   │       ├── loading-spinner.component.html
│       │   │   │       └── loading-spinner.component.scss
│       │   │   │
│       │   │   ├── pipes/
│       │   │   │   ├── currency.pipe.ts
│       │   │   │   └── date-format.pipe.ts
│       │   │   │
│       │   │   └── validators/
│       │   │       └── date-range.validator.ts
│       │   │
│       │   ├── app.component.ts
│       │   ├── app.component.html
│       │   ├── app.component.scss
│       │   ├── app-routing.module.ts
│       │   └── app.module.ts
│       │
│       ├── assets/
│       │   ├── images/
│       │   └── icons/
│       │
│       ├── environments/
│       │   ├── environment.ts
│       │   └── environment.prod.ts
│       │
│       ├── styles/
│       │   └── styles.scss
│       │
│       ├── index.html
│       └── main.ts
│       │
│       ├── angular.json
│       ├── package.json
│       ├── tsconfig.json
│       └── tsconfig.app.json
│
├── .gitignore
├── ARCHITECTURE.md
├── ARCHITECTURE_SUMMARY.md
├── FOLDER_STRUCTURE.md
└── README.md
```

## Notes

- **Backend**: ASP.NET Core 10 Web API project in `backend/RTracking.Api/`
- **Frontend**: Angular application in `frontend/`
- **File Storage**: Receipt images stored in `backend/RTracking.Api/wwwroot/uploads/receipts/`
- **Database**: MySQL connection string in `appsettings.json`
- **Swagger**: Available at `/swagger` when running backend
