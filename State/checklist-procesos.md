# Checklist de Procesos — InclusiON

**Última actualización:** 2026-03-25

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

---

## Gestión de Usuarios

### 04 — Gestión de Profesionales
- [x] Alta de profesional con contraseña temporal + envío email
- [x] Consulta paginada con filtros
- [x] Edición de profesional (sin campo dirección, eliminado)
- [x] Desactivación de profesional

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
- [ ] Timeline de diagnósticos en perfil de persona (vista admin readonly)
- [x] Permisos dedicados: diagnoses:read, diagnoses:create, diagnoses:update

### 10 — Gestión de Actividades
- [x] Consulta de tipos de template (catálogo)
- [x] Consulta de categorías de actividad (catálogo)
- [ ] Creación de actividad con wizard (área → template → contenido → metadatos)
- [ ] Integración de pictogramas ARASAAC
- [ ] Consulta del catálogo de actividades (propias + estándar)
- [ ] Edición de actividad propia
- [ ] Desactivación de actividad

### 11 — Plan de Trabajo (Roadmap)
- [ ] Creación del roadmap por persona
- [ ] Agregar actividades al roadmap por área
- [ ] Definir orden secuencial y umbral de desbloqueo
- [ ] Reordenamiento de actividades (drag-drop)
- [ ] Desbloqueo manual de actividad
- [ ] Eliminación de actividad del roadmap
- [ ] Configuración del motor adaptativo por actividad

---

## Ejecución

### 12 — Resolución de Actividades
- [ ] Visualización del roadmap (vista estudiante, estilo Duolingo)
- [ ] Carga de asignación con contenido completo
- [ ] Inicio de actividad (registro de respuesta)
- [ ] Player: Selección de figuras
- [ ] Player: Suma visual
- [ ] Player: Emparejar imagen-palabra
- [ ] Player: Ordenar secuencia
- [ ] Player: Completar letra
- [ ] Registro de progreso durante ejecución (intentos, frustración)
- [ ] Completar actividad y evaluar resultado
- [ ] Desbloqueo automático de siguiente actividad si supera umbral
- [ ] Monitoreo de frustración (pausa tras 3+ intentos)

### 13 — Dificultad Adaptativa (MDA)
- [ ] Evaluación automática de rendimiento tras cada actividad
- [ ] Cálculo de ajuste según estado (Estable/Progresando/Dificultad/Frustración)
- [ ] Aplicación de ajuste dentro de rangos configurados
- [ ] Registro de cada ajuste en historial de auditoría
- [ ] Alerta al profesional en estado de frustración
- [ ] Consulta del historial de ajustes (timeline)
- [ ] Búsqueda semántica de actividades por lenguaje natural

---

## Monitoreo y Reportes

### 14 — Seguimiento de Avances
- [x] Dashboard del profesional (contadores reales)
- [x] Mi Aula (cards de personas asignadas)
- [x] Detalle de persona con edición inline
- [ ] Radar chart de habilidades (promedio de éxito por área)
- [ ] Dashboard familiar (últimas actividades, mensajes, reportes)
- [ ] Portal familia con progreso completo

### 15 — Generación de Informes
- [ ] Creación de reporte de progreso (tipo, período, contenido)
- [ ] Consulta de reportes por profesional
- [ ] Consulta de reportes por familia
- [ ] Exportación de reporte a PDF

---

## Comunicación

### 16 — Comunicación entre Actores
- [x] Invitaciones por email (ver Proceso 07)
- [ ] Bandeja de entrada de mensajes
- [ ] Envío de mensajes con asunto y contenido
- [ ] Hilos de conversación (respuestas)
- [ ] Indicador de mensajes no leídos en sidebar
- [ ] Marcado automático como leído al abrir
- [ ] Notificaciones automáticas de eventos del sistema

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

---

## Administración de Cuentas

### 17 — Gestión de Usuarios
- [x] Listado centralizado de usuarios con filtros (rol, estado, institución)
- [x] Detalle de usuario con entidad asociada
- [x] Reseteo de contraseña (genera temporal + revoca sesiones + envío email)
- [x] Desactivación de cuenta (soft-delete + revocación de tokens)
- [x] Reactivación de cuenta (genera temporal + envío email)
- [ ] Consulta de actividad reciente del usuario

### 18 — Onboarding
- [x] Cambio obligatorio de contraseña en primer login
- [ ] Wizard de completado de perfil (profesional)
- [ ] Tour guiado del portal (profesional)
- [ ] Pantalla de bienvenida (familiar)
- [ ] Pantalla de bienvenida (persona con discapacidad)

---

## Soporte

### 19 — Soporte y Ayuda
- [ ] Centro de ayuda (FAQ) con categorías y búsqueda
- [ ] ABM de FAQ por admin
- [ ] Botón flotante para reportar problema
- [ ] Creación de ticket con captura automática de contexto
- [ ] Consulta de mis tickets por usuario
- [ ] Gestión de tickets por admin (listar, responder, cambiar estado)
- [ ] Cierre automático de tickets inactivos (30 días)

---

## Resumen

| | Hecho | Pendiente |
|---|:-----:|:---------:|
| Items checkeados | 95 | 57 |
