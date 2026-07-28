import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FamilyService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { FamilyResponse } from '@models';
import { formatDateTime } from '@shared/utils';
import {
  AlertComponent, BadgeComponent, ButtonDirective, CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, FormControlDirective, FormLabelDirective, RowComponent,
  ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-family-detail',
  imports: [
    AlertComponent, BadgeComponent, CardComponent, CardBodyComponent, CardHeaderComponent, RowComponent,
    ColComponent, FormControlDirective, FormLabelDirective, ButtonDirective,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly familyService = inject(FamilyService);
  private readonly toastService = inject(ToastService);

  family: FamilyResponse | null = null;
  showConfirmModal = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.familyService.getFamilyById(id).subscribe({
        next: (data) => (this.family = data),
        error: () => this.router.navigate([AppRoutes.Admin.Family]),
      });
    }
  }

  goToEdit(): void {
    if (this.family) {
      this.router.navigate([AppRoutes.Admin.Family, this.family.id, 'edit']);
    }
  }

  deactivate(): void {
    this.showConfirmModal = true;
  }

  confirmDeactivate(): void {
    if (!this.family) return;

    this.familyService.deactivateFamily(this.family.id).subscribe({
      next: () => {
        this.family!.isActive = false;
        this.showConfirmModal = false;
        this.toastService.success('Familiar desactivado exitosamente');
      },
      error: (err) => {
        this.showConfirmModal = false;
        if (!err?.errorCode) {
          this.toastService.error('Error al desactivar el familiar');
        }
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
  }

  formatDateTime = formatDateTime;

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Family]);
  }
}
