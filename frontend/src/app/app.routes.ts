import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/receipts',
    pathMatch: 'full'
  },
  {
    path: 'receipts',
    loadComponent: () => import('./components/receipts-list/receipts-list.component').then(m => m.ReceiptsListComponent)
  },
  {
    path: 'receipts/upload',
    loadComponent: () => import('./components/upload-receipt/upload-receipt.component').then(m => m.UploadReceiptComponent)
  },
  {
    path: 'receipts/:id',
    loadComponent: () => import('./components/receipt-detail/receipt-detail.component').then(m => m.ReceiptDetailComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./components/reports-dashboard/reports-dashboard.component').then(m => m.ReportsDashboardComponent)
  }
];
