import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminInstitutionsService, AdminUsersService, AuthService, CatalogsService, InstitutionsService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { InstitutionResponse, CreateAdminUserResponse, CatalogItem } from '@models';
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
  private readonly adminInstitutionsService = inject(AdminInstitutionsService);
  private readonly institutionsService = inject(InstitutionsService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly authService = inject(AuthService);

  institutions: InstitutionResponse[] = [];
  institutionalRoles: CatalogItem[] = [];
  currentInstitutionName = '';
  isGlobalAdmin = false;
  submitted = false;
  serverError = '';
  showPasswordModal = false;
  createdAdmin: CreateAdminUserResponse | null = null;

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    institutionId: ['', [Validators.required]],
    institutionalRoleId: ['', [Validators.required]],
  });

  get f() { return this.form.controls; }

  ngOnInit(): void {
    this.isGlobalAdmin = this.authService.isGlobalAdmin();

    // Cargar catálogo de roles/cargos institucionales (Director, Vicedirector, Secretaria, Preceptor)
    this.catalogsService.getInstitutionalRoles().subscribe({
      next: (roles) => {
        this.institutionalRoles = (roles || [])
          .filter(r => r.isActive)
          .filter(r => {
            const name = (r.name || '').toLowerCase();
            return !name.includes('coordinador') && !name.includes('orientaci');
          });
      },
      error: () => {
        this.institutionalRoles = [
          { id: 1, encryptedId: '1', name: 'Director / Directora', isActive: true },
          { id: 2, encryptedId: '2', name: 'Vicedirector / Vicedirectora', isActive: true },
          { id: 3, encryptedId: '3', name: 'Secretario / Secretaria', isActive: true },
          { id: 4, encryptedId: '4', name: 'Preceptor / Preceptora', isActive: true },
        ];
      }
    });

    // Resolver la institución del usuario en sesión para asignación automática
    this.adminInstitutionsService.getMyInstitutions().subscribe({
      next: (myInsts) => {
        if (myInsts && myInsts.length > 0) {
          const first = myInsts[0];
          this.currentInstitutionName = first.institutionName;
          this.form.patchValue({ institutionId: first.institutionId });
        } else {
          this.institutionsService.getAll().subscribe({
            next: (data) => {
              this.institutions = data.filter(i => i.isActive);
              if (this.institutions.length > 0) {
                this.currentInstitutionName = this.institutions[0].name;
                this.form.patchValue({ institutionId: this.institutions[0].id });
              }
            }
          });
        }
      },
      error: () => {
        this.institutionsService.getAll().subscribe({
          next: (data) => {
            this.institutions = data.filter(i => i.isActive);
            if (this.institutions.length > 0) {
              this.currentInstitutionName = this.institutions[0].name;
              this.form.patchValue({ institutionId: this.institutions[0].id });
            }
          }
        });
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const raw = this.form.value;
    this.adminUsersService.createAdmin({
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      institutionId: +raw.institutionId,
      institutionalRoleId: raw.institutionalRoleId ? +raw.institutionalRoleId : null,
    }).subscribe({
      next: (response) => {
        this.createdAdmin = response;
        this.showPasswordModal = true;
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al crear el administrador';
      },
    });
  }

  closeModalAndNavigate(): void {
    this.showPasswordModal = false;
    this.router.navigate([AppRoutes.Admin.Admins]);
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Admins]);
  }
}
