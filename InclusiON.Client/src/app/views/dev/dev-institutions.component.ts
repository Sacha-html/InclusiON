import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InstitutionsService, AuthService } from '@services';
import { CreateInstitutionRequest, InstitutionResponse, UpdateInstitutionRequest } from '@models';

@Component({
  selector: 'app-dev-institutions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  template: `
    <div class="dev-page">

      <!-- ========================================== -->
      <!-- PANTALLA 1: BARRERA DE SEGURIDAD (LOGIN)  -->
      <!-- ========================================== -->
      @if (!isDevAuthenticated) {
        <div class="auth-backdrop">
          <div class="auth-card">
            <div class="auth-header">
              <div class="lock-circle">🔒</div>
              <h2>Acceso de Desarrollador</h2>
              <p>Herramienta técnica para gestión directa de Instituciones.</p>
            </div>

            @if (loginError) {
              <div class="alert-error">
                {{ loginError }}
              </div>
            }

            <form [formGroup]="loginForm" (ngSubmit)="submitLogin()">
              <div class="field">
                <label for="devEmail">Correo de autorización</label>
                <input
                  id="devEmail"
                  type="email"
                  class="input-control"
                  formControlName="email"
                  placeholder="adminglobal@test.com"
                  autocomplete="username"
                />
              </div>

              <div class="field">
                <label for="devPass">Contraseña de seguridad</label>
                <input
                  id="devPass"
                  type="password"
                  class="input-control"
                  formControlName="password"
                  placeholder="admin123!"
                  autocomplete="current-password"
                />
              </div>

              <button type="submit" class="btn-submit" [disabled]="loginForm.invalid || isAuthenticating">
                {{ isAuthenticating ? 'Validando...' : 'Desbloquear Panel' }}
              </button>
            </form>

            <div class="auth-footer">
              <a (click)="goHome()" class="link-home">← Volver al sitio principal</a>
            </div>
          </div>
        </div>
      }

      <!-- ========================================== -->
      <!-- PANTALLA 2: ABM MINIMALISTA DE INSTITUCIONES -->
      <!-- ========================================== -->
      @if (isDevAuthenticated) {
        <div class="abm-layout">
          <!-- TOPBAR -->
          <header class="topbar">
            <div class="brand">
              <span class="tag">DEV</span>
              <h1>Instituciones Educativas</h1>
              <span class="count-badge">{{ institutions.length }} sedes</span>
            </div>

            <div class="user-box">
              <span class="user-email">👤 adminglobal&#64;test.com</span>
              <button class="btn-secondary-sm" (click)="lockDev()">Cerrar sesión</button>
              <button class="btn-secondary-sm" (click)="goHome()">Ir a la app</button>
            </div>
          </header>

          <!-- ALERTAS -->
          @if (alertMessage) {
            <div class="banner" [ngClass]="alertType === 'success' ? 'banner-ok' : 'banner-err'">
              <span>{{ alertMessage }}</span>
              <button class="banner-close" (click)="alertMessage = ''">✕</button>
            </div>
          }

          <!-- TOOLBAR -->
          <div class="actions-bar">
            <div class="search-box">
              <input
                type="text"
                class="input-control"
                placeholder="Buscar por nombre, domicilio o email..."
                [(ngModel)]="searchFilter"
                (ngModelChange)="applySearch()"
              />
            </div>
            <div class="buttons-group">
              <button class="btn-primary" (click)="openCreate()">+ Nueva Institución</button>
              <button class="btn-secondary" (click)="loadInstitutions()" [disabled]="isLoading">
                {{ isLoading ? 'Cargando...' : 'Actualizar' }}
              </button>
            </div>
          </div>

          <!-- TABLA -->
          <div class="table-container">
            @if (isLoading) {
              <div class="state-box">
                <div class="loader"></div>
                <p>Cargando datos desde PostgreSQL...</p>
              </div>
            } @else if (filtered.length === 0) {
              <div class="state-box">
                <p>No hay instituciones registradas.</p>
                <button class="btn-primary-sm" (click)="openCreate()">Crear primera sede</button>
              </div>
            } @else {
              <table class="simple-table">
                <thead>
                  <tr>
                    <th style="width: 60px;">ID</th>
                    <th>Denominación Oficial</th>
                    <th>Domicilio</th>
                    <th>Teléfono</th>
                    <th>Email Oficial</th>
                    <th style="width: 90px;">Estado</th>
                    <th style="width: 180px; text-align: right;">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  @for (item of filtered; track item.id) {
                    <tr [class.disabled-row]="!item.isActive">
                      <td class="id-col">#{{ item.id }}</td>
                      <td class="name-col">{{ item.name }}</td>
                      <td>{{ item.address || '—' }}</td>
                      <td>{{ item.phone || '—' }}</td>
                      <td>{{ item.email || '—' }}</td>
                      <td>
                        <span class="status-pill" [ngClass]="item.isActive ? 'pill-active' : 'pill-inactive'">
                          {{ item.isActive ? 'Activo' : 'Inactivo' }}
                        </span>
                      </td>
                      <td class="actions-col">
                        <button class="btn-link" (click)="openEdit(item)">Editar</button>
                        <span class="sep">|</span>
                        <button
                          class="btn-link"
                          [ngClass]="item.isActive ? 'btn-link-danger' : 'btn-link-success'"
                          (click)="toggleActive(item)"
                        >
                          {{ item.isActive ? 'Desactivar' : 'Activar' }}
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </div>
        </div>

        <!-- MODAL FORMULARIO (CREAR / EDITAR) -->
        @if (showModal) {
          <div class="modal-layer">
            <div class="modal-box">
              <div class="modal-top">
                <h3>{{ isEditing ? 'Editar Institución #' + editTargetId : 'Nueva Institución' }}</h3>
                <button class="modal-x" (click)="closeModal()">✕</button>
              </div>

              <form [formGroup]="form" (ngSubmit)="submitForm()">
                <div class="modal-content">
                  <div class="form-row">
                    <label for="fName">Nombre Oficial *</label>
                    <input
                      id="fName"
                      type="text"
                      class="input-control"
                      formControlName="name"
                      placeholder="Ej: Escuela Especial N° 1"
                    />
                    @if (form.get('name')?.touched && form.get('name')?.invalid) {
                      <span class="hint-error">El nombre es requerido (mínimo 2 caracteres).</span>
                    }
                  </div>

                  <div class="form-row">
                    <label for="fAddress">Domicilio Físico</label>
                    <input
                      id="fAddress"
                      type="text"
                      class="input-control"
                      formControlName="address"
                      placeholder="Ej: Av. San Martín 1234, Córdoba"
                    />
                  </div>

                  <div class="form-row">
                    <label for="fPhone">Teléfono de Contacto</label>
                    <input
                      id="fPhone"
                      type="text"
                      class="input-control"
                      formControlName="phone"
                      placeholder="Ej: 3514445566"
                    />
                    @if (form.get('phone')?.touched && form.get('phone')?.invalid) {
                      <span class="hint-error">Solo se permiten números.</span>
                    }
                  </div>

                  <div class="form-row">
                    <label for="fEmail">Correo Electrónico *</label>
                    <input
                      id="fEmail"
                      type="email"
                      class="input-control"
                      formControlName="email"
                      placeholder="contacto@escuela.edu.ar"
                    />
                    @if (form.get('email')?.touched && form.get('email')?.invalid) {
                      <span class="hint-error">Ingrese un correo válido.</span>
                    }
                  </div>
                </div>

                <div class="modal-bottom">
                  <button type="button" class="btn-secondary" (click)="closeModal()" [disabled]="isSaving">
                    Cancelar
                  </button>
                  <button type="submit" class="btn-primary" [disabled]="form.invalid || isSaving">
                    {{ isSaving ? 'Guardando...' : (isEditing ? 'Guardar Cambios' : 'Crear Sede') }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    * {
      box-sizing: border-box;
    }
    .dev-page {
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
      background: #f9fafb;
      min-height: 100vh;
      color: #111827;
      line-height: 1.5;
    }

    /* ─── PANTALLA LOGIN DE SEGURIDAD ─── */
    .auth-backdrop {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      padding: 20px;
      background: #f3f4f6;
    }
    .auth-card {
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      padding: 32px;
      max-width: 400px;
      width: 100%;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
    }
    .auth-header {
      text-align: center;
      margin-bottom: 24px;
    }
    .lock-circle {
      font-size: 28px;
      margin-bottom: 8px;
    }
    .auth-header h2 {
      margin: 0 0 4px 0;
      font-size: 20px;
      font-weight: 600;
      color: #111827;
    }
    .auth-header p {
      margin: 0;
      font-size: 13px;
      color: #6b7280;
    }
    .alert-error {
      background: #fef2f2;
      border: 1px solid #fecaca;
      color: #b91c1c;
      padding: 10px 12px;
      border-radius: 6px;
      font-size: 13px;
      margin-bottom: 16px;
      text-align: center;
    }
    .field {
      margin-bottom: 16px;
    }
    .field label {
      display: block;
      font-size: 12px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 6px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .btn-submit {
      width: 100%;
      background: #2563eb;
      color: white;
      border: none;
      padding: 10px;
      border-radius: 6px;
      font-weight: 600;
      font-size: 14px;
      cursor: pointer;
      margin-top: 8px;
    }
    .btn-submit:hover:not(:disabled) {
      background: #1d4ed8;
    }
    .btn-submit:disabled {
      background: #93c5fd;
      cursor: not-allowed;
    }
    .auth-footer {
      text-align: center;
      margin-top: 20px;
      padding-top: 16px;
      border-top: 1px solid #f3f4f6;
    }
    .link-home {
      font-size: 13px;
      color: #6b7280;
      cursor: pointer;
      text-decoration: none;
    }
    .link-home:hover {
      color: #111827;
    }

    /* ─── LAYOUT ABM ─── */
    .abm-layout {
      max-width: 1100px;
      margin: 0 auto;
      padding: 32px 20px;
    }
    .topbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
      padding-bottom: 16px;
      border-bottom: 1px solid #e5e7eb;
      flex-wrap: wrap;
      gap: 16px;
    }
    .brand {
      display: flex;
      align-items: center;
      gap: 12px;
    }
    .tag {
      background: #111827;
      color: white;
      font-size: 11px;
      font-weight: 700;
      padding: 2px 6px;
      border-radius: 4px;
      letter-spacing: 0.5px;
    }
    .brand h1 {
      margin: 0;
      font-size: 22px;
      font-weight: 600;
      color: #111827;
    }
    .count-badge {
      background: #e5e7eb;
      color: #374151;
      font-size: 12px;
      padding: 2px 8px;
      border-radius: 12px;
      font-weight: 500;
    }
    .user-box {
      display: flex;
      align-items: center;
      gap: 10px;
      font-size: 13px;
    }
    .user-email {
      color: #4b5563;
      font-weight: 500;
    }

    /* ─── BANNERS ─── */
    .banner {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px 14px;
      border-radius: 6px;
      font-size: 13px;
      margin-bottom: 16px;
    }
    .banner-ok {
      background: #f0fdf4;
      border: 1px solid #bbf7d0;
      color: #15803d;
    }
    .banner-err {
      background: #fef2f2;
      border: 1px solid #fecaca;
      color: #b91c1c;
    }
    .banner-close {
      background: none;
      border: none;
      cursor: pointer;
      font-size: 14px;
      color: inherit;
    }

    /* ─── ACTIONS BAR ─── */
    .actions-bar {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      margin-bottom: 16px;
      flex-wrap: wrap;
    }
    .search-box {
      flex: 1;
      min-width: 260px;
    }
    .buttons-group {
      display: flex;
      gap: 8px;
    }

    /* ─── INPUTS & BOTONES ─── */
    .input-control {
      width: 100%;
      padding: 8px 12px;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      font-size: 14px;
      background: #ffffff;
      color: #111827;
    }
    .input-control:focus {
      outline: none;
      border-color: #2563eb;
      box-shadow: 0 0 0 1px #2563eb;
    }
    .btn-primary {
      background: #2563eb;
      color: #ffffff;
      border: 1px solid #2563eb;
      padding: 8px 14px;
      border-radius: 6px;
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
    }
    .btn-primary:hover {
      background: #1d4ed8;
    }
    .btn-primary-sm {
      background: #2563eb;
      color: #ffffff;
      border: none;
      padding: 6px 12px;
      border-radius: 5px;
      font-size: 13px;
      cursor: pointer;
    }
    .btn-secondary {
      background: #ffffff;
      color: #374151;
      border: 1px solid #d1d5db;
      padding: 8px 14px;
      border-radius: 6px;
      font-size: 14px;
      cursor: pointer;
    }
    .btn-secondary:hover {
      background: #f9fafb;
    }
    .btn-secondary-sm {
      background: #ffffff;
      color: #4b5563;
      border: 1px solid #d1d5db;
      padding: 4px 8px;
      border-radius: 5px;
      font-size: 12px;
      cursor: pointer;
    }
    .btn-secondary-sm:hover {
      background: #f3f4f6;
    }

    /* ─── TABLA MINIMALISTA ─── */
    .table-container {
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      overflow-x: auto;
    }
    .simple-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 13px;
      text-align: left;
    }
    .simple-table th {
      background: #f9fafb;
      color: #4b5563;
      font-weight: 600;
      padding: 10px 14px;
      border-bottom: 1px solid #e5e7eb;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.4px;
    }
    .simple-table td {
      padding: 12px 14px;
      border-bottom: 1px solid #f3f4f6;
      vertical-align: middle;
    }
    .simple-table tbody tr:last-child td {
      border-bottom: none;
    }
    .simple-table tbody tr:hover {
      background: #f9fafb;
    }
    .disabled-row {
      background: #fbfbfb;
      opacity: 0.6;
    }
    .id-col {
      color: #6b7280;
      font-family: monospace;
      font-size: 12px;
    }
    .name-col {
      font-weight: 600;
      color: #111827;
    }
    .actions-col {
      text-align: right;
      white-space: nowrap;
    }
    .status-pill {
      font-size: 11px;
      padding: 2px 8px;
      border-radius: 10px;
      font-weight: 600;
    }
    .pill-active {
      background: #dcfce7;
      color: #166534;
    }
    .pill-inactive {
      background: #f3f4f6;
      color: #6b7280;
    }
    .btn-link {
      background: none;
      border: none;
      color: #2563eb;
      cursor: pointer;
      font-size: 13px;
      padding: 0;
    }
    .btn-link:hover {
      text-decoration: underline;
    }
    .btn-link-danger {
      color: #dc2626;
    }
    .btn-link-success {
      color: #16a34a;
    }
    .sep {
      margin: 0 6px;
      color: #d1d5db;
    }

    /* ─── MODAL ─── */
    .modal-layer {
      position: fixed;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0, 0, 0, 0.4);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      padding: 16px;
    }
    .modal-box {
      background: #ffffff;
      border-radius: 8px;
      max-width: 460px;
      width: 100%;
      border: 1px solid #e5e7eb;
      box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
    }
    .modal-top {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 14px 18px;
      border-bottom: 1px solid #e5e7eb;
    }
    .modal-top h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
      color: #111827;
    }
    .modal-x {
      background: none;
      border: none;
      font-size: 16px;
      cursor: pointer;
      color: #9ca3af;
    }
    .modal-content {
      padding: 18px;
    }
    .form-row {
      margin-bottom: 14px;
    }
    .form-row label {
      display: block;
      font-size: 12px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 4px;
    }
    .hint-error {
      color: #dc2626;
      font-size: 11px;
      margin-top: 3px;
      display: block;
    }
    .modal-bottom {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      padding: 12px 18px;
      background: #f9fafb;
      border-top: 1px solid #e5e7eb;
      border-bottom-left-radius: 8px;
      border-bottom-right-radius: 8px;
    }

    .state-box {
      padding: 40px 20px;
      text-align: center;
      color: #6b7280;
      font-size: 14px;
    }
    .loader {
      width: 24px;
      height: 24px;
      border: 2px solid #e5e7eb;
      border-top-color: #2563eb;
      border-radius: 50%;
      animation: spin 0.6s linear infinite;
      margin: 0 auto 10px auto;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class DevInstitutionsComponent implements OnInit {
  private readonly institutionsService = inject(InstitutionsService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  // ESTADO DE ACCESO AL PANEL DEV
  isDevAuthenticated = false;
  isAuthenticating = false;
  loginError = '';

  loginForm: FormGroup = this.fb.group({
    email: ['adminglobal@test.com', [Validators.required, Validators.email]],
    password: ['admin123!', [Validators.required]],
  });

  // DATOS DEL ABM
  institutions: InstitutionResponse[] = [];
  filtered: InstitutionResponse[] = [];
  isLoading = false;
  isSaving = false;
  searchFilter = '';

  alertMessage = '';
  alertType: 'success' | 'danger' = 'success';

  // MODAL
  showModal = false;
  isEditing = false;
  editTargetId: number | null = null;

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    address: ['', [Validators.maxLength(200)]],
    phone: ['', [Validators.maxLength(20), Validators.pattern(/^[0-9]+$/)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
  });

  ngOnInit(): void {
    // Si ya existe un token activo de Admin Global en el navegador, desbloqueamos directamente
    if (this.authService.getToken() && this.authService.isGlobalAdmin()) {
      this.isDevAuthenticated = true;
      this.loadInstitutions();
    }
  }

  // ── INGRESO DE SEGURIDAD ─────────────────────────
  submitLogin(): void {
    if (this.loginForm.invalid) return;

    this.isAuthenticating = true;
    this.loginError = '';
    const { email, password } = this.loginForm.value;

    const normalizedEmail = (email || '').trim().toLowerCase();
    const normalizedPass = (password || '').trim();

    // Validamos que use las credenciales requeridas
    const isAllowedEmail = normalizedEmail === 'adminglobal@test.com' || normalizedEmail === 'admin@test.com';
    const isAllowedPass = normalizedPass.toLowerCase() === 'admin123!' || normalizedPass === 'Admin123!';

    if (!isAllowedEmail || !isAllowedPass) {
      this.loginError = 'Credenciales no autorizadas. Use adminglobal@test.com / admin123!';
      this.isAuthenticating = false;
      return;
    }

    // Se autentica contra el backend de identity con las credenciales seeded de admin global
    this.authService.login({ email: 'admin@test.com', password: 'Admin123!' }).subscribe({
      next: () => {
        this.isAuthenticating = false;
        this.isDevAuthenticated = true;
        this.loadInstitutions();
      },
      error: (err) => {
        this.isAuthenticating = false;
        this.loginError = 'Error conectando al backend: ' + (err?.userMessage || err?.message || 'Servidor no disponible');
      }
    });
  }

  lockDev(): void {
    this.authService.logout();
    this.isDevAuthenticated = false;
    this.loginError = '';
    this.loginForm.patchValue({ email: 'adminglobal@test.com', password: 'admin123!' });
  }

  goHome(): void {
    this.router.navigate(['/']);
  }

  // ── ABM INSTITUCIONES ───────────────────────────
  loadInstitutions(): void {
    this.isLoading = true;
    this.institutionsService.getAll().subscribe({
      next: (data) => {
        this.institutions = data || [];
        this.applySearch();
        this.isLoading = false;
      },
      error: (err) => {
        this.showAlert(err?.userMessage || 'Error al obtener instituciones desde el servidor.', 'danger');
        this.isLoading = false;
      }
    });
  }

  applySearch(): void {
    if (!this.searchFilter.trim()) {
      this.filtered = [...this.institutions];
    } else {
      const q = this.searchFilter.toLowerCase();
      this.filtered = this.institutions.filter(
        i => i.name.toLowerCase().includes(q) ||
             (i.email && i.email.toLowerCase().includes(q)) ||
             (i.address && i.address.toLowerCase().includes(q))
      );
    }
  }

  openCreate(): void {
    this.isEditing = false;
    this.editTargetId = null;
    this.form.reset({ name: '', address: '', phone: '', email: '' });
    this.showModal = true;
  }

  openEdit(item: InstitutionResponse): void {
    this.isEditing = true;
    this.editTargetId = item.id;
    this.form.patchValue({
      name: item.name,
      address: item.address || '',
      phone: item.phone || '',
      email: item.email || '',
    });
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.isEditing = false;
    this.editTargetId = null;
  }

  submitForm(): void {
    if (this.form.invalid) return;

    this.isSaving = true;
    const v = this.form.value;

    if (this.isEditing && this.editTargetId) {
      const updateReq: UpdateInstitutionRequest = {
        name: v.name,
        address: v.address || null,
        phone: v.phone || null,
        email: v.email || null,
      };

      this.institutionsService.update(this.editTargetId.toString(), updateReq).subscribe({
        next: () => {
          this.showAlert(`Institución #${this.editTargetId} actualizada.`, 'success');
          this.closeModal();
          this.loadInstitutions();
          this.isSaving = false;
        },
        error: (err) => {
          this.showAlert(err?.userMessage || 'Error al modificar la sede.', 'danger');
          this.isSaving = false;
        }
      });
    } else {
      const createReq: CreateInstitutionRequest = {
        name: v.name,
        address: v.address || undefined,
        phone: v.phone || undefined,
        email: v.email || undefined,
      };

      this.institutionsService.create(createReq).subscribe({
        next: () => {
          this.showAlert('Institución creada con éxito.', 'success');
          this.closeModal();
          this.loadInstitutions();
          this.isSaving = false;
        },
        error: (err) => {
          this.showAlert(err?.userMessage || 'Error al dar de alta la institución.', 'danger');
          this.isSaving = false;
        }
      });
    }
  }

  toggleActive(item: InstitutionResponse): void {
    const nextState = !item.isActive;
    const verb = nextState ? 'activar' : 'desactivar';

    if (!confirm(`¿Confirma ${verb} la sede "${item.name}"?`)) return;

    this.institutionsService.patchStatus(item.id.toString(), nextState).subscribe({
      next: () => {
        this.showAlert(`Sede "${item.name}" ${nextState ? 'activada' : 'desactivada'}.`, 'success');
        this.loadInstitutions();
      },
      error: (err) => {
        this.showAlert(err?.userMessage || `Error al ${verb} la sede.`, 'danger');
      }
    });
  }

  private showAlert(msg: string, type: 'success' | 'danger'): void {
    this.alertMessage = msg;
    this.alertType = type;
  }
}
