# Checklist de Procesos — InclusiON

**Última actualización:** 2026-03-23

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

### 03 — Gestión de Catálogos
- [x] Consulta de catálogos (6 tipos)
- [x] Alta de items en catálogo
- [x] Edición de items en catálogo

---

## Gestión de Usuarios

### 04 — Gestión de Profesionales
- [x] Alta de profesional con contraseña temporal
- [x] Consulta paginada con filtros
- [x] Edición de profesional
- [x] Desactivación de profesional

### 05 — Gestión de Personas con Discapacidad
- [x] Alta de persona con perfil funcional
- [x] Consulta paginada con filtros
- [x] Edición de datos personales y funcionales
- [x] Configuración del método de login

### 06 — Gestión de Familiares
- [x] Alta directa por admin
- [x] Alta por invitación (auto-registro)
- [x] Consulta paginada
- [x] Edición de familiar
- [x] Desactivación de familiar

### 07 — Gestión de Invitaciones
- [x] Crear invitación y enviar email
- [x] Validación de código
- [x] Aceptación y registro automático
- [x] Consulta de invitaciones por profesional
- [x] Consulta de invitaciones por admin

---

## Asignaciones

### 08 — Asignación de Profesionales
- [x] Asignar profesional a institución
- [x] Desasignar profesional de institución
- [x] Asignar persona a profesional
- [x] Desactivar asignación persona-profesional
- [x] Vinculación familiar automática por invitación
- [x] Vinculación familiar directa por admin
- [x] Configuración de perfil de habilidades
- [x] Desvinculación lógica (soft-delete)

---

## Evaluación y Planificación

### 09 — Evaluación y Diagnóstico
- [x] Configuración del perfil de habilidades
- [x] Edición del perfil funcional
- [ ] Registro de diagnóstico funcional
- [ ] Consulta de historial de diagnósticos
- [ ] Edición de diagnóstico por su creador
- [ ] Timeline de diagnósticos en perfil de persona

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

### Sistema
- [x] Paginación con ordenamiento dinámico
- [x] Filtrado por institución para admins institucionales
- [x] Guards de ruta por rol y permiso
- [x] Directivas de permisos en interfaz
- [x] Toasts de notificación
- [x] Sidebar dinámico por rol
- [x] Seeder de base de datos con datos iniciales

---

## Administración de Cuentas

### 17 — Gestión de Usuarios
- [ ] Listado centralizado de usuarios con filtros (rol, estado, institución)
- [ ] Detalle de usuario con entidad asociada
- [ ] Reseteo de contraseña (genera temporal + revoca sesiones)
- [ ] Desactivación de cuenta (soft-delete + revocación de tokens)
- [ ] Reactivación de cuenta (genera temporal)
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
| Items checkeados | 48 | 51 |
