import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminUsersService, InstitutionsService } from '@services';
import { InstitutionResponse, CreateAdminUserResponse } from '@models';
import {
  ButtonDirective, CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, FormControlDirective, FormFeedbackComponent, FormLabelDirective,
  FormSelectDirective, RowComponent,
} from '@coreui/angular';
import { PasswordModalComponent } from '@shared/components/password-modal/password-modal.component';

@Component({
  selector: 'app-admin-new',
  standalone: true,
  imports: [
    ReactiveFormsModule, CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective,
    PasswordModalComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly institutionsService = inject(InstitutionsService);

  institutions: InstitutionResponse[] = [];
  submitted = false;
  serverError = '';
  showPasswordModal = false;
  createdAdmin: CreateAdminUserResponse | null = null;

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    institutionId: ['', [Validators.required]],
  });

  get f() { return this.form.controls; }

  ngOnInit(): void {
    this.institutionsService.getAll().subscribe({
      next: (data) => this.institutions = data.filter(i => i.isActive),
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    if (this.form.invalid) return;

    const raw = this.form.value;
    this.adminUsersService.createAdmin({
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      institutionId: +raw.institutionId,
    }).subscribe({
      next: (response) => {
        this.createdAdmin = response;
        this.showPasswordModal = true;
      },
      error: (err) => {
        this.serverError = err?.error?.message || 'Error al crear el administrador';
      },
    });
  }

  closeModalAndNavigate(): void {
    this.showPasswordModal = false;
    this.router.navigate(['/admin/admins']);
  }

  goBack(): void {
    this.router.navigate(['/admin/admins']);
  }
}
