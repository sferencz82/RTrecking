export interface Receipt {
  id: string;
  purchasedAt: string;
  merchantName: string;
  currency: string;
  total: number | null;
  imagePath: string;
  locationName: string | null;
  latitude: number | null;
  longitude: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface ReceiptUpdate {
  purchasedAt?: string;
  merchantName?: string;
  locationName?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  total?: number | null;
}

export interface DraftReceiptParse {
  merchantName: string | null;
  purchasedAt: string | null;
  items: DraftReceiptItem[];
}

export interface DraftReceiptItem {
  rawName: string;
  priceTotal: number;
  quantity?: number | null;
  unit?: string | null;
  weightKg?: number | null;
}
