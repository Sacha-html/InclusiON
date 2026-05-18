# Changelog de Documentación — 2026-05-15

> Cambios realizados para alinear la documentación con el código actual.
> Motivo: se corrigieron validaciones en DTOs, nullabilidad en modelos EF y discrepancias entre DER y migraciones.

---

## Carpeta: `Docs/`

### `der.md` — 24 correcciones de nullabilidad

El DER declaraba `"NOT NULL"` para columnas que el modelo C#, la configuración EF y la migración tratan como `nullable` (y viceversa). Se actualizaron para reflejar el estado real del código.

| # | Entidad | Columna | Antes | Después |
|---|---------|---------|:-----:|:-------:|
| 1 | User | `Email` | NOT NULL | nullable |
| 2 | User | `Name` | NOT NULL | nullable |
| 3 | User | `Surname` | NOT NULL | nullable |
| 4 | SkillArea | `Icon` | NOT NULL | nullable |
| 5 | SkillArea | `Color` | NOT NULL | nullable |
| 6 | Professional | `DocumentNumber` | NOT NULL | nullable |
| 7 | PersonWithDisability | `DisabilityTypeId` | NOT NULL | nullable |
| 8 | PersonWithDisability | `AutonomyLevelId` | NOT NULL | nullable |
| 9 | PersonWithDisability | `LoginMethodId` | NOT NULL | nullable |
| 10 | PersonWithDisability | `DocumentNumber` | NOT NULL | nullable |
| 11 | PersonWithDisability | `AvatarColor` | NOT NULL | nullable |
| 12 | PersonWithDisability | `BirthDate` | nullable | **NOT NULL** ⬅ |
| 13 | FamilyRepresentative | `DocumentNumber` | NOT NULL | nullable |
| 14 | FamilyRepresentative | `Relationship` | NOT NULL | nullable |
| 15 | PersonRepresentative | `Relationship` | NOT NULL | nullable |
| 16 | Invitation | `ForPersonId` | NOT NULL | nullable |
| 17 | Activity | `SkillAreaId` | NOT NULL | nullable |
| 18 | Activity | `ComplexityLevel` | NOT NULL | nullable |
| 19 | ActivityResponse | `Result` | NOT NULL | nullable |
| 20 | ActivityResponse | `SuccessPercentage` | NOT NULL | nullable |
| 21 | ActivityResponse | `FrustrationLevel` | NOT NULL | nullable |
| 22 | Report | `PeriodStartDate` | NOT NULL | nullable |
| 23 | Report | `PeriodEndDate` | NOT NULL | nullable |
| 24 | Message | `Subject` | NOT NULL | nullable |
| 25 | AccessAudit | `Role` | NOT NULL | nullable |

> **Nota**: La tabla `SkillArea.Color` perdió el comentario de formato `#RRGGBB` al cambiar a nullable. Si se desea mantener como documentación de formato, agregarlo manualmente.

### `CU/CU-02-gestion-usuarios.md`

**Línea 19** — Lista de campos obligatorios/opcionales en registro de profesional:
- Antes: `...DNI, teléfono, matrícula, institución (opcionales)`
- Después: `...DNI, especialidad, fecha de nacimiento (obligatorios); teléfono, matrícula, institución (opcionales)`
- **Motivo**: `CreateProfessionalRequest.DocumentNumber` se volvió `[Required]`

---

## Carpeta: `ABM/`

### `04-profesionales.md`

**Línea 21** — Tabla de campos de alta:
- Antes: `DNI | Texto (20) | No | Único si se ingresa`
- Después: `DNI | Texto (20) | Sí | Único`

**Línea 30** — Validación de integridad:
- Antes: `El DNI, si se ingresa, no puede existir en otro Professional.`
- Después: `El DNI no puede existir en otro Professional.`

- **Motivo**: `CreateProfessionalRequest.DocumentNumber` es `[Required]`

### `05-personas.md`

**Línea 21** — Tabla de campos de alta:
- Antes: `Tipo de discapacidad | Referencia | No | Debe existir y estar activo en el catálogo`
- Después: `Tipo de discapacidad | Referencia | Sí | Debe existir y estar activo en el catálogo`

- **Motivo**: `CreatePersonRequest.DisabilityTypeId` es `[Required]`

---

## Carpeta: `HU/`

### `HU-IN-149-auto-registro-profesional.md`

| Línea | Antes | Después |
|:-----:|-------|---------|
| 25 | `Nombre, Apellido, Email (obligatorios)` / `Documento (opcional)` | `Nombre, Apellido, Email, Documento (obligatorios)` |
| 26 | _(era línea separada para Documento)_ | _(eliminada, unificada con la línea anterior)_ |
| 43 | `...nombre, apellido, email, especialidad, fecha de nacimiento` | `...nombre, apellido, email, documento, especialidad, fecha de nacimiento` |
| 92 | `Validators.required en nombre, apellido, email, especialidad, fecha de nacimiento` | `Validators.required en nombre, apellido, email, documento, especialidad, fecha de nacimiento` |

- **Motivo**: `DocumentNumber` es obligatorio en creación profesional

---

## Carpeta: Raíz `InclusiON.Documents/`

### `diccionario-datos.md`

**Línea 126** — `Professional.DNI`:
- Antes: `No | Documento de identidad (único)`
- Después: `Sí* | Documento de identidad (único) — obligatorio en creación, opcional en BD para registros legacy`

**Línea 163** — `PersonWithDisability.Tipo de discapacidad`:
- Antes: `No | Del catálogo de tipos de discapacidad`
- Después: `Sí | Del catálogo de tipos de discapacidad`

- **Motivo**: alineación con validaciones `[Required]` en DTOs

---

## Sin cambios en código

Los siguientes cambios de código de la sesión actual **no** requirieron actualización de documentación porque los documentos ya estaban alineados:

| Cambio de código | Documentos verificados | Resultado |
|------------------|----------------------|-----------|
| `UpdateFamilyRequest.Relationship` nullable | `06-familiares.md` | Ya documentado como opcional ✅ |
| Route ordering `activities/new` > `activities/:id` | `08-actividades.md`, `CU-04-actividades.md` | No especifican orden de rutas ✅ |
| Messages endpoints `{id:int}` → `{id}` | `13-mensajes.md`, `CU-10-mensajeria.md` | Ya usan `{id}` sin `:int` ✅ |

---

## Pendiente para Confluence

Al migrar a Confluence, verificar:

1. **DER**: Las 25 columnas corregidas arriba
2. **ABM Profesionales**: DNI como obligatorio
3. **ABM Personas**: Tipo de discapacidad como obligatorio
4. **CU-02**: Lista de obligatorios actualizada
5. **HU-IN-149**: Documento en las 4 listas de obligatorios
6. **Diccionario de datos**: DNI (Sí\*) y DisabilityType (Sí)
