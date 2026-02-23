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
        <c-card class="mb-4 text-white bg-success">
          <c-card-body>
            <h5>Actividades Completadas</h5>
            <h2>15 / 20</h2>
            <p>Esta semana</p>
          </c-card-body>
        </c-card>
      </c-col>
      <c-col md="6">
        <c-card class="mb-4 text-white bg-info">
          <c-card-body>
            <h5>Proxima Cita</h5>
            <h2>Manana</h2>
            <p>10:00 AM - Sesion de terapia</p>
          </c-card-body>
        </c-card>
      </c-col>
    </c-row>
  `
})
export class FamilyDashboardComponent {}
