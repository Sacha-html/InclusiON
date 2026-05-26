import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ReportsService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ReportResponse } from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-family-report-detail',
  standalone: true,
  imports: [
    DatePipe,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    BadgeComponent,
    ButtonDirective,
    SpinnerComponent,
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
    if (id) this.loadReport(id);
  }

  loadReport(id: string): void {
    this.reportsService.getById(id).subscribe({
      next: (data) => {
        this.report.set(data);
        this.isLoading.set(false);
        // Marcar como leído fire-and-forget — el badge "Nuevo" desaparece en próxima carga
        if (!data.isReadByFamily) {
          this.reportsService.markAsRead(id).subscribe();
        }
      },
      error: () => { this.isLoading.set(false); this.router.navigate([AppRoutes.Family.Reports]); },
    });
  }

  downloadPdf(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.reportsService.exportPdf(id).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a   = document.createElement('a');
      a.href     = url;
      a.download = `reporte-${id}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    });
  }

  onBack(): void {
    this.router.navigate([AppRoutes.Family.Reports]);
  }
}
