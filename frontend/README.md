# Receipt Tracking Frontend

Angular application for receipt tracking with OCR capabilities.

## Setup

```bash
# Install dependencies
npm install

# Run development server
ng serve

# Build for production
ng build
```

## Development Server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`.

## Project Structure

```
src/
├── app/
│   ├── components/
│   │   ├── receipts-list/          # Receipt list with filtering
│   │   ├── upload-receipt/         # Upload and OCR flow
│   │   ├── receipt-detail/         # Edit receipt and items
│   │   └── reports-dashboard/      # Reports with charts
│   ├── models/                      # TypeScript interfaces
│   ├── services/                    # API services
│   └── app.component.ts             # Main layout with navigation
├── environments/                    # Environment configuration
└── styles.scss                      # Global styles
```

## Features

- **Receipts List**: View and filter receipts by date range
- **Upload Receipt**: Upload image → OCR → Review → Apply draft
- **Receipt Detail**: Edit receipt info and items with full CRUD
- **Reports Dashboard**: Date presets, custom range, charts

## API Configuration

Update `src/environments/environment.ts` to set the API base URL:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000'
};
```

## Dependencies

- Angular 19
- Angular Material
- Chart.js / ng2-charts
- RxJS
