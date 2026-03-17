import { Component } from '@angular/core';
import { CardBodyComponent, CardComponent, ColComponent, RowComponent } from '@coreui/angular';

@Component({
  selector: 'app-pro-dashboard',
  standalone: true,
  imports: [CardComponent, CardBodyComponent, RowComponent, ColComponent],
  template: `
    <c-row>
      <c-col xs="12">
        <c-card class="mb-4">
          <c-card-body>
            <h4>Dashboard Profesional</h4>
            <p class="text-body-secondary">
              Panel de gestion para profesionales. Proximamente: gestion de personas,
              actividades, evaluaciones y reportes.
            </p>
          </c-card-body>
        </c-card>
      </c-col>
    </c-row>

    <c-row>
      <c-col md="4">
        <c-card class="mb-4"
          [style.background-color]="'var(--a11y-primary)'"
          [style.color]="'var(--a11y-primary-text)'"
          aria-label="Personas a cargo: 12">
          <c-card-body>
            <h5 [style.color]="'var(--a11y-primary-text)'">Personas a Cargo</h5>
            <h2 [style.color]="'var(--a11y-primary-text)'">12</h2>
          </c-card-body>
        </c-card>
      </c-col>
      <c-col md="4">
        <c-card class="mb-4"
          [style.background-color]="'var(--a11y-success)'"
          [style.color]="'var(--a11y-success-text)'"
          aria-label="Actividades activas: 28">
          <c-card-body>
            <h5 [style.color]="'var(--a11y-success-text)'">Actividades Activas</h5>
            <h2 [style.color]="'var(--a11y-success-text)'">28</h2>
          </c-card-body>
        </c-card>
      </c-col>
      <c-col md="4">
        <c-card class="mb-4"
          [style.background-color]="'var(--a11y-warning)'"
          [style.color]="'var(--a11y-warning-text)'"
          aria-label="Evaluaciones pendientes: 5">
          <c-card-body>
            <h5 [style.color]="'var(--a11y-warning-text)'">Evaluaciones Pendientes</h5>
            <h2 [style.color]="'var(--a11y-warning-text)'">5</h2>
          </c-card-body>
        </c-card>
      </c-col>
    </c-row>
  `
})
export class ProDashboardComponent {}
