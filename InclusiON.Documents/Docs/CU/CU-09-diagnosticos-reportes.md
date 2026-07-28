# Módulo 9 — Diagnósticos y Reportes

---

## CU-37: Registrar diagnóstico funcional

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-08 |
| **Prioridad** | Crítica |

**Precondiciones**
- El Profesional tiene asignación activa con la Persona.

**Flujo principal**
1. El Profesional accede al perfil de la Persona y entra a la sección "Diagnósticos".
2. Selecciona "Nuevo diagnóstico".
3. El sistema muestra el formulario con los campos:
   - Fecha del diagnóstico (obligatoria)
   - Diagnóstico principal (obligatorio)
   - Observaciones iniciales
   - Capacidades identificadas
   - Desafíos identificados
   - Apoyos requeridos
   - Objetivos pedagógicos
   - Estrategias recomendadas
4. El Profesional completa los datos y guarda.
5. El sistema persiste el diagnóstico con los campos clínicos cifrados (AES-256-GCM).

**Flujos alternativos**
- **4a. Campos obligatorios vacíos:** El sistema muestra validación inline y bloquea el guardado.
- **1a. Profesional sin asignación:** El sistema devuelve `403 Forbidden`.

**Postcondiciones**
- El diagnóstico aparece en el historial cronológico de la Persona.
- Solo el Profesional creador puede editarlo; los demás lo ven en modo solo lectura.

---

## CU-38: Editar diagnóstico propio

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional (creador del diagnóstico) |
| **Actores secundarios** | — |
| **HU de referencia** | HU-08 |
| **Prioridad** | Media |

**Precondiciones**
- El Profesional es el creador del diagnóstico.
- Tiene asignación activa con la Persona.

**Flujo principal**
1. El Profesional accede al historial de diagnósticos de la Persona.
2. Selecciona "Editar" en el diagnóstico que creó.
3. El sistema carga el formulario con los datos actuales.
4. El Profesional modifica los campos deseados y guarda.
5. El sistema actualiza el diagnóstico con los campos clínicos cifrados.

**Flujos alternativos**
- **2a. Diagnóstico de otro Profesional:** El sistema no muestra el botón de edición; aparece indicador "Solo lectura — creado por [nombre]".

**Postcondiciones**
- El diagnóstico queda actualizado.

---

## CU-39: Consultar historial de diagnósticos

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-08 / HU-IN-86 |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional tiene asignación activa con la Persona.

**Flujo principal**
1. El Profesional accede a la sección "Diagnósticos" del perfil de la Persona.
2. El sistema muestra los diagnósticos en vista de timeline cronológico descendente:
   - Línea vertical con puntos por fecha.
   - Cada punto muestra fecha, diagnóstico principal y Profesional creador.
3. El Profesional expande un diagnóstico para ver el detalle completo.
4. Los diagnósticos ajenos muestran indicador "Solo lectura".

**Flujos alternativos**
- **Sin diagnósticos:** El sistema muestra estado vacío con acción "Registrar primer diagnóstico".
- **Profesional sin asignación:** El sistema devuelve `403 Forbidden`.

**Postcondiciones**
- El Profesional tiene visibilidad del historial clínico de la Persona.

---

## CU-40: Crear reporte de progreso

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | — |
| **HU de referencia** | HU-08 |
| **Prioridad** | Crítica |

**Precondiciones**
- El Profesional tiene asignación activa con la Persona.

**Flujo principal**
1. El Profesional accede al perfil de la Persona y entra a "Reportes".
2. Selecciona "Nuevo reporte".
3. Completa el formulario:
   - Tipo de reporte (del catálogo)
   - Título y fecha
   - Descripción del progreso (texto libre)
   - Metas alcanzadas
   - Áreas a reforzar
   - Recomendaciones futuras
   - Próximos objetivos
4. El sistema guarda el reporte en estado `Draft`.

**Flujos alternativos**
- **4a. Guardado parcial:** El Profesional puede guardar el borrador incompleto y retomarlo luego.

**Postcondiciones**
- El reporte existe en estado `Draft`, visible solo para el Profesional.
- El Familiar **no** puede ver el reporte en estado `Draft`.

---

## CU-41: Enviar reporte para aprobación

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | Admin (receptor), Sistema |
| **HU de referencia** | HU-08 |
| **Prioridad** | Alta |

**Precondiciones**
- El reporte existe en estado `Draft`.
- El Profesional es el creador del reporte.

**Flujo principal**
1. El Profesional abre el reporte en estado `Draft` y selecciona "Enviar para aprobación".
2. El sistema cambia el estado del reporte a `Submitted`.
3. El Admin recibe notificación del reporte pendiente de revisión.

**Flujos alternativos**
- **2a. Campos obligatorios incompletos:** El sistema muestra validación y bloquea el envío.

**Postcondiciones**
- El reporte queda en estado `Submitted`.
- El Admin puede ver el reporte en la cola de aprobación.
- El Familiar **no** puede ver el reporte en estado `Submitted`.

---

## CU-42: Aprobar reporte de progreso

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin Global / Admin Institucional |
| **Actores secundarios** | Sistema, Familiar (receptor) |
| **HU de referencia** | HU-08 |
| **Prioridad** | Alta |

**Precondiciones**
- El reporte existe en estado `Submitted`.
- Admin Institucional: el reporte pertenece a una persona de su institución.

**Flujo principal**
1. El Admin accede a la cola de reportes pendientes.
2. Lee el reporte completo.
3. Selecciona "Aprobar".
4. El sistema cambia el estado del reporte a `Approved`.
5. El reporte queda visible para el Familiar vinculado a la Persona.

**Postcondiciones**
- El Familiar puede consultar el reporte aprobado en su dashboard (CU-44).
- El reporte no puede volver a estado `Draft` o `Submitted`.

---

## CU-43: Rechazar reporte de progreso

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin Global / Admin Institucional |
| **Actores secundarios** | Sistema, Profesional (receptor) |
| **HU de referencia** | HU-08 |
| **Prioridad** | Alta |

**Precondiciones**
- El reporte existe en estado `Submitted`.

**Flujo principal**
1. El Admin accede a la cola de reportes pendientes y lee el reporte.
2. Selecciona "Rechazar" e ingresa un comentario con el motivo (obligatorio).
3. El sistema cambia el estado del reporte a `Rejected`.
4. El Profesional recibe notificación con el motivo del rechazo.
5. El Profesional puede editar el reporte y volver a enviarlo.

**Flujos alternativos**
- **2a. Sin comentario:** El sistema bloquea el rechazo sin motivo.

**Postcondiciones**
- El reporte queda en estado `Rejected` con el comentario del Admin.
- El Familiar **no** puede ver el reporte rechazado.

---

## CU-44: Consultar reportes aprobados

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Familiar |
| **Actores secundarios** | — |
| **HU de referencia** | HU-08 |
| **Prioridad** | Alta |

**Precondiciones**
- El Familiar está autenticado.
- Existe al menos un reporte en estado `Approved` para su persona vinculada.

**Flujo principal**
1. El Familiar accede a la sección "Reportes" desde su dashboard.
2. El sistema lista los reportes aprobados de su persona vinculada, en orden cronológico descendente.
3. El Familiar selecciona un reporte para ver el detalle completo.

**Flujos alternativos**
- **Sin reportes aprobados:** El sistema muestra "Aún no hay reportes disponibles."
- **Familiar sin vínculo activo:** El sistema devuelve `404` (política de privacidad).

**Postcondiciones**
- El Familiar accede a la información de progreso validada por el Admin.
