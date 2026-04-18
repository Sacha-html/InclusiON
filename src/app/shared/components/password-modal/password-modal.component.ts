import { Component, ElementRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewChild } from '@angular/core';
import {
  ButtonDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-password-modal',
  standalone: true,
  imports: [
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    ButtonDirective,
  ],
  template: `
    <c-modal [visible]="visible" backdrop="static" [keyboard]="true" [attr.aria-labelledby]="'pwd-modal-title'" (visibleChange)="onVisibleChange($event)">
      <c-modal-header><h5 cModalTitle id="pwd-modal-title">{{ entityType }} creado exitosamente</h5></c-modal-header>
      <c-modal-body>
        @if (entityName) {
          <p>Se creo {{ entityArticle }} {{ entityTypeLower }} <strong>{{ entityName }}</strong>.</p>
          <p>Se genero una contraseña temporal para su acceso al sistema:</p>
          <div class="alert alert-warning d-flex align-items-center justify-content-between">
            <code class="fs-5">{{ password }}</code>
            <button cButton color="light" size="sm" (click)="copyPassword()"
                    [attr.aria-label]="copied ? 'Contraseña copiada al portapapeles' : 'Copiar contraseña al portapapeles'">
              <span aria-hidden="true">{{ copied ? 'Copiado!' : 'Copiar' }}</span>
            </button>
          </div>
          <span class="visually-hidden" aria-live="polite">
            {{ copied ? 'Contraseña copiada al portapapeles' : '' }}
          </span>
          <p class="text-body-secondary mb-0">
            Asegurese de copiar esta contraseña. No se podra volver a consultar.
            {{ entityArticleUpper }} {{ entityTypeLower }} debera cambiarla en su primer inicio de sesion.
          </p>
        }
      </c-modal-body>
      <c-modal-footer>
        <button #closeBtn cButton color="primary" (click)="close.emit()">Entendido</button>
      </c-modal-footer>
    </c-modal>
  `,
})
export class PasswordModalComponent implements OnChanges {
  @Input() visible = false;
  @Input() entityType = 'Usuario';
  @Input() entityArticle = 'el';
  @Input() entityName = '';
  @Input() password = '';
  @Output() close = new EventEmitter<void>();

  @ViewChild('closeBtn', { read: ElementRef }) closeBtn?: ElementRef<HTMLButtonElement>;

  copied = false;

  get entityTypeLower(): string { return this.entityType.toLowerCase(); }
  get entityArticleUpper(): string { return this.entityArticle.charAt(0).toUpperCase() + this.entityArticle.slice(1); }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible']?.currentValue === true) {
      setTimeout(() => this.closeBtn?.nativeElement.focus(), 150);
    }
  }

  copyPassword(): void {
    navigator.clipboard.writeText(this.password).then(() => {
      this.copied = true;
      setTimeout(() => this.copied = false, 2000);
    });
  }

  onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.close.emit();
    }
  }
}
