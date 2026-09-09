import { Component, Input } from '@angular/core';
import { PersonResponse } from '@models';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';

@Component({
  selector: 'app-person-accessibility',
  standalone: true,
  imports: [
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
  ],
  templateUrl: './person-accessibility.component.html',
})
export class PersonAccessibilityComponent {
  @Input({ required: true }) person!: PersonResponse;
}
