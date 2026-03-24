import { Component, EventEmitter, Input, Output } from '@angular/core';
import {
  ButtonDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    ButtonDirective,
  ],
  template: `
    <c-modal [visible]="visible" (visibleChange)="visible = $event; cancel.emit()" alignment="center">
      <c-modal-header><h5 cModalTitle>{{ title }}</h5></c-modal-header>
      <c-modal-body>
        <p>{{ messagePrefix }}<strong>{{ itemName }}</strong>{{ messageSuffix }}</p>
        @if (detail) {
          <p class="text-body-secondary mb-0">{{ detail }}</p>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="cancel.emit()">Cancelar</button>
        <button cButton [color]="confirmColor" (click)="confirm.emit()">{{ confirmLabel }}</button>
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
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
}
