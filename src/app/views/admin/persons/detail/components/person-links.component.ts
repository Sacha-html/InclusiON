import { Component, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PersonResponse, PersonRepresentativeResponse, FamilyResponse } from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormControlDirective,
  FormLabelDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
  TableDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-person-links',
  standalone: true,
  imports: [
    CommonModule,
    BadgeComponent,
    ButtonDirective,
    FormControlDirective,
    FormLabelDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    SpinnerComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    TableDirective,
    FormsModule,
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
  @Input() availableFamilies: FamilyResponse[] = [];
  @Input() loadingFamilies = false;
  @Input() linkError = '';
  @Input() linking = false;

  // Unlink modal
  @Input() showUnlinkModal = false;
  @Input() unlinkingRepresentative: PersonRepresentativeResponse | null = null;
  @Input() unlinking = false;

  // History modal
  @Input() showHistoryModal = false;
  @Input() linkHistory: any[] = [];
  @Input() loadingHistory = false;

  @Output() openLinkModal = new EventEmitter<void>();
  @Output() closeLinkModal = new EventEmitter<void>();
  @Output() searchChange = new EventEmitter<string>();
  @Output() confirmLink = new EventEmitter<{ familyId: string; relationship: string; isPrimary: boolean }>();

  @Output() unlink = new EventEmitter<PersonRepresentativeResponse>();
  @Output() closeUnlinkModal = new EventEmitter<void>();
  @Output() confirmUnlink = new EventEmitter<string>();

  @Output() openHistoryModal = new EventEmitter<void>();
  @Output() closeHistoryModal = new EventEmitter<void>();

  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];
  readonly PARENT_RELATIONSHIPS = ['Madre', 'Padre'];

  searchFamily = '';
  selectedFamilyId = '';
  selectedRelationship = '';
  selectedIsPrimary = false;
  unlinkObservation = '';

  ngOnChanges(): void {
    if (!this.showLinkModal) {
      this.searchFamily = '';
      this.selectedFamilyId = '';
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
