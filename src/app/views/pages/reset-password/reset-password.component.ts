import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '@services';
import {
  AlertComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  ColComponent,
  ContainerComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  InputGroupComponent,
  RowComponent,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-reset-password',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardComponent,
    CardBodyComponent,
    FormControlDirective,
    FormLabelDirective,
    FormFeedbackComponent,
    InputGroupComponent,
    ButtonDirective,
    AlertComponent,
    IconDirective,
  ],
  templateUrl: './reset-password.component.html',
})
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  submitted = false;
  isLoading = false;
  isSuccess = false;
  serverError = '';
  tokenError = '';
  showNewPassword = false;
  showConfirmPassword = false;

  private token = '';

  form: FormGroup = this.fb.group({
    newPassword: ['', [
      Validators.required,
      Validators.minLength(8),
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$/),
    ]],
    confirmNewPassword: ['', [Validators.required]],
  });

  get f() {
    return this.form.controls;
  }

  get passwordsMismatch(): boolean {
    return this.form.value.newPassword !== this.form.value.confirmNewPassword;
  }

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.token) {
      this.tokenError = 'El enlace de recuperación no es válido. Solicitá uno nuevo.';
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

    if (this.form.invalid || this.passwordsMismatch || !this.token) return;

    this.isLoading = true;

    this.authService.resetPassword({
      token: this.token,
      newPassword: this.form.value.newPassword,
      confirmNewPassword: this.form.value.confirmNewPassword,
    }).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response?.success) {
          this.isSuccess = true;
          setTimeout(() => this.router.navigate(['/']), 3000);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.serverError = err?.userMessage || 'El enlace no es válido o ya fue utilizado. Solicitá uno nuevo.';
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }
}
