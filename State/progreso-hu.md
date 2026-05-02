# InclusiON — Estado de Avance por HU

**Última actualización:** 2026-05-01
**Fuente de verdad:** Jira

---

## Leyenda

| Símbolo | Jira | Significado |
|---|---|---|
| ✅ | Done | Implementado y funcionando |
| 🔵 | For Review | Implementado, en revisión |
| 🔄 | In Progress | En desarrollo activo |
| 🔧 | — | Entidades y migraciones listas, sin handlers/controller |
| ⏳ | Backlog | No iniciado |

---

# MVP — Práctica II

---

## Configurar Sistema
*Instituciones, catálogos, roles y permisos*

| Historia | Jira | Estado |
|---|---|---|
| Registrar institución educativa | IN-21 | 🔵 |
| Consultar instituciones | IN-22 | 🔵 |
| Editar institución | IN-23 | 🔵 |
| Crear administrador institucional | IN-26 | 🔵 |
| Asignar institución a administrador | IN-27 | 🔵 |
| Filtrar datos por institución | IN-28 | 🔵 |
| Enforcement de aislamiento por institución | IN-29 | 🔵 |
| Consultar roles del sistema | IN-24 | 🔵 |
| Asignar permisos por módulo | IN-25 | 🔵 |
| Confirmar cambios de permisos con aviso de cierre de sesiones | IN-30 | 🔵 |
| Consultar catálogos del sistema (6 tipos) | IN-33 | 🔵 |
| Registrar ítem en catálogo | IN-34 | 🔵 |
| Editar ítem en catálogo | IN-35 | 🔵 |
| Consultar tipos de template de actividad | IN-103 | 🔵 |
| Consultar categorías de actividad | IN-104 | 🔵 |

---

## Gestionar Usuarios
*Profesionales, personas, familiares e invitaciones*

| Historia | Jira | Estado |
|---|---|---|
| Alta de profesional con contraseña temporal | IN-36 | 🔵 |
| Consulta paginada de profesionales con filtros | IN-37 | 🔵 |
| Edición de profesional | IN-38 | 🔵 |
| Desactivación de profesional | IN-39 | 🔵 |
| Auto-registro profesional (Sign-up público) | IN-149 | 🔵 |
| Validación institucional por administrador | IN-150 | 🔵 |
| Alta de persona con perfil funcional | IN-40 | 🔵 |
| Consulta paginada de personas con filtros | IN-41 | 🔵 |
| Edición de datos personales y funcionales | IN-42 | 🔵 |
| Configuración del método de login de la persona | IN-43 | 🔵 |
| Desactivación de persona | IN-44 | 🔵 |
| Configuración del perfil de habilidades | IN-63 | 🔵 |
| Edición del perfil funcional | IN-82 | 🔵 |
| Alta directa de familiar con vinculación automática | IN-45, IN-51 | 🔵 |
| Alta de familiar por invitación (auto-registro) | IN-46 | 🔵 |
| Consulta paginada de familiares | IN-47 | 🔵 |
| Edición de familiar | IN-49 | 🔵 |
| Desactivación de familiar | IN-50 | 🔵 |
| Crear invitación familiar y enviar email | IN-53 | 🔵 |
| Validar código de invitación | IN-54 | 🔵 |
| Aceptar registro por invitación | IN-55 | 🔵 |
| Consultar invitaciones (profesional) | IN-56 | 🔵 |
| Asignar profesional a institución | IN-58 | 🔵 |
| Desasignar profesional de institución | IN-59 | 🔵 |
| Asignar persona a profesional | IN-60 | 🔵 |
| Desactivar asignación persona-profesional | IN-61 | 🔵 |
| Detalle de usuario con entidad asociada | IN-94 | 🔵 |
| Reseteo de contraseña | IN-95 | 🔵 |
| Desactivación de cuenta | IN-96 | 🔵 |
| Reactivación de cuenta | IN-97 | 🔵 |

---

## Acceder al Sistema
*Login, sesión y accesibilidad*

| Historia | Jira | Estado |
|---|---|---|
| Login estándar (email + contraseña) | IN-65 | 🔵 |
| Login por PIN (4 dígitos) | IN-67 | 🔵 |
| Login asistido (supervisor autoriza) | IN-68 | 🔵 |
| Login familiar | IN-69 | 🔵 |
| Refresh de token automático | IN-71 | 🔵 |
| Cambio de contraseña obligatorio en primer login | IN-72 | 🔵 |
| Redirección por rol al portal correspondiente | IN-73 | 🔵 |
| Guards de ruta por rol y permiso | IN-79 | 🔵 |
| Directivas de permisos en interfaz | IN-80 | 🔵 |
| 7 perfiles visuales de accesibilidad | IN-75 | 🔵 |
| Modo claro y oscuro (14 combinaciones) | IN-76 | 🔵 |
| Panel de accesibilidad (Alt+A) | IN-77 | 🔵 |
| Toasts con colores de accesibilidad | IN-78 | 🔵 |
| Revisión de accesibilidad en componentes | IN-170 | 🔵 |
| Autorización por recurso (row-level) | IN-172 | ✅ |
| Hardening: Argon2id + AES-256-GCM + Rate limiting | IN-173 | ✅ |

---

## Crear Actividad
*Contenido educativo (Profesional)*

| Historia | Jira | Estado | Nota |
|---|---|---|---|
| Crear actividad con wizard (área → template → contenido) | IN-105 | 🔵 | BE+FE. Wizard multi-paso, ARASAAC, template SelectFigure |
| Consultar catálogo de actividades propias | IN-107 | 🔵 | Filtros, búsqueda semántica IA, paginación |
| Editar actividad propia | IN-108 | 🔵 | Ruta protegida por permissionGuard (activities:update) |
| Desactivar actividad | IN-109 | 🔵 | Valida asignaciones activas. Bloquea actividades estándar |

---

## Planificar Roadmap
*Roadmap de aprendizaje por persona (Profesional)*

| Historia | Jira | Estado | Nota |
|---|---|---|---|
| Crear roadmap por persona | IN-110 | 🔵 | |
| Agregar actividades al roadmap por área | IN-111 | 🔵 | |
| Definir orden secuencial y umbral de desbloqueo | IN-112 | 🔵 | |
| Reordenar actividades con drag-drop | IN-113 | 🔵 | CDK drag-drop + PUT /areas/{id}/activities/reorder |
| Desbloqueo manual de actividad | IN-114 | 🔵 | |
| Eliminar actividad del roadmap | IN-115 | 🔵 | |

---

## Asignar Actividad
*Actividad asignada a persona (Profesional)*

| Historia | Jira | Estado | Nota |
|---|---|---|---|
| Cargar asignación con contenido completo | IN-118 | 🔵 | GET /api/activity-assignments/{id}, valida requester es persona o profesional |

---

## Ejecutar Actividad
*Roadmap y players (Persona)*

| Historia | Jira | Estado |
|---|---|---|
| Visualizar roadmap propio (estilo Duolingo) | IN-117 | 🔵 |
| Iniciar actividad — ActivityPlayerShell | IN-119 | 🔵 |
| Player: Selección de figuras | IN-120 | 🔵 |
| Player: Completar letra | IN-124 | 🔵 |
| Player: Suma visual | IN-121 | 🔵 |
| Player: Emparejar imagen-palabra | IN-122 | 🔵 |
| Player: Ordenar secuencia | IN-123 | 🔵 |

---

## Registrar Respuesta
*Resultado de la actividad (Persona)*

| Historia | Jira | Estado | Nota |
|---|---|---|---|
| Completar actividad y registrar resultado | IN-126 | 🔵 | CompleteActivityResponseCommandHandler implementado |

---

## Ver Resultado
*Respuestas y dashboard (Profesional)*

| Historia | Jira | Estado |
|---|---|---|
| Dashboard del profesional con contadores reales | IN-87 | 🔵 |
| Mi Aula — cards de personas asignadas | IN-88 | 🔵 |
| Detalle de persona + ver respuestas por asignación (tab Actividades) | IN-89 | 🔵 |

---

## Reportar Progreso
*Diagnósticos y reportes (Profesional / Familiar)*

| Historia | Jira | Estado |
|---|---|---|
| Registrar diagnóstico funcional | IN-83 | 🔵 |
| Consultar historial de diagnósticos | IN-84 | 🔵 |
| Editar diagnóstico | IN-85 | 🔵 |
| Crear reporte de progreso | IN-156 | 🔵 |
| Consultar reportes (profesional) | IN-164 | 🔵 |
| Consultar reportes (familiar) | IN-138 | 🔵 |

---

# Post-MVP — Práctica III

---

## Crear Actividad — Post-MVP
*Contenido educativo avanzado*

| Historia | Jira | Estado |
|---|---|---|
| Integración de pictogramas ARASAAC | IN-106 | ✅ |
| Búsqueda semántica por lenguaje natural | IN-135 | ✅ |

---

## Planificar Roadmap — Post-MVP
*Motor de dificultad adaptativa*

| Historia | Jira | Estado | Nota |
|---|---|---|---|
| Configurar motor adaptativo por actividad | IN-116 | ⏳ | |
| Desbloqueo automático por umbral de rendimiento | IN-127 | ✅ | CompleteActivityResponseCommandHandler: unlock next si successPercentage >= unlockThresholdPercent |
| Monitoreo de frustración (pausa tras 3+ intentos) | IN-128 | ⏳ | |
| Evaluación automática de rendimiento | IN-129 | ⏳ | |
| Cálculo y aplicación de ajuste de dificultad | IN-130, IN-131 | 🔧 ⏳ | Entidades + migraciones listas |
| Registro de ajustes en historial de auditoría | IN-132 | 🔧 ⏳ | Entidades + migraciones listas |
| Alerta al profesional en estado de frustración | IN-133 | ⏳ | |
| Timeline de historial de ajustes | IN-134 | ⏳ | |

---

## Ver Resultado — Post-MVP
*Métricas y radar chart*

| Historia | Jira | Estado |
|---|---|---|
| Radar chart de habilidades por área | IN-90 | ⏳ |
| Timeline de diagnósticos en perfil | IN-86 | 🔵 |
| Dashboard familiar (actividades, mensajes, reportes) | IN-92 | 🔵 |
| Panel de visualización de progreso (familiar) | IN-153 | ⏳ |

---

## Reportar Progreso — Post-MVP
*Exportación*

| Historia | Jira | Estado |
|---|---|---|
| Exportación de reporte a PDF | IN-139 | ⏳ |

---

## Acceder al Sistema — Post-MVP
*Onboarding*

| Historia | Jira | Estado |
|---|---|---|
| Wizard de completado de perfil (profesional) | IN-99 | ⏳ |
| Tour guiado del portal | IN-100 | ⏳ |
| Pantalla de bienvenida (familiar) | IN-101 | ⏳ |
| Pantalla de bienvenida (persona con discapacidad) | IN-102 | ⏳ |

---

## Gestionar Usuarios — Post-MVP
*Comunicación interna*

| Historia | Jira | Estado |
|---|---|---|
| Bandeja de entrada de mensajes | IN-140 | ✅ |
| Envío de mensajes con asunto y contenido | IN-141 | ✅ |
| Hilos de conversación | IN-142 | ✅ |
| Indicador de mensajes no leídos en sidebar | IN-143 | ✅ |
| Marcado automático como leído al abrir | IN-144 | ✅ |
| Consulta de actividad reciente del usuario | IN-98 | ⏳ |

---

## Resumen

### MVP

| Estado | Cantidad |
|---|:---:|
| ✅ Hecho | 2 |
| 🔵 En revisión | 85 |
| 🔄 En curso | 0 |
| 🔧 Base lista, sin handlers | 0 |
| ⏳ Pendiente | 0 |
| **Total MVP** | **87** |

### Post-MVP

| Estado | Cantidad |
|---|:---:|
| ✅ Hecho | 8 |
| 🔵 En revisión | 2 |
| 🔧 Base lista, sin handlers | 2 |
| ⏳ Pendiente | 20 |
| **Total Post-MVP** | **32** |
