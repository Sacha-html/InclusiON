import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CardModule,
  FormModule,
  ButtonModule,
  AlertModule,
  SpinnerModule,
  GridModule,
  BadgeModule
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';
import { AuthService, UpdateLoginMethodApiResponse } from '../../../services/auth.service';
import { LoginMethod, UpdateLoginMethodRequest } from '../../../models';

@Component({
  selector: 'app-login-method-selector',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    FormModule,
    ButtonModule,
    AlertModule,
    SpinnerModule,
    GridModule,
    IconModule,
    BadgeModule
  ],
  templateUrl: './login-method-selector.component.html',
  styleUrls: ['./login-method-selector.component.scss']
})
export class LoginMethodSelectorComponent implements OnInit {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);

  @Input() userId?: string;
  @Input() currentLoginMethodId?: number;
  @Output() methodUpdated = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  loginMethods: LoginMethod[] = [];
  selectedMethod: LoginMethod | null = null;
  configForm!: FormGroup;

  isLoading = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  // Login method IDs
  readonly LOGIN_STANDARD = 1;
  readonly LOGIN_PIN = 2;
  readonly LOGIN_ASSISTED = 5;

  ngOnInit(): void {
    this.loadLoginMethods();
    this.initForm();
  }

  private initForm(): void {
    this.configForm = this.fb.group({
      pin: [''],
      confirmPin: [''],
      supervisorUserId: ['']
    });
  }

  private loadLoginMethods(): void {
    this.isLoading = true;
    this.authService.getLoginMethods().subscribe({
      next: (response) => {
        if (response.success) {
          this.loginMethods = response.data;
          if (this.currentLoginMethodId) {
            this.selectedMethod = this.loginMethods.find(m => m.id === this.currentLoginMethodId) || null;
          }
        } else {
          this.errorMessage = response.message || 'Error al cargar metodos de login';
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.userMessage || 'Error al cargar metodos de login';
        this.isLoading = false;
      }
    });
  }

  selectMethod(method: LoginMethod): void {
    this.selectedMethod = method;
    this.errorMessage = '';
    this.successMessage = '';
    this.configForm.reset();

    // Set up form validators based on method
    if (method.id === this.LOGIN_PIN) {
      this.configForm.get('pin')?.setValidators([
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(6),
        Validators.pattern(/^\d+$/)
      ]);
      this.configForm.get('confirmPin')?.setValidators([Validators.required]);
    } else if (method.id === this.LOGIN_ASSISTED) {
      this.configForm.get('supervisorUserId')?.setValidators([Validators.required]);
    }

    this.configForm.get('pin')?.updateValueAndValidity();
    this.configForm.get('confirmPin')?.updateValueAndValidity();
    this.configForm.get('supervisorUserId')?.updateValueAndValidity();
  }

  getMethodIcon(method: LoginMethod): string {
    switch (method.id) {
      case this.LOGIN_STANDARD:
        return 'cil-lock-locked';
      case this.LOGIN_PIN:
        return 'cil-calculator';
      case this.LOGIN_ASSISTED:
        return 'cil-people';
      default:
        return 'cil-fingerprint';
    }
  }

  getMethodColor(method: LoginMethod): string {
    switch (method.id) {
      case this.LOGIN_STANDARD:
        return 'primary';
      case this.LOGIN_PIN:
        return 'success';
      case this.LOGIN_ASSISTED:
        return 'info';
      default:
        return 'secondary';
    }
  }

  validateAndSave(): void {
    if (!this.selectedMethod) {
      this.errorMessage = 'Por favor selecciona un metodo de login';
      return;
    }

    // Validate PIN confirmation
    if (this.selectedMethod.id === this.LOGIN_PIN) {
      const pin = this.configForm.get('pin')?.value;
      const confirmPin = this.configForm.get('confirmPin')?.value;

      if (!pin) {
        this.errorMessage = 'El PIN es requerido';
        return;
      }

      if (pin !== confirmPin) {
        this.errorMessage = 'Los PINs no coinciden';
        return;
      }

      if (!/^\d{4}$/.test(pin)) {
        this.errorMessage = 'El PIN debe tener 4 dígitos numéricos';
        return;
      }
    }

    // Validate supervisor for assisted login
    if (this.selectedMethod.id === this.LOGIN_ASSISTED) {
      const supervisorUserId = this.configForm.get('supervisorUserId')?.value;
      if (!supervisorUserId) {
        this.errorMessage = 'Debes seleccionar un supervisor';
        return;
      }
    }

    this.saveLoginMethod();
  }

  private saveLoginMethod(): void {
    if (!this.selectedMethod) return;

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: UpdateLoginMethodRequest = {
      loginMethodId: this.selectedMethod.id
    };

    if (this.selectedMethod.id === this.LOGIN_PIN) {
      request.pin = this.configForm.get('pin')?.value;
    }

    if (this.selectedMethod.id === this.LOGIN_ASSISTED) {
      request.supervisorUserId = this.configForm.get('supervisorUserId')?.value;
    }

    const saveObservable = this.userId
      ? this.authService.updateUserLoginMethod(this.userId, request)
      : this.authService.updateMyLoginMethod(request);

    saveObservable.subscribe({
      next: (response: UpdateLoginMethodApiResponse) => {
        if (response.success && response.data?.updated) {
          this.successMessage = `Metodo de login actualizado a: ${response.data.loginMethodName}`;
          this.methodUpdated.emit();
        } else {
          this.errorMessage = response.message || 'Error al actualizar metodo de login';
        }
        this.isSaving = false;
      },
      error: (error) => {
        this.errorMessage = error.userMessage || 'Error al actualizar metodo de login';
        this.isSaving = false;
      }
    });
  }

  cancel(): void {
    this.cancelled.emit();
  }
}
