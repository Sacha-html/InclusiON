import { Component } from '@angular/core';
import {
  CardBodyComponent,
  CardComponent,
  ColComponent,
  RowComponent,
} from '@coreui/angular';

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss'],
  imports: [
    CardComponent,
    CardBodyComponent,
    RowComponent,
    ColComponent,
  ]
})
export class DashboardComponent {
}
