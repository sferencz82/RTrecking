# Receipts API Endpoints

## Overview

The Receipts API provides endpoints for managing receipt uploads, retrieval, and deletion. All endpoints use DTOs and follow a service layer pattern to keep controllers thin.

## Endpoints

### POST /api/receipts/upload

Upload a receipt image and create a receipt record.

**Request:**
- Method: `POST`
- Content-Type: `multipart/form-data`
- Body: Form data with `file` field containing the image

**Response:**
- `201 Created` - Returns `ReceiptDto` with the created receipt
- `400 Bad Request` - Invalid file or file type not allowed

**Example:**
```bash
curl -X POST "https://localhost:5001/api/receipts/upload" \
  -F "file=@receipt.jpg"
```

**Response Body:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "purchasedAt": "2026-01-22T19:00:00Z",
  "merchantName": "",
  "currency": "CHF",
  "total": null,
  "imagePath": "uploads/receipts/3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg",
  "locationName": null,
  "latitude": null,
  "longitude": null,
  "createdAt": "2026-01-22T19:00:00Z",
  "updatedAt": "2026-01-22T19:00:00Z"
}
```

### GET /api/receipts

Get a list of receipts with optional date filtering.

**Query Parameters:**
- `from` (optional, DateTime) - Filter receipts from this date
- `to` (optional, DateTime) - Filter receipts up to this date

**Response:**
- `200 OK` - Returns array of `ReceiptDto`

**Example:**
```bash
# Get all receipts
curl "https://localhost:5001/api/receipts"

# Get receipts from a date range
curl "https://localhost:5001/api/receipts?from=2026-01-01&to=2026-01-31"
```

### GET /api/receipts/{id}

Get a single receipt by ID.

**Path Parameters:**
- `id` (Guid) - Receipt ID

**Response:**
- `200 OK` - Returns `ReceiptDto`
- `404 Not Found` - Receipt not found

**Example:**
```bash
curl "https://localhost:5001/api/receipts/3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

### DELETE /api/receipts/{id}

Delete a receipt and its associated image file.

**Path Parameters:**
- `id` (Guid) - Receipt ID

**Response:**
- `204 No Content` - Receipt deleted successfully
- `404 Not Found` - Receipt not found

**Example:**
```bash
curl -X DELETE "https://localhost:5001/api/receipts/3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

## File Storage

- Files are saved to `/uploads/receipts/` directory
- Unique filenames are generated using GUIDs
- Supported file types: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
- Files are accessible via static file serving at `/uploads/receipts/{filename}`

## Architecture

### DTOs
- `ReceiptDto` - Data transfer object for receipt data
- `ReceiptCreateDto` - DTO for creating receipts

### Services
- `IReceiptService` / `ReceiptService` - Business logic for receipt operations
- `IFileStorageService` / `FileStorageService` - File upload and deletion operations

### Controller
- `ReceiptsController` - Thin controller that delegates to services

## Notes

- All timestamps are in UTC
- Image paths are stored as relative paths (e.g., `uploads/receipts/{guid}.jpg`)
- When a receipt is deleted, the associated image file is also deleted
- Date filtering in GET /api/receipts includes the entire day for the `to` parameter
