# Angular Frontend Setup Commands

## Initial Setup

```bash
# Create Angular project (already done)
ng new frontend --routing --style=scss --skip-git

# Install dependencies
cd frontend
npm install @angular/material@^19.0.0 @angular/cdk@^19.0.0 @angular/animations chart.js ng2-charts --legacy-peer-deps
```

## Development

```bash
# Start development server
ng serve

# Build for production
ng build --configuration production
```

## Project Structure Created

```
frontend/
├── src/
│   ├── app/
│   │   ├── components/
│   │   │   ├── receipts-list/
│   │   │   │   └── receipts-list.component.ts
│   │   │   ├── upload-receipt/
│   │   │   │   └── upload-receipt.component.ts
│   │   │   ├── receipt-detail/
│   │   │   │   └── receipt-detail.component.ts
│   │   │   └── reports-dashboard/
│   │   │       └── reports-dashboard.component.ts
│   │   ├── models/
│   │   │   ├── receipt.model.ts
│   │   │   ├── receipt-item.model.ts
│   │   │   ├── category.model.ts
│   │   │   └── report.model.ts
│   │   ├── services/
│   │   │   ├── receipts-api.service.ts
│   │   │   ├── reports-api.service.ts
│   │   │   └── categories.service.ts
│   │   ├── app.component.ts
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   └── styles.scss
```

## Features Implemented

### 1. Receipts List
- Date range filtering
- Table view with merchant, date, total
- Edit and delete actions

### 2. Upload Receipt
- File selection with preview
- Upload → OCR → Review draft → Apply flow
- Inline editing of draft items
- Add/remove items before applying

### 3. Receipt Detail
- Edit merchant name, date/time, location
- Editable items table with:
  - Name, normalized name
  - Category dropdown
  - Price, quantity, unit, weight
  - Edible fraction
  - Computed edible weight display
- Add/delete items
- Auto-save on blur

### 4. Reports Dashboard
- Date presets: This Week, Last Week, This Month, Last Month
- Custom date range with 6-month validation
- Charts:
  - Spend by Category (pie chart)
  - Spend Over Time (line chart)

## API Integration

All services configured to use `http://localhost:5000` by default.

Update `src/environments/environment.ts` for different environments.

## Material UI

- Navigation sidebar
- Cards, tables, forms
- Date pickers
- Buttons and icons

## Charts

Using Chart.js with ng2-charts:
- Pie chart for category breakdown
- Line chart for time series
