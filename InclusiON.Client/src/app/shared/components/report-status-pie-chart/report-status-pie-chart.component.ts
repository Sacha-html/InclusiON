import { Component, computed, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportStatusDistributionItem } from '@models';

interface StatusSlice {
  estado: string;
  estadoKey: string;
  cantidad: number;
  pct: number;
  color: string;
  pathD: string;
  hoverTransform: string;
}

@Component({
  selector: 'app-report-status-pie-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (!items() || items().length === 0 || totalReports() === 0) {
      <div class="no-data-msg p-4 text-center text-body-secondary">
        <p class="mb-0">No hay informes registrados este mes.</p>
      </div>
    } @else {
      <div class="pie-chart-wrapper">
        <div class="svg-container position-relative">
          <svg viewBox="-140 -140 280 280" class="pie-svg" role="img" aria-label="Gráfico de torta de distribución de estados de reportes">
            <title>Distribución de Estados de Reportes</title>
            @for (slice of slices(); track slice.estadoKey) {
              <path [attr.d]="slice.pathD"
                    [attr.fill]="slice.color"
                    class="pie-slice"
                    [class.is-hovered]="hoveredSlice()?.estadoKey === slice.estadoKey"
                    [style.transform]="hoveredSlice()?.estadoKey === slice.estadoKey ? slice.hoverTransform : 'none'"
                    (mouseenter)="onSliceHover(slice)"
                    (mouseleave)="onSliceLeave()"
                    stroke="#ffffff"
                    stroke-width="3">
                <title>{{ slice.estado }}: {{ slice.cantidad }} ({{ slice.pct | number:'1.1-1' }}%)</title>
              </path>
            }

            <!-- Centro Donut -->
            <circle cx="0" cy="0" r="54" fill="var(--cui-body-bg, #ffffff)" stroke="#e9ecef" stroke-width="2" />
            <text x="0" y="-6" text-anchor="middle" class="donut-total-val" font-size="18" font-weight="bold" fill="currentColor">
              {{ hoveredSlice() ? hoveredSlice()!.cantidad : totalReports() }}
            </text>
            <text x="0" y="14" text-anchor="middle" class="donut-total-lbl" font-size="10" font-weight="600" fill="#6c757d">
              {{ hoveredSlice() ? hoveredSlice()!.estado : 'Informes' }}
            </text>
          </svg>

          <!-- Tooltip interactivo -->
          @if (hoveredSlice()) {
            <div class="interactive-status-tooltip shadow-sm" [style.border-left-color]="hoveredSlice()!.color">
              <div class="fw-bold">{{ hoveredSlice()!.estado }}</div>
              <div class="small text-body-secondary">
                {{ hoveredSlice()!.cantidad }} informes ({{ hoveredSlice()!.pct | number:'1.1-1' }}%)
              </div>
            </div>
          }
        </div>

        <!-- Leyenda -->
        <div class="pie-legend">
          @for (slice of slices(); track slice.estadoKey) {
            <div class="legend-item"
                 [class.active-item]="hoveredSlice()?.estadoKey === slice.estadoKey"
                 (mouseenter)="onSliceHover(slice)"
                 (mouseleave)="onSliceLeave()">
              <span class="legend-swatch" [style.background-color]="slice.color"></span>
              <span class="legend-label">{{ slice.estado }}</span>
              <span class="legend-count badge bg-light text-dark border ms-auto">{{ slice.cantidad }}</span>
            </div>
          }
        </div>
      </div>
    }
  `,
  styles: [`
    .pie-chart-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 1.2rem;
      width: 100%;
      min-height: 380px;
      padding: 0.5rem;
    }
    .svg-container {
      width: 250px;
      height: 250px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .pie-svg {
      width: 100%;
      height: 100%;
      overflow: visible;
    }
    .pie-slice {
      transition: transform 0.25s ease, filter 0.2s ease;
      cursor: pointer;
      transform-origin: 0 0;
      &:hover, &.is-hovered {
        filter: brightness(1.15) drop-shadow(0 4px 8px rgba(0,0,0,0.18));
      }
    }
    .interactive-status-tooltip {
      position: absolute;
      bottom: -15px;
      left: 50%;
      transform: translateX(-50%);
      background: var(--cui-body-bg, #ffffff);
      padding: 0.4rem 0.75rem;
      border-radius: 6px;
      border: 1px solid rgba(0,0,0,0.12);
      border-left-width: 4px;
      min-width: 180px;
      z-index: 10;
      pointer-events: none;
      text-align: center;
    }
    .pie-legend {
      width: 100%;
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }
    .legend-item {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      font-size: 0.85rem;
      padding: 0.3rem 0.5rem;
      border-radius: 6px;
      cursor: pointer;
      transition: background-color 0.15s ease;
      &:hover, &.active-item {
        background-color: var(--cui-tertiary-bg, #f8f9fa);
      }
    }
    .legend-swatch {
      width: 12px;
      height: 12px;
      border-radius: 3px;
      flex-shrink: 0;
    }
    .legend-label {
      font-weight: 500;
    }
  `]
})
export class ReportStatusPieChartComponent {
  items = input<ReportStatusDistributionItem[]>([]);
  hoveredSlice = signal<StatusSlice | null>(null);

  totalReports = computed(() =>
    this.items().reduce((acc, item) => acc + item.cantidad, 0)
  );

  slices = computed(() => {
    const data = this.items().filter(i => i.cantidad > 0);
    const total = this.totalReports();
    if (!data || data.length === 0 || total === 0) return [];

    let currentAngle = -Math.PI / 2;
    const radius = 115;
    const innerRadius = 54;

    return data.map((item) => {
      const fraction = item.cantidad / total;
      const angle = fraction * 2 * Math.PI;
      const startAngle = currentAngle;
      const endAngle = currentAngle + angle;
      const midAngle = startAngle + angle / 2;
      currentAngle = endAngle;

      const x1 = radius * Math.cos(startAngle);
      const y1 = radius * Math.sin(startAngle);
      const x2 = radius * Math.cos(endAngle);
      const y2 = radius * Math.sin(endAngle);

      const ix1 = innerRadius * Math.cos(startAngle);
      const iy1 = innerRadius * Math.sin(startAngle);
      const ix2 = innerRadius * Math.cos(endAngle);
      const iy2 = innerRadius * Math.sin(endAngle);

      const largeArc = angle > Math.PI ? 1 : 0;

      const pathD = `
        M ${ix1} ${iy1}
        L ${x1} ${y1}
        A ${radius} ${radius} 0 ${largeArc} 1 ${x2} ${y2}
        L ${ix2} ${iy2}
        A ${innerRadius} ${innerRadius} 0 ${largeArc} 0 ${ix1} ${iy1}
        Z
      `;

      const hoverX = 12 * Math.cos(midAngle);
      const hoverY = 12 * Math.sin(midAngle);
      const hoverTransform = `translate(${hoverX}px, ${hoverY}px)`;

      return {
        estado: item.estado,
        estadoKey: item.estadoKey,
        cantidad: item.cantidad,
        pct: fraction * 100,
        color: item.color || '#00C853',
        pathD,
        hoverTransform
      };
    });
  });

  onSliceHover(slice: StatusSlice): void {
    this.hoveredSlice.set(slice);
  }

  onSliceLeave(): void {
    this.hoveredSlice.set(null);
  }
}
