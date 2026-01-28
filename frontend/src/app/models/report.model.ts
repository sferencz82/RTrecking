export interface ReportSummary {
  fromDate: string;
  toDate: string;
  totalSpend: number;
  spendByCategory: SpendByCategory[];
  spendOverTimeDaily: DailySpend[];
}

export interface SpendByCategory {
  categoryId: number;
  categoryName: string;
  totalSpend: number;
}

export interface DailySpend {
  date: string;
  totalSpend: number;
}

export interface ReportItems {
  items: ItemAggregate[];
}

export interface ItemAggregate {
  itemName: string;
  totalSpent: number;
  totalWeightKg: number | null;
  itemCount: number;
}
