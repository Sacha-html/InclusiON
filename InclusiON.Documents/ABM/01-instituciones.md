# ABM — Institución Educativa

**Actor:** Administrador Institucional  
**Justificación:** El Administrador Institucional gestiona la información de la sede educativa (escuela, centro de rehabilitación) donde operan los profesionales y personas con discapacidad. Permite registrar y mantener actualizada la denominación oficial, el domicilio y los canales de contacto institucional que encabezan los informes pedagógicos y boletines.

**Entidades:** `EducationalInstitution`

---

## Alta — Configuración Inicial de la Sede

**Actor:** Administrador Institucional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (255) | Sí | No vacío; único en el sistema |
| Dirección | Texto (255) | No | — |
| Teléfono | Texto (20) | No | Solo números si se ingresa |
| Email | Texto (100) | No | Formato email válido si se ingresa |

**Validaciones de integridad:**
- El nombre de la institución debe ser único (case-insensitive).
- Se persiste con `Activo = true`.

**Resultado:** Se crea o inicializa el registro en `EducationalInstitution`.

---

## Modificación — Datos Institucionales

**Actor:** Administrador Institucional

Campos editables:

| Campo | Validaciones |
|-------|--------------|
| Nombre | No vacío; único (excluyendo el registro actual) |
| Dirección | Domicilio físico de la sede |
| Teléfono | Formato numérico de contacto |
| Email | Formato válido de correo electrónico |

---

## Consulta / Ficha de la Sede

**Actor:** Administrador Institucional

| Columna / Campo | Descripción |
|-----------------|-------------|
| Nombre | Nombre oficial de la escuela o centro |
| Email | Correo institucional |
| Teléfono | Teléfono de contacto oficial |
| Dirección | Domicilio físico de la sede |
| Profesionales activos | Cantidad de docentes y terapeutas vinculados |
| Estado | Activo / Inactivo |

**Persistencia:** Consulta y actualización en `EducationalInstitution`.
