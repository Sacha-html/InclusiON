import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, ErrorCodeService } from '@services';
import { RegisterUserRequest, ErrorCode } from '@models';

// CoreUI imports
import { ContainerComponent, RowComponent, ColComponent, CardComponent, CardBodyComponent } from '@coreui/angular';
import { FormControlDirective, FormLabelDirective, FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective, ButtonDirective, InputGroupComponent, InputGroupTextDirective } from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardComponent,
    CardBodyComponent,
    FormControlDirective,
    FormLabelDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    ButtonDirective,
    InputGroupComponent,
    InputGroupTextDirective,
    IconDirective
  ],
  templateUrl: './register.component.html',
  // styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private errorCodeService = inject(ErrorCodeService);
  private router = inject(Router);

  registerForm!: FormGroup;
  isLoading = false;
  errorMessage = '';
  showPassword = false;
  showConfirmPassword = false;

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      surname: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        this.passwordStrengthValidator
      ]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, [Validators.requiredTrue]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  /**
   * Getter para acceso fácil a los campos
   */
  get f() {
    return this.registerForm.controls;
  }

  /**
   * Validador personalizado para la fortaleza de la contraseña
   */
  private passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;

    if (!value) {
      return null;
    }

    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumeric = /[0-9]/.test(value);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(value);

    const passwordValid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecialChar;

    return !passwordValid ? { passwordStrength: true } : null;
  }

  /**
   * Validador para verificar que las contraseñas coincidan
   */
  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  /**
   * Alternar visibilidad de la contraseña
   */
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  /**
   * Enviar formulario de registro
   */
  onSubmit(): void {
    this.errorMessage = '';

    // Validar formulario
    if (this.registerForm.invalid) {
      this.markFormGroupTouched(this.registerForm);
      return;
    }

    this.isLoading = true;

    // Preparar datos de registro
    const registerData: RegisterUserRequest = {
      email: this.registerForm.value.email.trim(),
      password: this.registerForm.value.password,
      confirmPassword: this.registerForm.value.confirmPassword,
      name: this.registerForm.value.name.trim(),
      surname: this.registerForm.value.surname.trim(),
      acceptTerms: this.registerForm.value.acceptTerms
    };

    // Llamar al servicio de autenticación
    this.authService.register(registerData).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.isLoading = false;

        if (error.errorCode !== undefined) {
          // Mensaje específico para email duplicado
          if (error.errorCode === ErrorCode.EmailAlreadyExists) {
            this.errorMessage = this.errorCodeService.getFullMessage(error.errorCode);
          } else {
            this.errorMessage = this.errorCodeService.getMessage(error.errorCode);
          }
        } else {
          this.errorMessage = error.userMessage
            || 'Error al registrar usuario. Por favor, intenta nuevamente.';
        }
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  /**
   * Marcar todos los campos como touched
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  /**
   * Verificar si un campo tiene error
   */
  hasError(fieldName: string, errorType: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field?.hasError(errorType) && field?.touched);
  }

  /**
   * Obtener mensaje de error para un campo
   */
  getErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);

    if (!field?.touched) {
      return '';
    }

    if (field.hasError('required')) {
      return 'Este campo es requerido';
    }

    if (field.hasError('email')) {
      return 'Ingresa un email válido';
    }

    if (field.hasError('minlength')) {
      const minLength = field.getError('minlength').requiredLength;
      return `Mínimo ${minLength} caracteres`;
    }

    if (field.hasError('passwordStrength')) {
      return 'La contraseña debe contener mayúsculas, minúsculas, números y caracteres especiales';
    }

    return '';
  }

  /**
   * Verificar si las contraseñas coinciden
   */
  get passwordsMatch(): boolean {
    return !this.registerForm.hasError('passwordMismatch');
  }

  /**
   * Obtener el nivel de fortaleza de la contraseña (0-4)
   */
  get passwordStrength(): number {
    const password = this.registerForm.get('password')?.value || '';
    let strength = 0;

    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) strength++;

    return Math.min(strength, 4);
  }

  /**
   * Obtener el color del indicador de fortaleza
   */
  get passwordStrengthColor(): string {
    const strength = this.passwordStrength;
    if (strength === 0) return 'danger';
    if (strength === 1) return 'danger';
    if (strength === 2) return 'warning';
    if (strength === 3) return 'info';
    return 'success';
  }

  /**
   * Obtener el texto del indicador de fortaleza
   */
  get passwordStrengthText(): string {
    const strength = this.passwordStrength;
    if (strength === 0) return 'Muy débil';
    if (strength === 1) return 'Débil';
    if (strength === 2) return 'Regular';
    if (strength === 3) return 'Buena';
    return 'Fuerte';
  }
}