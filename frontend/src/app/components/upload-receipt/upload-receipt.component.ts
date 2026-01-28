import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatMenuModule } from '@angular/material/menu';
import { FormsModule } from '@angular/forms';
import { ReceiptsApiService } from '../../services/receipts-api.service';
import { CameraService } from '../../services/camera.service';
import { DraftReceiptParse, DraftReceiptItem } from '../../models/receipt.model';

@Component({
  selector: 'app-upload-receipt',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatMenuModule,
    FormsModule
  ],
  template: `
    <div class="upload-container">
      <h1>Upload Receipt</h1>

      <!-- Step 1: File Selection -->
      <mat-card *ngIf="!receiptId" class="step-card">
        <mat-card-content>
          <input type="file" #fileInput accept="image/*" (change)="onFileSelected($event)" style="display: none">
          
          <div class="upload-options">
            <button mat-raised-button color="primary" [matMenuTriggerFor]="uploadMenu">
              <mat-icon>add_photo_alternate</mat-icon>
              Select Image
            </button>
            <mat-menu #uploadMenu="matMenu">
              <button mat-menu-item (click)="takePhoto()" *ngIf="isNative">
                <mat-icon>camera_alt</mat-icon>
                <span>Take Photo</span>
              </button>
              <button mat-menu-item (click)="pickFromGallery()" *ngIf="isNative">
                <mat-icon>photo_library</mat-icon>
                <span>Choose from Gallery</span>
              </button>
              <button mat-menu-item (click)="fileInput.click()">
                <mat-icon>folder</mat-icon>
                <span>Choose File</span>
              </button>
            </mat-menu>
          </div>
          <div *ngIf="selectedFile" class="file-info">
            <p>Selected: {{ selectedFile.name }}</p>
            <img [src]="imagePreview" alt="Preview" class="preview-image" *ngIf="imagePreview">
          </div>
          <button *ngIf="selectedFile" mat-raised-button color="primary" (click)="uploadReceipt()" [disabled]="uploading">
            <mat-icon>upload</mat-icon>
            Upload
          </button>
          <mat-progress-bar *ngIf="uploading" mode="indeterminate"></mat-progress-bar>
        </mat-card-content>
      </mat-card>

      <!-- Step 2: OCR Processing -->
      <mat-card *ngIf="receiptId && !draftParse" class="step-card">
        <mat-card-content>
          <p>Processing OCR...</p>
          <mat-progress-bar mode="indeterminate"></mat-progress-bar>
        </mat-card-content>
      </mat-card>

      <!-- Step 3: Review Draft -->
      <mat-card *ngIf="draftParse" class="step-card">
        <mat-card-content>
          <h2>Review OCR Results</h2>
          
          <div class="draft-info">
            <mat-form-field>
              <mat-label>Merchant Name</mat-label>
              <input matInput [(ngModel)]="draftParse.merchantName">
            </mat-form-field>

            <mat-form-field>
              <mat-label>Purchase Date</mat-label>
              <input matInput type="datetime-local" [value]="getDateTimeLocal(draftParse.purchasedAt)" 
                     (change)="onDateChange($event)">
            </mat-form-field>
          </div>

          <table mat-table [dataSource]="draftParse.items" class="draft-table">
            <ng-container matColumnDef="rawName">
              <th mat-header-cell *matHeaderCellDef>Item Name</th>
              <td mat-cell *matCellDef="let item; let i = index">
                <input matInput [(ngModel)]="item.rawName" (blur)="updateItem(i)">
              </td>
            </ng-container>

            <ng-container matColumnDef="priceTotal">
              <th mat-header-cell *matHeaderCellDef>Price</th>
              <td mat-cell *matCellDef="let item; let i = index">
                <input matInput type="number" [(ngModel)]="item.priceTotal" (blur)="updateItem(i)" step="0.01">
              </td>
            </ng-container>

            <ng-container matColumnDef="quantity">
              <th mat-header-cell *matHeaderCellDef>Quantity</th>
              <td mat-cell *matCellDef="let item; let i = index">
                <input matInput type="number" [(ngModel)]="item.quantity" (blur)="updateItem(i)" step="0.001">
              </td>
            </ng-container>

            <ng-container matColumnDef="unit">
              <th mat-header-cell *matHeaderCellDef>Unit</th>
              <td mat-cell *matCellDef="let item; let i = index">
                <input matInput [(ngModel)]="item.unit" (blur)="updateItem(i)">
              </td>
            </ng-container>

            <ng-container matColumnDef="weightKg">
              <th mat-header-cell *matHeaderCellDef>Weight (kg)</th>
              <td mat-cell *matCellDef="let item; let i = index">
                <input matInput type="number" [(ngModel)]="item.weightKg" (blur)="updateItem(i)" step="0.001">
              </td>
            </ng-container>

            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let item; let i = index">
                <button mat-icon-button (click)="removeItem(i)">
                  <mat-icon>delete</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="draftColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: draftColumns;"></tr>
          </table>

          <button mat-raised-button (click)="addItem()" style="margin-top: 16px;">
            <mat-icon>add</mat-icon>
            Add Item
          </button>

          <div class="actions">
            <button mat-raised-button color="primary" (click)="applyDraft()" [disabled]="applying">
              <mat-icon>check</mat-icon>
              Apply to Receipt
            </button>
            <button mat-button (click)="cancel()">Cancel</button>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .upload-container {
      max-width: 1200px;
      margin: 0 auto;
    }

    .step-card {
      margin-bottom: 20px;
    }

    .file-info {
      margin: 16px 0;
    }

    .preview-image {
      max-width: 300px;
      max-height: 300px;
      margin-top: 16px;
    }

    .draft-info {
      display: flex;
      gap: 16px;
      margin-bottom: 20px;
    }

    .draft-table {
      width: 100%;
    }

    .draft-table input {
      width: 100%;
      border: none;
      padding: 8px;
    }

    .actions {
      margin-top: 20px;
      display: flex;
      gap: 16px;
    }
  `]
})
export class UploadReceiptComponent {
  selectedFile: File | null = null;
  imagePreview: string | null = null;
  uploading = false;
  receiptId: string | null = null;
  draftParse: DraftReceiptParse | null = null;
  applying = false;
  draftColumns: string[] = ['rawName', 'priceTotal', 'quantity', 'unit', 'weightKg', 'actions'];

  isNative = false;

  constructor(
    private receiptsApi: ReceiptsApiService,
    private cameraService: CameraService,
    private router: Router
  ) {
    this.isNative = this.cameraService.isNativePlatform();
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.setSelectedFile(input.files[0]);
    }
  }

  async takePhoto() {
    const file = await this.cameraService.takePhoto();
    if (file) {
      this.setSelectedFile(file);
    }
  }

  async pickFromGallery() {
    const file = await this.cameraService.pickPhoto();
    if (file) {
      this.setSelectedFile(file);
    }
  }

  private setSelectedFile(file: File) {
    this.selectedFile = file;
    const reader = new FileReader();
    reader.onload = (e) => {
      this.imagePreview = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  }

  uploadReceipt() {
    if (!this.selectedFile) return;

    this.uploading = true;
    this.receiptsApi.uploadReceipt(this.selectedFile).subscribe({
      next: (receipt) => {
        this.receiptId = receipt.id;
        this.processOcr();
      },
      error: (err) => {
        console.error('Error uploading receipt:', err);
        alert('Error uploading receipt');
        this.uploading = false;
      }
    });
  }

  processOcr() {
    if (!this.receiptId) return;

    this.receiptsApi.processOcr(this.receiptId).subscribe({
      next: (draft) => {
        this.draftParse = draft;
        this.uploading = false;
      },
      error: (err) => {
        console.error('Error processing OCR:', err);
        alert('Error processing OCR');
        this.uploading = false;
      }
    });
  }

  updateItem(index: number) {
    // Item is updated via two-way binding
  }

  removeItem(index: number) {
    if (this.draftParse) {
      this.draftParse.items.splice(index, 1);
    }
  }

  addItem() {
    if (this.draftParse) {
      this.draftParse.items.push({
        rawName: '',
        priceTotal: 0,
        quantity: 1,
        unit: null,
        weightKg: null
      });
    }
  }

  getDateTimeLocal(dateString: string | null): string {
    if (!dateString) return '';
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
    if (this.draftParse && input.value) {
      this.draftParse.purchasedAt = new Date(input.value).toISOString();
    }
  }

  applyDraft() {
    if (!this.receiptId || !this.draftParse) return;

    this.applying = true;
    this.receiptsApi.applyDraft(this.receiptId, this.draftParse).subscribe({
      next: () => {
        this.router.navigate(['/receipts', this.receiptId]);
      },
      error: (err) => {
        console.error('Error applying draft:', err);
        alert('Error applying draft: ' + (err.error?.message || 'Unknown error'));
        this.applying = false;
      }
    });
  }

  cancel() {
    this.router.navigate(['/receipts']);
  }
}
