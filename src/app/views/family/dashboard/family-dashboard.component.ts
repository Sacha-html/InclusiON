import { Component } from '@angular/core';
import { CardBodyComponent, CardComponent, ColComponent, RowComponent } from '@coreui/angular';

@Component({
  selector: 'app-family-dashboard',
  standalone: true,
  imports: [CardComponent, CardBodyComponent, RowComponent, ColComponent],
  templateUrl: './family-dashboard.component.html',
})
export class FamilyDashboardComponent {}
