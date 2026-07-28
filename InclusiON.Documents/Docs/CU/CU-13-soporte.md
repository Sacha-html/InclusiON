# Módulo 13 — Soporte

---

## CU-54: Consultar FAQ

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Todos los usuarios autenticados |
| **Actores secundarios** | — |
| **HU de referencia** | HU-13 |
| **Prioridad** | Media |

**Precondiciones**
- El usuario está autenticado.
- Existen entradas de FAQ activas en el sistema.

**Flujo principal**
1. El usuario accede a `/help` desde cualquier portal.
2. El sistema muestra las FAQ organizadas por categoría.
3. El usuario puede buscar por palabras clave.
4. El usuario selecciona una pregunta para ver la respuesta completa.

**Flujos alternativos**
- **Sin resultados de búsqueda:** El sistema muestra "No encontramos resultados. Podés reportar un problema si no encontrás lo que buscás."

**Postcondiciones**
- El usuario puede resolver su duda sin asistencia del Admin.

---

## CU-55: Reportar problema técnico

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Todos los usuarios autenticados |
| **Actores secundarios** | Admin (receptor), Sistema |
| **HU de referencia** | HU-13 |
| **Prioridad** | Media |

**Precondiciones**
- El usuario está autenticado.

**Flujo principal**
1. El usuario hace clic en el botón flotante "Reportar problema" disponible desde cualquier sección.
2. El sistema muestra el formulario de ticket. Precompleta automáticamente: sección actual, rol del usuario, navegador y versión.
3. El usuario describe el problema y envía.
4. El sistema crea el ticket en estado `Abierto` y notifica al Admin.
5. El usuario puede ver el estado del ticket desde "Mis tickets".

**Flujos alternativos**
- **3a. Descripción vacía:** El sistema bloquea el envío.

**Postcondiciones**
- El ticket queda registrado y visible para el Admin.
- El usuario puede consultar su estado y las respuestas del Admin.

---

## CU-56: Gestionar tickets de soporte

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin Global / Admin Institucional |
| **Actores secundarios** | Sistema, Usuario (receptor de respuesta) |
| **HU de referencia** | HU-13 |
| **Prioridad** | Media |

**Precondiciones**
- El Admin está autenticado.
- Existen tickets en el sistema.

**Flujo principal**
1. El Admin accede a la sección de gestión de soporte.
2. El sistema lista todos los tickets (Admin Global) o los de su institución (Admin Institucional), con filtros por estado.
3. El Admin abre un ticket, lee el contexto capturado automáticamente (sección, rol, navegador) y escribe la respuesta.
4. El Admin cambia el estado del ticket: Abierto → En proceso → Resuelto / Cerrado.
5. El sistema notifica al usuario con la respuesta.

**Flujos alternativos**
- **Ticket sin actividad 30 días:** El sistema cierra el ticket automáticamente.

**Postcondiciones**
- El usuario puede ver la respuesta del Admin en "Mis tickets".

---

## CU-57: Gestionar entradas de FAQ

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Admin Global |
| **Actores secundarios** | — |
| **HU de referencia** | HU-13 |
| **Prioridad** | Baja |

**Precondiciones**
- El Admin Global está autenticado.

**Flujo principal**
1. El Admin accede a la sección de gestión de FAQ.
2. Puede crear una nueva entrada: categoría, pregunta y respuesta.
3. Puede editar entradas existentes.
4. Puede desactivar una entrada (soft-delete). Deja de ser visible para los usuarios.

**Flujos alternativos**
- **Categoría inexistente:** El Admin puede crear una nueva categoría al momento de crear la FAQ.

**Postcondiciones**
- La FAQ queda actualizada y los cambios se reflejan inmediatamente en `/help`.
