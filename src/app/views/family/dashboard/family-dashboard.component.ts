import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { IconDirective } from '@coreui/icons-angular';
import {
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  BadgeComponent,
  SpinnerComponent,
  AlertComponent,
} from '@coreui/angular';
import { FamilyService } from '@services';
import { FamilyDashboardResponse } from '@models';

@Component({
  selector: 'app-family-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    IconDirective,
    DatePipe,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    BadgeComponent,
    SpinnerComponent,
    AlertComponent,
  ],
  templateUrl: './family-dashboard.component.html',
  styleUrl: './family-dashboard.component.scss',
})
export class FamilyDashboardComponent implements OnInit {
  private readonly familyService = inject(FamilyService);
  readonly #el = inject(ElementRef<HTMLElement>);

  dashboard: FamilyDashboardResponse | null = null;
  loading = true;
  error = false;

  ngOnInit(): void {
    this.familyService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard = data;
        this.loading = false;
        setTimeout(() => {
          const h = this.#el.nativeElement.querySelector('h1') as HTMLElement;
          h?.focus();
        }, 80);
      },
      error: () => {
        this.error = true;
        this.loading = false;
      },
    });
  }

  getSuccessColor(pct: number): string {
    if (pct >= 80) return 'success';
    if (pct >= 50) return 'warning';
    return 'danger';
  }
}
