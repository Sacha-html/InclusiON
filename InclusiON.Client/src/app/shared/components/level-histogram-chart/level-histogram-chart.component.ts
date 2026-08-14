import { Component, computed, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LevelDistributionItem } from '@models';

interface HistogramBar {
  nivel: number;
  nombreActividad: string;
  totalAlumnos: number;
  superaron: number;
  estancados: number;
  x: number;
  y: number;
  width: number;
  height: number;
  estancadosHeight: number;
  estancadosY: number;
}

@Component({
  selector: 'app-level-histogram-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (!items() || items().length === 0) {
      <div class="no-data-msg p-5 text-center text-body-secondary">
        <p class="mb-0">No hay datos de distribución por niveles registrados en esta aula.</p>
      </div>
    } @else {
      <div class="histogram-wrapper">
        <div class="svg-container position-relative">
          <svg viewBox="0 0 540 250" class="histogram-svg" role="img" aria-label="Histograma interactivo de distribución por niveles del Roadmap">
            <title>Distribución de Alumnos por Niveles (1 al 10)</title>

            <!-- Líneas de cuadrícula horizontal -->
            <line x1="45" y1="25" x2="520" y2="25" stroke="#e0e0e0" stroke-dasharray="3,3" />
            <line x1="45" y1="95" x2="520" y2="95" stroke="#e0e0e0" stroke-dasharray="3,3" />
            <line x1="45" y1="165" x2="520" y2="165" stroke="#e0e0e0" stroke-dasharray="3,3" />
            <line x1="45" y1="205" x2="520" y2="205" stroke="#cccccc" stroke-width="1.5" />

            <!-- Eje Y Labels -->
            <text x="36" y="29" text-anchor="end" font-size="11" font-weight="600" fill="#6c757d">{{ maxCount() }}</text>
            <text x="36" y="99" text-anchor="end" font-size="11" fill="#6c757d">{{ midHighCount() }}</text>
            <text x="36" y="169" text-anchor="end" font-size="11" fill="#6c757d">{{ midLowCount() }}</text>
            <text x="36" y="209" text-anchor="end" font-size="11" fill="#6c757d">0</text>

            <!-- Barras por nivel (1 al 10) -->
            @for (bar of bars(); track bar.nivel) {
              <g class="bar-group"
                 [attr.transform]="'translate(' + bar.x + ', 0)'"
                 (mouseenter)="onBarHover(bar)"
                 (mouseleave)="onBarLeave()">

                <!-- Área invisible ampliada para hover táctil/mouse -->
                <rect [attr.x]="-5" y="10" [attr.width]="bar.width + 10" height="200" fill="transparent" class="hover-hitbox" />

                <!-- Barra principal: Alumnos en el nivel (Azul Marino Profundo) -->
                <rect [attr.x]="0"
                      [attr.y]="bar.y"
                      [attr.width]="bar.width"
                      [attr.height]="bar.height"
                      rx="4"
                      fill="#1A237E"
                      class="bar-rect"
                      [class.is-hovered]="hoveredBar()?.nivel === bar.nivel">
                  <title>Nivel {{ bar.nivel }}: {{ bar.nombreActividad }} | {{ bar.totalAlumnos }} alumnos llegaron</title>
                </rect>

                <!-- Barra superpuesta: Alumnos estancados (Naranja Intenso) -->
                @if (bar.estancadosHeight > 0) {
                  <rect [attr.x]="0"
                        [attr.y]="bar.estancadosY"
                        [attr.width]="bar.width"
                        [attr.height]="bar.estancadosHeight"
                        rx="4"
                        fill="#FF6D00"
                        class="bar-rect-stuck"
                        [class.is-hovered]="hoveredBar()?.nivel === bar.nivel">
                    <title>Nivel {{ bar.nivel }}: {{ bar.estancados }} alumnos estancados en este nivel</title>
                  </rect>
                }

                <!-- Valor numérico sobre la barra -->
                @if (bar.totalAlumnos > 0) {
                  <text [attr.x]="bar.width / 2"
                        [attr.y]="bar.y - 6"
                        text-anchor="middle"
                        font-size="11"
                        font-weight="bold"
                        fill="#1A237E">
                    {{ bar.totalAlumnos }}
                  </text>
                }

                <!-- Etiqueta del Nivel en Eje X -->
                <text [attr.x]="bar.width / 2"
                      y="226"
                      text-anchor="middle"
                      font-size="11"
                      font-weight="bold"
                      [attr.fill]="hoveredBar()?.nivel === bar.nivel ? '#1A237E' : '#495057'">
                  N{{ bar.nivel }}
                </text>
              </g>
            }
          </svg>

          <!-- Tooltip interactivo flotante -->
          @if (hoveredBar()) {
            <div class="interactive-histogram-tooltip shadow-sm">
              <div class="fw-bold text-primary">Nivel {{ hoveredBar()!.nivel }}: {{ hoveredBar()!.nombreActividad }}</div>
              <div class="d-flex justify-content-between gap-3 mt-1 small">
                <span>Alumnos en el nivel: <strong>{{ hoveredBar()!.totalAlumnos }}</strong></span>
                <span class="text-success">Superaron: <strong>{{ hoveredBar()!.superaron }}</strong></span>
                @if (hoveredBar()!.estancados > 0) {
                  <span class="text-danger">Estancados: <strong>{{ hoveredBar()!.estancados }}</strong></span>
                }
              </div>
            </div>
          }
        </div>

        <!-- Leyenda accesible del histograma -->
        <div class="histogram-legend d-flex justify-content-center gap-4 pt-3">
          <div class="d-flex align-items-center gap-2 font-monospace small">
            <span class="legend-box" style="background-color: #1A237E; width: 14px; height: 14px; border-radius: 3px;"></span>
            <span class="text-body fw-medium">Alumnos en el nivel</span>
          </div>
          <div class="d-flex align-items-center gap-2 font-monospace small">
            <span class="legend-box" style="background-color: #FF6D00; width: 14px; height: 14px; border-radius: 3px;"></span>
            <span class="text-body fw-medium">Estancados (≤60%)</span>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .histogram-wrapper {
      width: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 420px;
      padding: 0.5rem;
    }
    .svg-container {
      width: 100%;
      max-width: 580px;
    }
    .histogram-svg {
      width: 100%;
      height: auto;
      display: block;
      overflow: visible;
    }
    .bar-group {
      cursor: pointer;
    }
    .bar-rect, .bar-rect-stuck {
      transition: y 0.3s ease, height 0.3s ease, filter 0.2s ease, transform 0.2s ease;
      transform-origin: bottom center;
      &:hover, &.is-hovered {
        filter: brightness(1.2) drop-shadow(0 4px 6px rgba(0,0,0,0.18));
      }
    }
    .interactive-histogram-tooltip {
      position: absolute;
      top: 0;
      left: 50%;
      transform: translateX(-50%);
      background: var(--cui-body-bg, #ffffff);
      padding: 0.55rem 0.95rem;
      border-radius: 8px;
      border: 1px solid rgba(0,0,0,0.15);
      border-top: 3px solid #1A237E;
      min-width: 280px;
      z-index: 10;
      pointer-events: none;
      animation: fadeIn 0.2s ease;
      text-align: center;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translate(-50%, -5px); }
      to { opacity: 1; transform: translate(-50%, 0); }
    }
  `]
})
export class LevelHistogramChartComponent {
  items = input<LevelDistributionItem[]>([]);
  hoveredBar = signal<HistogramBar | null>(null);

  maxCount = computed(() => {
    const rawMax = Math.max(...this.items().map(i => i.totalAlumnos), 1);
    return Math.max(rawMax, 6);
  });

  midHighCount = computed(() => Math.round((this.maxCount() * 2) / 3));
  midLowCount = computed(() => Math.round(this.maxCount() / 3));

  bars = computed(() => {
    const data = this.items();
    const max = this.maxCount();
    const plotHeight = 180; // De Y=25 a Y=205
    const baseY = 205;
    const barWidth = 32;
    const spacing = 47;
    const startX = 52;

    return data.map((item, idx) => {
      const height = (item.totalAlumnos / max) * plotHeight;
      const y = baseY - height;

      const estancadosHeight = (item.alumnosEstancados / max) * plotHeight;
      const estancadosY = baseY - estancadosHeight;

      return {
        nivel: item.nivel,
        nombreActividad: item.nombreActividad,
        totalAlumnos: item.totalAlumnos,
        superaron: item.alumnosSuperaron,
        estancados: item.alumnosEstancados,
        x: startX + idx * spacing,
        y,
        width: barWidth,
        height,
        estancadosHeight,
        estancadosY
      };
    });
  });

  onBarHover(bar: HistogramBar): void {
    this.hoveredBar.set(bar);
  }

  onBarLeave(): void {
    this.hoveredBar.set(null);
  }
}
