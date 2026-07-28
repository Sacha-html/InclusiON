# ABM — Catálogos de Referencia

**Actor:** Administrador Global  
**Justificación:** Los catálogos son los datos maestros que alimentan todos los formularios del sistema (tipos de discapacidad, niveles de autonomía, categorías de actividad, áreas de habilidad, templates, tipos de reporte). Si no existen o están desactualizados, ningún otro actor puede operar correctamente: los profesionales no pueden clasificar actividades, los familiares no pueden registrar a la persona, etc. El Admin Global es quien los mantiene actualizados.

**Entidades:** `DisabilityType`, `AutonomyLevel`, `ActivityCategory`, `SkillArea`, `ActivityTemplateType`, `ReportType`

> **`LoginMethod`** es de solo lectura en el panel admin (sus valores son seed del sistema: STANDARD, PIN, ASSISTED, FAMILY). No tiene ABM de usuario.

---

## Catálogos simples: DisabilityType, ActivityCategory, ReportType

Comparten el mismo esquema de operaciones.

### Alta

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (100) | Sí | No vacío; único dentro del catálogo |
| Descripción | Texto (500) | No | — |

**Resultado:** Registro creado con `Activo = true`.

### Baja

- Baja lógica: `Activo = false`.
- **Validación antes de dar de baja:**
  - `DisabilityType`: no debe estar asignado a ninguna `PersonWithDisability` activa.
  - `ActivityCategory`: no debe tener `Activity` activas asociadas.
  - `ReportType`: no debe tener `Report` activos asociados.

### Modificación

| Campo | Validaciones |
|-------|--------------|
| Nombre | No vacío; único (excluyendo registro actual) |
| Descripción | — |

### Listado

| Columna | Descripción |
|---------|-------------|
| Nombre | Nombre del ítem del catálogo |
| Descripción | Descripción breve |
| Registros vinculados | Cantidad de entidades que lo referencian |
| Estado | Activo / Inactivo |

---

## AutonomyLevel

### Alta

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (100) | Sí | No vacío; único |
| Descripción | Texto (500) | No | — |
| Requiere supervisión | Booleano | Sí | — |
| Orden de visualización | Entero | Sí | Positivo; único |

### Baja

- Baja lógica.
- **Validación:** No debe estar asignado a ninguna `PersonWithDisability` activa.

### Modificación

Todos los campos excepto `Id` son editables, con las mismas validaciones del alta.

### Listado

Nombre, nivel de supervisión, orden, estado. Ordenado por `Orden de visualización`.

---

## SkillArea

### Alta

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (100) | Sí | No vacío; único |
| Descripción | Texto (500) | No | — |
| Ícono | Texto (50) | No | — |
| Color | Texto (10) | No | Formato hex `#RRGGBB` si se ingresa |
| Orden de visualización | Entero | Sí | Positivo; único |

### Baja

- Baja lógica.
- **Validación:** No debe tener `ActivityTemplateType` activos asociados ni estar en `PersonSkillProfile` de personas activas.

### Modificación

Todos los campos excepto `Id` son editables.

### Listado

Nombre, ícono, color, orden, estado. Ordenado por `Orden de visualización`.

---

## ActivityTemplateType

### Alta

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Área de habilidad | Referencia | Sí | Debe existir y estar activa |
| Nombre | Texto (150) | Sí | No vacío; único |
| Código | Texto (único) | Sí | Mayúsculas y guiones bajos; único en el sistema |
| Descripción | Texto (500) | No | — |
| Esquema de contenido | JSON | Sí | JSON válido que define los campos del formulario dinámico |
| Nombre del componente | Texto (100) | Sí | No vacío |
| Usa pictogramas | Booleano | Sí | — |
| Tiene audio | Booleano | Sí | — |
| Orden de visualización | Entero | Sí | Positivo |

### Baja

- Baja lógica.
- **Validación:** No debe tener `ActivityContent` activos asociados.

### Modificación

Todos los campos excepto `Id` y `Código` son editables. El código no puede modificarse una vez creado (podría romper integración con el frontend que lo usa para routing de componentes).

### Listado

Nombre, código, área de habilidad, usa pictogramas, tiene audio, estado. Filtros: área de habilidad, estado.
