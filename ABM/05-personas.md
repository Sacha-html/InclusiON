# ABM — Personas con Discapacidad

**Actor:** Profesional  
**Justificación:** El Profesional es quien conoce a las personas con discapacidad que atiende y es responsable de incorporarlas al sistema. Sin este ABM, la persona no puede acceder a la plataforma, no puede tener un roadmap asignado ni realizar actividades. El profesional también configura el perfil funcional y de accesibilidad que determina cómo la persona interactúa con el sistema.

**Entidades:** `PersonWithDisability`, `User`, `PersonSkillProfile`

---

## Alta — Persona con Discapacidad

**Actor:** Profesional (aprobado por su institución)

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (100) | Sí | No vacío |
| Apellido | Texto (100) | Sí | No vacío |
| DNI | Texto (20) | No | Único si se ingresa |
| Fecha de nacimiento | Fecha | Sí | Debe ser pasada |
| Email (para cuenta) | Texto (255) | No | Formato válido; único en `User` si se ingresa |
| Tipo de discapacidad | Referencia | No | Debe existir y estar activo en el catálogo |
| Nivel de autonomía | Referencia | No | Debe existir y estar activo |
| Método de login | Referencia | No | Debe existir y estar activo; coherente con nivel de autonomía |
| Color de avatar | Texto (10) | No | Hex válido (`#RRGGBB`) |
| **Perfil funcional** | | | |
| Nivel de atención | Entero (1-5) | No | Entre 1 y 5 |
| Nivel de comunicación | Entero (1-5) | No | Entre 1 y 5 |
| Nivel de motricidad | Entero (1-5) | No | Entre 1 y 5 |
| Usa CAA | Booleano | No | — |
| Usa lengua de señas | Booleano | No | — |
| Intereses y motivadores | Texto (500) | No | — |
| Estilo de aprendizaje | Enumerado | No | Visual, Auditivo, Kinestésico |
| Recursos disponibles | Texto (255) | No | — |
| Terapias adicionales | Texto (500) | No | — |
| **Accesibilidad** | | | |
| Requiere fuente grande | Booleano | No | — |
| Requiere alto contraste | Booleano | No | — |
| Sensibilidad al ruido visual | Booleano | No | — |
| Sensibilidad al sonido | Booleano | No | — |

**Validaciones de integridad:**
- El email (si se ingresa) no puede existir ya en `User`.
- El DNI (si se ingresa) no puede existir en otro `PersonWithDisability`.
- El método de login debe ser coherente con el nivel de autonomía seleccionado.

**Resultado:**
- Se crea `PersonWithDisability` con `Activo = true`.
- Si se ingresó email, se crea `User` con `MustChangePassword = true`.
- Se vincula automáticamente el Profesional creador con la persona en `ProfessionalPerson` (como profesional principal).

---

## Alta — Áreas de Habilidad del Perfil

**Actor:** Profesional

Permite asignar las áreas de habilidad que se trabajarán con la persona (tabla `PersonSkillProfile`).

| Campo | Validaciones |
|-------|--------------|
| Persona | Debe existir y estar activa |
| Área de habilidad | Debe existir y estar activa; no puede estar ya asignada a esta persona |

---

## Baja — Persona con Discapacidad

**Actor:** Profesional / Administrador Institucional

- Se establece `Activo = false` en `PersonWithDisability` e `IsActive = false` en su `User`.
- **Impacto en cadena:** Se desactivan todas las `ProfessionalPerson`, `PersonRepresentative` y `ActivityAssignment` pendientes asociadas.
- **Validación:** El profesional que da de baja debe tener asignada a la persona (`ProfessionalPerson` activo).

---

## Baja — Área de Habilidad del Perfil

**Actor:** Profesional

- Se establece `Activo = false` en `PersonSkillProfile`.
- **Validación:** El área no debe tener un `PersonRoadmapArea` activo en el roadmap de la persona.

---

## Modificación — Persona con Discapacidad

**Actor:** Profesional

Todos los campos del alta son editables excepto el Email de cuenta (requiere flujo aparte por cambio de credenciales).

| Campo | Validaciones |
|-------|--------------|
| DNI | Único (excluyendo registro actual) |
| Nivel de autonomía + Método de login | Coherentes entre sí |
| Niveles funcionales | Entre 1 y 5 |
| Color de avatar | Hex válido si se ingresa |

---

## Listado — Personas con Discapacidad

**Actor:** Profesional (ve solo sus personas asignadas)

| Columna | Descripción |
|---------|-------------|
| Nombre y Apellido | Identidad de la persona |
| DNI | Documento (si existe) |
| Edad | Calculada desde fecha de nacimiento |
| Tipo de discapacidad | Del catálogo |
| Nivel de autonomía | Del catálogo |
| Profesional principal | Nombre del profesional principal |
| Tiene roadmap | Sí / No |
| Estado | Activo / Inactivo |

**Filtros disponibles:** nombre/DNI, tipo de discapacidad, nivel de autonomía, tiene roadmap, estado.  
**Persistencia:** Consulta a `PersonWithDisability` filtrada por `ProfessionalPerson.ProfessionalId` del profesional autenticado.  
**Admin Global/Institucional:** puede ver todas las personas de su institución sin filtro de asignación.
