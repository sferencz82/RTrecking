# Receipts and Items API Endpoints

## Overview

Complete API for managing receipts and their line items, including OCR draft application and item CRUD operations.

## Receipt Endpoints

### PUT /api/receipts/{id}

Update receipt fields (PurchasedAt, MerchantName, Location fields, Total).

**Request Body:**
```json
{
  "purchasedAt": "2026-01-22T14:30:00Z",
  "merchantName": "Migros",
  "locationName": "Migros Zürich",
  "latitude": 47.3769,
  "longitude": 8.5417,
  "total": 45.80
}
```

**Response:**
- `200 OK` - Returns updated `ReceiptDto`
- `404 Not Found` - Receipt not found

**Note:** All fields are optional - only provided fields are updated.

### POST /api/receipts/{id}/apply-draft

Apply draft parse results to create receipt items.

**Request Body:**
```json
{
  "merchantName": "Migros",
  "purchasedAt": "2026-01-22T14:30:00Z",
  "items": [
    {
      "rawName": "Brot 500g",
      "priceTotal": 2.50,
      "quantity": 1,
      "unit": "g",
      "weightKg": 0.5,
      "edibleFraction": 1.0
    },
    {
      "rawName": "Milch 1l",
      "priceTotal": 1.80,
      "quantity": 1,
      "unit": "l",
      "edibleFraction": 1.0
    }
  ]
}
```

**Response:**
- `200 OK` - Returns updated `ReceiptDto` with calculated total
- `400 Bad Request` - Validation error (PriceTotal < 0 or EdibleFraction out of range)
- `404 Not Found` - Receipt not found

**Behavior:**
- Deletes all existing items for the receipt
- Creates new items from the draft
- Updates receipt.MerchantName and receipt.PurchasedAt if provided
- Calculates receipt.Total as sum of all item PriceTotal values
- Defaults item CategoryId to 9 ("Other") - can be updated later

**Validation:**
- `PriceTotal` must be >= 0
- `EdibleFraction` must be between 0.1 and 1.0

## Item Endpoints

### GET /api/receipts/{id}/items

Get all items for a receipt.

**Response:**
- `200 OK` - Returns array of `ReceiptItemDto`

### POST /api/receipts/{id}/items

Add a new item to a receipt.

**Request Body:**
```json
{
  "rawName": "Chicken Breast",
  "normalizedName": "Chicken Breast",
  "categoryId": 1,
  "quantity": 2,
  "unit": "pcs",
  "weightKg": 0.5,
  "priceTotal": 12.50,
  "edibleFraction": 0.95,
  "notes": "Boneless"
}
```

**Response:**
- `201 Created` - Returns created `ReceiptItemDto`
- `400 Bad Request` - Validation error
- `404 Not Found` - Receipt or Category not found

**Validation:**
- `PriceTotal` must be >= 0
- `EdibleFraction` must be between 0.1 and 1.0
- `CategoryId` must exist

**Note:** Automatically recalculates receipt.Total after creation.

### GET /api/receipts/{id}/items/{itemId}

Get a single item by ID.

**Response:**
- `200 OK` - Returns `ReceiptItemDto`
- `404 Not Found` - Item not found or doesn't belong to receipt

### PUT /api/receipts/{id}/items/{itemId}

Update an item (including category, edibleFraction, weightKg, unit, normalizedName).

**Request Body:**
```json
{
  "rawName": "Chicken Breast",
  "normalizedName": "Chicken Breast - Boneless",
  "categoryId": 1,
  "quantity": 2,
  "unit": "pcs",
  "weightKg": 0.5,
  "priceTotal": 12.50,
  "edibleFraction": 0.95,
  "notes": "Updated notes"
}
```

**Response:**
- `200 OK` - Returns updated `ReceiptItemDto`
- `400 Bad Request` - Validation error
- `404 Not Found` - Item not found or doesn't belong to receipt

**Note:** All fields are optional - only provided fields are updated. Automatically recalculates receipt.Total after update.

### DELETE /api/receipts/{id}/items/{itemId}

Delete an item from a receipt.

**Response:**
- `204 No Content` - Item deleted successfully
- `404 Not Found` - Item not found or doesn't belong to receipt

**Note:** Automatically recalculates receipt.Total after deletion.

## DTOs

### ReceiptUpdateDto
```csharp
{
  purchasedAt?: DateTime;
  merchantName?: string;
  locationName?: string;
  latitude?: double;
  longitude?: double;
  total?: decimal;
}
```

### ReceiptItemDto
```csharp
{
  id: Guid;
  receiptId: Guid;
  rawName: string;
  normalizedName?: string;
  categoryId: int;
  quantity?: decimal;
  unit?: string;
  weightKg?: decimal;
  priceTotal: decimal;
  edibleFraction: decimal;
  notes?: string;
}
```

### ReceiptItemCreateDto
```csharp
{
  rawName: string;
  normalizedName?: string;
  categoryId: int;
  quantity?: decimal;
  unit?: string;
  weightKg?: decimal;
  priceTotal: decimal;
  edibleFraction: decimal; // default: 1.0
  notes?: string;
}
```

### ReceiptItemUpdateDto
```csharp
{
  rawName?: string;
  normalizedName?: string;
  categoryId?: int;
  quantity?: decimal;
  unit?: string;
  weightKg?: decimal;
  priceTotal?: decimal;
  edibleFraction?: decimal;
  notes?: string;
}
```

## Validation Rules

### PriceTotal
- Must be >= 0
- Applied to: Create, Update, Apply Draft

### EdibleFraction
- Must be between 0.1 and 1.0 (inclusive)
- Applied to: Create, Update, Apply Draft
- Default: 1.0 (100% edible)

### CategoryId
- Must exist in Categories table
- Applied to: Create, Update

## Automatic Total Calculation

The receipt total is automatically recalculated:
- After creating an item
- After updating an item (if PriceTotal changed)
- After deleting an item
- When applying draft parse

Calculation: `receipt.Total = sum(all items.PriceTotal)`

## Usage Flow

1. **Upload receipt**: `POST /api/receipts/upload`
2. **Run OCR**: `POST /api/receipts/{id}/ocr`
3. **Review draft**: Check returned `DraftReceiptParseDto`
4. **Apply draft**: `POST /api/receipts/{id}/apply-draft` (with reviewed/corrected data)
5. **Update items**: `PUT /api/receipts/{id}/items/{itemId}` (set categories, edible fractions, etc.)
6. **Update receipt**: `PUT /api/receipts/{id}` (set merchant, date, location)

## Error Handling

All endpoints return appropriate HTTP status codes:
- `200 OK` - Success
- `201 Created` - Resource created
- `204 No Content` - Success (delete)
- `400 Bad Request` - Validation error
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

Validation errors include descriptive messages explaining what failed.
