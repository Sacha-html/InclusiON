# FE-14 — Implementación: Timeline de Diagnósticos

**Jira:** IN-86  
**Fecha:** 2026-04-17  
**Componente existente:** `src/app/views/professional/person-detail/components/professional-diagnoses.component.ts`

---

## Estado actual

El componente ya existe con funcionalidad básica:
- Lista plana de cards (sin visual de timeline)
- Modal de creación/edición con todos los campos
- Carga y guarda via `DiagnosesService`

**Lo que falta (según HU-08):**
1. `loadDiagnoses()` en el padre está vacío — el badge y la carga inicial no funcionan
2. Sin chequeo de permisos (`diagnoses:create`, `diagnoses:update`)
3. Sin indicador "Solo lectura" para diagnósticos de otros profesionales
4. Sin estados de carga ni toasts de feedback
5. Sin validación visible de campos requeridos
6. Sin visual de timeline (línea vertical + puntos por fecha)

---

## Paso 1 — Corregir `loadDiagnoses()` en el padre

**Archivo:** `person-detail.component.ts`

El método está vacío en la línea 80. Necesitás:

1. Inyectar `DiagnosesService` en el padre:
```typescript
private readonly diagnosesService = inject(DiagnosesService);
// Importar: import { DiagnosesService } from '@services/diagnoses.service';
```

2. Implementar el método:
```typescript
private loadDiagnoses(): void {
  if (!this.person) return;
  this.diagnosesService.getByPerson(this.person.id).subscribe({
    next: (data) => this.diagnoses.set(data),
  });
}
```

3. Verificar que el template del padre pasa `[diagnoses]` al componente hijo. Si no lo tiene, agregar:
```html
<app-professional-diagnoses
  [personId]="person.id"
  [diagnoses]="diagnoses()"
  (diagnosesChange)="onDiagnosesChange($event)">
</app-professional-diagnoses>
```

---

## Paso 2 — Agregar permisos y detección del profesional actual

**Archivo:** `professional-diagnoses.component.ts`

1. Inyectar `AuthService` y `ToastService`:
```typescript
private readonly authService = inject(AuthService);
private readonly toastService = inject(ToastService);
// Importar: import { AuthService, ToastService } from '@services';
```

2. Agregar propiedades de permisos y estado:
```typescript
canCreate = this.authService.hasPermission('diagnoses:create');
canUpdate = this.authService.hasPermission('diagnoses:update');
private readonly currentUserId = this.authService.getCurrentUser()?.id ?? '';
saving = signal(false);
```

> **Por qué `getCurrentUser()?.id`:** El JWT no tiene un claim `professionalId`. Tiene `sub` (el UserId del User). El backend ahora incluye `createdByUserId` (el UserId del Professional) en `DiagnosisListItemResponse`, por lo que la comparación es directa.

3. Agregar helper para saber si el usuario actual es el creador:
```typescript
isCreator(diag: DiagnosisListItemResponse): boolean {
  return diag.createdByUserId === this.currentUserId;
}
```

---

## Paso 3 — Timeline visual

**En el template del componente**, reemplazar el `@for` actual por una estructura de timeline:

```html
<div class="diagnosis-timeline">
  @for (diag of currentDiagnoses(); track diag.id; let last = $last) {
    <div class="timeline-item" [class.timeline-last]="last">
      <div class="timeline-marker"></div>
      <div class="timeline-content">
        <div class="d-flex justify-content-between align-items-start">
          <div>
            <span class="timeline-date">{{ diag.diagnosisDate | date:'dd/MM/yyyy' }}</span>
            <span class="text-body-secondary ms-2 small">{{ diag.professionalName }}</span>
            @if (!isCreator(diag)) {
              <c-badge color="secondary" class="ms-2">Solo lectura</c-badge>
            }
            <p class="mb-0 mt-1">{{ diag.primaryDiagnosis }}</p>
          </div>
          @if (canUpdate && isCreator(diag)) {
            <button cButton color="link" size="sm" (click)="openEdit(diag)"
                    aria-label="Editar diagnóstico">Editar</button>
          } @else {
            <button cButton color="link" size="sm" (click)="openEdit(diag)"
                    aria-label="Ver diagnóstico">Ver</button>
          }
        </div>
      </div>
    </div>
  }
</div>
```

**CSS** — Crear `professional-diagnoses.component.scss` (o agregar estilos al componente si es inline):

```scss
.diagnosis-timeline {
  position: relative;
  padding-left: 1.5rem;

  &::before {
    content: '';
    position: absolute;
    left: 0.5rem;
    top: 0.5rem;
    bottom: 0.5rem;
    width: 2px;
    background-color: var(--cui-border-color);
  }
}

.timeline-item {
  position: relative;
  margin-bottom: 1rem;

  &.timeline-last {
    margin-bottom: 0;
  }
}

.timeline-marker {
  position: absolute;
  left: -1.25rem;
  top: 0.25rem;
  width: 0.75rem;
  height: 0.75rem;
  border-radius: 50%;
  background-color: var(--a11y-text-secondary);
  border: 2px solid var(--a11y-bg);
}

.timeline-date {
  font-weight: 600;
  color: var(--a11y-text);
}

.timeline-content {
  background-color: var(--cui-card-bg, var(--a11y-bg-secondary));
  border: 1px solid var(--cui-border-color);
  border-radius: var(--cui-border-radius);
  padding: 0.75rem 1rem;
}
```

Si el componente usa `template` inline, convertilo a `templateUrl` + `styleUrl` y mové el HTML y SCSS a archivos separados.

---

## Paso 4 — Modal: read-only, loading y validación

### 4a — Modo lectura cuando no es el creador

En el modal, deshabilitar todos los campos si el usuario no es el creador:

```html
<c-modal [visible]="showModal()" ...>
  <c-modal-header>
    <strong>
      @if (editing() && !isCreatorEditing()) {
        Ver Diagnóstico
        <c-badge color="secondary" class="ms-2">Solo lectura</c-badge>
      } @else {
        {{ editing() ? 'Editar Diagnóstico' : 'Nuevo Diagnóstico' }}
      }
    </strong>
  </c-modal-header>
  <c-modal-body>
    <!-- Agregar [attr.disabled]="(editing() && !isCreatorEditing()) ? true : null"
         a todos los inputs y textareas -->
    <input cFormControl type="date" [(ngModel)]="form.diagnosisDate"
           [attr.disabled]="(editing() && !isCreatorEditing()) ? true : null" />
    ...
  </c-modal-body>
  <c-modal-footer>
    <button cButton color="secondary" (click)="closeModal()">
      {{ editing() && !isCreatorEditing() ? 'Cerrar' : 'Cancelar' }}
    </button>
    @if (!editing() || isCreatorEditing()) {
      <button cButton color="primary" (click)="save()"
              [disabled]="!form.primaryDiagnosis || saving()"
              aria-label="Guardar diagnóstico">
        @if (saving()) { <c-spinner size="sm" class="me-2"></c-spinner> }
        {{ editing() ? 'Guardar cambios' : 'Crear diagnóstico' }}
      </button>
    }
  </c-modal-footer>
</c-modal>
```

Agregar en el componente:
```typescript
isCreatorEditing(): boolean {
  const d = this.editing();
  return d ? d.professionalId === this.currentProfessionalId : true;
}
```

### 4b — Spinner y toasts en `save()`

```typescript
save(): void {
  if (!this.form.primaryDiagnosis?.trim()) return;
  this.saving.set(true);

  const op = this.editing()
    ? this.diagnosesService.update(this.editing()!.id, this.form)
    : this.diagnosesService.create(this.personId, this.form);

  op.subscribe({
    next: () => {
      this.toastService.success(
        this.editing() ? 'Diagnóstico actualizado' : 'Diagnóstico creado'
      );
      this.saving.set(false);
      this.showModal.set(false);
      this.loadDiagnoses();
    },
    error: (err) => {
      this.saving.set(false);
      const msg = err?.error?.message ?? 'Error al guardar el diagnóstico';
      this.toastService.error(msg);
    },
  });
}
```

### 4c — Validación visible del campo requerido

Agregar una variable de control:
```typescript
submitted = false;
```

En `save()`, al inicio: `this.submitted = true;`

En el template, mostrar error bajo el textarea de diagnóstico principal:
```html
<textarea cFormControl rows="2" [(ngModel)]="form.primaryDiagnosis"
          [valid]="submitted && !form.primaryDiagnosis ? false : undefined"></textarea>
@if (submitted && !form.primaryDiagnosis) {
  <c-form-feedback [valid]="false">El diagnóstico principal es obligatorio.</c-form-feedback>
}
```

Agregar `FormFeedbackComponent` a los imports del componente.

Resetear `submitted = false` en `closeModal()` y `openNew()`.

---

## Paso 5 — Header con botón "Nuevo" con permisos

```html
<div class="d-flex justify-content-between align-items-center mb-3">
  <h6 class="mb-0">Diagnósticos registrados</h6>
  @if (canCreate) {
    <button cButton color="primary" size="sm" (click)="openNew()"
            aria-label="Nuevo diagnóstico">
      <svg cIcon name="cilPlus" class="me-1"></svg>
      Nuevo diagnóstico
    </button>
  }
</div>
```

---

## Paso 6 — Estado vacío y loading

Agregar un signal `loading`:
```typescript
loading = signal(false);
```

Actualizar `loadDiagnoses()`:
```typescript
private loadDiagnoses(): void {
  this.loading.set(true);
  this.diagnosesService.getByPerson(this.personId).subscribe({
    next: (data) => {
      this.currentDiagnoses.set(data);
      this.diagnosesChange.emit(data);
      this.loading.set(false);
    },
    error: () => {
      this.loading.set(false);
      this.toastService.error('Error al cargar los diagnósticos');
    },
  });
}
```

En el template, antes del `@for`:
```html
@if (loading()) {
  <div class="text-center py-4">
    <c-spinner aria-label="Cargando diagnósticos"></c-spinner>
  </div>
} @else if (currentDiagnoses().length === 0) {
  <p class="text-body-secondary">No hay diagnósticos registrados para esta persona.</p>
} @else {
  <!-- timeline -->
}
```

---

## Checklist de implementación

- [ ] Paso 1: `loadDiagnoses()` implementado en el padre
- [ ] Paso 2: `AuthService` y `ToastService` inyectados, permisos y `isCreator()` definidos
- [ ] Paso 3: Timeline visual con CSS (línea + dots + cards)
- [ ] Paso 4a: Modal read-only cuando no es el creador
- [ ] Paso 4b: Spinner y toasts en `save()`
- [ ] Paso 4c: Validación visible del campo requerido
- [ ] Paso 5: Botón "Nuevo" con check de permiso `diagnoses:create`
- [ ] Paso 6: Estado de carga con spinner y manejo de error
- [ ] Verificar que el badge en el tab del padre muestra el conteo correcto
- [ ] Probar con diagnóstico de otro profesional → debe aparecer "Solo lectura" y no mostrar "Editar"
- [ ] Probar creación → toast de éxito, timeline actualizado
- [ ] Probar edición → toast de éxito
- [ ] Probar error de red → toast de error

---

## Imports necesarios en el componente

Agregar a `imports: [...]`:
```typescript
SpinnerComponent,      // @coreui/angular
FormFeedbackComponent, // @coreui/angular
BadgeComponent,        // @coreui/angular
IconDirective,         // @coreui/icons-angular (cilPlus)
```

Agregar a `@coreui/angular` imports del TS:
```typescript
SpinnerComponent,
FormFeedbackComponent,
BadgeComponent,
```
