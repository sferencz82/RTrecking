# Reports API Endpoints

## Overview

Reporting endpoints for analyzing receipt data with date range filtering and aggregation.

## Endpoints

### GET /api/reports/summary

Get summary report with totals, spend by category, and daily spend over time.

**Query Parameters:**
- `from` (required, DateTime) - Start date (YYYY-MM-DD) - treated as Swiss local time
- `to` (required, DateTime) - End date (YYYY-MM-DD) - treated as Swiss local time

**Validation:**
- Date range cannot exceed 184 days (approximately 6 months)
- Returns `400 Bad Request` if exceeded with message: "Date range cannot exceed 184 days (approximately 6 months). Requested range: X days."
- `to` date must be after `from` date

**Response:**
```json
{
  "fromDate": "2026-01-01T00:00:00",
  "toDate": "2026-01-31T00:00:00",
  "totalSpend": 1234.56,
  "spendByCategory": [
    {
      "categoryId": 1,
      "categoryName": "Meat & Fish",
      "totalSpend": 450.00
    },
    {
      "categoryId": 4,
      "categoryName": "Fruit & Vegetables",
      "totalSpend": 320.50
    }
  ],
  "spendOverTimeDaily": [
    {
      "date": "2026-01-01T00:00:00",
      "totalSpend": 45.80
    },
    {
      "date": "2026-01-02T00:00:00",
      "totalSpend": 0.00
    },
    {
      "date": "2026-01-03T00:00:00",
      "totalSpend": 67.20
    }
  ]
}
```

**Features:**
- Total spend across all receipts in date range
- Spend broken down by category (ordered by total spend descending)
- Daily spend buckets (includes all days in range, even with zero spend)
- Dates returned in Swiss local time

**Example:**
```bash
GET /api/reports/summary?from=2026-01-01&to=2026-01-31
```

### GET /api/reports/items

Get aggregated items report by normalized name.

**Query Parameters:**
- `from` (optional, DateTime) - Start date (YYYY-MM-DD) - treated as Swiss local time
- `to` (optional, DateTime) - End date (YYYY-MM-DD) - treated as Swiss local time
- `categoryId` (optional, int) - Filter by category ID

**Response:**
```json
{
  "items": [
    {
      "itemName": "Brot 500g",
      "totalSpent": 125.50,
      "totalWeightKg": 25.0,
      "itemCount": 50
    },
    {
      "itemName": "Milch 1l",
      "totalSpent": 89.00,
      "totalWeightKg": null,
      "itemCount": 45
    }
  ]
}
```

**Features:**
- Aggregates items by `NormalizedName` (falls back to `RawName` if NormalizedName is null/empty)
- Calculates total spent per item
- Calculates total weight (if available) per item
- Counts number of times item appears
- Ordered by total spent (descending)
- Optional date range filtering
- Optional category filtering

**Example:**
```bash
# All items
GET /api/reports/items

# Items in date range
GET /api/reports/items?from=2026-01-01&to=2026-01-31

# Items by category
GET /api/reports/items?categoryId=1

# Items in date range by category
GET /api/reports/items?from=2026-01-01&to=2026-01-31&categoryId=1
```

## Timezone Handling

### Storage
- All dates stored in **UTC** in the database
- `Receipt.PurchasedAt` is stored as UTC

### Input/Output
- API inputs/outputs are treated as **Swiss local time (CET/CEST)**
- Dates are converted to UTC for database queries
- Results are converted back to Swiss time for responses

### Conversion
- Input dates (e.g., `from=2026-01-22`) are treated as Swiss local time
- Converted to UTC for database queries
- Results converted back to Swiss local time

See `TIMEZONE_HANDLING.md` for detailed explanation.

## Date Range Validation

### Summary Report
- Maximum range: **184 days** (approximately 6 months)
- Returns `400 Bad Request` if exceeded
- Error message includes requested range in days

### Items Report
- No maximum range limit
- Optional date filtering

## Aggregation Logic

### Items Aggregation
1. Groups items by `NormalizedName` (if not null/empty)
2. Falls back to `RawName` if `NormalizedName` is null/empty
3. Sums `PriceTotal` for each group
4. Sums `WeightKg` for each group (if available)
5. Counts occurrences
6. Orders by `TotalSpent` descending

### Category Aggregation
1. Groups receipt items by `CategoryId` and `Category.Name`
2. Sums `PriceTotal` for each category
3. Orders by `TotalSpend` descending

### Daily Aggregation
1. Groups receipts by date (Swiss local time)
2. Sums `Receipt.Total` for each day
3. Fills in missing days with zero spend
4. Orders by date ascending

## Error Handling

- `400 Bad Request` - Invalid date range (exceeds 6 months or to < from)
- `500 Internal Server Error` - Server error during report generation

All errors include descriptive messages.

## Performance Considerations

- Reports query receipts with included items and categories
- Consider adding database indexes on `Receipt.PurchasedAt` and `ReceiptItem.CategoryId`
- Large date ranges may be slow - consider pagination for items report in future

## Usage Examples

### Monthly Summary
```bash
GET /api/reports/summary?from=2026-01-01&to=2026-01-31
```

### Quarterly Summary (within 6-month limit)
```bash
GET /api/reports/summary?from=2026-01-01&to=2026-03-31
```

### All Meat & Fish Items
```bash
GET /api/reports/items?categoryId=1
```

### Items Purchased in January
```bash
GET /api/reports/items?from=2026-01-01&to=2026-01-31
```
