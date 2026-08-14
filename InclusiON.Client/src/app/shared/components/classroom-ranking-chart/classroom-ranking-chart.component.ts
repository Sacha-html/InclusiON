import { Component, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClassroomRankingItem } from '@models';

@Component({
  selector: 'app-classroom-ranking-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (!items() || items().length === 0) {
      <div class="p-4 text-center text-body-secondary">
        <p class="mb-0">No se encontraron aulas para comparar.</p>
      </div>
    } @else {
      <div class="ranking-container">
        @for (item of items(); track item.classroomId; let idx = $index) {
          <div class="ranking-row mb-3">
            <!-- Encabezado de la fila: Nombre del aula + Ranking + Metadatos -->
            <div class="d-flex justify-content-between align-items-center mb-1 flex-wrap gap-2">
              <div class="d-flex align-items-center gap-2">
                <span class="badge" [ngClass]="getBadgeClass(idx)">
                  #{{ idx + 1 }}
                </span>
                <span class="fw-semibold text-body">{{ item.nombreAula }}</span>
                <span class="badge bg-light text-secondary border font-monospace">
                  {{ item.totalAlumnos }} {{ item.totalAlumnos === 1 ? 'alumno' : 'alumnos' }}
                </span>
              </div>
              <div class="d-flex align-items-center gap-2">
                <span class="small text-body-secondary">{{ item.totalSesiones }} partidas</span>
                <span class="fw-bold fs-6" [style.color]="getSuccessColor(item.promedioExitoAula)">
                  {{ item.promedioExitoAula }}%
                </span>
              </div>
            </div>

            <!-- Barra horizontal de progreso de Alto Contraste -->
            <div class="progress progress-contrast" role="progressbar" [attr.aria-valuenow]="item.promedioExitoAula" aria-valuemin="0" aria-valuemax="100">
              <div class="progress-bar"
                   [style.width.%]="item.promedioExitoAula"
                   [style.background-color]="getSuccessColor(item.promedioExitoAula)">
              </div>
            </div>
          </div>
        }
      </div>
    }
  `,
  styles: [`
    .ranking-container {
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
      font-weight: 600;
    }
  `]
})
export class ClassroomRankingChartComponent {
  items = input<ClassroomRankingItem[]>([]);

  getBadgeClass(index: number): string {
    if (index === 0) return 'bg-warning text-dark border'; // 1er lugar (Oro)
    if (index === 1) return 'bg-secondary-subtle text-body border'; // 2do lugar (Plata)
    if (index === 2) return 'bg-light text-secondary border'; // 3er lugar (Bronce)
    return 'bg-light text-body-tertiary border';
  }

  getSuccessColor(success: number): string {
    if (success >= 75) return '#00C853'; // Verde Brillante
    if (success >= 60) return '#1A237E'; // Azul Marino Intenso
    if (success >= 45) return '#FF6D00'; // Naranja Vivo
    return '#D50000'; // Rojo Escarlata
  }
}
