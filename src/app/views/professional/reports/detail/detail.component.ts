import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ReportsService } from '@services';
import { ReportResponse } from '@models/responses/reports/report.response';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  SpinnerComponent,
  ColComponent,
  RowComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-report-detail',
  standalone: true,
  imports: [
    DatePipe,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    BadgeComponent,
    ButtonDirective,
    SpinnerComponent,
    ColComponent,
    RowComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly reportsService = inject(ReportsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  report = signal<ReportResponse | null>(null);
  isLoading = signal(true);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadReport(+id);
    }
  }

  loadReport(id: number): void {
    this.reportsService.getById(id).subscribe({
      next: (data) => {
        this.report.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.router.navigate(['/pro/reports']);
      },
    });
  }

  onBack(): void {
    this.router.navigate(['/pro/reports']);
  }

  getStatusColor(isActive: boolean): string {
    return isActive ? 'success' : 'secondary';
  }
}