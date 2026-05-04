import { Component } from '@angular/core';
import { RowComponent, ColComponent, CardComponent, CardBodyComponent } from '@coreui/angular';

@Component({
  selector: 'app-coming-soon',
  standalone: true,
  imports: [RowComponent, ColComponent, CardComponent, CardBodyComponent],
  templateUrl: './coming-soon.component.html',
})
export class ComingSoonComponent {}
