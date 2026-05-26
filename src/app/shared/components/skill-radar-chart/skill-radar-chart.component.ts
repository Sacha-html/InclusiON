import { Component, computed, input } from '@angular/core';
import { SkillRadarPointResponse } from '@models';

interface RadarCoord { x: number; y: number; }
interface LabelPos extends RadarCoord { name: string; anchor: string; dy: number; }
interface DataCoord extends RadarCoord { pct: number; areaName: string; }

@Component({
  selector: 'app-skill-radar-chart',
  standalone: true,
  template: `
    @if (n() < 3) {
      <p class="no-radar-msg">
        Se necesitan al menos 3 áreas en el roadmap para mostrar el radar.
        @if (n() > 0) { Áreas actuales: <strong>{{ n() }}</strong>. }
      </p>
    } @else {
      <div class="radar-container">
        <svg [attr.viewBox]="viewBox" class="radar-svg" role="img"
             [attr.aria-label]="'Radar de habilidades — ' + n() + ' áreas'">
          <title>Radar de rendimiento por área de habilidad</title>

          <!-- background grid rings -->
          @for (ring of gridRings; track ring.pct) {
            <polygon [attr.points]="ringPoints(ring.fraction)" class="grid-ring"/>
            <text [attr.x]="cx + 3" [attr.y]="cy - ring.fraction * r - 3"
                  class="ring-label">{{ ring.pct }}%</text>
          }

          <!-- axis lines -->
          @for (pt of axisEndpoints(); track $index) {
            <line [attr.x1]="cx" [attr.y1]="cy"
                  [attr.x2]="pt.x" [attr.y2]="pt.y" class="axis-line"/>
          }

          <!-- data fill polygon -->
          @if (hasAnyData()) {
            <polygon [attr.points]="dataPolygonPoints()" class="data-polygon"/>
          }

          <!-- data dots -->
          @for (coord of dataCoords(); track $index) {
            <circle [attr.cx]="coord.x" [attr.cy]="coord.y" r="5" class="data-dot">
              <title>{{ coord.areaName }}: {{ fmtPct(coord.pct) }}</title>
            </circle>
          }

          <!-- area name labels -->
          @for (lbl of labelPositions(); track $index) {
            <text [attr.x]="lbl.x" [attr.y]="lbl.y + lbl.dy"
                  [attr.text-anchor]="lbl.anchor" class="area-label">
              {{ truncate(lbl.name) }}
            </text>
          }
        </svg>

        <!-- legend table -->
        <div class="radar-legend" role="list" aria-label="Leyenda del radar">
          @for (pt of points(); track pt.areaName) {
            <div class="legend-row" role="listitem">
              <span class="legend-dot"
                    [style.background]="pt.color || 'var(--a11y-primary, #2196F3)'"
                    aria-hidden="true"></span>
              <span class="legend-name">{{ pt.areaName }}</span>
              <span class="legend-value"
                    [attr.aria-label]="pt.areaName + ': ' + (pt.avgSuccessPercent != null ? pt.avgSuccessPercent!.toFixed(1) + '%' : 'sin datos')">
                @if (pt.avgSuccessPercent != null) {
                  {{ pt.avgSuccessPercent!.toFixed(1) }}%
                } @else {
                  <span class="no-data-chip">Sin datos</span>
                }
              </span>
              <span class="legend-count" aria-label="{{ pt.totalResponses }} respuestas">
                {{ pt.totalResponses }} resp.
              </span>
            </div>
          }
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    .no-radar-msg {
      color: var(--a11y-text-muted, #757575);
      font-size: .9rem;
      padding: 1rem 0;
    }

    .radar-container {
      display: flex;
      flex-wrap: wrap;
      align-items: flex-start;
      gap: 1.5rem;
    }

    .radar-svg {
      width: 280px;
      height: 280px;
      flex-shrink: 0;
      overflow: visible;
    }

    /* SVG elements */
    .grid-ring {
      fill: none;
      stroke: var(--a11y-border, #E0E0E0);
      stroke-width: 1;
    }

    .ring-label {
      font-size: 8px;
      fill: var(--a11y-text-muted, #9E9E9E);
    }

    .axis-line {
      stroke: var(--a11y-border, #E0E0E0);
      stroke-width: 1;
    }

    .data-polygon {
      fill: rgba(33, 150, 243, 0.15);
      stroke: var(--a11y-primary, #2196F3);
      stroke-width: 2;
      stroke-linejoin: round;
    }

    .data-dot {
      fill: var(--a11y-primary, #2196F3);
      stroke: var(--a11y-bg, #fff);
      stroke-width: 1.5;
    }

    .area-label {
      font-size: 10px;
      fill: var(--a11y-text, #212121);
      font-weight: 600;
    }

    /* Legend */
    .radar-legend {
      display: flex;
      flex-direction: column;
      gap: .5rem;
      min-width: 180px;
      padding-top: .25rem;
    }

    .legend-row {
      display: grid;
      grid-template-columns: 10px 1fr auto auto;
      align-items: center;
      gap: .5rem;
      font-size: .85rem;
    }

    .legend-dot {
      width: 10px;
      height: 10px;
      border-radius: 50%;
      flex-shrink: 0;
    }

    .legend-name {
      color: var(--a11y-text, #212121);
      font-weight: 500;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .legend-value {
      font-weight: 700;
      color: var(--a11y-primary, #1565C0);
      min-width: 48px;
      text-align: right;
    }

    .legend-count {
      color: var(--a11y-text-muted, #757575);
      font-size: .78rem;
      min-width: 52px;
      text-align: right;
    }

    .no-data-chip {
      font-size: .75rem;
      color: var(--a11y-text-muted, #9E9E9E);
      font-weight: 400;
    }

    @media (prefers-reduced-motion: reduce) {
      * { animation: none !important; transition: none !important; }
    }
  `]
})
export class SkillRadarChartComponent {
  readonly points = input<SkillRadarPointResponse[]>([]);

  // SVG geometry constants
  readonly cx = 140;
  readonly cy = 140;
  readonly r  = 95;
  readonly labelR = 123;
  readonly viewBox = '0 0 280 280';

  readonly gridRings = [
    { pct: 25,  fraction: 0.25 },
    { pct: 50,  fraction: 0.50 },
    { pct: 75,  fraction: 0.75 },
    { pct: 100, fraction: 1.00 },
  ];

  protected readonly n = computed(() => this.points().length);

  protected readonly angles = computed(() => {
    const n = this.n();
    return Array.from({ length: n }, (_, i) =>
      -Math.PI / 2 + (2 * Math.PI * i / n));
  });

  protected readonly hasAnyData = computed(() =>
    this.points().some(p => p.avgSuccessPercent != null));

  /** Polygon points string for a concentric ring at [fraction] of radius */
  protected ringPoints(fraction: number): string {
    return this.angles()
      .map(a => `${this.cx + fraction * this.r * Math.cos(a)},${this.cy + fraction * this.r * Math.sin(a)}`)
      .join(' ');
  }

  protected axisEndpoints(): RadarCoord[] {
    return this.angles().map(a => ({
      x: this.cx + this.r * Math.cos(a),
      y: this.cy + this.r * Math.sin(a),
    }));
  }

  protected dataPolygonPoints(): string {
    return this.angles().map((a, i) => {
      const v = (this.points()[i].avgSuccessPercent ?? 0) / 100;
      return `${this.cx + v * this.r * Math.cos(a)},${this.cy + v * this.r * Math.sin(a)}`;
    }).join(' ');
  }

  protected dataCoords(): DataCoord[] {
    return this.angles().map((a, i) => {
      const pt = this.points()[i];
      const v  = (pt.avgSuccessPercent ?? 0) / 100;
      return {
        x: this.cx + v * this.r * Math.cos(a),
        y: this.cy + v * this.r * Math.sin(a),
        pct: pt.avgSuccessPercent ?? 0,
        areaName: pt.areaName,
      };
    });
  }

  protected labelPositions(): LabelPos[] {
    return this.angles().map((a, i) => {
      const cos = Math.cos(a);
      const sin = Math.sin(a);
      const anchor = cos > 0.15 ? 'start' : cos < -0.15 ? 'end' : 'middle';
      // push labels slightly further out than the grid boundary
      const lx = this.cx + this.labelR * cos;
      const ly = this.cy + this.labelR * sin;
      // vertical centering nudge for top/bottom axis labels
      const dy = sin < -0.5 ? -4 : sin > 0.5 ? 12 : 4;
      return { x: lx, y: ly, dy, name: this.points()[i].areaName, anchor };
    });
  }

  protected truncate(name: string, max = 13): string {
    return name.length > max ? name.slice(0, max - 1) + '…' : name;
  }

  protected fmtPct(v: number): string {
    return v.toFixed(1) + '%';
  }
}
