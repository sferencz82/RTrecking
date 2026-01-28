import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { ReportsApiService } from '../../services/reports-api.service';
import { ReportSummary } from '../../models/report.model';

@Component({
  selector: 'app-reports-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    FormsModule,
    BaseChartDirective
  ],
  template: `
    <div class="reports-container">
      <h1>Reports Dashboard</h1>

      <!-- Date Range Selector -->
      <mat-card class="date-selector-card">
        <mat-card-content>
          <div class="date-controls">
            <div class="presets">
              <button mat-button (click)="setPreset('thisWeek')">This Week</button>
              <button mat-button (click)="setPreset('lastWeek')">Last Week</button>
              <button mat-button (click)="setPreset('thisMonth')">This Month</button>
              <button mat-button (click)="setPreset('lastMonth')">Last Month</button>
            </div>

            <div class="custom-range">
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

              <button mat-raised-button color="primary" (click)="loadReport()" [disabled]="loading">
                Load Report
              </button>
            </div>
          </div>

          <div *ngIf="errorMessage" class="error-message">
            {{ errorMessage }}
          </div>
        </mat-card-content>
      </mat-card>

      <!-- Summary Cards -->
      <div class="summary-cards" *ngIf="report">
        <mat-card>
          <mat-card-content>
            <h2>Total Spend</h2>
            <p class="amount">{{ formatCurrency(report.totalSpend) }}</p>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Charts -->
      <div class="charts-grid" *ngIf="report">
        <!-- Spend by Category -->
        <mat-card>
          <mat-card-header>
            <mat-card-title>Spend by Category</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <canvas baseChart
              [data]="categoryChartData"
              [type]="categoryChartType"
              [options]="categoryChartOptions">
            </canvas>
          </mat-card-content>
        </mat-card>

        <!-- Spend Over Time -->
        <mat-card>
          <mat-card-header>
            <mat-card-title>Spend Over Time</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <canvas baseChart
              [data]="timeChartData"
              [type]="timeChartType"
              [options]="timeChartOptions">
            </canvas>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .reports-container {
      max-width: 1400px;
      margin: 0 auto;
    }

    .date-selector-card {
      margin-bottom: 20px;
    }

    .date-controls {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .presets {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .custom-range {
      display: flex;
      gap: 16px;
      align-items: center;
    }

    .error-message {
      color: red;
      margin-top: 16px;
      padding: 8px;
      background-color: #ffebee;
      border-radius: 4px;
    }

    .summary-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 20px;
      margin-bottom: 20px;
    }

    .summary-cards .amount {
      font-size: 2em;
      font-weight: bold;
      color: #3f51b5;
    }

    .charts-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }

    @media (max-width: 1200px) {
      .charts-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ReportsDashboardComponent implements OnInit {
  fromDate: Date | null = null;
  toDate: Date | null = null;
  report: ReportSummary | null = null;
  loading = false;
  errorMessage: string | null = null;

  // Category Chart
  categoryChartType: ChartType = 'pie';
  categoryChartData: ChartData<'pie'> = {
    labels: [],
    datasets: [{
      data: []
    }]
  };
  categoryChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    plugins: {
      legend: {
        position: 'right'
      }
    }
  };

  // Time Chart
  timeChartType: ChartType = 'line';
  timeChartData: ChartData<'line'> = {
    labels: [],
    datasets: [{
      label: 'Daily Spend',
      data: [],
      borderColor: '#3f51b5',
      backgroundColor: 'rgba(63, 81, 181, 0.1)',
      fill: true
    }]
  };
  timeChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => {
            return new Intl.NumberFormat('de-CH', {
              style: 'currency',
              currency: 'CHF',
              minimumFractionDigits: 0
            }).format(value as number);
          }
        }
      }
    },
    plugins: {
      tooltip: {
        callbacks: {
          label: (context) => {
            return new Intl.NumberFormat('de-CH', {
              style: 'currency',
              currency: 'CHF'
            }).format(context.parsed.y);
          }
        }
      }
    }
  };

  constructor(private reportsApi: ReportsApiService) {}

  ngOnInit() {
    this.setPreset('thisMonth');
  }

  setPreset(preset: string) {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    switch (preset) {
      case 'thisWeek':
        const thisWeekStart = new Date(today);
        thisWeekStart.setDate(today.getDate() - today.getDay());
        this.fromDate = thisWeekStart;
        this.toDate = new Date(today);
        break;

      case 'lastWeek':
        const lastWeekEnd = new Date(today);
        lastWeekEnd.setDate(today.getDate() - today.getDay() - 1);
        const lastWeekStart = new Date(lastWeekEnd);
        lastWeekStart.setDate(lastWeekEnd.getDate() - 6);
        this.fromDate = lastWeekStart;
        this.toDate = lastWeekEnd;
        break;

      case 'thisMonth':
        this.fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
        this.toDate = new Date(today);
        break;

      case 'lastMonth':
        const lastMonthEnd = new Date(today.getFullYear(), today.getMonth(), 0);
        const lastMonthStart = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        this.fromDate = lastMonthStart;
        this.toDate = lastMonthEnd;
        break;
    }

    this.loadReport();
  }

  loadReport() {
    if (!this.fromDate || !this.toDate) {
      this.errorMessage = 'Please select both from and to dates';
      return;
    }

    // Validate 6-month limit
    const daysDiff = Math.ceil((this.toDate.getTime() - this.fromDate.getTime()) / (1000 * 60 * 60 * 24));
    if (daysDiff > 184) {
      this.errorMessage = `Date range cannot exceed 184 days (approximately 6 months). Selected range: ${daysDiff} days.`;
      return;
    }

    if (this.toDate < this.fromDate) {
      this.errorMessage = 'To date must be after from date';
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    const from = this.formatDateForApi(this.fromDate);
    const to = this.formatDateForApi(this.toDate);

    this.reportsApi.getSummary(from, to).subscribe({
      next: (data) => {
        this.report = data;
        this.updateCharts();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading report:', err);
        this.errorMessage = err.error?.message || 'Error loading report';
        this.loading = false;
      }
    });
  }

  updateCharts() {
    if (!this.report) return;

    // Update category chart
    this.categoryChartData = {
      labels: this.report.spendByCategory.map(c => c.categoryName),
      datasets: [{
        data: this.report.spendByCategory.map(c => c.totalSpend)
      }]
    };

    // Update time chart
    this.timeChartData = {
      labels: this.report.spendOverTimeDaily.map(d => this.formatDateShort(d.date)),
      datasets: [{
        label: 'Daily Spend',
        data: this.report.spendOverTimeDaily.map(d => d.totalSpend),
        borderColor: '#3f51b5',
        backgroundColor: 'rgba(63, 81, 181, 0.1)',
        fill: true
      }]
    };
  }

  formatDateForApi(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  formatDateShort(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('de-CH', { day: '2-digit', month: '2-digit' });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('de-CH', {
      style: 'currency',
      currency: 'CHF'
    }).format(amount);
  }
}
