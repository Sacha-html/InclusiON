import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { filter, take } from 'rxjs';
import { AdminUsersService, UpdateAdminUserRequest } from '@services/admin-users.service';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { ValidationService } from '@services/validation.service';
import { AppRoutes } from '@shared/constants/app-routes';
import { uniqueEmailValidator } from '@shared/utils';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  RowComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-edit-admin',
  imports: [
    ReactiveFormsModule,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    FormFeedbackComponent,
    ButtonDirective,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly validationService = inject(ValidationService);

  submitted = false;
  serverError = '';
  userId = '';
  initialEmail = '';

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    surname: ['', [Validators.required, Validators.maxLength(100)]],
    email: [
      '',
      [Validators.required, Validators.email, Validators.maxLength(200)],
      [uniqueEmailValidator(
        email => this.validationService.checkEmail(email, { excludeUserId: this.userId }),
        () => this.initialEmail,
      )],
    ],
  });

  get f() {
    return this.form.controls;
  }

  showFieldError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    if (!control || !control.invalid) return false;
    if (control.errors?.['emailExists']) return true;
    return this.submitted || control.touched;
  }

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (!user) {
      this.router.navigate([AppRoutes.Admin.Admins]);
      return;
    }
    this.userId = user.id;
    this.adminUsersService.getAdmins().subscribe({
      next: (admins) => {
        const me = admins.data.find((a) => a.id === user.id);
        if (me) {
          this.initialEmail = me.email ?? '';
          this.form.patchValue({
            name: me.name,
            surname: me.surname,
            email: me.email,
          });
        }
      },
      error: () => this.toastService.error('Error al cargar los datos del administrador'),
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    this.form.markAllAsTouched();

    if (this.form.pending) {
      this.form.statusChanges.pipe(filter(s => s !== 'PENDING'), take(1)).subscribe(() => {
        this.attemptSubmit();
      });
      return;
    }

    this.attemptSubmit();
  }

  private attemptSubmit(): void {
    if (this.form.invalid) return;

    const request: UpdateAdminUserRequest = {
      name: this.form.value.name.trim(),
      surname: this.form.value.surname.trim(),
      email: this.form.value.email.trim(),
    };

    this.adminUsersService.updateAdmin(this.userId, request).subscribe({
      next: () => {
        this.toastService.success('Datos actualizados exitosamente');
        this.router.navigate([AppRoutes.Admin.Admins]);
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al actualizar los datos';
      },
    });
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Admins]);
  }
}
