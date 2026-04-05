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
  template: `
    <h5 class="mb-3">Familiares Vinculados</h5>

    @if (loading) {
      <div class="text-center py-3">
        <c-spinner></c-spinner>
      </div>
    } @else if (representatives.length === 0) {
      <p class="text-body-secondary">No hay familiares vinculados a esta persona.</p>
    } @else {
      <table cTable hover responsive class="mb-3">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Relacion</th>
            <th>Principal</th>
            <th>Estado</th>
            @if (person.isActive && canUnlink) {
              <th>Acciones</th>
            }
          </tr>
        </thead>
        <tbody>
          @for (rep of representatives; track rep.representativeId) {
            <tr>
              <td>{{ rep.representativeFullName }}</td>
              <td>{{ rep.relationship ?? '—' }}</td>
              <td>{{ rep.isPrimary ? 'Si' : 'No' }}</td>
              <td>
                <c-badge [color]="rep.isActive ? 'success' : 'secondary'">
                  {{ rep.isActive ? 'Activo' : 'Inactivo' }}
                </c-badge>
              </td>
              @if (person.isActive && canUnlink && rep.isActive) {
                <td>
                  <button cButton color="danger" size="sm" (click)="unlink.emit(rep)"
                          [attr.aria-label]="'Desvincular a ' + rep.representativeFullName">
                    Desvincular
                  </button>
                </td>
              }
            </tr>
          }
        </tbody>
      </table>
    }

    @if (person.isActive && canLink) {
      <button cButton color="primary" (click)="openLinkModal.emit()" class="mt-2">
        Vincular Familiar
      </button>
    }

    @if (canViewHistory) {
      <button cButton color="outline-secondary" size="sm" (click)="openHistoryModal.emit()" class="mt-2 ms-2">
        Ver historial
      </button>
    }

    <!-- Modal Historial de Vinculos -->
    <c-modal [visible]="showLinkModal" (visibleChange)="!$event && closeLinkModal.emit()"
             aria-labelledby="link-family-modal-title" [size]="'lg'">
      <c-modal-header>
        <strong id="link-family-modal-title">Vincular Familiar</strong>
      </c-modal-header>
      <c-modal-body>
        @if (linkError) {
          <div class="alert alert-danger">{{ linkError }}</div>
        }

        <div class="mb-3">
          <label cLabel for="search-family">Buscar familiar por nombre</label>
          <input cFormControl id="search-family" 
                 [(ngModel)]="searchFamily" 
                 (ngModelChange)="searchChange.emit($event)"
                 placeholder="Escriba el nombre..." />
        </div>

        @if (loadingFamilies) {
          <div class="text-center py-3">
            <c-spinner></c-spinner>
          </div>
        } @else if (availableFamilies.length === 0) {
          <p class="text-body-secondary">No hay familiares disponibles para vincular.</p>
        } @else {
          <p class="text-body-secondary mb-2">Seleccione un familiar:</p>
          <div class="list-group" style="max-height: 200px; overflow-y: auto;">
            @for (family of availableFamilies; track family.id) {
              <button type="button" class="list-group-item list-group-item-action"
                      [class.active]="selectedFamilyId === family.id"
                      (click)="selectedFamilyId = family.id">
                <strong>{{ family.fullName }}</strong>
                @if (family.email) {
                  <br /><small class="text-body-secondary">{{ family.email }}</small>
                }
              </button>
            }
          </div>
        }

        @if (selectedFamilyId) {
          <hr class="my-3" />

          <div class="mb-3">
            <label cLabel for="link-relationship">Relacion</label>
            <select cFormControl id="link-relationship" [(ngModel)]="selectedRelationship">
              <option value="">Seleccione...</option>
              @for (rel of relationships; track rel) {
                <option [value]="rel">{{ rel }}</option>
              }
            </select>
          </div>

          @if (validateParentLimit()) {
            <div class="alert alert-warning">{{ validateParentLimit() }}</div>
          }

          <div class="mb-3">
            <c-form-check>
              <input cFormCheckInput type="checkbox" id="link-is-primary" 
                     [(ngModel)]="selectedIsPrimary" />
              <label cFormCheckLabel for="link-is-primary">Familiar principal</label>
            </c-form-check>
          </div>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="closeLinkModal.emit()">Cancelar</button>
        <button cButton color="primary" (click)="confirmLink.emit({ familyId: selectedFamilyId, relationship: selectedRelationship, isPrimary: selectedIsPrimary })"
                [disabled]="!selectedFamilyId || !selectedRelationship || !!validateParentLimit() || linking">
          @if (linking) { <c-spinner size="sm" class="me-1"></c-spinner> }
          Vincular
        </button>
      </c-modal-footer>
    </c-modal>

    <!-- Modal Desvincular Familiar -->
    <c-modal [visible]="showUnlinkModal" (visibleChange)="!$event && closeUnlinkModal.emit()"
             aria-labelledby="unlink-family-modal-title">
      <c-modal-header>
        <strong id="unlink-family-modal-title">Desvincular Familiar</strong>
      </c-modal-header>
      <c-modal-body>
        <p>Esta seguro de que desea desvincular a <strong>{{ unlinkingRepresentative?.representativeFullName }}</strong> de esta persona?</p>
        
        <div class="mb-3">
          <label cLabel for="unlink-observation">Motivo de desvinculacion (requerido)</label>
          <textarea cFormControl id="unlink-observation" 
                    [(ngModel)]="unlinkObservation"
                    rows="3"
                    placeholder="Indique el motivo..."
                    required></textarea>
        </div>
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="closeUnlinkModal.emit()">Cancelar</button>
        <button cButton color="danger" (click)="confirmUnlink.emit(unlinkObservation)"
                [disabled]="!unlinkObservation.trim() || unlinking">
          @if (unlinking) { <c-spinner size="sm" class="me-1"></c-spinner> }
          Desvincular
        </button>
      </c-modal-footer>
    </c-modal>

    <!-- Modal Historial de Vinculos -->
    <c-modal [visible]="showHistoryModal" (visibleChange)="!$event && closeHistoryModal.emit()"
             aria-labelledby="history-modal-title" size="lg">
      <c-modal-header>
        <strong id="history-modal-title">Historial de Vinculaciones</strong>
      </c-modal-header>
      <c-modal-body>
        @if (loadingHistory) {
          <div class="text-center py-3">
            <c-spinner></c-spinner>
          </div>
        } @else if (linkHistory.length === 0) {
          <p class="text-body-secondary">No hay historial de vinculaciones.</p>
        } @else {
          <table cTable hover>
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Familiar</th>
                <th>Accion</th>
                <th>Relacion</th>
                <th>Motivo</th>
              </tr>
            </thead>
            <tbody>
              @for (h of linkHistory; track h.id) {
                <tr>
                  <td>{{ h.createdAt | date:'dd/MM/yyyy HH:mm' }}</td>
                  <td>{{ h.familyFullName }}</td>
                  <td>
                    <c-badge [color]="h.action === 'Linked' ? 'success' : 'danger'">
                      {{ h.action === 'Linked' ? 'Vinculado' : 'Desvinculado' }}
                    </c-badge>
                  </td>
                  <td>{{ h.relationship ?? '—' }}</td>
                  <td>{{ h.observation ?? '—' }}</td>
                </tr>
              }
            </tbody>
          </table>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="closeHistoryModal.emit()">Cerrar</button>
      </c-modal-footer>
    </c-modal>
  `,
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
