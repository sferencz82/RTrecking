import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { ReceiptsApiService } from '../../services/receipts-api.service';
import { CategoriesService } from '../../services/categories.service';
import { Receipt, ReceiptUpdate } from '../../models/receipt.model';
import { ReceiptItem, ReceiptItemUpdate } from '../../models/receipt-item.model';
import { Category } from '../../models/category.model';

@Component({
  selector: 'app-receipt-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDialogModule,
    FormsModule
  ],
  template: `
    <div class="detail-container" *ngIf="receipt">
      <div class="header">
        <button mat-button (click)="goBack()">
          <mat-icon>arrow_back</mat-icon>
          Back
        </button>
        <h1>Receipt Details</h1>
        <button mat-raised-button color="primary" (click)="saveReceipt()" [disabled]="saving">
          <mat-icon>save</mat-icon>
          Save
        </button>
      </div>

      <div class="content-grid">
        <!-- Receipt Info -->
        <mat-card>
          <mat-card-header>
            <mat-card-title>Receipt Information</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="form-row">
              <mat-form-field>
                <mat-label>Merchant Name</mat-label>
                <input matInput [(ngModel)]="receipt.merchantName">
              </mat-form-field>

              <mat-form-field>
                <mat-label>Purchase Date</mat-label>
                <input matInput type="datetime-local" [value]="getDateTimeLocal(receipt.purchasedAt)" 
                       (change)="onDateChange($event)">
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-form-field>
                <mat-label>Location</mat-label>
                <input matInput [(ngModel)]="receipt.locationName">
              </mat-form-field>

              <mat-form-field>
                <mat-label>Total</mat-label>
                <input matInput type="number" [(ngModel)]="receipt.total" step="0.01" readonly>
              </mat-form-field>
            </div>

            <div *ngIf="receipt.imagePath" class="image-preview">
              <img [src]="getImageUrl(receipt.imagePath)" alt="Receipt" class="receipt-image">
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Items -->
        <mat-card>
          <mat-card-header>
            <mat-card-title>Items</mat-card-title>
            <button mat-icon-button (click)="addItem()">
              <mat-icon>add</mat-icon>
            </button>
          </mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="items" class="items-table">
              <ng-container matColumnDef="rawName">
                <th mat-header-cell *matHeaderCellDef>Name</th>
                <td mat-cell *matCellDef="let item; let i = index">
                  <input matInput [(ngModel)]="item.rawName" (blur)="saveItem(i)">
                </td>
              </ng-container>

              <ng-container matColumnDef="normalizedName">
                <th mat-header-cell *matHeaderCellDef>Normalized Name</th>
                <td mat-cell *matCellDef="let item; let i = index">
                  <input matInput [(ngModel)]="item.normalizedName" (blur)="saveItem(i)">
                </td>
              </ng-container>

              <ng-container matColumnDef="category">
                <th mat-header-cell *matHeaderCellDef>Category</th>
                <td mat-cell *matCellDef="let item; let i = index">
                  <mat-select [(ngModel)]="item.categoryId" (selectionChange)="saveItem(i)">
                    <mat-option *ngFor="let cat of categories" [value]="cat.id">
                      {{ cat.name }}
                    </mat-option>
                  </mat-select>
                </td>
              </ng-container>

              <ng-container matColumnDef="priceTotal">
                <th mat-header-cell *matHeaderCellDef>Price</th>
                <td mat-cell *matCellDef="let item; let i = index">
                  <input matInput type="number" [(ngModel)]="item.priceTotal" (blur)="saveItem(i)" step="0.01">
                </td>
              </ng-container>

              <ng-container matColumnDef="quantity">
                <th mat-header-cell *matHeaderCellDef>Quantity</th>
                <td mat-cell *matCellDef="let item; let i = index">
                  <input matInput type="number" [(ngModel)]="item.quantity" (blur)="saveItem(i)" step="0.001">
                </td>
              </ng-container>

              <ng-container matColumnDef="unit">
                <th mat-header-cell *matHeaderCellDef>Unit</th>
                <td mat-cell *matCellDef="let item; let i = index">
                  <input matInput [(ngModel)]="item.unit" (blur)="saveItem(i)">
                </td>
              </ng-container>

              <ng-container matColumnDef="weightKg">
                <th mat-header-cell *matHeaderCellDef>Weight (kg)</th>
                <td mat-cell *matCellDef="let item; let i = index">
                  <input matInput type="number" [(ngModel)]="item.weightKg" (blur)="saveItem(i)" step="0.001">
                </td>
              </ng-container>

              <ng-container matColumnDef="edibleFraction">
                <th mat-header-cell *matHeaderCellDef>Edible Fraction</th>
                <td mat-cell *matCellDef="let item; let i = index">
                  <input matInput type="number" [(ngModel)]="item.edibleFraction" (blur)="saveItem(i)" 
                         step="0.001" min="0.1" max="1.0">
                </td>
              </ng-container>

              <ng-container matColumnDef="edibleWeight">
                <th mat-header-cell *matHeaderCellDef>Edible Weight (kg)</th>
                <td mat-cell *matCellDef="let item">
                  {{ getEdibleWeight(item) }}
                </td>
              </ng-container>

              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Actions</th>
                <td mat-cell *matCellDef="let item; let i = index">
                  <button mat-icon-button (click)="deleteItem(item.id, i)">
                    <mat-icon>delete</mat-icon>
                  </button>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="itemColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: itemColumns;"></tr>
            </table>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .detail-container {
      max-width: 1400px;
      margin: 0 auto;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .content-grid {
      display: grid;
      grid-template-columns: 1fr 2fr;
      gap: 20px;
    }

    .form-row {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
    }

    .form-row mat-form-field {
      flex: 1;
    }

    .image-preview {
      margin-top: 16px;
    }

    .receipt-image {
      max-width: 100%;
      max-height: 400px;
      border: 1px solid #ddd;
    }

    .items-table {
      width: 100%;
    }

    .items-table input, .items-table mat-select {
      width: 100%;
    }

    @media (max-width: 1200px) {
      .content-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ReceiptDetailComponent implements OnInit {
  receipt: Receipt | null = null;
  items: ReceiptItem[] = [];
  categories: Category[] = [];
  saving = false;
  itemColumns: string[] = ['rawName', 'normalizedName', 'category', 'priceTotal', 'quantity', 'unit', 'weightKg', 'edibleFraction', 'edibleWeight', 'actions'];
  receiptId: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private receiptsApi: ReceiptsApiService,
    private categoriesService: CategoriesService
  ) {}

  ngOnInit() {
    this.receiptId = this.route.snapshot.paramMap.get('id') || '';
    this.loadReceipt();
    this.loadCategories();
  }

  loadReceipt() {
    this.receiptsApi.getReceipt(this.receiptId).subscribe({
      next: (data) => {
        this.receipt = data;
        this.loadItems();
      },
      error: (err) => {
        console.error('Error loading receipt:', err);
      }
    });
  }

  loadItems() {
    this.receiptsApi.getReceiptItems(this.receiptId).subscribe({
      next: (data) => {
        this.items = data;
      },
      error: (err) => {
        console.error('Error loading items:', err);
      }
    });
  }

  loadCategories() {
    this.categoriesService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (err) => {
        console.error('Error loading categories:', err);
      }
    });
  }

  saveReceipt() {
    if (!this.receipt) return;

    this.saving = true;
    const update: ReceiptUpdate = {
      purchasedAt: this.receipt.purchasedAt,
      merchantName: this.receipt.merchantName,
      locationName: this.receipt.locationName,
      latitude: this.receipt.latitude,
      longitude: this.receipt.longitude,
      total: this.receipt.total
    };

    this.receiptsApi.updateReceipt(this.receipt.id, update).subscribe({
      next: () => {
        this.saving = false;
        alert('Receipt saved successfully');
      },
      error: (err) => {
        console.error('Error saving receipt:', err);
        alert('Error saving receipt');
        this.saving = false;
      }
    });
  }

  saveItem(index: number) {
    const item = this.items[index];
    const update: ReceiptItemUpdate = {
      rawName: item.rawName,
      normalizedName: item.normalizedName,
      categoryId: item.categoryId,
      quantity: item.quantity,
      unit: item.unit,
      weightKg: item.weightKg,
      priceTotal: item.priceTotal,
      edibleFraction: item.edibleFraction
    };

    this.receiptsApi.updateItem(this.receiptId, item.id, update).subscribe({
      next: () => {
        this.loadReceipt(); // Reload to get updated total
      },
      error: (err) => {
        console.error('Error saving item:', err);
        alert('Error saving item');
      }
    });
  }

  addItem() {
    const newItem: any = {
      rawName: '',
      normalizedName: '',
      categoryId: 9, // Default to "Other"
      quantity: 1,
      unit: null,
      weightKg: null,
      priceTotal: 0,
      edibleFraction: 1.0
    };

    this.receiptsApi.createItem(this.receiptId, newItem).subscribe({
      next: () => {
        this.loadItems();
        this.loadReceipt();
      },
      error: (err) => {
        console.error('Error creating item:', err);
        alert('Error creating item');
      }
    });
  }

  deleteItem(itemId: string, index: number) {
    if (confirm('Are you sure you want to delete this item?')) {
      this.receiptsApi.deleteItem(this.receiptId, itemId).subscribe({
        next: () => {
          this.items.splice(index, 1);
          this.loadReceipt(); // Reload to get updated total
        },
        error: (err) => {
          console.error('Error deleting item:', err);
          alert('Error deleting item');
        }
      });
    }
  }

  getEdibleWeight(item: ReceiptItem): string {
    if (item.weightKg === null || item.weightKg === undefined) {
      return '-';
    }
    const edibleWeight = item.weightKg * item.edibleFraction;
    return edibleWeight.toFixed(3);
  }

  getDateTimeLocal(dateString: string): string {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  onDateChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (this.receipt && input.value) {
      this.receipt.purchasedAt = new Date(input.value).toISOString();
    }
  }

  getImageUrl(imagePath: string): string {
    return this.receiptsApi.getImageUrl(imagePath);
  }

  goBack() {
    this.router.navigate(['/receipts']);
  }
}
