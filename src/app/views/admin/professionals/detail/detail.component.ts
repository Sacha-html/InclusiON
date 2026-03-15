import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProfessionalsService } from '@services';
import { ProfessionalResponse } from '../../../../models';
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
  private readonly professionalsService = inject(ProfessionalsService);

  professional: ProfessionalResponse | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.professionalsService.getProfessionalById(id).subscribe({
        next: (data) => (this.professional = data),
        error: () => this.router.navigate(['/admin/professionals']),
      });
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

  goBack(): void {
    this.router.navigate(['/admin/professionals']);
  }
}
