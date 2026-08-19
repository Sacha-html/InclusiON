# HU-20 — Mensajería para Administrador, Calendario y Notificaciones Segmentadas

| Campo | Contenido |
|---|---|
| ID | HU-20 |
| Épica | Comunicación y Planificación |
| Título | Mensajería Administrador, Notificaciones Segmentadas y Validaciones de Calendario |
| Prioridad | Alta |
| Estimación | 8 puntos de historia |
| Sprint asignado | Sprint 10 |
| Estado | Completada |

**Procesos relacionados:** 14 (Seguimiento de Avances), 16 (Comunicación entre Actores)

---

## 1. Historias de Usuario

### 1.1 Administrador
**Como** Administrador del sistema  
**Quiero** disponer de un módulo de mensajería interna con filtrado por tipo de actor (Profesionales y Familiares)  
**Para** comunicarme de forma bidireccional con cualquier docente, terapeuta o tutor legal sin recibir alertas inapropiadas de eventos clínicos o escolares.

### 1.2 Profesional
**Como** Profesional (Docente / Terapeuta)  
**Quiero** agendar actividades en el calendario clasificadas como Tutoría, Clase o Tarea sin permitir fechas pasadas  
**Para** organizar el cronograma pedagógico y notificar automáticamente al tutor correspondiente según la naturaleza de la actividad.

### 1.3 Familiar
**Como** Familiar / Tutor legal  
**Quiero** recibir avisos puntuales de tutorías personalizadas (si soy el tutor principal) o de clases/tareas generales (para todos los tutores a cargo)  
**Para** estar al tanto de las actividades de mi representado sin saturación de mensajes.

---

## 2. Descripción Funcional

### 2.1 Módulo de Mensajería del Administrador (`/admin/messages`)
- Reutiliza la interfaz interactiva tipo WhatsApp Web (`MessagesComponent`).
- Incorpora un control de pestañas/botones en la cabecera de la lista de contactos para filtrar:
  - **Todos**: Listado completo de profesionales y familiares con los que se tiene conversación.
  - **Profesionales**: Filtra únicamente usuarios con rol `Professional`.
  - **Familiares**: Filtra únicamente usuarios con rol `FamilyRepresentative`.
- Barra de búsqueda reactiva por nombre o correo electrónico que respeta el filtro de rol activo.
- Insignia visual roja (Badge) con el conteo de mensajes no leídos y estampa de hora del último mensaje.

### 2.2 Notificaciones y Redirección Inteligente
- Al hacer clic en una notificación de "Nuevo Mensaje" desde la campana, el sistema evalúa el rol del usuario:
  - Administrador &rarr; `/admin/messages?contactId={id}`
  - Profesional &rarr; `/pro/messages?contactId={id}`
  - Familiar &rarr; `/family/messages?contactId={id}`
- Al acceder al chat correspondiente, los mensajes no leídos se marcan automáticamente como leídos en la base de datos y se actualiza el contador de la campana.

### 2.3 Calendario de Actividades y Segmentación de Notificaciones
- **Tipos de Evento Unificados**: `Tutoría`, `Clase`, `Tarea` (se elimina la opción `Consulta`).
- **Lógica de Despacho de Notificaciones Push (Backend)**:
  - `Tutoría`: Despacha notificación en segundo plano exclusivamente al tutor principal (`IsPrimary`) del alumno.
  - `Clase` y `Tarea`: Despacha notificación a todos los tutores activos vinculados al alumno (o a todos los tutores de los alumnos del aula si `targetScope == "all"`).
  - **Exclusión de Administradores**: El sistema excluye explícitamente a usuarios con rol `Admin` de recibir alertas de calendario.

---

## 3. Criterios de Aceptación

- [x] El Administrador tiene acceso a la ruta `/admin/messages` y al ítem "Mensajes" en su menú lateral.
- [x] La vista de mensajería del Administrador permite alternar entre "Todos", "Profesionales" y "Familiares".
- [x] Al hacer clic en una notificación de mensaje, el Administrador es redirigido a `/admin/messages` sin generar errores 404.
- [x] El Administrador no recibe notificaciones push de eventos de calendario ni en tiempo real ni al cargar la campana.
- [x] El formulario de calendario bloquea la selección y guardado de fechas anteriores al día actual (`[min]="today"`).
- [x] Los eventos de calendario se persisten en PostgreSQL con fecha UTC (`DateTimeKind.Utc`).
- [x] La notificación de `Tutoría` llega únicamente al tutor con `IsPrimary = true`.
- [x] La notificación de `Clase` o `Tarea` se envía a todos los tutores asignados.
- [x] En eventos generales de aula, se deduplican los identificadores de tutores para no enviar alertas repetidas.

---

## 4. Casos de Borde (Edge Cases) y Validaciones Defensivas

| # | Escenario / Caso de Borde | Comportamiento Esperado y Validación Implementada |
|---|---|---|
| **CB-01** | **Clic en notificación de calendario por parte de un Administrador** | Si por inconsistencia de datos residuales un Admin hace clic en una notificación con URL `calendar`, el handler de redirección lo intercepta y redirige con seguridad a `/admin/dashboard`, evitando pantallas de "Página no encontrada" (404). |
| **CB-02** | **Tutor sin marca de tutor principal (`IsPrimary = false`) en evento de Tutoría** | Si ningún familiar tiene `IsPrimary = true`, el sistema toma de forma defensiva al primer tutor activo disponible (`activeReps.FirstOrDefault(r => r.IsPrimary) ?? activeReps.First()`) para garantizar que la notificación no se pierda. |
| **CB-03** | **Evento general con tutores repetidos en múltiples alumnos** | Al agendar con `targetScope = "all"`, se utiliza una colección `HashSet<Guid> notifiedTutorIds` para registrar los `UserId` ya procesados, asegurando que un tutor con dos o más hijos en el aula reciba un único aviso por evento. |
| **CB-04** | **Intento de agendar evento en fecha pasada** | El input de fecha contiene el atributo `[min]="todayDate"` y el método `saveEventSubmit()` ejecuta la validación `isDateInvalid()`, mostrando un toast de error y bloqueando el botón Guardar. |
| **CB-05** | **Zona horaria y compatibilidad con PostgreSQL (Npgsql)** | Las cadenas de fecha recibidas en el backend se parsean y normalizan con `DateTime.SpecifyKind(dateVal.Date, DateTimeKind.Utc)`, impidiendo excepciones `InvalidOperationException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'`. |
| **CB-06** | **Filtro de contactos del Admin sin coincidencias** | Al filtrar por "Profesionales" o "Familiares" con una búsqueda de texto que no arroja resultados, el componente muestra un estado visual vacío amigable ("No se encontraron contactos") en lugar de un error de renderizado. |
| **CB-07** | **Llamadas duplicadas a `/api/Messages` desde el cliente** | Se eliminó el método redundante `notifyTutorsAboutEvent` del cliente en `calendar.component.ts` que pasaba `representativeId` a `/api/Messages` (causando errores 404 por incompatibilidad de GUIDs de entidad vs cuenta de usuario), centralizando todo el despacho en el backend. |
| **CB-08** | **Validación de fecha de nacimiento en ABM Personas** | En los formularios de alta y edición de alumnos se aplican validadores reactivos `maxDateTodayValidator` (no fechas futuras) y `validBirthDateValidator` (límite de 120 años atrás), impidiendo registros con fechas irreales. |
| **CB-09** | **Filtros por fecha en Dashboards y compatibilidad con PostgreSQL `timestamptz`** | En `AnalyticsController` se implementó `NormalizeToUtc` para descartar años fuera de rango ($1900 \le \text{year} \le 3000$) y forzar `DateTimeKind.Utc`, evitando caídas 500 al tipear fechas parciales (ej. año `0002-..`). En el cliente (`dashboard.component.ts` y `detail.component.ts`) se validó `isValidDateString` y se blindó el estado de carga para no ocultar la pantalla del panel ante fallos de consulta de filtros. |
