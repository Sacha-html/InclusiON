# Sprint 3 — Autenticación y Accesibilidad (IN-65 a IN-80)

**Período:** 

**Objetivo:** Login múltiples métodos y sistema de accesibilidad

---

## Tareas

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-65 | Login estándar (email + contraseña) | POST /api/auth/login | auth/login | ✅ |
| IN-66 | Login visual estándar (identificación por nombre) | POST /api/auth/login/visual-standard | auth/login | ✅ |
| IN-67 | Login por PIN (4 dígitos) | POST /api/auth/login/pin | auth/login | ✅ |
| IN-68 | Login asistido (supervisor autoriza) | POST /api/auth/login/assisted | auth/login | ✅ |
| IN-69 | Login familiar | POST /api/auth/login/family | auth/login | ✅ |
| IN-70 | Identificación de usuario por nombre | POST /api/auth/identify | auth/identify | ✅ |
| IN-71 | Refresh de token automático | POST /api/auth/refresh | token interceptor | ✅ |
| IN-72 | Cambio de contraseña obligatorio en primer login | PUT /api/auth/change-password | auth/change-password | ✅ |
| IN-73 | Redirección por rol al portal correspondiente | - | auth service + router | ✅ |
| IN-74 | Validación de rol en login admin/profesional | LoginCommand allowedRoles | auth/login | ✅ |
| IN-75 | 7 perfiles visuales de accesibilidad | - | _accessibility-themes.scss | ✅ |
| IN-76 | Modo claro y oscuro (14 combinaciones) | - | accessibility service | ✅ |
| IN-77 | Panel de accesibilidad (Alt+A) | - | accessibility-panel.component | ✅ |
| IN-78 | Toasts con colores de accesibilidad | - | _accessibility-themes.scss | ✅ |
| IN-79 | Guards de ruta por rol y permiso | - | shared/directives/ | ✅ |
| IN-80 | Directivas de permisos en interfaz | - | if-institutional-admin, has-permission | ✅ |

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 16 |
| Completadas | 16 |

---

## Épicas padre

- **IN-6:** Autenticación y Accesibilidad