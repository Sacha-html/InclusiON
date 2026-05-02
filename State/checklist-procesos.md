# Checklist de Procesos — InclusiON

**Última actualización:** 2026-05-01

---

# MVP — Práctica II

---

## Configuración del Sistema

### 01 — Gestión de Instituciones
- [x] Alta de institución
- [x] Consulta de instituciones
- [x] Edición de institución

### 02 — Gestión de Roles y Permisos
- [x] Consulta de roles
- [x] Asignación de permisos por módulo
- [x] Creación de administradores institucionales
- [x] Asignación de instituciones a admins
- [x] Filtrado de datos por institución
- [x] Enforcement de aislamiento por institución (InstitutionAccessFilter)
- [x] Confirmación al guardar permisos con aviso de cierre de sesiones
- [x] Revocación de tokens al cambiar permisos de un rol
- [x] Invalidación correcta de caché de permisos (fix NormalizedName)

### 03 — Gestión de Catálogos
- [x] Consulta de catálogos (6 tipos)
- [x] Alta de items en catálogo
- [x] Edición de items en catálogo
- [x] Catálogo de colores de avatar
- [x] Catálogo de métodos de login

---

## Gestión de Usuarios

### 04 — Gestión de Profesionales
- [x] Alta de profesional con contraseña temporal + envío email
- [x] Consulta paginada con filtros
- [x] Edición de profesional (sin campo dirección, eliminado)
- [x] Desactivación de profesional
- [x] Auto-registro público de profesional (IN-149)
- [x] Selección opcional de institución durante registro (IN-149)
- [x] Validación asíncrona de email y matrícula en registro (IN-149)
- [x] Validación de solicitudes pendientes por admin (IN-150)
- [x] Aprobación/rechazo con motivo y email (IN-150)
- [x] Reactivación de profesionales dados de baja (IN-150)
- [x] Historial de estados del profesional (IN-150)
- [x] Vinculación de familiar a persona por profesional (IN-148)

### 05 — Gestión de Personas con Discapacidad
- [x] Alta de persona con perfil funcional
- [x] Consulta paginada con filtros
- [x] Edición de datos personales y funcionales
- [x] Configuración del método de login (con confirm popup)
- [x] Desactivación de persona (soft-delete + revocación de tokens + confirm modal)

### 06 — Gestión de Familiares
- [x] Alta directa por admin (con selector de persona obligatorio, búsqueda mín. 3 caracteres)
- [x] Alta por invitación (auto-registro → redirige a login)
- [x] Consulta paginada con columna "Familiar de"
- [x] Detalle con personas vinculadas (linkedPersons)
- [x] Edición de familiar (muestra personas vinculadas readonly)
- [x] Desactivación de familiar
- [x] Vinculación automática persona-familiar en alta directa
- [x] Envío de email con contraseña temporal en alta directa
- [x] Listar familiares disponibles con filtro personId (IN-148)
- [x] Reactivación de vínculos desvinculados previamente (IN-148)
- [x] Desvinculación con motivo obligatorio (IN-148)
- [x] Historial de cambios de vinculación (IN-148)

### 07 — Gestión de Invitaciones
- [x] Crear invitación y enviar email
- [x] Validación de código
- [x] Aceptación y registro automático (redirige a login post-registro)
- [x] Consulta de invitaciones por profesional (solo personas asignadas)
- [x] Consulta de invitaciones por admin

---

## Asignaciones

### 08 — Asignación de Profesionales
- [x] Asignar profesional a institución
- [x] Desasignar profesional de institución
- [x] Asignar persona a profesional (búsqueda con mín. 3 caracteres)
- [x] Desactivar asignación persona-profesional
- [x] Vinculación familiar automática por invitación
- [x] Vinculación familiar directa por admin
- [x] Configuración de perfil de habilidades (selección múltiple con checkboxes)
- [x] Desvinculación lógica (soft-delete)

---

## Evaluación y Planificación

### 09 — Evaluación y Diagnóstico
- [x] Configuración del perfil de habilidades (selección múltiple)
- [x] Edición del perfil funcional
- [x] Registro de diagnóstico funcional (BE + FE tab en detalle profesional)
- [x] Consulta de historial de diagnósticos (lista por fecha desc)
- [x] Edición de diagnóstico por su creador (validación server-side)
- [x] Timeline de diagnósticos en perfil de persona (tab en detalle del profesional, filtro por fecha)
- [x] Permisos dedicados: diagnoses:read, diagnoses:create, diagnoses:update

### 10 — Gestión de Actividades
- [x] Consulta de tipos de template (catálogo)
- [x] Consulta de categorías de actividad (catálogo)
- [x] Creación de actividad con wizard (área → template → contenido → metadatos)
- [x] Consulta del catálogo de actividades (propias + estándar, paginado, filtros)
- [x] Búsqueda semántica por texto libre (embedding + pgvector)
- [x] Edición de actividad propia
- [x] Desactivación/activación de actividad
- [x] Asignación de actividad a persona (modal desde lista)
- [x] Lista de asignaciones del estudiante (`/app/activities`)
- [x] Player SELECT_FIGURE — selección de figura con pictograma
- [x] Player ORDER_SEQUENCE — ordenar secuencia con botones ▲▼
- [x] Player MATCH_IMAGE_WORD — emparejar imagen-palabra click-click
- [x] Player VISUAL_SUM — suma visual con bolitas/pictogramas
- [x] Inicio y completado de actividad (score, tiempo, intentos)
- [x] Tab "Actividades" en person-detail del profesional (historial + resultados expandibles)
- [x] Player COMPLETE_LETTER — completar letras faltantes con selector por hueco
- [x] Wizard — editor dinámico para los 5 tipos de template (CONTENT_EDITOR_REGISTRY + ViewContainerRef)
- [x] Wizard — refactor 3 pasos (Identificación / Detalles / Contenido)
- [x] Asignación post-creación — pantalla "¿Asignar ahora?" con modal integrado
- [x] Visualización del roadmap (vista estudiante, estilo Duolingo)

### 11 — Plan de Trabajo (Roadmap)
- [x] Creación del roadmap por persona
- [x] Agregar actividades al roadmap por área
- [x] Definir orden secuencial y umbral de desbloqueo
- [x] Reordenamiento de actividades (drag-drop)
- [x] Desbloqueo manual de actividad
- [x] Eliminación de actividad del roadmap

---

## Ejecución

### 12 — Resolución de Actividades
- [x] Carga de asignación con contenido completo
- [x] Inicio de actividad (registro de respuesta via startResponse)
- [x] Player: Selección de figuras (SELECT_FIGURE)
- [x] Player: Suma visual (VISUAL_SUM)
- [x] Player: Emparejar imagen-palabra (MATCH_IMAGE_WORD)
- [x] Player: Ordenar secuencia (ORDER_SEQUENCE)
- [x] Player: Completar letra (COMPLETE_LETTER)
- [x] Completar actividad y evaluar resultado (score%, tiempo, requiredSupport)
- [x] Ver asignaciones por persona (tab en person-detail del profesional)
- [x] Visualización del roadmap (vista estudiante, estilo Duolingo)

---

## Monitoreo y Reportes

### 13 — Seguimiento de Avances
- [x] Dashboard del profesional (contadores reales)
- [x] Mi Aula (cards de personas asignadas)
- [x] Detalle de persona con edición inline

### 14 — Generación de Informes
- [x] Consulta de reportes por profesional
- [x] Consulta de reportes por familia
- [x] Creación de reporte de progreso (tipo, período, contenido)
- [x] Envío a revisión (submit por profesional)
- [x] Aprobación/rechazo por admin con email de notificación

---

## Administración de Cuentas

### 15 — Gestión de Usuarios
- [x] Listado centralizado de usuarios con filtros (rol, estado, institución)
- [x] Detalle de usuario con entidad asociada
- [x] Reseteo de contraseña (genera temporal + revoca sesiones + envío email)
- [x] Desactivación de cuenta (soft-delete + revocación de tokens)
- [x] Reactivación de cuenta (genera temporal + envío email)
- [x] Consulta de actividad reciente del usuario

---

## Capacidades Transversales

### Accesibilidad
- [x] 7 perfiles visuales (default, alto contraste, dislexia, baja visión, deuteranopía, protanopía, tritanopía)
- [x] Modo claro y oscuro (14 combinaciones)
- [x] Variables CSS por perfil
- [x] Panel de accesibilidad (Alt+A)
- [x] Toasts con colores de accesibilidad (--a11y-success, --a11y-danger, etc.)

### Autenticación
- [x] Login estándar (email + contraseña)
- [x] Login visual estándar (identificación por nombre + contraseña)
- [x] Login por PIN (4 dígitos)
- [x] Login asistido (supervisor autoriza)
- [x] Login familiar
- [x] Identificación de usuario por nombre
- [x] Refresh de token automático
- [x] Cambio de contraseña obligatorio
- [x] Redirección por rol al portal correspondiente
- [x] Validación de rol en login admin/profesional (allowedRoles)
- [x] Autofocus en todos los formularios de login
- [x] Multi-match en identificación (lista de candidatos si hay homónimos)

### Sistema
- [x] Paginación con ordenamiento dinámico (elipsis + conteo de registros)
- [x] Filtrado por institución para admins institucionales
- [x] Enforcement server-side de acceso por institución (InstitutionAccessFilter)
- [x] Guards de ruta por rol y permiso (con toast de aviso)
- [x] Directivas de permisos en interfaz
- [x] Toasts de notificación (con colores de accesibilidad)
- [x] Sidebar dinámico por rol
- [x] Seeder de base de datos con datos iniciales (vinculación familiar incluida)
- [x] Iconos en menú de acciones de tablas
- [x] Botones homologados (aria-labels, confirm modals, layout consistente)
- [x] Constantes centralizadas para razones de revocación (RevokeReasons)
- [x] Permisos completos en seed (Admin: invitaciones, instituciones, mensajes, diagnósticos:read; Professional: diagnósticos CRUD)
- [x] Templates de email con tildes y eñes correctas
- [x] Envío de email en alta de profesional y familiar
- [x] Selectores de persona con búsqueda (mín. 3 caracteres)
- [x] Campo Address eliminado de profesional (entidad + migración)
- [x] Ordenamiento por columna en tablas (sort dinámico con SortField)

### Seguridad
- [x] Autorización por recurso row-level (IN-172)
- [x] Rate limiting en endpoints de autenticación (IN-173)
- [x] Argon2id para PINs con migración lazy desde BCrypt (IN-173)
- [x] Cifrado AES-256-GCM en datos clínicos con annotation [Encrypted] (IN-173)

---

# Post-MVP — Práctica III

---

## Motor Adaptativo (MDA)

- [ ] Configuración del motor adaptativo por actividad
- [ ] Desbloqueo automático de siguiente actividad si supera umbral
- [ ] Monitoreo de frustración (pausa tras 3+ intentos)
- [ ] Evaluación automática de rendimiento tras cada actividad
- [ ] Cálculo de ajuste según estado (Estable/Progresando/Dificultad/Frustración)
- [ ] Aplicación de ajuste dentro de rangos configurados
- [ ] Registro de cada ajuste en historial de auditoría
- [ ] Alerta al profesional en estado de frustración
- [ ] Consulta del historial de ajustes (timeline)

## Radar Chart y Métricas

- [ ] Radar chart de habilidades (promedio de éxito por área)
- [x] Timeline de diagnósticos en perfil de persona
- [x] Dashboard familiar (últimas actividades, mensajes, reportes)
- [x] Portal familia con progreso completo

## Búsqueda Semántica y Pictogramas

- [x] Búsqueda semántica de actividades por lenguaje natural
- [x] Integración de pictogramas ARASAAC (arasaac.service.ts + picker en los 4 editores)

## Mensajería Interna

- [x] Bandeja de entrada de mensajes
- [x] Envío de mensajes con asunto y contenido
- [x] Hilos de conversación (respuestas)
- [x] Indicador de mensajes no leídos en sidebar
- [x] Marcado automático como leído al abrir
- [ ] Notificaciones automáticas de eventos del sistema

## Onboarding

- [ ] Wizard de completado de perfil (profesional)
- [ ] Tour guiado del portal (profesional)
- [ ] Pantalla de bienvenida (familiar)
- [ ] Pantalla de bienvenida (persona con discapacidad)

## Reportes

- [ ] Exportación de reporte a PDF

## Soporte y Ayuda

- [ ] Centro de ayuda (FAQ) con categorías y búsqueda
- [ ] ABM de FAQ por admin
- [ ] Botón flotante para reportar problema
- [ ] Creación de ticket con captura automática de contexto
- [ ] Consulta de mis tickets por usuario
- [ ] Gestión de tickets por admin (listar, responder, cambiar estado)
- [ ] Cierre automático de tickets inactivos (30 días)

## Administración de Cuentas

- [ ] Consulta de actividad reciente del usuario

---

## Resumen

| | Hecho | Pendiente |
|---|:-----:|:---------:|
| MVP | 154 | 0 |
| Post-MVP | 10 | 23 |
| **Total** | **164** | **23** |
