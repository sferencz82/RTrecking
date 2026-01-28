# Timezone Handling

## Overview

The application handles timezones consistently to ensure accurate date-based queries and reporting for Switzerland.

## Storage Strategy

**All dates are stored in UTC in the database.**

- `Receipt.PurchasedAt` is stored as UTC
- This ensures consistency regardless of server location or daylight saving time changes

## Input/Output Strategy

**API inputs and outputs are treated as Swiss local time (CET/CEST).**

### Input (API → Database)

When the frontend sends dates (e.g., `from=2026-01-22&to=2026-01-31`):
1. Dates are parsed as Swiss local time (start/end of day)
2. Converted to UTC using `TimeZoneInfo.ConvertTimeToUtc()`
3. Stored/queried in database as UTC

Example:
- Input: `from=2026-01-22` (Swiss local, start of day: 00:00:00)
- Converted to UTC: `2026-01-21T23:00:00Z` (if CET, UTC+1)
- Stored in database as UTC

### Output (Database → API)

When returning dates to the frontend:
1. UTC dates from database are converted to Swiss local time
2. Returned as Swiss local time to the client

Example:
- Database: `2026-01-22T10:00:00Z` (UTC)
- Converted to Swiss: `2026-01-22T11:00:00+01:00` (CET)
- Returned to client

## Implementation

### Timezone Conversion

The `ReportService` uses:
- **Swiss Timezone ID:**
  - Windows: `"W. Europe Standard Time"`
  - Linux/Mac: `"Europe/Zurich"`

### Conversion Methods

```csharp
// Convert Swiss local time to UTC (for queries)
private static DateTime ConvertToUtc(DateTime swissLocalTime)

// Convert UTC to Swiss local time (for responses)
private static DateTime ConvertToSwissTime(DateTime utcTime)
```

## Date Range Queries

For date range queries (e.g., reports):
1. Frontend sends: `from=2026-01-22&to=2026-01-31`
2. Backend treats as Swiss local dates:
   - `from`: `2026-01-22 00:00:00` (Swiss time)
   - `to`: `2026-01-31 23:59:59` (Swiss time)
3. Converts to UTC for database query
4. Queries receipts where `PurchasedAt` falls within UTC range

## Daylight Saving Time (DST)

Switzerland observes DST:
- **CET (UTC+1)**: Winter (October - March)
- **CEST (UTC+2)**: Summer (March - October)

The `TimeZoneInfo` class automatically handles DST conversions.

## Best Practices

1. **Always use UTC for storage** - Prevents timezone-related bugs
2. **Convert at boundaries** - Convert to/from local time only at API boundaries
3. **Be explicit** - Document timezone assumptions in code
4. **Test DST transitions** - Verify behavior during DST changes

## Example Flow

### Receipt Upload
1. User uploads receipt at `2026-01-22 14:30` (Swiss local)
2. Frontend sends: `PurchasedAt: "2026-01-22T14:30:00+01:00"`
3. Backend converts to UTC: `2026-01-22T13:30:00Z`
4. Stored in database as UTC

### Report Query
1. Frontend requests: `GET /api/reports/summary?from=2026-01-01&to=2026-01-31`
2. Backend treats as Swiss local:
   - `from`: `2026-01-01 00:00:00` (Swiss)
   - `to`: `2026-01-31 23:59:59` (Swiss)
3. Converts to UTC:
   - `from`: `2025-12-31T23:00:00Z` (if CET)
   - `to`: `2026-01-31T22:59:59Z` (if CET)
4. Queries database with UTC range
5. Converts results back to Swiss time for response

## Notes

- The `DateTime` type in .NET can be `Unspecified`, `Utc`, or `Local`
- We use `DateTimeKind.Unspecified` for Swiss local inputs (treating them as Swiss time)
- We use `DateTimeKind.Utc` for database values
- The `TimeZoneInfo` class handles all conversions automatically
