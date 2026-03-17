import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, ErrorCodeService } from '@services';
import { ChangePasswordRequest } from '@models';
import {
  AlertComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  ContainerComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  RowComponent,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-change-password',
  imports: [
    ReactiveFormsModule,
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    FormControlDirective,
    FormLabelDirective,
    FormFeedbackComponent,
    InputGroupComponent,
    ButtonDirective,
    AlertComponent,
    IconDirective,
  ],
  templateUrl: './change-password.component.html',
})
export class ChangePasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly errorCodeService = inject(ErrorCodeService);

  submitted = false;
  isLoading = false;
  serverError = '';
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  form: FormGroup = this.fb.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(8),
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$/)]],
    confirmNewPassword: ['', [Validators.required]],
  });

  get f() {
    return this.form.controls;
  }

  get passwordsMismatch(): boolean {
    return this.form.value.newPassword !== this.form.value.confirmNewPassword;
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

    if (this.form.invalid || this.passwordsMismatch) return;

    this.isLoading = true;

    const request: ChangePasswordRequest = this.form.value;

    this.authService.changePassword(request).subscribe({
      next: (response) => {
        if (response?.success) {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        if (err.errorCode !== undefined) {
          this.serverError = this.errorCodeService.getFullMessage(err.errorCode);
        } else {
          this.serverError = err?.error?.message || 'Error al cambiar la contraseña';
        }
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }
}
