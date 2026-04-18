import { Component, EventEmitter, Input, Output, ViewChild, ElementRef } from '@angular/core';
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
  templateUrl: './confirm-modal.component.html',
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

  @ViewChild('confirmBtn') confirmBtn!: ElementRef<HTMLButtonElement>;
  private lastFocusedElement: HTMLElement | null = null;

  observation = '';

  onConfirm(): void {
    this.confirm.emit(this.observation);
  }

  onVisibleChange(visible: boolean): void {
    this.visible = visible;
    if (visible) {
      this.lastFocusedElement = document.activeElement as HTMLElement;
      setTimeout(() => this.confirmBtn?.nativeElement.focus(), 100);
    } else {
      this.observation = '';
      this.cancel.emit();
      this.lastFocusedElement?.focus();
    }
  }
}
