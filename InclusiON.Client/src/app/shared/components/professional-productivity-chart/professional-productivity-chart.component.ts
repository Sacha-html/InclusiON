import { Component, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfessionalReportsProductivityItem } from '@models';

@Component({
  selector: 'app-professional-productivity-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (!items() || items().length === 0) {
      <div class="p-4 text-center text-body-secondary">
        <p class="mb-0">No se encontraron profesionales con reportes registrados.</p>
      </div>
    } @else {
      <div class="productivity-container">
        @for (item of items(); track item.professionalId; let idx = $index) {
          <div class="productivity-row mb-3">
            <div class="d-flex justify-content-between align-items-center mb-1 flex-wrap gap-2">
              <div class="d-flex align-items-center gap-2">
                <span class="badge" [ngClass]="getBadgeClass(idx)">
                  #{{ idx + 1 }}
                </span>
                <span class="fw-semibold text-body">{{ item.nombreProfesional }}</span>
                <span class="badge bg-light text-secondary border font-monospace">
                  {{ item.totalReportes }} {{ item.totalReportes === 1 ? 'informe' : 'informes' }}
                </span>
              </div>
              <div class="d-flex align-items-center gap-2">
                <span class="badge bg-success-subtle text-success border fw-bold">
                  {{ item.reportesAprobados }} aprobados
                </span>
              </div>
            </div>

            <!-- Barra horizontal comparativa -->
            <div class="progress progress-contrast" role="progressbar" [attr.aria-valuenow]="getPercentage(item.reportesAprobados)" aria-valuemin="0" aria-valuemax="100">
              <div class="progress-bar"
                   [style.width.%]="getPercentage(item.reportesAprobados)"
                   [style.background-color]="getBarColor(idx)">
              </div>
            </div>
          </div>
        }
      </div>
    }
  `,
  styles: [`
    .productivity-container {
      width: 100%;
      padding: 0.5rem 0;
    }
    .progress-contrast {
      height: 18px;
      background-color: var(--cui-tertiary-bg, #f0f2f5);
      border-radius: 6px;
      overflow: hidden;
      box-shadow: inset 0 1px 2px rgba(0,0,0,0.08);
    }
    .progress-bar {
      transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);
      border-radius: 6px;
    }
  `]
})
export class ProfessionalProductivityChartComponent {
  items = input<ProfessionalReportsProductivityItem[]>([]);

  maxAprobados = computed(() => {
    const list = this.items();
    if (!list || list.length === 0) return 1;
    return Math.max(...list.map(i => i.reportesAprobados), 1);
  });

  getPercentage(aprobados: number): number {
    return Math.round((aprobados / this.maxAprobados()) * 100);
  }

  getBadgeClass(index: number): string {
    if (index === 0) return 'bg-warning text-dark border'; // 1er lugar
    if (index === 1) return 'bg-secondary-subtle text-body border';
    if (index === 2) return 'bg-light text-secondary border';
    return 'bg-light text-body-tertiary border';
  }

  getBarColor(index: number): string {
    if (index === 0) return '#00C853'; // Verde Esmeralda (Top 1)
    if (index === 1) return '#1A237E'; // Azul Marino
    if (index === 2) return '#304FFE'; // Azul Eléctrico
    return '#455A64'; // Azul Grisáceo
  }
}
