import { ChangeDetectionStrategy, Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
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
import { LoginMethodItem, SupervisorCandidate, UpdateLoginMethodRequest, UpdateLoginMethodResponse } from '@models';
import { PersonsService } from '@services';

const METHOD_STANDARD = 1;
const METHOD_PIN = 2;
const METHOD_ASSISTED = 3;

type Step = 'choose' | 'configure' | 'result';

/**
 * Modal-wizard para cambiar el método de login de una persona con discapacidad.
 * Maneja las 3 ramas: STANDARD (genera contraseña temporal), PIN (4-6 dígitos),
 * ASSISTED (requiere supervisor de la lista de candidatos).
 */
@Component({
  selector: 'app-change-login-method-modal',
  standalone: true,
  imports: [
    FormsModule,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    ButtonDirective, SpinnerComponent,
    FormControlDirective, FormLabelDirective, FormSelectDirective,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  ],
  templateUrl: './change-login-method-modal.component.html',
  styleUrl: './change-login-method-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangeLoginMethodModalComponent implements OnChanges {
  private readonly personsService = inject(PersonsService);

  @Input() visible = false;
  @Input({ required: true }) userId!: string;
  @Input({ required: true }) personId!: string;
  @Input() currentLoginMethodId: number | null = null;
  @Input({ required: true }) loginMethods: LoginMethodItem[] = [];

  @Output() closed = new EventEmitter<void>();
  @Output() updated = new EventEmitter<UpdateLoginMethodResponse>();

  readonly METHOD_STANDARD = METHOD_STANDARD;
  readonly METHOD_PIN = METHOD_PIN;
  readonly METHOD_ASSISTED = METHOD_ASSISTED;

  step: Step = 'choose';
  selectedMethodId: number | null = null;
  pin = '';
  pinConfirm = '';
  supervisorUserId: string | null = null;
  supervisors: SupervisorCandidate[] = [];
  loadingSupervisors = false;
  submitting = false;
  errorMessage = '';
  result: UpdateLoginMethodResponse | null = null;
  passwordRevealed = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible) {
      this.reset();
    }
  }

  get availableMethods(): LoginMethodItem[] {
    return this.loginMethods.filter(m => [METHOD_STANDARD, METHOD_PIN, METHOD_ASSISTED].includes(m.id));
  }

  get selectedMethod(): LoginMethodItem | null {
    return this.availableMethods.find(m => m.id === this.selectedMethodId) ?? null;
  }

  get isCurrentMethod(): boolean {
    return this.selectedMethodId !== null && this.selectedMethodId === this.currentLoginMethodId;
  }

  chooseMethod(id: number): void {
    this.selectedMethodId = id;
    this.errorMessage = '';
  }

  goToConfigure(): void {
    if (this.selectedMethodId === null) {
      this.errorMessage = 'Elegí un método para continuar.';
      return;
    }
    if (this.isCurrentMethod) {
      this.errorMessage = 'Ese ya es el método actual.';
      return;
    }
    this.step = 'configure';
    this.errorMessage = '';

    if (this.selectedMethodId === METHOD_ASSISTED && this.supervisors.length === 0) {
      this.loadSupervisors();
    }
  }

  backToChoose(): void {
    this.step = 'choose';
    this.errorMessage = '';
  }

  private loadSupervisors(): void {
    this.loadingSupervisors = true;
    this.personsService.getSupervisorCandidates(this.personId).subscribe({
      next: (data) => {
        this.supervisors = data;
        this.loadingSupervisors = false;
      },
      error: () => {
        this.loadingSupervisors = false;
        this.errorMessage = 'No se pudieron cargar los supervisores. Intentá de nuevo.';
      },
    });
  }

  canSubmit(): boolean {
    if (this.submitting || this.selectedMethodId === null) return false;
    switch (this.selectedMethodId) {
      case METHOD_PIN:
        return /^\d{4,6}$/.test(this.pin) && this.pin === this.pinConfirm;
      case METHOD_ASSISTED:
        return !!this.supervisorUserId;
      case METHOD_STANDARD:
        return true;
      default:
        return false;
    }
  }

  submit(): void {
    if (!this.canSubmit() || this.selectedMethodId === null) return;

    const request: UpdateLoginMethodRequest = {
      loginMethodId: this.selectedMethodId,
      ...(this.selectedMethodId === METHOD_PIN && { pin: this.pin }),
      ...(this.selectedMethodId === METHOD_ASSISTED && this.supervisorUserId && { supervisorUserId: this.supervisorUserId }),
    };

    this.submitting = true;
    this.errorMessage = '';

    this.personsService.updateLoginMethod(this.userId, request).subscribe({
      next: (response) => {
        this.submitting = false;
        this.result = response;
        this.step = 'result';
        this.updated.emit(response);
      },
      error: (err) => {
        this.submitting = false;
        this.errorMessage = err?.userMessage || 'No se pudo cambiar el método. Intentá de nuevo.';
      },
    });
  }

  copyPasswordToClipboard(): void {
    if (this.result?.temporaryPassword && navigator.clipboard) {
      navigator.clipboard.writeText(this.result.temporaryPassword);
    }
  }

  close(): void {
    this.closed.emit();
  }

  onVisibleChange(value: boolean): void {
    if (!value) {
      this.close();
    }
  }

  private reset(): void {
    this.step = 'choose';
    this.selectedMethodId = null;
    this.pin = '';
    this.pinConfirm = '';
    this.supervisorUserId = null;
    this.submitting = false;
    this.errorMessage = '';
    this.result = null;
    this.passwordRevealed = false;
  }
}
