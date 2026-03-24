# HU-13 — Soporte y Ayuda

**Proceso relacionado:** 19
**Prioridad:** Media

---

## Historia de Usuario

**Como** usuario de la plataforma
**Quiero** consultar un centro de ayuda y poder reportar problemas técnicos
**Para** resolver dudas de forma autónoma o recibir asistencia del administrador cuando lo necesite

---

## Descripción funcional

- **Centro de ayuda (FAQ):** Preguntas frecuentes organizadas por categoría, gestionadas por el admin
- **Guías contextuales:** Tooltips inline en secciones clave del portal (frontend puro, sin endpoint)
- **Reportar problema:** Formulario accesible desde cualquier sección con captura automática de contexto (sección, rol, navegador)
- **Gestión de tickets:** El admin recibe, revisa y responde tickets; el usuario consulta el estado

---

## Criterios de Aceptación

### FAQ
- [ ] Existe una sección `/help` accesible desde todos los portales
- [ ] Las FAQ están organizadas por categoría con búsqueda
- [ ] El admin puede crear, editar y desactivar entradas de FAQ

### Tickets
- [ ] Cualquier usuario puede crear un ticket de soporte desde un botón flotante
- [ ] El ticket captura automáticamente el contexto (sección, rol, navegador)
- [ ] El usuario puede ver el estado de sus tickets y las respuestas
- [ ] El admin puede listar, responder y cambiar el estado de los tickets
- [ ] Los tickets sin actividad se cierran automáticamente a los 30 días

---

## Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/support/faq` | FAQ para usuarios |
| POST | `/api/support/faq` | Crear FAQ (admin) |
| PUT | `/api/support/faq/{id}` | Editar FAQ (admin) |
| PUT | `/api/support/faq/{id}/deactivate` | Desactivar FAQ (admin) |
| POST | `/api/support/tickets` | Crear ticket |
| GET | `/api/support/tickets` | Listado tickets (admin) |
| GET | `/api/support/tickets/mine` | Mis tickets (usuario) |
| GET | `/api/support/tickets/{id}` | Detalle ticket |
| POST | `/api/support/tickets/{id}/respond` | Responder ticket (admin) |
| PUT | `/api/support/tickets/{id}/status` | Cambiar estado ticket (admin) |

---

## Vistas (FE)

| Ruta | Rol | Descripción |
|------|-----|-------------|
| `/help` | Todos | Centro de ayuda (FAQ) |
| `/help/tickets` | Todos | Mis tickets de soporte |
| `/admin/support/faq` | Admin | ABM de preguntas frecuentes |
| `/admin/support/tickets` | Admin | Gestión de tickets de soporte |
