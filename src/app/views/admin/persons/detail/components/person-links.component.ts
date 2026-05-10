import { Component, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { PersonResponse, PersonRepresentativeResponse, FamilyResponse, PersonRepresentativeHistoryResponse } from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormControlDirective,
  FormLabelDirective,
  FormSelectDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { SearchableSelectComponent } from '@shared/components/searchable-select/searchable-select.component';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn, ActionItem, HeaderButton } from '@shared/components/data-table/data-table.models';

@Component({
  selector: 'app-person-links',
  standalone: true,
  imports: [
    DatePipe,
    DataTableComponent,
    BadgeComponent,
    ButtonDirective,
    FormControlDirective,
    FormSelectDirective,
    FormLabelDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    SpinnerComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    FormsModule,
    ReactiveFormsModule,
    SearchableSelectComponent,
  ],
  templateUrl: './person-links.component.html',
})
export class PersonLinksComponent implements OnChanges {
  @Input({ required: true }) person!: PersonResponse;
  @Input() representatives: PersonRepresentativeResponse[] = [];
  @Input() loading = false;
  @Input() canLink = false;
  @Input() canUnlink = false;
  @Input() canViewHistory = false;

  // Link modal
  @Input() showLinkModal = false;
  @Input() linkError = '';
  @Input() linking = false;

  // Unlink modal
  @Input() showUnlinkModal = false;
  @Input() unlinkingRepresentative: PersonRepresentativeResponse | null = null;
  @Input() unlinking = false;

  // History modal
  @Input() showHistoryModal = false;
  @Input() linkHistory: PersonRepresentativeHistoryResponse[] = [];
  @Input() loadingHistory = false;

  @Input({ required: true }) searchFamilyFn!: (query: string) => Observable<FamilyResponse[]>;

  @Output() openLinkModal = new EventEmitter<void>();
  @Output() closeLinkModal = new EventEmitter<void>();
  @Output() confirmLink = new EventEmitter<{ familyId: string; relationship: string; isPrimary: boolean }>();

  @Output() unlink = new EventEmitter<PersonRepresentativeResponse>();
  @Output() closeUnlinkModal = new EventEmitter<void>();
  @Output() confirmUnlink = new EventEmitter<string>();

  @Output() openHistoryModal = new EventEmitter<void>();
  @Output() closeHistoryModal = new EventEmitter<void>();

  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];
  readonly PARENT_RELATIONSHIPS = ['Madre', 'Padre'];

  selectedFamilyControl = new FormControl<string | null>(null);
  readonly displayFamilyFn = (f: FamilyResponse) => f.fullName ?? '';
  readonly subDisplayFamilyFn = (f: FamilyResponse) => f.email ?? '';
  readonly valueFamilyFn = (f: FamilyResponse) => f.id;

  selectedRelationship = '';
  selectedIsPrimary = false;
  unlinkObservation = '';

  get tableColumns(): TableColumn[] {
    const cols: TableColumn[] = [
      { key: 'representativeFullName', label: 'Nombre' },
      { key: 'relationship',           label: 'Relación' },
      {
        key: 'isPrimary', label: 'Principal', type: 'badge',
        badgeMap: {
          'true':  { color: 'primary',   label: 'Sí' },
          'false': { color: 'secondary', label: 'No' },
        },
      },
      {
        key: 'isActive', label: 'Estado', type: 'badge',
        badgeMap: {
          'true':  { color: 'success',   label: 'Activo'   },
          'false': { color: 'secondary', label: 'Inactivo' },
        },
      },
    ];

    if (this.person?.isActive && this.canUnlink) {
      cols.push({
        key: '', label: 'Acciones', type: 'actions',
        actions: [
          {
            action: 'unlink', label: 'Desvincular', icon: 'cil-trash',
            visible: (item: PersonRepresentativeResponse) => item.isActive,
          },
        ],
      });
    }
    return cols;
  }

  get repHeaderButtons(): HeaderButton[] {
    const buttons: HeaderButton[] = [];
    if (this.person?.isActive && this.canLink) {
      buttons.push({ action: 'link', label: 'Vincular Familiar', color: 'primary' });
    }
    if (this.canViewHistory) {
      buttons.push({ action: 'history', label: 'Ver historial', color: 'secondary' });
    }
    return buttons;
  }

  onRowAction(event: { action: string; item: PersonRepresentativeResponse }): void {
    if (event.action === 'unlink') this.unlink.emit(event.item);
  }

  onHeaderAction(action: string): void {
    if (action === 'link') this.openLinkModal.emit();
    if (action === 'history') this.openHistoryModal.emit();
  }

  ngOnChanges(): void {
    if (!this.showLinkModal) {
      this.selectedFamilyControl.setValue(null);
      this.selectedRelationship = '';
      this.selectedIsPrimary = false;
    }
    if (!this.showUnlinkModal) {
      this.unlinkObservation = '';
    }
  }

  validateParentLimit(): string {
    if (!this.selectedRelationship) return '';

    const isParent = this.PARENT_RELATIONSHIPS.includes(this.selectedRelationship);
    if (!isParent) return '';

    const existingParent = this.representatives.find(
      r => r.isActive && r.relationship === this.selectedRelationship
    );

    if (existingParent) {
      return `Ya existe un familiar vinculado con la relación "${this.selectedRelationship}". Solo puede haber una.`;
    }

    return '';
  }
}
