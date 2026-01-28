import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';
import { ReceiptsApiService } from '../../services/receipts-api.service';
import { Receipt } from '../../models/receipt.model';

@Component({
  selector: 'app-receipts-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    FormsModule
  ],
  template: `
    <div class="receipts-container">
      <div class="header">
        <h1>Receipts</h1>
        <button mat-raised-button color="primary" routerLink="/receipts/upload">
          <mat-icon>add</mat-icon>
          Upload Receipt
        </button>
      </div>

      <mat-card class="filter-card">
        <mat-card-content>
          <div class="filter-row">
            <mat-form-field>
              <mat-label>From Date</mat-label>
              <input matInput [matDatepicker]="fromPicker" [(ngModel)]="fromDate">
              <mat-datepicker-toggle matSuffix [for]="fromPicker"></mat-datepicker-toggle>
              <mat-datepicker #fromPicker></mat-datepicker>
            </mat-form-field>

            <mat-form-field>
              <mat-label>To Date</mat-label>
              <input matInput [matDatepicker]="toPicker" [(ngModel)]="toDate">
              <mat-datepicker-toggle matSuffix [for]="toPicker"></mat-datepicker-toggle>
              <mat-datepicker #toPicker></mat-datepicker>
            </mat-form-field>

            <button mat-button (click)="applyFilter()">Apply Filter</button>
            <button mat-button (click)="clearFilter()">Clear</button>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card>
        <mat-card-content>
          <table mat-table [dataSource]="receipts" class="receipts-table">
            <ng-container matColumnDef="merchant">
              <th mat-header-cell *matHeaderCellDef>Merchant</th>
              <td mat-cell *matCellDef="let receipt">{{ receipt.merchantName || 'Unknown' }}</td>
            </ng-container>

            <ng-container matColumnDef="date">
              <th mat-header-cell *matHeaderCellDef>Date</th>
              <td mat-cell *matCellDef="let receipt">{{ formatDate(receipt.purchasedAt) }}</td>
            </ng-container>

            <ng-container matColumnDef="total">
              <th mat-header-cell *matHeaderCellDef>Total</th>
              <td mat-cell *matCellDef="let receipt">{{ formatCurrency(receipt.total) }}</td>
            </ng-container>

            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let receipt">
                <button mat-icon-button [routerLink]="['/receipts', receipt.id]">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button color="warn" (click)="deleteReceipt(receipt.id)">
                  <mat-icon>delete</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>

          <div *ngIf="receipts.length === 0" class="empty-state">
            <p>No receipts found</p>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .receipts-container {
      max-width: 1200px;
      margin: 0 auto;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .filter-card {
      margin-bottom: 20px;
    }

    .filter-row {
      display: flex;
      gap: 16px;
      align-items: center;
    }

    .receipts-table {
      width: 100%;
    }

    .empty-state {
      text-align: center;
      padding: 40px;
      color: #666;
    }
  `]
})
export class ReceiptsListComponent implements OnInit {
  receipts: Receipt[] = [];
  displayedColumns: string[] = ['merchant', 'date', 'total', 'actions'];
  fromDate: Date | null = null;
  toDate: Date | null = null;

  constructor(private receiptsApi: ReceiptsApiService) {}

  ngOnInit() {
    this.loadReceipts();
  }

  loadReceipts() {
    const from = this.fromDate ? this.formatDateForApi(this.fromDate) : undefined;
    const to = this.toDate ? this.formatDateForApi(this.toDate) : undefined;
    
    this.receiptsApi.getReceipts(from, to).subscribe({
      next: (data) => {
        this.receipts = data;
      },
      error: (err) => {
        console.error('Error loading receipts:', err);
      }
    });
  }

  applyFilter() {
    this.loadReceipts();
  }

  clearFilter() {
    this.fromDate = null;
    this.toDate = null;
    this.loadReceipts();
  }

  deleteReceipt(id: string) {
    if (confirm('Are you sure you want to delete this receipt?')) {
      this.receiptsApi.deleteReceipt(id).subscribe({
        next: () => {
          this.loadReceipts();
        },
        error: (err) => {
          console.error('Error deleting receipt:', err);
          alert('Error deleting receipt');
        }
      });
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('de-CH');
  }

  formatCurrency(amount: number | null): string {
    if (amount === null) return '-';
    return new Intl.NumberFormat('de-CH', {
      style: 'currency',
      currency: 'CHF'
    }).format(amount);
  }

  formatDateForApi(date: Date): string {
    return date.toISOString().split('T')[0];
  }
}
