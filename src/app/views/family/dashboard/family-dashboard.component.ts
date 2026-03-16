import { Component } from '@angular/core';
import { CardBodyComponent, CardComponent, ColComponent, RowComponent } from '@coreui/angular';

@Component({
  selector: 'app-family-dashboard',
  standalone: true,
  imports: [CardComponent, CardBodyComponent, RowComponent, ColComponent],
  template: `
    <c-row>
      <c-col xs="12">
        <c-card class="mb-4">
          <c-card-body>
            <h4>Panel Familiar</h4>
            <p class="text-body-secondary">
              Bienvenido al portal familiar. Aqui podras ver el progreso y las
              actividades de tu familiar.
            </p>
          </c-card-body>
        </c-card>
      </c-col>
    </c-row>

    <c-row>
      <c-col md="6">
        <c-card class="mb-4"
          [style.background-color]="'var(--a11y-success)'"
          [style.color]="'var(--a11y-success-text)'"
          aria-label="Actividades completadas: 15 de 20 esta semana">
          <c-card-body>
            <h5 [style.color]="'var(--a11y-success-text)'">Actividades Completadas</h5>
            <h2 [style.color]="'var(--a11y-success-text)'">15 / 20</h2>
            <p [style.color]="'var(--a11y-success-text)'">Esta semana</p>
          </c-card-body>
        </c-card>
      </c-col>
      <c-col md="6">
        <c-card class="mb-4"
          [style.background-color]="'var(--a11y-primary)'"
          [style.color]="'var(--a11y-primary-text)'"
          aria-label="Proxima cita: manana a las 10:00 AM">
          <c-card-body>
            <h5 [style.color]="'var(--a11y-primary-text)'">Proxima Cita</h5>
            <h2 [style.color]="'var(--a11y-primary-text)'">Manana</h2>
            <p [style.color]="'var(--a11y-primary-text)'">10:00 AM - Sesion de terapia</p>
          </c-card-body>
        </c-card>
      </c-col>
    </c-row>
  `
})
export class FamilyDashboardComponent {}
