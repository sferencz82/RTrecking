# Complete Setup Guide - Receipt Tracking Application

## Backend Setup

```bash
# Navigate to backend
cd backend/RTracking.Api

# Restore packages (if needed)
dotnet restore

# Create and apply migration
dotnet ef migrations add InitialCreate --output-dir Migrations
dotnet ef database update

# Run backend
dotnet run
```

Backend will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

## Frontend Setup

```bash
# Navigate to frontend
cd frontend

# Install dependencies (if not already done)
npm install

# Start development server
ng serve
```

Frontend will be available at:
- `http://localhost:4200`

## Complete User Flow

### 1. Upload Receipt Flow

1. Navigate to "Upload Receipt"
2. Click "Choose Image" → Select receipt photo
3. Click "Upload" → Image uploaded, receipt created
4. OCR automatically processes → Shows draft results
5. Review and edit draft items inline:
   - Edit item names
   - Adjust prices
   - Add/remove items
   - Set quantities, units, weights
6. Click "Apply to Receipt" → Items created, navigate to detail page

### 2. Edit Receipt Flow

1. From Receipts List, click edit icon
2. Edit receipt information:
   - Merchant name
   - Purchase date/time
   - Location
3. Edit items in table:
   - All fields editable inline
   - Category dropdown
   - Edible fraction (0.1-1.0)
   - Weight × Edible Fraction = Edible Weight (auto-calculated)
4. Changes auto-save on blur
5. Add/delete items as needed

### 3. Reports Flow

1. Navigate to "Reports"
2. Select date range:
   - Use presets: This Week, Last Week, This Month, Last Month
   - Or custom range (max 6 months)
3. Click "Load Report"
4. View:
   - Total spend summary
   - Spend by Category (pie chart)
   - Spend Over Time (line chart)

## API Endpoints Used

### Receipts
- `POST /api/receipts/upload` - Upload image
- `POST /api/receipts/{id}/ocr` - Process OCR
- `POST /api/receipts/{id}/apply-draft` - Apply draft parse
- `GET /api/receipts` - List receipts (with date filters)
- `GET /api/receipts/{id}` - Get receipt
- `PUT /api/receipts/{id}` - Update receipt
- `DELETE /api/receipts/{id}` - Delete receipt

### Items
- `GET /api/receipts/{id}/items` - Get items
- `POST /api/receipts/{id}/items` - Create item
- `PUT /api/receipts/{id}/items/{itemId}` - Update item
- `DELETE /api/receipts/{id}/items/{itemId}` - Delete item

### Reports
- `GET /api/reports/summary?from=YYYY-MM-DD&to=YYYY-MM-DD` - Summary report
- `GET /api/reports/items?from=&to=&categoryId=` - Items report

### Categories
- `GET /api/categories` - Get all categories

## Key Features

✅ **Upload Flow**: Upload → OCR → Review → Apply  
✅ **Inline Editing**: All fields editable in tables  
✅ **Auto-save**: Changes saved on blur  
✅ **Computed Fields**: Edible Weight = WeightKg × EdibleFraction  
✅ **Date Presets**: Quick date range selection  
✅ **6-Month Validation**: Enforced in reports  
✅ **Charts**: Pie and line charts with Chart.js  
✅ **Image Preview**: Receipt images from backend static files  
✅ **Category Dropdown**: All categories available  

## Troubleshooting

### Backend Issues
- Ensure MySQL is running
- Check connection string in `appsettings.json`
- Verify migrations are applied

### Frontend Issues
- Ensure backend is running on port 5000
- Check CORS configuration
- Verify API base URL in `environment.ts`

### OCR Issues
- Ensure tessdata files are in `backend/RTracking.Api/tessdata/`
- Check logs for OCR errors
- Verify image format is supported

## Next Steps

1. **Test Complete Flow**: Upload → OCR → Edit → Reports
2. **Add Categories**: Ensure categories are seeded in database
3. **Configure Production**: Update environment files
4. **Deploy**: Build and deploy both backend and frontend
