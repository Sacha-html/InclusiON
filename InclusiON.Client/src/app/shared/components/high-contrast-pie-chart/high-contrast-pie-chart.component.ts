import { Component, computed, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryPerformanceItem } from '@models';

interface PieSlice {
  categoria: string;
  promedioExito: number;
  totalSesiones: number;
  pct: number;
  color: string;
  pathD: string;
  midAngle: number;
  hoverTransform: string;
}

@Component({
  selector: 'app-high-contrast-pie-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (!items() || items().length === 0) {
      <div class="no-data-msg p-5 text-center text-body-secondary">
        <p class="mb-0">No hay datos de categorías registrados en esta aula.</p>
      </div>
    } @else {
      <div class="pie-chart-wrapper">
        <div class="svg-container position-relative">
          <svg viewBox="-160 -160 320 320" class="pie-svg" role="img" aria-label="Gráfico de torta de rendimiento pedagógico">
            <title>Rendimiento por Categoría Pedagógica</title>
            @for (slice of slices(); track slice.categoria) {
              <path [attr.d]="slice.pathD"
                    [attr.fill]="slice.color"
                    class="pie-slice"
                    [class.is-hovered]="hoveredSlice()?.categoria === slice.categoria"
                    [style.transform]="hoveredSlice()?.categoria === slice.categoria ? slice.hoverTransform : 'none'"
                    (mouseenter)="onSliceHover(slice)"
                    (mouseleave)="onSliceLeave()"
                    stroke="#ffffff"
                    stroke-width="3">
                <title>{{ slice.categoria }}: {{ slice.totalSesiones }} sesiones ({{ slice.pct | number:'1.1-1' }}%) • Éxito: {{ slice.promedioExito }}%</title>
              </path>
            }

            <!-- Centro Donut con resumen interactivo -->
            <circle cx="0" cy="0" r="62" fill="var(--cui-body-bg, #ffffff)" stroke="#e9ecef" stroke-width="2" />
            <text x="0" y="-8" text-anchor="middle" class="donut-total-val" font-size="20" font-weight="bold" fill="currentColor">
              {{ hoveredSlice() ? (hoveredSlice()!.promedioExito + '%') : totalSessions() }}
            </text>
            <text x="0" y="16" text-anchor="middle" class="donut-total-lbl" font-size="11" font-weight="600" fill="#6c757d">
              {{ hoveredSlice() ? 'Éxito' : 'Sesiones' }}
            </text>
          </svg>

          <!-- Floating Tooltip interactivo al hacer hover -->
          @if (hoveredSlice()) {
            <div class="interactive-pie-tooltip shadow-sm" [style.border-left-color]="hoveredSlice()!.color">
              <div class="fw-bold text-truncate">{{ hoveredSlice()!.categoria }}</div>
              <div class="d-flex justify-content-between gap-2 small">
                <span>{{ hoveredSlice()!.totalSesiones }} sesiones ({{ hoveredSlice()!.pct | number:'1.1-1' }}%)</span>
                <span class="badge bg-primary">{{ hoveredSlice()!.promedioExito }}% éxito</span>
              </div>
            </div>
          }
        </div>

        <!-- Leyenda accesible de Alto Contraste -->
        <div class="pie-legend">
          @for (slice of slices(); track slice.categoria) {
            <div class="legend-item"
                 [class.active-item]="hoveredSlice()?.categoria === slice.categoria"
                 (mouseenter)="onSliceHover(slice)"
                 (mouseleave)="onSliceLeave()">
              <span class="legend-swatch" [style.background-color]="slice.color"></span>
              <span class="legend-label text-truncate" [title]="slice.categoria">{{ slice.categoria }}</span>
              <span class="legend-sessions text-body-secondary small">{{ slice.totalSesiones }} ses.</span>
              <span class="legend-pct badge bg-light text-dark border ms-2">{{ slice.promedioExito }}%</span>
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
      gap: 1.5rem;
      width: 100%;
      min-height: 420px;
      padding: 0.5rem;
    }
    .svg-container {
      width: 300px;
      height: 300px;
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
      transition: transform 0.25s cubic-bezier(0.175, 0.885, 0.32, 1.275), filter 0.2s ease, stroke-width 0.2s;
      cursor: pointer;
      transform-origin: 0 0;
      &:hover, &.is-hovered {
        filter: brightness(1.15) drop-shadow(0 4px 8px rgba(0,0,0,0.2));
        stroke-width: 4;
      }
    }
    .interactive-pie-tooltip {
      position: absolute;
      bottom: -10px;
      left: 50%;
      transform: translateX(-50%);
      background: var(--cui-body-bg, #ffffff);
      padding: 0.5rem 0.85rem;
      border-radius: 8px;
      border: 1px solid rgba(0,0,0,0.12);
      border-left-width: 5px;
      min-width: 220px;
      z-index: 10;
      pointer-events: none;
      animation: fadeIn 0.2s ease;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translate(-50%, 5px); }
      to { opacity: 1; transform: translate(-50%, 0); }
    }
    .pie-legend {
      width: 100%;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      max-height: 180px;
      overflow-y: auto;
      padding: 0 0.5rem;
    }
    .legend-item {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      font-size: 0.88rem;
      padding: 0.35rem 0.5rem;
      border-radius: 6px;
      transition: background-color 0.15s ease;
      cursor: pointer;
      &:hover, &.active-item {
        background-color: var(--cui-tertiary-bg, #f8f9fa);
      }
    }
    .legend-swatch {
      width: 14px;
      height: 14px;
      border-radius: 4px;
      flex-shrink: 0;
    }
    .legend-label {
      flex: 1;
      font-weight: 500;
    }
    .legend-sessions {
      font-size: 0.78rem;
    }
  `]
})
export class HighContrastPieChartComponent {
  items = input<CategoryPerformanceItem[]>([]);
  hoveredSlice = signal<PieSlice | null>(null);

  totalSessions = computed(() =>
    this.items().reduce((acc, item) => acc + item.totalSesiones, 0)
  );

  slices = computed(() => {
    const data = this.items();
    const total = this.totalSessions();
    if (!data || data.length === 0 || total === 0) return [];

    let currentAngle = -Math.PI / 2; // Arriba (12 en punto)
    const radius = 135;
    const innerRadius = 62;

    return data.map((item) => {
      const fraction = item.totalSesiones / total;
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

      // Vector de elevación para hover offset (14px hacia afuera)
      const hoverX = 14 * Math.cos(midAngle);
      const hoverY = 14 * Math.sin(midAngle);
      const hoverTransform = `translate(${hoverX}px, ${hoverY}px)`;

      return {
        categoria: item.categoria,
        promedioExito: item.promedioExito,
        totalSesiones: item.totalSesiones,
        pct: fraction * 100,
        color: item.color || '#1A237E',
        pathD,
        midAngle,
        hoverTransform
      };
    });
  });

  onSliceHover(slice: PieSlice): void {
    this.hoveredSlice.set(slice);
  }

  onSliceLeave(): void {
    this.hoveredSlice.set(null);
  }
}
