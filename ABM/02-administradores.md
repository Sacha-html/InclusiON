# ABM — Administradores

**Actor:** Administrador Global  
**Justificación:** El Administrador Global necesita delegar la gestión de instituciones específicas a Administradores Institucionales. Sin este ABM, toda la carga administrativa recae en el Admin Global; es inescalable para una plataforma con múltiples instituciones. También requiere gestionar su propio panel de admins globales.

**Entidades:** `User` (rol Admin Global y Admin Institucional), `AdminInstitution`

---

## Alta — Administrador

**Actor:** Administrador Global

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (100) | Sí | No vacío |
| Apellido | Texto (100) | Sí | No vacío |
| Email | Texto (255) | Sí | Formato válido; único en `User` |
| Tipo de admin | Enumerado | Sí | `GlobalAdmin` o `InstitutionalAdmin` |
| Institución (si institucional) | Referencia | Condicional | Obligatorio si tipo = `InstitutionalAdmin`; institución debe estar activa |

**Validaciones de integridad:**
- El email no puede existir ya en la tabla `User`.
- Si es Admin Institucional, la institución de destino debe estar activa.

**Resultado:**
- Se crea un `User` con `MustChangePassword = true` y contraseña temporal.
- Si es Admin Institucional, se crea el registro en `AdminInstitution`.
- Se envía email de bienvenida con contraseña temporal.

---

## Baja — Administrador

**Actor:** Administrador Global

- Se establece `IsActive = false` en `User` (baja lógica).
- Si era Admin Institucional, se desactiva el registro en `AdminInstitution`.
- **Validación:** No se puede dar de baja al propio usuario que realiza la operación.

---

## Modificación — Administrador

**Actor:** Administrador Global

Campos editables:

| Campo | Validaciones |
|-------|--------------|
| Nombre | No vacío |
| Apellido | No vacío |
| Email | Formato válido; único (excluyendo registro actual) |

**No se puede modificar:** el tipo de admin (Global → Institucional requiere recrear el usuario).

---

## Listado — Administradores

**Actor:** Administrador Global

| Columna | Descripción |
|---------|-------------|
| Nombre y Apellido | Identidad del administrador |
| Email | Email de la cuenta |
| Tipo | Global / Institucional |
| Institución | Institución asignada (solo admins institucionales) |
| Último acceso | Fecha del último login |
| Estado | Activo / Inactivo |

**Filtros disponibles:** nombre/email, tipo de admin, institución, estado.  
**Persistencia:** Consulta a `User` filtrado por rol admin, con join a `AdminInstitution` para los institucionales.
