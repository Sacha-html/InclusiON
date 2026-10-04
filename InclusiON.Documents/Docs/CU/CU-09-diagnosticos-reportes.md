# Módulo 9 — Diagnósticos y Reportes

Este módulo gestiona la valoración funcional continua bajo el Diseño Universal para el Aprendizaje (DUA) y el ciclo de vida de los informes pedagógicos y evolutivos del estudiante. Garantiza el resguardo de la información clínica y pedagógica sensible mediante cifrado criptográfico AES-256-GCM, el control de acceso estricto por asignación activa, el circuito de aprobación y auditoría por parte del Equipo Directivo de la institución escolar, la exportación formal a PDF y la comunicación transparente con las familias mediante acuse de recibo.

---

## CU-37: Registrar Diagnóstico Funcional

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente / Terapeuta |
| **Actores secundarios** | Sistema (cifrado criptográfico AES-256-GCM y auditoría) |
| **HU de referencia** | HU-08 / HU-IN-83–85 |
| **Prioridad** | Crítica |
| **Precondiciones** | El Profesional tiene sesión activa y asignación formal vigente con el Alumno (`[PersonAccess(AccessMode.Write)]`). |

**Flujo principal**
1. El Profesional accede a la ficha del estudiante en "Mi Aula" o "Alumnos" y selecciona la solapa "Diagnósticos".
2. Selecciona la opción "Nuevo diagnóstico".
3. El sistema despliega el formulario modal de valoración funcional con los siguientes campos:
   - Fecha del diagnóstico (`diagnosisDate`, obligatoria, no futura).
   - Diagnóstico principal (`primaryDiagnosis`, obligatorio).
   - Observaciones iniciales (`initialObservations`, opcional).
   - Capacidades identificadas (`identifiedCapabilities`, obligatorio, fortalezas del estudiante bajo enfoque DUA).
   - Desafíos identificados (`identifiedChallenges`, obligatorio, barreras para el aprendizaje y participación).
   - Apoyos requeridos (`requiredSupports`, opcional, andamiajes pedagógicos y adaptaciones de acceso).
   - Objetivos pedagógicos (`pedagogicalObjectives`, opcional, metas individualizadas).
   - Estrategias recomendadas (`recommendedStrategies`, opcional, orientaciones para el aula y el hogar).
4. El Profesional completa los campos y presiona "Guardar".
5. El sistema valida los datos y cifra los campos clínicos sensibles mediante AES-256-GCM antes de persistir el registro en la base de datos.
6. El sistema genera el identificador cifrado URL-safe (`encryptedId`), actualiza la lista cronológica del alumno y emite mensaje de confirmación: *"Diagnóstico creado exitosamente"*.

**Flujos alternativos**
- **4a. Campos obligatorios incompletos:** El sistema muestra validación inline, destaca los campos requeridos y bloquea el guardado.
- **4b. Fecha futura:** Si la fecha supera el día actual, el sistema bloquea la acción indicando: *"La fecha del diagnóstico no puede ser futura"*.
- **1a. Profesional sin asignación activa:** El sistema responde `403 Forbidden` impidiendo el registro sobre alumnos no asignados.

**Postcondiciones**
- El diagnóstico queda incorporado al historial cronológico del alumno con sus campos clínicos protegidos por cifrado AES-256-GCM.
- Solo el profesional autor mantiene privilegios de edición; los demás integrantes del equipo escolar asignados al alumno lo visualizan en modo solo lectura.

---

## CU-38: Editar Diagnóstico Propio

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente / Terapeuta (autor del diagnóstico) |
| **Actores secundarios** | Sistema (re-cifrado criptográfico y auditoría) |
| **HU de referencia** | HU-08 / HU-IN-83–85 |
| **Prioridad** | Media |
| **Precondiciones** | El Profesional es el autor original del diagnóstico (`diagnosis.ProfessionalId == currentProfessionalId`), mantiene asignación activa con el alumno y el registro se encuentra activo. |

**Flujo principal**
1. El Profesional accede al historial de diagnósticos del Alumno.
2. Localiza el diagnóstico de su autoría y selecciona "Editar".
3. El sistema descifra los campos en memoria y precarga el formulario con la información vigente.
4. El Profesional modifica las observaciones, capacidades, desafíos o estrategias pedagógicas y presiona "Guardar".
5. El sistema valida los datos, re-cifra el contenido con AES-256-GCM, actualiza `UpdatedAt` y `UpdatedBy`, y persiste las modificaciones.
6. El sistema notifica al usuario: *"Diagnóstico actualizado exitosamente"*.

**Flujos alternativos**
- **2a. Diagnóstico creado por otro Profesional:** El sistema oculta el botón de edición y despliega la vista en modo visor con la etiqueta: *"Solo lectura — creado por [Nombre del Profesional]"*. Si se invoca el endpoint directamente, el backend devuelve `403 Forbidden`.
- **4a. Datos inconsistentes o fecha futura:** El sistema bloquea el guardado y exige corregir los datos.

**Postcondiciones**
- El diagnóstico clínico queda actualizado en su versión cifrada más reciente, preservando la trazabilidad de autoría y auditoría.

---

## CU-39: Consultar Historial y Exportar Diagnóstico Funcional

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente / Equipo Directivo Escolar |
| **Actores secundarios** | Sistema (generación de PDF clínico oficial) |
| **HU de referencia** | HU-08 / HU-IN-86 |
| **Prioridad** | Alta |
| **Precondiciones** | El usuario cuenta con asignación o jurisdicción institucional activa sobre el Alumno (`[PersonAccess(AccessMode.Read)]`). |

**Flujo principal**
1. El usuario accede a la sección "Diagnósticos" dentro del perfil del Alumno.
2. El sistema despliega el historial en formato de timeline cronológico descendente, con filtros por rango de fechas (desde/hasta) y estado (activo/inactivo).
3. Cada registro del timeline expone: fecha del diagnóstico, diagnóstico principal, profesional responsable y badges de estado.
4. El usuario puede expandir cualquier diagnóstico para consultar el detalle completo de capacidades, barreras, apoyos DUA y estrategias en un modal de lectura.
5. El usuario puede seleccionar "Descargar PDF" (`GET /api/diagnoses/{id}/export-pdf`).
6. El sistema genera y descarga un documento PDF formal con membrete institucional, datos del alumno, profesional actuante y valoración funcional estructurada.

**Flujos alternativos**
- **Sin diagnósticos previos:** El sistema muestra una pantalla de estado vacío: *"No hay diagnósticos registrados para este alumno. Comenzá registrando la valoración funcional inicial."* junto a la acción rápida "Nuevo diagnóstico".
- **Usuario sin asignación activa:** El sistema devuelve `403 Forbidden` resguardando la confidencialidad médica y pedagógica.

**Postcondiciones**
- El equipo docente y directivo obtiene visibilidad de la evolución diagnóstica del estudiante y dispone del archivo PDF para legajo o reuniones interdisciplinarias.

---

## CU-40: Crear Reporte de Progreso (Informe Pedagógico y Evolutivo)

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente / Terapeuta |
| **Actores secundarios** | — |
| **HU de referencia** | HU-08 / HU-IN-136 |
| **Prioridad** | Crítica |
| **Precondiciones** | El Profesional tiene asignación activa con el Alumno (`[PersonAccess(AccessMode.Write)]`). |

**Flujo principal**
1. El Profesional accede a "Reportes" (o a la pestaña "Reportes" del perfil del Alumno) y presiona "Nuevo reporte".
2. Completa el formulario de informe:
   - Alumno destinatario (con autocompletado y validación de asignación).
   - Tipo de reporte (seleccionado del catálogo oficial institucional).
   - Título del reporte (obligatorio).
   - Fecha de emisión (por defecto hoy).
   - Período evaluado (fechas de inicio y fin, opcionales).
   - Descripción del progreso (cuerpo narrativo obligatorio).
   - Metas alcanzadas (opcional).
   - Áreas a reforzar (opcional).
   - Próximos objetivos (opcional).
   - Recomendaciones futuras para el aula y el hogar (opcional).
3. El Profesional presiona "Guardar reporte".
4. El sistema valida los datos y persiste el registro en estado `Draft` (Borrador).
5. El sistema presenta un diálogo de confirmación con dos opciones:
   - *Opción A ("Enviar para revisión"):* Transiciona de inmediato el reporte a estado de auditoría directiva (CU-41).
   - *Opción B ("Revisar más tarde"):* Conserva el reporte como borrador y redirige al listado personal.

**Flujos alternativos**
- **2a. Campos obligatorios incompletos:** Si falta el título, el contenido, el tipo o el alumno, el sistema deshabilita el guardado y resalta los campos con error.
- **Guardado parcial de borrador:** El docente puede guardar borradores preliminares incompletos y reanudarlos sucesivamente hasta su finalización.

**Postcondiciones**
- El reporte se crea en base de datos en estado `Draft`.
- Es accesible y editable **únicamente** por el profesional autor.
- El Familiar **no tiene conocimiento ni acceso** a los informes en estado `Draft`.

---

## CU-41: Enviar Reporte para Aprobación Directiva

| Campo | Detalle |
|---|---|
| **Actor principal** | Profesional Docente (autor del reporte) |
| **Actores secundarios** | Equipo Directivo Escolar (receptor notificado), Sistema (notificaciones multicanal) |
| **HU de referencia** | HU-08 / HU-IN-138 |
| **Prioridad** | Alta |
| **Precondiciones** | El reporte se encuentra en estado `Draft` o `Rejected`, y el Profesional autenticado es su autor (`report.ProfessionalId == currentProfessionalId`). |

**Flujo principal**
1. El Profesional abre el informe en estado `Draft` (o subsanado tras un rechazo) y presiona "Enviar para aprobación".
2. El sistema valida la integridad de los campos obligatorios y cambia el estado del reporte a `Submitted` (Enviado / Pendiente de revisión).
3. El sistema encola en segundo plano notificaciones push y alertas para el Equipo Directivo de la institución escolar.
4. El sistema notifica al docente: *"Reporte enviado al equipo directivo para su revisión"*.

**Flujos alternativos**
- **1a. Campos obligatorios vacíos:** El sistema bloquea el envío y exige completar el cuerpo del informe antes de elevarlo.
- **1b. Profesional no es el autor:** El backend rechaza la solicitud con `403 Forbidden`.

**Postcondiciones**
- El reporte pasa a estado `Submitted`.
- El informe ingresa a la bandeja de auditoría del Equipo Directivo de la escuela.
- El docente ya no puede modificar el contenido mientras se encuentre en revisión.
- El Familiar **continúa sin visibilidad** del informe hasta su aprobación.

---

## CU-42: Aprobar Reporte de Progreso

| Campo | Detalle |
|---|---|
| **Actor principal** | Equipo Directivo Escolar (Director/a, Vicedirector/a, Secretaría) |
| **Actores secundarios** | Familiares vinculados (receptores notificados), Sistema (Background Jobs: Email + SignalR) |
| **HU de referencia** | HU-08 / HU-IN-164 |
| **Prioridad** | Alta |
| **Precondiciones** | El reporte existe en estado `Submitted` y pertenece a un alumno de la sede escolar del Directivo. |

**Flujo principal**
1. El Directivo accede a la cola de informes pendientes (`Submitted`) en su panel de administración institucional.
2. Abre el informe y revisa detalladamente las metas, el progreso pedagógico y las recomendaciones.
3. Presiona "Aprobar reporte".
4. El sistema despliega un modal de confirmación advirtiendo que el informe pasará a estar disponible para la familia.
5. El Directivo confirma la acción.
6. El sistema cambia el estado del reporte a `Approved`, estampa `ApprovedBy = DirectivoUserId` y `ApprovedAt = UtcNow`.
7. El sistema encola automáticamente en segundo plano la cascada de notificaciones para todos los familiares activos vinculados al alumno:
   - Correo electrónico formal con plantilla `ReportApproved` (nombre del alumno, título, tipo de reporte y docente responsable).
   - Notificación push / SignalR in-app con acceso directo a la lectura.
8. El sistema notifica al directivo: *"Reporte aprobado. El familiar ya puede consultarlo"*.

**Flujos alternativos**
- **Reporte fuera de estado Submitted:** El sistema bloquea la acción informando que solo se pueden aprobar reportes enviados a revisión.
- **Directivo de otra institución:** El backend valida el alcance institucional y deniega el acceso con `403 Forbidden`.

**Postcondiciones**
- El informe queda formalmente en estado `Approved`, sin posibilidad de retornar a `Draft` o `Submitted`.
- El informe queda visible de inmediato en el portal familiar (CU-44) con la insignia destacada `Nuevo`.
- Se habilita la exportación oficial en PDF con validez institucional.

---

## CU-43: Rechazar Reporte de Progreso con Observaciones

| Campo | Detalle |
|---|---|
| **Actor principal** | Equipo Directivo Escolar (Director/a, Vicedirector/a, Secretaría) |
| **Actores secundarios** | Profesional Docente (autor notificado), Sistema (Background Jobs: Email + SignalR) |
| **HU de referencia** | HU-08 / HU-IN-164 |
| **Prioridad** | Alta |
| **Precondiciones** | El reporte se encuentra en estado `Submitted` y pertenece a la institución educativa del Directivo. |

**Flujo principal**
1. El Directivo examina el informe en la bandeja de revisión y constata que requiere ajustes o ampliaciones pedagógicas.
2. Presiona "Rechazar reporte".
3. El sistema solicita de forma obligatoria el motivo u observaciones directivas (`adminComment`).
4. El Directivo redacta las observaciones puntuales que el docente debe corregir y confirma el rechazo.
5. El sistema cambia el estado a `Rejected`, graba las observaciones en `AdminComment` y actualiza la fecha de modificación.
6. El sistema encola en segundo plano las notificaciones destinadas al docente autor:
   - Correo electrónico oficial con plantilla `ReportRejected` conteniendo el detalle de las observaciones para la corrección.
   - Notificación push / SignalR con acceso directo a la edición del informe.
7. El sistema confirma al directivo: *"Reporte rechazado. El profesional fue notificado para su corrección"*.

**Flujos alternativos**
- **4a. Intento de rechazo sin motivo:** El sistema valida que el comentario no esté vacío; si no contiene texto, deshabilita la confirmación o devuelve error: *"El motivo del rechazo es obligatorio"*.

**Postcondiciones**
- El reporte adquiere estado `Rejected` con las observaciones pedagógicas directivas.
- El Profesional autor recupera la potestad de editar el informe, subsanar las observaciones y volver a elevarlo a revisión (CU-41).
- El Familiar **no tiene conocimiento ni acceso** al informe rechazado ni a las observaciones internas.

---

## CU-44: Consultar, Descargar y Registrar Lectura de Reportes Aprobados

| Campo | Detalle |
|---|---|
| **Actor principal** | Familiar / Representante Legal (o Persona con Discapacidad autorizada) |
| **Actores secundarios** | Sistema (control de acuse de lectura y exportación PDF) |
| **HU de referencia** | HU-08 / HU-IN-138 |
| **Prioridad** | Alta |
| **Precondiciones** | El Familiar tiene sesión activa y vínculo activo verificado con el Alumno (`PersonRepresentative.IsActive == true`), y existen reportes en estado `Approved`. |

**Flujo principal**
1. El Familiar accede a la sección "Reportes" desde el menú lateral de su portal familiar (`/family/reports`).
2. El sistema consulta el endpoint seguro `GET /api/reports/family` y lista exclusivamente los informes aprobados correspondientes a sus representados en orden cronológico descendente.
3. Los reportes no consultados previamente exhiben una insignia destacada `Nuevo`.
4. El Familiar selecciona un informe para abrir la lectura completa (título, fecha, docente, metas, áreas trabajadas y recomendaciones para el hogar).
5. Al abrir el detalle, el sistema envía automáticamente una petición asíncrona `PATCH /api/reports/{id}/mark-read`.
6. El sistema asienta el acuse de recibo del familiar (`isReadByFamily = true`), removiendo el badge `Nuevo` para futuras sesiones.
7. El Familiar puede presionar "Descargar PDF" (`GET /api/reports/{id}/export-pdf`) para obtener el documento oficial con membrete escolar.

**Flujos alternativos**
- **Sin reportes aprobados:** El sistema muestra: *"Aún no hay informes de progreso disponibles. Te avisaremos cuando el equipo docente publique el primero."*
- **Familiar sin vínculo activo:** El backend devuelve `404 Not Found` en resguardo estricto de la privacidad y confidencialidad del alumno.

**Postcondiciones**
- La familia accede a información pedagógica confiable y formalmente validada por el Equipo Directivo escolar.
- Queda registrado en el sistema el acuse formal de lectura del informe por parte de la familia.
