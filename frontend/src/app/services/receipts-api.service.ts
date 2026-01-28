import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Receipt, ReceiptUpdate, DraftReceiptParse } from '../models/receipt.model';
import { ReceiptItem, ReceiptItemCreate, ReceiptItemUpdate } from '../models/receipt-item.model';

@Injectable({
  providedIn: 'root'
})
export class ReceiptsApiService {
  private apiUrl = `${environment.apiBaseUrl}/api/receipts`;

  constructor(private http: HttpClient) {}

  uploadReceipt(file: File): Observable<Receipt> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<Receipt>(`${this.apiUrl}/upload`, formData);
  }

  getReceipts(from?: string, to?: string): Observable<Receipt[]> {
    let url = this.apiUrl;
    const params: string[] = [];
    if (from) params.push(`from=${from}`);
    if (to) params.push(`to=${to}`);
    if (params.length > 0) url += '?' + params.join('&');
    return this.http.get<Receipt[]>(url);
  }

  getReceipt(id: string): Observable<Receipt> {
    return this.http.get<Receipt>(`${this.apiUrl}/${id}`);
  }

  updateReceipt(id: string, update: ReceiptUpdate): Observable<Receipt> {
    return this.http.put<Receipt>(`${this.apiUrl}/${id}`, update);
  }

  deleteReceipt(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  processOcr(id: string): Observable<DraftReceiptParse> {
    return this.http.post<DraftReceiptParse>(`${this.apiUrl}/${id}/ocr`, {});
  }

  applyDraft(id: string, draft: DraftReceiptParse): Observable<Receipt> {
    return this.http.post<Receipt>(`${this.apiUrl}/${id}/apply-draft`, draft);
  }

  getReceiptItems(receiptId: string): Observable<ReceiptItem[]> {
    return this.http.get<ReceiptItem[]>(`${this.apiUrl}/${receiptId}/items`);
  }

  createItem(receiptId: string, item: ReceiptItemCreate): Observable<ReceiptItem> {
    return this.http.post<ReceiptItem>(`${this.apiUrl}/${receiptId}/items`, item);
  }

  updateItem(receiptId: string, itemId: string, update: ReceiptItemUpdate): Observable<ReceiptItem> {
    return this.http.put<ReceiptItem>(`${this.apiUrl}/${receiptId}/items/${itemId}`, update);
  }

  deleteItem(receiptId: string, itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${receiptId}/items/${itemId}`);
  }

  getImageUrl(imagePath: string): string {
    return `${environment.apiBaseUrl}/${imagePath}`;
  }
}
