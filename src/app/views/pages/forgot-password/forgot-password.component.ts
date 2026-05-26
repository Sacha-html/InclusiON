import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, ToastService } from '@services';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
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
  selector: 'app-forgot-password',
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
    InputGroupTextDirective,
    ButtonDirective,
    IconDirective,
  ],
  templateUrl: './forgot-password.component.html',
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  submitted = false;
  isLoading = false;

  form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  get f() {
    return this.form.controls;
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) return;

    this.isLoading = true;

    this.authService.forgotPassword(this.form.value.email).subscribe({
      next: () => {
        this.isLoading = false;
        this.toastService.success('Si el email está registrado, recibirás un enlace en los próximos minutos.');
        this.form.reset();
        this.submitted = false;
        this.router.navigate(['/admin-login']);
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('No se pudo enviar el correo. Verificá tu conexión e intentá de nuevo.');
        this.form.reset();
        this.submitted = false;
      },
    });
  }
}
