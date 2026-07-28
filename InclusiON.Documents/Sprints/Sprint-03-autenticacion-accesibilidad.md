# Sprint 3 — Autenticación y Accesibilidad (IN-65 a IN-80)

**Período:** 08/04/2025 – 21/04/2025  
**Duración:** 2 semanas  
**Épica:** IN-6 — Autenticación y Accesibilidad

**Objetivo:** Implementar el sistema de login multi-método para personas con discapacidad (STANDARD, PIN, ASSISTED) y el sistema de accesibilidad visual con 7 perfiles configurables.

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

## Métricas del Sprint

| Métrica | Valor |
|---------|-------|
| Total tareas | 16 |
| Completadas | 16 |
| Velocidad | 16 / 16 (100%) |
| Story points estimados | 34 pts |
| Story points completados | 34 pts |
| Impedimentos | 0 bloqueantes — un bug de integración frontend/backend detectado en daily (resuelto en el mismo sprint) |
| Deuda técnica generada | Ninguna |

## Automatizaciones entregadas en este sprint

| Automatización | Descripción | HU |
|---|---|---|
| Redirect automático por rol | Al hacer login, el sistema detecta el rol del JWT y redirige al portal correcto sin intervención del usuario | HU-IN-172 |
| Primer login obligatorio — cambio de contraseña | Si `MustChangePassword = true`, el sistema intercepta cualquier acción y fuerza el cambio antes de continuar | HU-01 |
| Identificación visual sin email | Para PCD, el sistema identifica al usuario por nombre visible (`POST /api/auth/identify`) sin requerir email | HU-14 |

## Retrospectiva

> Retrospectiva completa: [`PP2/sprint-3/retro-sprint-3-2025-04-21.md`](./PP2/sprint-3/retro-sprint-3-2025-04-21.md)

**Qué funcionó bien:**
- Entorno compartido (Docker Compose) eliminó problemas de ambiente
- Buena comunicación de equipo durante toda la práctica
- Entrevistas de relevamiento con cliente real (Del Barrio Sacha) dieron sustento a las HU

**Qué mejorar:**
- Las pruebas de integración frontend/backend se dejaron para el final; detectar antes
- La estimación del tiempo de integración subestimó la complejidad real

## Dailies

- [`daily-07 — 09/04/2025`](./PP2/sprint-3/daily-07-2025-04-09.md)
- [`daily-08 — 16/04/2025`](./PP2/sprint-3/daily-08-2025-04-16.md)

---

## Épicas padre

- **IN-6:** Autenticación y Accesibilidad