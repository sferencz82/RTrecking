import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ReportSummary, ReportItems } from '../models/report.model';

@Injectable({
  providedIn: 'root'
})
export class ReportsApiService {
  private apiUrl = `${environment.apiBaseUrl}/api/reports`;

  constructor(private http: HttpClient) {}

  getSummary(from: string, to: string): Observable<ReportSummary> {
    const params = new HttpParams()
      .set('from', from)
      .set('to', to);
    return this.http.get<ReportSummary>(`${this.apiUrl}/summary`, { params });
  }

  getItems(from?: string, to?: string, categoryId?: number): Observable<ReportItems> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    if (categoryId) params = params.set('categoryId', categoryId.toString());
    return this.http.get<ReportItems>(`${this.apiUrl}/items`, { params });
  }
}
