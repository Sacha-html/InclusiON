import { Component, EventEmitter, Input, Output } from '@angular/core';
import {
  ButtonDirective,
  FormControlDirective,
  FormLabelDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    ButtonDirective, FormControlDirective, FormLabelDirective, FormsModule, SpinnerComponent,
  ],
  template: `
    <c-modal [visible]="visible" (visibleChange)="onVisibleChange($event)" alignment="center">
      <c-modal-header><h5 cModalTitle>{{ title }}</h5></c-modal-header>
      <c-modal-body>
        <p>{{ messagePrefix }}<strong>{{ itemName }}</strong>{{ messageSuffix }}</p>
        @if (detail) {
          <p class="text-body-secondary mb-0">{{ detail }}</p>
        }
        @if (showObservation) {
          <div class="mt-3">
            <label cLabel for="obs">{{ observationLabel }}</label>
            <textarea cFormControl id="obs" [(ngModel)]="observation" rows="3"
                      [placeholder]="observationPlaceholder"></textarea>
          </div>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="cancel.emit()" [disabled]="loading">Cancelar</button>
        <button cButton [color]="confirmColor" (click)="onConfirm()" [disabled]="(showObservation && !observation.trim()) || loading">
          @if (loading) {
            <c-spinner size="sm" class="me-1"></c-spinner>
          }
          {{ confirmLabel }}
        </button>
      </c-modal-footer>
    </c-modal>
  `,
})
export class ConfirmModalComponent {
  @Input() visible = false;
  @Input() title = 'Confirmar accion';
  @Input() messagePrefix = '¿Esta seguro de que desea realizar esta accion sobre ';
  @Input() itemName = '';
  @Input() messageSuffix = '?';
  @Input() detail = '';
  @Input() confirmLabel = 'Confirmar';
  @Input() confirmColor = 'danger';
  @Input() showObservation = false;
  @Input() observationLabel = 'Observación';
  @Input() observationPlaceholder = '';
  @Input() loading = false;
  @Output() confirm = new EventEmitter<string>();
  @Output() cancel = new EventEmitter<void>();

  observation = '';

  onConfirm(): void {
    this.confirm.emit(this.observation);
  }

  onVisibleChange(visible: boolean): void {
    this.visible = visible;
    if (!visible) {
      this.observation = '';
      this.cancel.emit();
    }
  }
}
