import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PersonsService } from '@services';
import { PersonResponse } from '../../../../models';
import {
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
  selector: 'app-detail',
  imports: [
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    ButtonDirective,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);

  person: PersonResponse | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.personsService.getPersonById(id).subscribe({
        next: (response) => (this.person = response.data),
        error: () => this.router.navigate(['/admin/persons']),
      });
    }
  }

  goToEdit(): void {
    if (this.person) {
      this.router.navigate(['/admin/persons', this.person.id, 'edit']);
    }
  }

  formatDate(date: string | null | undefined): string {
    if (!date) return 'Sin especificar';
    const d = new Date(date);
    if (isNaN(d.getTime())) return 'Sin especificar';
    return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  formatDateTime(date: string | null | undefined): string {
    if (!date) return 'Sin especificar';
    const d = new Date(date);
    if (isNaN(d.getTime())) return 'Sin especificar';
    return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' })
      + ' ' + d.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
  }

  formatLevel(level: number | null | undefined): string {
    return level != null ? `${level} / 5` : 'Sin especificar';
  }

  formatBoolean(value: boolean): string {
    return value ? 'Sí' : 'No';
  }

  goBack(): void {
    this.router.navigate(['/admin/persons']);
  }
}
