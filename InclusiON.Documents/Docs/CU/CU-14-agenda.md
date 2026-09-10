# Módulo 14 — Agenda y Calendario

---

## CU-60: Agendar y gestionar eventos de calendario

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional |
| **Actores secundarios** | Familiar / Alumno (consulta y notificaciones), Sistema |
| **HU de referencia** | HU-16, HU-20 (IN-212, IN-217, IN-218, IN-219, IN-220) |
| **Prioridad** | Alta |

**Precondiciones**
- El Profesional está autenticado.
- El usuario tiene rol Profesional (el rol Administrador tiene expresamente bloqueadas las rutas y notificaciones de calendario por regla de negocio — IN-217).

**Flujo principal**
1. El Profesional accede a la sección de Calendario (`/pro/calendar`).
2. El sistema consulta `GET /api/calendar` y renderiza la vista interactiva (mensual/semanal) con los eventos programados por el profesional.
3. El Profesional selecciona un día u oprime "Nuevo Evento".
4. El sistema despliega el formulario/modal de creación solicitando:
   - **Título** del evento (máx. 200 caracteres).
   - **Tipo** de evento: selector con opciones `Consulta`, `Tutoría`, `Clase`, `Tarea`.
   - **Fecha** del evento.
   - **Hora** del evento (formato HH:MM).
   - **Descripción** o consignas adicionales (opcional, máx. 1000 caracteres).
   - **Alcance** (`TargetScope`): Alumno individual (`single`) o Todo el grupo/aula (`all`).
   - En caso de seleccionar `single`, se elige el alumno destinatario de la lista de personas vinculadas al profesional.
5. El Profesional confirma la acción de guardado.
6. El sistema valida los datos:
   - Valida que la fecha no sea anterior a la fecha actual (IN-220: no se permiten fechas pasadas).
   - Valida la obligatoriedad del título, tipo, fecha, hora y alcance.
7. El sistema persiste el evento en la tabla `CalendarEvents` a través de `POST /api/calendar` (`CalendarController.SaveEvent`).
8. El evento queda agendado y se emiten las notificaciones contextuales segmentadas por tipo de evento (IN-219) para el familiar responsable.

**Flujos alternativos**
- **6a. Selección de fecha pasada:** La interfaz deshabilita la selección de días pasados en el datepicker y el backend rechaza con error de validación (`400 Bad Request`) cualquier intento de persistencia con fecha anterior a la fecha actual UTC.
- **2a. Acceso por rol no permitido:** Si un usuario con rol Administrador o no autorizado intenta acceder a la ruta `/pro/calendar`, los guards de Angular bloquean el acceso con toast informativo y redirigen al dashboard administrativo (IN-217).
- **4a. Edición de evento existente:** El Profesional selecciona un evento agendado de su autoría, edita sus datos y guarda los cambios (`CalendarController` actualiza el registro e incrementa timestamp `UpdatedAt`).
- **4b. Cancelación de evento:** El Profesional selecciona "Cancelar evento"; el sistema aplica soft-delete (`IsActive = false`) y lo remueve del listado visual activo de eventos.

**Postcondiciones**
- El evento queda persistido en la base de datos relacional PostgreSQL 17 (`CalendarEvents`).
- El evento es visible para el profesional y genera recordatorios oportunos para los familiares vinculados.
