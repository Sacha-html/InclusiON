import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GridModule } from '@coreui/angular';
import { FamilyService, AuthService, ToastService } from '@services';
import { Permissions } from '@shared/constants/permissions';
import {
  PersonRepresentativeResponse,
  FamilyResponse,
} from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormControlDirective,
  FormLabelDirective,
  FormSelectDirective,
  SpinnerComponent,
  AlertComponent,
} from '@coreui/angular';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn, HeaderButton } from '@shared/components/data-table/data-table.models';

@Component({
  selector: 'app-professional-family-tab',
  standalone: true,
  imports: [
    FormsModule,
    GridModule,
    BadgeComponent,
    ButtonDirective,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    FormControlDirective,
    FormLabelDirective,
    FormSelectDirective,
    SpinnerComponent,
    AlertComponent,
    DataTableComponent,
  ],
  templateUrl: './professional-family-tab.component.html',
  styleUrl: './professional-family-tab.component.scss',
})
export class ProfessionalFamilyTabComponent {
  private readonly familyService = inject(FamilyService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);

  @Input() personId: string = '';
  @Input() representatives: PersonRepresentativeResponse[] = [];
  @Input() loading = false;

  @Output() refresh = new EventEmitter<void>();

  canLinkFamily = this.authService.hasPermission(Permissions.Professionals.LinkFamily) || this.authService.hasPermission(Permissions.Family.Link);
  canUnlinkFamily = this.authService.hasPermission(Permissions.Professionals.UnlinkFamily) || this.authService.hasPermission(Permissions.Family.Unlink);

  familyCols: TableColumn[] = [
    { key: 'representativeFullName', label: 'Nombre' },
    { key: 'relationship', label: 'Relación' },
    {
      key: 'isPrimary',
      label: 'Principal',
      type: 'badge',
      badgeMap: { 'true': { color: 'success', label: 'Sí' }, 'false': { color: 'secondary', label: 'No' } }
    },
    {
      key: 'isActive',
      label: 'Estado',
      type: 'badge',
      badgeMap: { 'true': { color: 'success', label: 'Activo' }, 'false': { color: 'danger', label: 'Inactivo' } }
    },
    {
      key: 'actions',
      label: '',
      type: 'actions',
      actions: [
        { action: 'unlink', label: 'Desvincular', icon: 'cil-x', visible: (item) => this.canUnlinkFamily && item.isActive },
      ],
    },
  ];

  get familyHeaderButtons(): HeaderButton[] {
    return this.canLinkFamily
      ? [{ action: 'link', label: 'Vincular', icon: 'cil-plus', color: 'primary' }]
      : [];
  }

  showLinkModal = false;
  availableFamilies: FamilyResponse[] = [];
  loadingFamilies = false;
  searchFamily = '';
  confirmingLink = false;
  selectedFamilyId = '';
  selectedFamilyIsPreviouslyLinked = false;
  linkRelationship = '';
  linkIsPrimary = false;
  showConfirmLinkModal = false;

  showUnlinkModal = false;
  unlinkingRepresentative: PersonRepresentativeResponse | null = null;
  unlinkObservation = '';
  unlinking = false;

  openLinkModal(): void {
    this.showLinkModal = true;
    this.selectedFamilyId = '';
    this.selectedFamilyIsPreviouslyLinked = false;
    this.linkRelationship = '';
    this.linkIsPrimary = false;
    this.loadAvailableFamilies();
  }

  closeLinkModal(): void {
    this.showLinkModal = false;
    this.selectedFamilyId = '';
    this.selectedFamilyIsPreviouslyLinked = false;
    this.searchFamily = '';
    this.availableFamilies = [];
  }

  private loadAvailableFamilies(): void {
    this.loadingFamilies = true;
    this.familyService.getAvailableFamiliesForProfessional(this.searchFamily || undefined, this.personId).subscribe({
      next: (data) => {
        this.availableFamilies = data;
        this.loadingFamilies = false;
      },
      error: () => {
        this.loadingFamilies = false;
        this.toastService.error('Error al cargar familiares disponibles');
      },
    });
  }

  onSearchFamily(): void {
    this.loadAvailableFamilies();
  }

  onSelectFamily(family: FamilyResponse): void {
    this.selectedFamilyId = family.id;
    this.selectedFamilyIsPreviouslyLinked = family.wasPreviouslyLinked === true;
    this.linkRelationship = '';
    this.linkIsPrimary = false;
  }

  confirmLink(): void {
    if (!this.selectedFamilyId) return;

    if (this.selectedFamilyIsPreviouslyLinked) {
      this.showConfirmLinkModal = true;
    } else {
      this.doLink();
    }
  }

  doLink(): void {
    if (!this.personId || !this.selectedFamilyId) return;
    this.showConfirmLinkModal = false;
    this.confirmingLink = true;

    this.familyService.linkFamilyToPersonAsProfessional(this.selectedFamilyId, this.personId, {
      relationship: this.linkRelationship,
      isPrimary: this.linkIsPrimary,
    }).subscribe({
      next: () => {
        this.toastService.success('Familiar vinculado exitosamente');
        this.confirmingLink = false;
        this.closeLinkModal();
        this.refresh.emit();
      },
      error: (err) => {
        this.confirmingLink = false;
        if (err?.status === 409) return;
        const message = err?.userMessage ?? 'Error al vincular familiar';
        this.toastService.error(message);
      },
    });
  }

  onRowAction(event: { action: string; item: PersonRepresentativeResponse }): void {
    if (event.action === 'unlink') {
      this.openUnlinkModal(event.item);
    }
  }

  onHeaderAction(action: string): void {
    if (action === 'link') {
      this.openLinkModal();
    }
  }

  openUnlinkModal(rep: PersonRepresentativeResponse): void {
    this.unlinkingRepresentative = rep;
    this.showUnlinkModal = true;
  }

  closeUnlinkModal(): void {
    this.showUnlinkModal = false;
    this.unlinkingRepresentative = null;
    this.unlinkObservation = '';
  }

  confirmUnlink(): void {
    if (!this.personId || !this.unlinkingRepresentative) return;
    this.unlinking = true;

    this.familyService.unlinkFamilyFromPersonAsProfessional(
      this.unlinkingRepresentative.representativeId,
      this.personId,
      this.unlinkObservation.trim()
    ).subscribe({
      next: () => {
        this.toastService.success('Familiar desvinculado exitosamente');
        this.unlinking = false;
        this.closeUnlinkModal();
        this.refresh.emit();
      },
      error: (err) => {
        this.unlinking = false;
        const message = err?.userMessage ?? 'Error al desvincular familiar';
        this.toastService.error(message);
      },
    });
  }
}
