import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ProfessionalResponse } from '@models';
import { formatDate, formatDateTime } from '@shared/utils';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormLabelDirective,
  RowComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-basic-info',
  standalone: true,
  imports: [
    BadgeComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    ButtonDirective,
  ],
  templateUrl: './professional-basic-info.component.html',
})
export class ProfessionalBasicInfoComponent {
  @Input({ required: true }) professional!: ProfessionalResponse;
  @Output() deactivate = new EventEmitter<void>();

  readonly formatDate = formatDate;
  readonly formatDateTime = formatDateTime;

  constructor(private router: Router) {}

  goBack(): void {
    this.router.navigate(['/admin/professionals']);
  }

  goToEdit(): void {
    this.router.navigate(['/admin/professionals', this.professional.id, 'edit']);
  }
}
