import { Component, OnInit, inject } from '@angular/core';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { InvitationsService, ToastService, ErrorCodeService, AuthService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { InvitationValidationResponse, AcceptInvitationRequest } from '@models';

import {
  ContainerComponent,
  RowComponent,
  ColComponent,
  CardComponent,
  CardBodyComponent,
  CardHeaderComponent,
  AlertComponent,
  FormControlDirective,
  ButtonDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  SpinnerComponent,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-register-by-invitation',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    AlertComponent,
    FormControlDirective,
    ButtonDirective,
    InputGroupComponent,
    InputGroupTextDirective,
    SpinnerComponent,
    IconDirective,
  ],
  templateUrl: './register-by-invitation.component.html',
  styleUrl: './register-by-invitation.component.scss',
})
export class RegisterByInvitationComponent implements OnInit {
  private fb = inject(FormBuilder);
  private invitationsService = inject(InvitationsService);
  private toastService = inject(ToastService);
  private errorCodeService = inject(ErrorCodeService);
  private authService = inject(AuthService);
  readonly router = inject(Router);
  private route = inject(ActivatedRoute);

  registerForm!: FormGroup;
  invitationData: InvitationValidationResponse | null = null;
  isLoading = true;
  isSubmitting = false;
  registrationComplete = false;
  errorMessage = '';
  showPassword = false;
  showConfirmPassword = false;

  ngOnInit(): void {
    const code = this.route.snapshot.paramMap.get('code');

    if (!code) {
      this.errorMessage = 'Codigo de invitacion no proporcionado';
      this.isLoading = false;
      return;
    }

    this.invitationsService.validateCode(code).subscribe({
      next: (data) => {
        this.invitationData = data;
        this.initForm(data);
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        if (error.errorCode !== undefined) {
          this.errorMessage = this.errorCodeService.getFullMessage(error.errorCode);
        } else {
          this.errorMessage = error.userMessage || 'La invitacion no es valida';
        }
      },
    });
  }

  private initForm(data: InvitationValidationResponse): void {
    this.registerForm = this.fb.group({
      firstName: [{ value: data.firstName || '', disabled: true }],
      lastName: [{ value: data.lastName || '', disabled: true }],
      relationship: [{ value: data.relationship || '', disabled: true }],
      email: [data.email, [Validators.required, Validators.email]],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$/),
      ]],
      confirmPassword: ['', [Validators.required]],
    });
  }

  get f() {
    return this.registerForm.controls;
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit(): void {
    this.errorMessage = '';

    if (this.registerForm.invalid) {
      Object.keys(this.registerForm.controls).forEach((key) => {
        this.registerForm.get(key)?.markAsTouched();
      });
      return;
    }

    const values = this.registerForm.getRawValue();

    if (values.password !== values.confirmPassword) {
      this.errorMessage = 'Las contraseñas no coinciden';
      return;
    }

    this.isSubmitting = true;

    const request: AcceptInvitationRequest = {
      email: values.email.trim(),
      password: values.password,
      confirmPassword: values.confirmPassword,
    };

    this.invitationsService.accept(this.invitationData!.code, request).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.toastService.success('Registro completado exitosamente');

        // Si hay un usuario logueado, mostrar mensaje de exito sin redirigir
        if (this.authService.getCurrentUser()) {
          this.registrationComplete = true;
        } else {
          this.router.navigate([AppRoutes.Login]);
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        if (error.errorCode !== undefined) {
          this.errorMessage = this.errorCodeService.getFullMessage(error.errorCode);
        } else {
          this.errorMessage = error.userMessage || 'Error al completar el registro';
        }
      },
    });
  }

  hasError(fieldName: string, errorType: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field?.hasError(errorType) && field?.touched);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);
    if (!field?.touched) return '';

    if (field.hasError('required')) return 'Este campo es requerido';
    if (field.hasError('email')) return 'Ingresa un email valido';
    if (field.hasError('minlength')) {
      const min = field.getError('minlength').requiredLength;
      return `Minimo ${min} caracteres`;
    }
    if (field.hasError('pattern')) return 'Debe contener al menos una mayuscula, una minuscula, un numero y un caracter especial';
    return '';
  }
}
