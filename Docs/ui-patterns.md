# Guía de patrones UI — InclusiON Client

Referencia para construir pantallas nuevas de forma homogénea con el resto de la aplicación.
Todos los ejemplos están basados en pantallas reales del proyecto.

---

## Índice

1. [Botones](#1-botones)
2. [Cards (tarjetas)](#2-cards-tarjetas)
3. [Grilla (grid)](#3-grilla-grid)
4. [Formularios](#4-formularios)
5. [Filtros](#5-filtros)
6. [Tabla de datos — `app-data-table`](#6-tabla-de-datos--app-data-table)
7. [Modales de confirmación — `app-confirm-modal`](#7-modales-de-confirmación--app-confirm-modal)
8. [Modal de contraseña temporal — `app-password-modal`](#8-modal-de-contraseña-temporal--app-password-modal)
9. [Modal genérico — `c-modal`](#9-modal-genérico--c-modal)
10. [Badges](#10-badges)
11. [Tabs de navegación](#11-tabs-de-navegación)
12. [Filtro de institución — `app-institution-filter`](#12-filtro-de-institución--app-institution-filter)
13. [Estado vacío](#13-estado-vacío)
14. [Iconos](#14-iconos)
15. [Imports requeridos por componente](#15-imports-requeridos-por-componente)

---

## 1. Botones

### Regla fundamental
Usar siempre la directiva `cButton` de CoreUI. **Nunca usar** `class="btn btn-*"` de Bootstrap puro.

### Atributo `type` — obligatorio en botones dentro de formularios
Todo botón que **no** sea el submit de un formulario debe tener `type="button"`.  
Sin él, el navegador lo trata como `type="submit"` y puede enviar el formulario accidentalmente.

```html
<!-- Guardar (submit del form) -->
<button cButton color="primary" type="submit" [disabled]="form.invalid">
  Guardar
</button>

<!-- Cancelar (nunca debe hacer submit) -->
<button cButton color="secondary" type="button" (click)="goBack()">
  Cancelar
</button>

<!-- Acción peligrosa -->
<button cButton color="danger" type="button" (click)="showDeactivateModal = true">
  Desactivar cuenta
</button>

<!-- Acción de advertencia -->
<button cButton color="warning" type="button" (click)="resetPassword()">
  Resetear contraseña
</button>

<!-- Botón pequeño (tablas, cabeceras) -->
<button cButton color="primary" size="sm" type="button">
  Nueva institución
</button>

<!-- Botón con icono -->
<button cButton color="primary" size="sm" type="button">
  <svg cIcon name="cil-plus" size="sm" class="me-1"></svg>
  Agregar
</button>

<!-- Volver / link -->
<button cButton color="link" class="mb-3 ps-0" type="button" (click)="goBack()">
  &larr; Volver al listado
</button>
```

### Paleta de colores y cuándo usarlos

| Color       | Uso                                              |
|-------------|--------------------------------------------------|
| `primary`   | Acción principal (Guardar, Crear, Confirmar)     |
| `secondary` | Acción secundaria (Cancelar, Cerrar, Volver)     |
| `danger`    | Acción destructiva (Eliminar, Desactivar)        |
| `warning`   | Acción de impacto moderado (Resetear contraseña) |
| `success`   | Acción de restitución (Reactivar, Aprobar)       |
| `link`      | Navegación contextual (Volver, enlaces inline)   |

### Grupo de botones en formularios
Siempre juntar los botones de acción con `d-flex gap-2`, sin tarjeta extra:

```html
<div class="mt-3 d-flex gap-2">
  <button cButton color="primary" type="submit" [disabled]="form.invalid">Guardar</button>
  <button cButton color="secondary" type="button" (click)="goBack()">Cancelar</button>
</div>
```

---

## 2. Cards (tarjetas)

Toda pantalla está envuelta en al menos una `c-card`. Estructura estándar:

```html
<c-card class="mb-4">
  <c-card-header>
    <strong>Título de la sección</strong>
  </c-card-header>
  <c-card-body>
    <!-- Contenido -->
  </c-card-body>
</c-card>
```

### Card con botón en el header (detalle con acciones)

```html
<c-card class="mb-4">
  <c-card-header class="d-flex justify-content-between align-items-center">
    <strong>{{ user.fullName }}</strong>
    <c-badge [color]="user.isActive ? 'success' : 'danger'">
      {{ user.isActive ? 'Activo' : 'Inactivo' }}
    </c-badge>
  </c-card-header>
  <c-card-body>
    <!-- ... -->
  </c-card-body>
</c-card>
```

### Card de acciones (columna lateral)

```html
<c-card class="mb-4">
  <c-card-header><strong>Acciones</strong></c-card-header>
  <c-card-body class="d-grid gap-2">
    @if (user.isActive) {
      <button cButton color="warning" type="button" (click)="resetPassword()">Resetear contraseña</button>
      <button cButton color="danger" type="button" (click)="showDeactivateModal = true">Desactivar cuenta</button>
    } @else {
      <button cButton color="success" type="button" (click)="reactivateUser()">Reactivar cuenta</button>
    }
  </c-card-body>
</c-card>
```

---

## 3. Grilla (grid)

Usa `c-row` y `c-col` de CoreUI. El breakpoint habitual es `sm`.

### Layout de detalle: contenido principal + sidebar de acciones

```html
<c-row>
  <c-col lg="8">
    <!-- Card principal con info -->
  </c-col>
  <c-col lg="4">
    <!-- Card de acciones -->
    <!-- Card de entidad vinculada -->
  </c-col>
</c-row>
```

### Campos de formulario en dos columnas

```html
<c-row class="mb-3">
  <c-col sm="6">
    <!-- Campo izquierdo -->
  </c-col>
  <c-col sm="6">
    <!-- Campo derecho -->
  </c-col>
</c-row>
```

### Fila de datos en una card de detalle

```html
<c-row class="mb-2">
  <c-col sm="4"><strong>Email</strong></c-col>
  <c-col sm="8">{{ user.email }}</c-col>
</c-row>
```

---

## 4. Formularios

### Estructura base (Reactive Forms)

```html
<c-card>
  <c-card-header>
    <strong>Nuevo Profesional</strong>
  </c-card-header>
  <c-card-body>
    @if (serverError) {
      <div class="alert alert-danger">{{ serverError }}</div>
    }

    <p class="text-body-secondary mb-3">Los campos marcados con (*) son obligatorios.</p>

    <form [formGroup]="form" (ngSubmit)="onSubmit()">

      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel for="firstName">Nombre *</label>
          <input cFormControl id="firstName" formControlName="firstName"
                 [valid]="submitted ? f['firstName'].valid : undefined" />
          @if (submitted && f['firstName'].errors) {
            <c-form-feedback [valid]="false">
              @if (f['firstName'].errors['required']) { El nombre es obligatorio. }
              @if (f['firstName'].errors['minlength']) { Mínimo 2 caracteres. }
            </c-form-feedback>
          }
        </c-col>
      </c-row>

      <div class="mt-3 d-flex gap-2">
        <button cButton color="primary" type="submit" [disabled]="form.invalid">Guardar</button>
        <button cButton color="secondary" type="button" (click)="goBack()">Cancelar</button>
      </div>

    </form>
  </c-card-body>
</c-card>
```

### Reglas de validación visual

| Cuándo mostrar `[valid]`            | Valor              |
|-------------------------------------|--------------------|
| Antes de hacer submit               | `undefined` (sin colorear) |
| Después de submit, campo válido     | `true`             |
| Después de submit, campo inválido   | `false`            |

```html
[valid]="submitted ? f['campo'].valid : undefined"
```

Para campos con validación asíncrona (email duplicado, matrícula):

```html
[valid]="showFieldError('email') ? (f['email'].valid ? true : false) : undefined"
```

### Campo de solo lectura

```html
<input cFormControl [value]="professional.documentNumber ?? 'Sin especificar'" readonly
       aria-label="Documento (no editable)" />
```

### Select dentro de formulario reactivo

```html
<label cLabel for="role">Rol *</label>
<select cSelect id="role" formControlName="role"
        [valid]="submitted ? f['role'].valid : undefined">
  <option value="">Seleccione un rol</option>
  <option value="Admin">Administrador</option>
  <option value="Professional">Profesional</option>
</select>
@if (submitted && f['role'].errors?.['required']) {
  <c-form-feedback [valid]="false">El rol es obligatorio.</c-form-feedback>
}
```

### Textarea

```html
<label cLabel for="obs">Observación</label>
<textarea cFormControl id="obs" formControlName="observation" rows="3"
          placeholder="Escriba una observación..."></textarea>
```

### Campo de fecha

- Formato de entrada: `dd/mm/aaaa`
- Siempre agregar `placeholder="dd/mm/aaaa"`
- Validar con `invalidDate` y `futureDate` según corresponda

```html
<label cLabel for="birthDate">Fecha de nacimiento *</label>
<input cFormControl id="birthDate" formControlName="birthDate"
       placeholder="dd/mm/aaaa"
       [valid]="submitted ? f['birthDate'].valid : undefined" />
@if (submitted && f['birthDate'].errors) {
  <c-form-feedback [valid]="false">
    @if (f['birthDate'].errors['required'])    { La fecha es obligatoria. }
    @else if (f['birthDate'].errors['invalidDate']) { Formato inválido. Use dd/mm/aaaa. }
    @else if (f['birthDate'].errors['futureDate'])  { La fecha no puede ser futura. }
  </c-form-feedback>
}
```

---

## 5. Filtros

Los filtros van **encima** del `app-data-table`, envueltos en un `<fieldset>` con leyenda.

```html
<fieldset class="p-3 border rounded mb-4">
  <legend class="px-2 mb-2 fw-bold text-body-secondary" style="font-size: 0.9rem; float: none;">
    Filtros
  </legend>
  <c-row class="align-items-end">
    <c-col sm="3">
      <label class="form-label">Rol</label>
      <select cSelect [(ngModel)]="selectedRole" (ngModelChange)="onRoleFilterChange()">
        <option value="">Todos los roles</option>
        <option value="Admin">Administrador</option>
        <option value="Professional">Profesional</option>
      </select>
    </c-col>
    <c-col sm="3">
      <label class="form-label">Estado</label>
      <select cSelect [(ngModel)]="selectedStatus" (ngModelChange)="onStatusFilterChange()">
        <option value="">Todos</option>
        <option value="true">Activos</option>
        <option value="false">Inactivos</option>
      </select>
    </c-col>
  </c-row>
</fieldset>
```

### Reglas de selects en filtros

- Usar `cSelect` (directiva CoreUI), **no** `class="form-select"`.
- Usar `[(ngModel)]` para binding (no reactive forms).
- El handler debe resetear `currentPage = 1` y llamar a `loadData()`.
- Primera opción siempre es "Todos / Todos los X" con `value=""`.

```typescript
onRoleFilterChange(): void {
  this.currentPage = 1;
  this.loadUsers();
}
```

---

## 6. Tabla de datos — `app-data-table`

El componente central para cualquier listado. Incluye búsqueda, paginación y acciones por fila.

### Uso básico

```html
<app-data-table
  title="Gestión de Usuarios"
  [columns]="cols"
  [items]="users"
  [totalItems]="totalItems"
  [pageSize]="pageSize"
  [currentPage]="currentPage"
  [headerButtons]="[]"
  (pageChange)="onPageChange($event)"
  (searchAction)="onSearch($event)"
  (rowAction)="onRowAction($event)">
</app-data-table>
```

### Con ordenamiento y botón en cabecera

```html
<app-data-table
  title="Gestión de Profesionales"
  [columns]="cols"
  [items]="professionals"
  [totalItems]="totalItems"
  [pageSize]="pageSize"
  [currentPage]="currentPage"
  [sortable]="true"
  [loading]="loading"
  [headerButtons]="getHeaderButtons()"
  (pageChange)="onPageChange($event)"
  (searchAction)="onSearch($event)"
  (sortAction)="onSort($event)"
  (rowAction)="onRowAction($event)"
  (headerAction)="onHeaderAction($event)">
</app-data-table>
```

### Definición de columnas

```typescript
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';

cols: TableColumn[] = [
  // Columna de acciones — siempre primera
  {
    key: 'actions',
    label: 'Acciones',
    type: 'actions',
    actions: [
      { action: 'view',        label: 'Ver detalle',        icon: 'cil-search' },
      { action: 'edit',        label: 'Editar',             icon: 'cil-pencil' },
      { action: 'reset-password', label: 'Resetear contraseña', icon: 'cil-lock-unlocked',
        visible: (item) => item.isActive },
      { action: 'deactivate',  label: 'Desactivar',         icon: 'cil-x',
        visible: (item) => item.isActive },
      { action: 'reactivate',  label: 'Reactivar',          icon: 'cil-check',
        visible: (item) => !item.isActive },
    ],
  },
  // Columnas de datos
  { key: 'fullName',      label: 'Nombre' },
  { key: 'email',         label: 'Email' },
  { key: 'role',          label: 'Rol',    type: 'badge' },
  { key: 'isActive',      label: 'Estado', type: 'badge' },
  { key: 'lastLoginDate', label: 'Último acceso', type: 'date' },
  // Columna numérica
  { key: 'amount',        label: 'Monto',  type: 'number', sortable: true },
];
```

#### Tipos de columna disponibles

| `type`     | Renderizado                                         |
|------------|-----------------------------------------------------|
| `text`     | Texto plano (default)                               |
| `number`   | Valor numérico                                      |
| `date`     | Fecha con `DatePipe`                                |
| `boolean`  | `true`/`false`                                      |
| `badge`    | `c-badge` con color automático según valor          |
| `actions`  | Dropdown con las acciones definidas en `actions[]`  |

#### Badge con mapa personalizado

Para valores que no están en el mapa automático de `getBadgeColor`, usar `badgeMap`:

```typescript
{ key: 'status', label: 'Estado', type: 'badge',
  badgeMap: {
    'pending':  { color: 'warning', label: 'Pendiente' },
    'approved': { color: 'success', label: 'Aprobado'  },
    'rejected': { color: 'danger',  label: 'Rechazado' },
  }
}
```

### Botones en la cabecera de la tabla

```typescript
import { HeaderButton } from 'src/app/shared/components/data-table/data-table.models';

getHeaderButtons(): HeaderButton[] {
  return [
    { action: 'new', label: 'Nuevo Profesional', color: 'primary', icon: 'cil-plus' }
  ];
}
```

```typescript
onHeaderAction(action: string): void {
  if (action === 'new') {
    this.router.navigate(['/admin/professionals/new']);
  }
}
```

### Manejador de acciones de fila

```typescript
onRowAction(event: { action: string; item: any }): void {
  const item = event.item as MiModelo;
  switch (event.action) {
    case 'view':
      this.router.navigate(['/admin/professionals', item.id]);
      break;
    case 'edit':
      this.router.navigate(['/admin/professionals', item.id, 'edit']);
      break;
    case 'deactivate':
      this.itemToDeactivate = item;
      this.showConfirmModal = true;
      break;
    case 'reactivate':
      this.reactivateItem(item);
      break;
  }
}
```

### Inputs disponibles del componente

| Input            | Tipo        | Default | Descripción                               |
|------------------|-------------|---------|-------------------------------------------|
| `title`          | `string`    | `''`    | Título en la cabecera de la card          |
| `showTitle`      | `boolean`   | `true`  | Ocultar cabecera completa si es false     |
| `columns`        | `TableColumn[]` | `[]` | Definición de columnas                   |
| `items`          | `any[]`     | `[]`    | Datos de la página actual                 |
| `totalItems`     | `number`    | `0`     | Total de registros (para paginación)      |
| `pageSize`       | `number`    | `10`    | Registros por página                      |
| `currentPage`    | `number`    | `1`     | Página activa                             |
| `headerButtons`  | `HeaderButton[]` | `[]` | Botones en la cabecera de la tabla      |
| `showSearch`     | `boolean`   | `true`  | Mostrar/ocultar input de búsqueda         |
| `showPagination` | `boolean`   | `true`  | Mostrar/ocultar paginación                |
| `sortable`       | `boolean`   | `false` | Habilitar ordenamiento global             |
| `loading`        | `boolean`   | `false` | Muestra spinner sobre la tabla            |
| `debounceMs`     | `number`    | `400`   | Debounce del buscador en milisegundos     |

### Outputs del componente

| Output         | Tipo emitido                                     | Cuándo                    |
|----------------|--------------------------------------------------|---------------------------|
| `pageChange`   | `number`                                         | Cambio de página          |
| `searchAction` | `string`                                         | Texto del buscador        |
| `sortAction`   | `{ sortBy: string; sortDirection: 'ASC'\|'DESC' }` | Click en columna sortable |
| `rowAction`    | `{ action: string; item: any }`                  | Acción del dropdown       |
| `headerAction` | `string`                                         | Click en botón de cabecera|

### Patrón completo en el componente TypeScript

```typescript
// Variables de estado
users: MiModelo[] = [];
totalItems = 0;
pageSize = 10;
currentPage = 1;
searchTerm = '';
loading = false;

loadUsers(): void {
  this.loading = true;
  this.service.getAll({
    page: this.currentPage,
    pageSize: this.pageSize,
    search: this.searchTerm || undefined,
  }).subscribe({
    next: (response) => {
      this.users = [...response.data];
      this.totalItems = response.totalRecords;
      this.loading = false;
    },
    error: () => {
      this.toastService.error('Error al cargar los datos');
      this.loading = false;
    },
  });
}

onPageChange(page: number): void {
  this.currentPage = page;
  this.loadUsers();
}

onSearch(term: string): void {
  this.searchTerm = term;
  this.currentPage = 1;
  this.loadUsers();
}

onSort(event: { sortBy: string; sortDirection: 'ASC' | 'DESC' }): void {
  this.sortBy = event.sortBy;
  this.sortDirection = event.sortDirection;
  this.currentPage = 1;
  this.loadUsers();
}
```

---

## 7. Modales de confirmación — `app-confirm-modal`

Para cualquier acción que requiera confirmación (desactivar, eliminar, resetear, aprobar/rechazar).

### Uso básico (acción destructiva)

```html
<app-confirm-modal
  [visible]="showConfirmModal"
  title="Confirmar desactivación"
  messagePrefix="¿Está seguro de que desea desactivar al usuario "
  [itemName]="itemToDeactivate?.fullName ?? ''"
  messageSuffix="?"
  detail="Se revocará su acceso inmediatamente."
  confirmLabel="Desactivar"
  confirmColor="danger"
  (confirm)="confirmDeactivate()"
  (cancel)="cancelDeactivate()">
</app-confirm-modal>
```

### Con campo de observación obligatorio

```html
<app-confirm-modal
  [visible]="showDeactivateModal"
  [loading]="isDeactivateLoading"
  title="Desactivar Profesional"
  messagePrefix="¿Está seguro de que desea desactivar al profesional "
  [itemName]="itemToDeactivate?.fullName ?? ''"
  messageSuffix="?"
  [showObservation]="true"
  observationLabel="Motivo de la desactivación"
  observationPlaceholder="Indique el motivo..."
  confirmLabel="Desactivar"
  confirmColor="danger"
  (confirm)="confirmDeactivate($event)"
  (cancel)="cancelDeactivate()">
</app-confirm-modal>
```

Cuando `showObservation="true"`, el evento `(confirm)` emite la observación como `string`:
```typescript
confirmDeactivate(observation: string): void {
  this.service.deactivate(this.item!.id, observation).subscribe({ ... });
}
```

### Inputs disponibles

| Input                  | Tipo      | Default          | Descripción                                       |
|------------------------|-----------|------------------|---------------------------------------------------|
| `visible`              | `boolean` | `false`          | Controla la visibilidad del modal                 |
| `title`                | `string`  | `'Confirmar accion'` | Título del modal                              |
| `messagePrefix`        | `string`  | —                | Texto antes del nombre del ítem                   |
| `itemName`             | `string`  | `''`             | Nombre del ítem a confirmar (en negrita)          |
| `messageSuffix`        | `string`  | `'?'`            | Texto después del nombre del ítem                 |
| `detail`               | `string`  | `''`             | Párrafo adicional de detalle/advertencia          |
| `confirmLabel`         | `string`  | `'Confirmar'`    | Texto del botón de confirmación                   |
| `confirmColor`         | `string`  | `'danger'`       | Color CoreUI del botón confirmar                  |
| `showObservation`      | `boolean` | `false`          | Agrega textarea de observación (bloquea si vacío) |
| `observationLabel`     | `string`  | `'Observación'`  | Label del textarea                                |
| `observationPlaceholder`| `string` | `''`             | Placeholder del textarea                          |
| `loading`              | `boolean` | `false`          | Muestra spinner y deshabilita botones             |

### Colores de confirmación según acción

| Acción              | `confirmColor` |
|---------------------|----------------|
| Desactivar / Eliminar | `danger`     |
| Resetear contraseña | `warning`      |
| Reactivar / Aprobar | `success`      |
| Acción neutra       | `primary`      |

---

## 8. Modal de contraseña temporal — `app-password-modal`

Para mostrar la contraseña temporal después de **crear** un usuario nuevo.

```html
<app-password-modal
  [visible]="showPasswordModal"
  entityType="Profesional"
  entityArticle="el"
  [entityName]="createdProfessional?.fullName ?? ''"
  [password]="createdProfessional?.temporaryPassword ?? ''"
  (close)="closeModalAndNavigate()">
</app-password-modal>
```

> **Nota:** Para mostrar contraseña temporal después de un **reset** o **reactivación** (no creación), usar el modal genérico `c-modal` directamente (ver sección 9), ya que el flujo es diferente.

---

## 9. Modal genérico — `c-modal`

Para modales con contenido libre (historial, contraseña post-reset, etc.).

```html
<c-modal [visible]="showPasswordModal" (visibleChange)="closePasswordModal()">
  <c-modal-header>
    <h5 cModalTitle>Contraseña temporal generada</h5>
  </c-modal-header>
  <c-modal-body>
    <p>Se generó una contraseña temporal para <strong>{{ tempPasswordEmail }}</strong>.</p>
    <p>El usuario deberá cambiarla en su próximo inicio de sesión.</p>
    <c-alert color="warning" class="d-flex align-items-center justify-content-between">
      <code class="fs-5">{{ tempPassword }}</code>
      <button cButton color="primary" size="sm" type="button" (click)="copyPassword()">Copiar</button>
    </c-alert>
  </c-modal-body>
  <c-modal-footer>
    <button cButton color="secondary" type="button" (click)="closePasswordModal()">Cerrar</button>
  </c-modal-footer>
</c-modal>
```

### Modal bloqueante (sin cierre al hacer clic afuera)

Usar `backdrop="static"` y `[keyboard]="false"` cuando el usuario **debe** leer el contenido antes de continuar (ej. contraseña no recuperable):

```html
<c-modal [visible]="showPasswordModal" backdrop="static" [keyboard]="false">
```

### Modal centrado y de tamaño grande

```html
<c-modal [visible]="showHistoryModal" (visibleChange)="showHistoryModal = $event"
         alignment="center" size="lg">
```

### Variables de estado para modales

```typescript
// Un modal por acción
showConfirmModal = false;
itemToDeactivate: MiModelo | null = null;

showResetPasswordModal = false;
itemToReset: MiModelo | null = null;

showPasswordModal = false;
tempPassword = '';
tempPasswordEmail = '';
```

---

## 10. Badges

```html
<!-- Badge de estado (isActive es boolean) -->
<c-badge [color]="item.isActive ? 'success' : 'danger'">
  {{ item.isActive ? 'Activo' : 'Inactivo' }}
</c-badge>

<!-- Badge de rol -->
<c-badge [color]="roleBadgeColor">{{ roleLabel }}</c-badge>
```

### Colores estándar por valor

| Valor              | Color CoreUI |
|--------------------|--------------|
| `true` / Activo    | `success`    |
| `false` / Inactivo | `danger`     |
| `Admin`            | `primary`    |
| `Professional`     | `info`       |
| `FamilyRepresentative` | `success` |
| `PersonWithDisability` | `warning` |
| `approved`         | `success`    |
| `suspended`        | `warning`    |
| `terminated`       | `secondary`  |
| `rejected`         | `danger`     |

> El componente `app-data-table` aplica estos colores automáticamente en columnas `type: 'badge'`.

---

## 11. Tabs de navegación

Para pantallas con múltiples sub-secciones (ej. Activos / Pendientes):

```html
<div class="mb-3">
  <ul class="nav nav-tabs" role="tablist">
    <li class="nav-item">
      <button class="nav-link" [class.active]="activeTab === 'active'"
              (click)="switchTab('active')" type="button">
        Activos
      </button>
    </li>
    <li class="nav-item">
      <button class="nav-link" [class.active]="activeTab === 'pending'"
              (click)="switchTab('pending')" type="button">
        Pendientes
        @if (pendingCount > 0) {
          <span class="badge bg-warning ms-2">{{ pendingCount }}</span>
        }
      </button>
    </li>
  </ul>
</div>

@if (activeTab === 'active') {
  <!-- Contenido tab activos -->
}
@if (activeTab === 'pending') {
  <!-- Contenido tab pendientes -->
}
```

```typescript
activeTab: 'active' | 'pending' = 'active';

switchTab(tab: 'active' | 'pending'): void {
  this.activeTab = tab;
  this.currentPage = 1;
  this.loadData();
}
```

---

## 12. Filtro de institución — `app-institution-filter`

Se ubica al inicio de toda pantalla de listado en el módulo admin. Se auto-oculta si el admin es global o si solo tiene una institución asignada.

```html
<app-institution-filter
  (filterChange)="onInstitutionFilterChange($event)"
  (loaded)="onFilterLoaded()">
</app-institution-filter>
```

```typescript
import { InstitutionFilterComponent } from '@shared/components';

selectedInstitutionId: number | undefined;
filterLoaded = false;

onInstitutionFilterChange(id: number | undefined): void {
  this.selectedInstitutionId = id;
  if (this.filterLoaded) {
    this.currentPage = 1;
    this.loadData();
  }
}

onFilterLoaded(): void {
  this.filterLoaded = true;
  this.loadData();
}
```

> **Regla:** Esperar el evento `(loaded)` antes de hacer la primera carga. De lo contrario se lanza la request antes de tener el `institutionId` correcto.

---

## 13. Estado vacío

Cuando una tabla o sección no tiene registros, mostrar feedback visual **debajo** del componente:

```html
@if (!items.length && !activeFilter) {
  <div class="text-center text-body-secondary py-4">
    <svg cIcon name="cilUser" size="3xl" class="mb-2"></svg>
    <p class="mb-0">No hay registros.</p>
  </div>
}

@if (!items.length && activeFilter) {
  <div class="text-center text-body-secondary py-4">
    <p class="mb-0">No se encontraron registros con el filtro seleccionado.</p>
  </div>
}
```

---

## 14. Iconos

Se usan iconos de CoreUI Icons con la directiva `cIcon`. Los nombres son `camelCase` del set `cil-*`.

```html
<svg cIcon name="cil-search"    size="sm"></svg>
<svg cIcon name="cil-pencil"    size="sm"></svg>
<svg cIcon name="cil-x"         size="sm"></svg>
<svg cIcon name="cil-check"     size="sm"></svg>
<svg cIcon name="cil-plus"      size="sm"></svg>
<svg cIcon name="cil-lock-unlocked" size="sm"></svg>
<svg cIcon name="cil-history"   size="sm"></svg>
<svg cIcon name="cilMenu"       size="sm"></svg>
<svg cIcon name="cilUser"       size="3xl"></svg>
<svg cIcon name="cilClock"      size="3xl"></svg>
```

### Tamaños disponibles

| `size`  | Uso                               |
|---------|-----------------------------------|
| `sm`    | Dentro de botones y dropdowns     |
| `lg`    | Iconos standalone medianos        |
| `xl`    | Iconos de sección                 |
| `3xl`   | Iconos de estado vacío            |

> **No inventar nombres de iconos.** Verificar que existan en `@coreui/icons` antes de usar.
> Lista completa: https://icons.coreui.io/

---

## 15. Imports requeridos por componente

### Componente de listado típico

```typescript
import { FormsModule } from '@angular/forms';
import { DataTableComponent } from '@shared/components';
import { ConfirmModalComponent } from '@shared/components';
import { InstitutionFilterComponent } from '@shared/components';
import {
  ColComponent, RowComponent, FormSelectDirective,
  AlertComponent, ModalComponent, ModalHeaderComponent,
  ModalBodyComponent, ModalFooterComponent, ButtonDirective,
} from '@coreui/angular';
```

### Componente de detalle típico

```typescript
import { DatePipe } from '@angular/common';
import { ConfirmModalComponent } from '@shared/components';
import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  ColComponent, RowComponent, BadgeComponent, ButtonDirective,
  AlertComponent, ModalComponent, ModalHeaderComponent,
  ModalBodyComponent, ModalFooterComponent,
} from '@coreui/angular';
```

### Componente de formulario (crear/editar)

```typescript
import { ReactiveFormsModule } from '@angular/forms';
import { PasswordModalComponent } from '@shared/components';
import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  ColComponent, RowComponent, ButtonDirective,
  FormControlDirective, FormLabelDirective, FormFeedbackComponent,
  FormSelectDirective,
} from '@coreui/angular';
```
