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
        <c-card class="mb-4 text-white bg-primary">
          <c-card-body>
            <h5>Personas a Cargo</h5>
            <h2>12</h2>
          </c-card-body>
        </c-card>
      </c-col>
      <c-col md="4">
        <c-card class="mb-4 text-white bg-success">
          <c-card-body>
            <h5>Actividades Activas</h5>
            <h2>28</h2>
          </c-card-body>
        </c-card>
      </c-col>
      <c-col md="4">
        <c-card class="mb-4 text-white bg-warning">
          <c-card-body>
            <h5>Evaluaciones Pendientes</h5>
            <h2>5</h2>
          </c-card-body>
        </c-card>
      </c-col>
    </c-row>
  `
})
export class ProDashboardComponent {}
