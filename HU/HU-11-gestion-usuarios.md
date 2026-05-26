# HU-11 — Gestión Centralizada de Usuarios

**Proceso relacionado:** 17
**Prioridad:** Alta

---

## Historia de Usuario

**Como** administrador del sistema
**Quiero** gestionar de forma centralizada las cuentas de usuario (resetear contraseñas, desactivar, reactivar y consultar actividad)
**Para** mantener el control sobre el acceso al sistema sin depender de los CRUDs individuales de cada tipo de actor

---

## Descripción funcional

El administrador necesita una vista centralizada de todas las cuentas del sistema, independiente de los ABMs de profesionales, familiares y personas. Desde esta vista puede:

- **Listar usuarios** con filtros por rol, estado activo/inactivo, institución y búsqueda por nombre/email
- **Resetear contraseña** generando una nueva temporal, forzando cambio en el próximo login y revocando sesiones activas
- **Desactivar una cuenta** cortando el acceso inmediatamente (soft-delete + revocación de tokens)
- **Reactivar una cuenta** previamente desactivada, con nueva contraseña temporal
- **Consultar actividad** del usuario: último login, cantidad de accesos, acciones recientes del audit log

---

## Criterios de Aceptación

- [x] El admin puede ver un listado centralizado de todos los usuarios del sistema con filtros por rol, estado e institución
- [x] El admin institucional solo ve usuarios de sus instituciones asignadas
- [x] Al resetear contraseña se genera una temporal, se activa `MustChangePassword` y se revocan sesiones activas
- [x] La contraseña temporal se muestra una sola vez con botón de copiar
- [x] Al desactivar un usuario se corta el acceso inmediatamente (revocación de tokens)
- [x] No se puede desactivar al propio usuario
- [x] Al reactivar un usuario se genera nueva contraseña temporal
- [x] Se puede consultar último login y acciones recientes del usuario
- [x] Todas las operaciones se registran en AccessAudit

---

## Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/admin/users` | Listado paginado con filtros |
| GET | `/api/admin/users/{id}` | Detalle de usuario |
| POST | `/api/admin/users/{id}/reset-password` | Reset de contraseña |
| PUT | `/api/admin/users/{id}/deactivate` | Desactivar cuenta |
| PUT | `/api/admin/users/{id}/reactivate` | Reactivar cuenta |
| GET | `/api/admin/users/{id}/activity` | Actividad reciente |

---

## Vistas (FE)

| Ruta | Rol | Descripción |
|------|-----|-------------|
| `/admin/users` | Admin | Listado centralizado de usuarios |
| `/admin/users/{id}` | Admin | Detalle de usuario con acciones |
