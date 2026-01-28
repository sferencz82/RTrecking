export interface ReceiptItem {
  id: string;
  receiptId: string;
  rawName: string;
  normalizedName: string | null;
  categoryId: number;
  quantity: number | null;
  unit: string | null;
  weightKg: number | null;
  priceTotal: number;
  edibleFraction: number;
  notes: string | null;
}

export interface ReceiptItemCreate {
  rawName: string;
  normalizedName?: string | null;
  categoryId: number;
  quantity?: number | null;
  unit?: string | null;
  weightKg?: number | null;
  priceTotal: number;
  edibleFraction: number;
  notes?: string | null;
}

export interface ReceiptItemUpdate {
  rawName?: string;
  normalizedName?: string | null;
  categoryId?: number;
  quantity?: number | null;
  unit?: string | null;
  weightKg?: number | null;
  priceTotal?: number;
  edibleFraction?: number;
  notes?: string | null;
}
