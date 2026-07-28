# Sprint 2 — Usuarios Profesionales y Familiares (IN-36 a IN-64)

**Épicas:** 
- IN-4 - Gestión de Usuarios
- IN-5 - Invitaciones y Asignaciones

**Período:** 

**Objetivo:** Desarrollar el ecosistema central de perfiles (User Management). Este sprint provee los mecanismos para registrar tanto a los educadores y profesionales (Terapeutas) como a los pacientes vinculados (Personas con Discapacidad) y lograr la inserción de las familias en la estructura relacional de la plataforma.

---

## Tareas por Épica

### Épica IN-4 — Gestión de Usuarios (IN-36 a IN-52)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-36 | Alta de profesional con contraseña temporal + email | POST /api/professionals | admin/professionals/new | ✅ |
| IN-37 | Consulta paginada de profesionales con filtros | GET /api/professionals | admin/professionals/list | ✅ |
| IN-38 | Edición de profesional | PUT /api/professionals/{id} | admin/professionals/edit | ✅ |
| IN-39 | Desactivación de profesional | PUT /api/professionals/{id}/deactivate | admin/professionals/detail | ✅ |
| IN-40 | Alta de persona con perfil funcional | POST /api/persons | admin/persons/new | ✅ |
| IN-41 | Consulta paginada de personas con filtros | GET /api/persons | admin/persons/list | ✅ |
| IN-42 | Edición de datos personales y funcionales | PUT /api/persons/{id} | admin/persons/edit | ✅ |
| IN-43 | Configuración del método de login | PUT /api/persons/{id}/login-method | admin/persons/detail | ✅ |
| IN-44 | Desactivación de persona | PUT /api/persons/{id}/deactivate | admin/persons/detail | ✅ |
| IN-45 | Alta directa de familiar con selector de persona | POST /api/family | admin/family/new | ✅ |
| IN-46 | Alta de familiar por invitación (auto-registro) | InvitationsController | pages/register-by-invitation | ✅ |
| IN-47 | Consulta paginada de familiares | GET /api/family | admin/family/list | ✅ |
| IN-48 | Detalle de familiar con personas vinculadas | GET /api/family/{id} | admin/family/detail | ✅ |
| IN-49 | Edición de familiar | PUT /api/family/{id} | admin/family/edit | ✅ |
| IN-50 | Desactivación de familiar | PUT /api/family/{id}/deactivate | admin/family/detail | ✅ |
| IN-51 | Vinculación automática persona-familiar en alta directa | POST /api/family/{id}/link/{pid} | integrad en new | ✅ |
| IN-52 | Envío de email con contraseña temporal | CreateFamilyCommand | integrad | ✅ |

**Subtotal IN-4: 17 HUs ✅**

---

### Épica IN-5 — Invitaciones y Asignaciones (IN-53 a IN-64)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-53 | Crear invitación y enviar email | POST /api/invitations | pro/invitations, admin/invitations | ✅ |
| IN-54 | Validación de código de invitación | GET /api/invitations/{code} | register-by-invitation | ✅ |
| IN-55 | Aceptación y registro automático | POST /api/invitations/{code}/accept | register-by-invitation | ✅ |
| IN-56 | Consulta de invitaciones por profesional | GET /api/invitations | pro/invitations | ✅ |
| IN-57 | Consulta de invitaciones por admin | GET /api/invitations | admin/invitations | ✅ |
| IN-58 | Asignar profesional a institución | POST /api/professionals/{id}/institutions | professional-detail/institutions | ✅ |
| IN-59 | Desasignar profesional de institución | DELETE /api/professionals/{id}/inst | professional-detail/institutions | ✅ |
| IN-60 | Asignar persona a profesional | POST /api/professionals/{id}/persons | professional-detail/persons | ✅ |
| IN-61 | Desactivar asignación persona-profesional | PUT /api/professionals/{id}/persons/{pid}/deactivate | professional-detail/persons | ✅ |
| IN-62 | Vinculación familiar automática por invitación | AcceptInvitationCommand | integrad | ✅ |
| IN-63 | Configuración del perfil de habilidades | persons/{id}/skill-profile | persons/detail | ✅ |
| IN-64 | Desvinculación lógica (soft-delete) | soft-delete en endpoints | todas las vistas | ✅ |

**Subtotal IN-5: 12 HUs ✅**

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 29 |
| Completadas | 29 |

---

## Épicas padre

- **IN-4:** Gestión de Usuarios
- **IN-5:** Invitaciones y Asignaciones