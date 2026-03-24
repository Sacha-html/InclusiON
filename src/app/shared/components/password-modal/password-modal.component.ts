import { Component, EventEmitter, Input, Output } from '@angular/core';
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
    <c-modal [visible]="visible" backdrop="static" [keyboard]="false">
      <c-modal-header><strong>{{ entityType }} creado exitosamente</strong></c-modal-header>
      <c-modal-body>
        @if (entityName) {
          <p>Se creo {{ entityArticle }} {{ entityTypeLower }} <strong>{{ entityName }}</strong>.</p>
          <p>Se genero una contraseña temporal para su acceso al sistema:</p>
          <div class="alert alert-warning d-flex align-items-center justify-content-between">
            <code class="fs-5">{{ password }}</code>
            <button cButton color="light" size="sm" (click)="copyPassword()">
              {{ copied ? 'Copiado!' : 'Copiar' }}
            </button>
          </div>
          <p class="text-body-secondary mb-0">
            Asegurese de copiar esta contraseña. No se podra volver a consultar.
            {{ entityArticleUpper }} {{ entityTypeLower }} debera cambiarla en su primer inicio de sesion.
          </p>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="primary" (click)="close.emit()">Entendido</button>
      </c-modal-footer>
    </c-modal>
  `,
})
export class PasswordModalComponent {
  @Input() visible = false;
  @Input() entityType = 'Usuario';
  @Input() entityArticle = 'el';
  @Input() entityName = '';
  @Input() password = '';
  @Output() close = new EventEmitter<void>();

  copied = false;

  get entityTypeLower(): string { return this.entityType.toLowerCase(); }
  get entityArticleUpper(): string { return this.entityArticle.charAt(0).toUpperCase() + this.entityArticle.slice(1); }

  copyPassword(): void {
    navigator.clipboard.writeText(this.password).then(() => {
      this.copied = true;
      setTimeout(() => this.copied = false, 2000);
    });
  }
}
