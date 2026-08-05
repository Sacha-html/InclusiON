# Checklist de Flujos — InclusiON MVP

**Última actualización:** 2026-05-03  
**Propósito:** Verificación funcional manual end-to-end de todos los flujos del sistema.  
**Convención:** ✅ Verificado · ❌ Falla · ⏭ Omitido (Post-MVP)

---

## 1. Autenticación

### 1.1 Login estándar (Admin / Profesional / Familiar)
- [ ] Login con email + contraseña válidos → redirige al portal correcto según rol
- [ ] Login con contraseña incorrecta → mensaje de error, sin token
- [ ] Login con cuenta inactiva → mensaje de cuenta desactivada
- [ ] Primer login con contraseña temporal → fuerza cambio de contraseña
- [ ] Cambio de contraseña exitoso → redirige al portal
- [ ] Refresh token automático al expirar access token → sesión continúa sin re-login

### 1.2 Login visual (Persona con Discapacidad)
- [ ] Identificación por nombre → lista de personas filtrada
- [ ] Selección de persona + contraseña visual → acceso al portal AAC
- [ ] PIN incorrecto → mensaje de error

### 1.3 Login por PIN
- [ ] Ingresar 4 dígitos correctos → acceso al portal AAC
- [ ] PIN incorrecto → mensaje de error

### 1.4 Login asistido
- [ ] Supervisor autorizado aprueba acceso → persona entra al portal AAC
- [ ] Supervisor no autorizado → acceso denegado

### 1.5 Login familiar
- [ ] Email + contraseña correctos → portal familiar
- [ ] Credenciales incorrectas → error

---

## 2. Portal Administrador

### ~~2.1 Instituciones~~ — ⚠️ ELIMINADO DEL DASHBOARD ADMIN
> **Decisión de modelo de negocio:** El administrador *es* la institución. La UI de gestión de instituciones fue removida del dashboard en la sesión del 2026-08-02.
> Los endpoints de backend `/api/institutions` siguen existiendo. Si en el futuro se necesita gestión multi-institución, se deberá reimplementar la UI.
> Los ítems de navegación `Instituciones` y `Mis Instituciones` fueron eliminados del sidebar.


### 2.2 Profesionales
- [ ] Crear profesional → email con contraseña temporal enviado; estado Pendiente
- [ ] Aprobar profesional (Pendiente → Aprobado) → puede loguearse
- [ ] Rechazar profesional (Pendiente → Rechazado) → no puede acceder
- [ ] Editar datos del profesional → cambios persisten
- [ ] Desactivar profesional → estado inactivo; tokens revocados
- [ ] Reactivar profesional → puede volver a loguearse
- [ ] Ver detalle de profesional → tabs: datos, personas asignadas, reportes, diagnósticos

### 2.3 Personas con Discapacidad
- [ ] Crear persona con todos los campos → aparece en listado
- [ ] DNI duplicado → error de validación
- [ ] Editar persona → cambios persisten
- [ ] Desactivar persona → inactiva; tokens revocados si tenía sesión

### 2.4 Representantes Familiares
- [ ] Crear familiar directamente (Alta directa) → email con credenciales enviado
- [ ] Email duplicado → error
- [ ] Editar familiar → cambios persisten
- [ ] Desvincular familiar de persona → vínculo inactivo
- [ ] Desactivar familiar → inactivo

### 2.5 Usuarios (gestión centralizada)
- [ ] Listado con filtros (rol, estado, institución) → resultados correctos
- [ ] Ver detalle de usuario → muestra entidad asociada (profesional/familiar)
- [ ] Resetear contraseña → email enviado; próximo login fuerza cambio
- [ ] Desactivar cuenta → usuario no puede loguearse
- [ ] Reactivar cuenta → email con temporal enviado

### 2.6 Catálogos
- [ ] Ver los 6 tipos de catálogo
- [ ] Crear ítem en catálogo → aparece en listado
- [ ] Editar ítem → cambios persisten
- [ ] Dar de baja ítem sin referencias → éxito
- [ ] Dar de baja ítem con referencias activas → error 409 con mensaje del backend

### 2.7 Reportes (cola de aprobación)
- [ ] Ver cola de reportes Enviados
- [ ] Aprobar reporte → estado Aprobado; email enviado a familiares
- [ ] Rechazar reporte con comentario → estado Rechazado; email enviado al profesional
- [ ] Ver detalle de reporte → contenido completo visible

---

## 3. Portal Profesional

### 3.1 Dashboard
- [ ] Contadores reales: personas, invitaciones, reportes
- [ ] Acceso rápido a Mi Aula y últimas actividades

### 3.2 Mi Aula
- [ ] Cards de personas asignadas con avatar y datos
- [ ] Click en card → navega a detalle de persona

### 3.3 Detalle de Persona
- [ ] Ver datos personales, discapacidad, autonomía, accesibilidad, método de login
- [ ] Edición inline: click campo → input → Enter guarda / Escape cancela
- [ ] Tab Perfil de Habilidades → seleccionar/deseleccionar áreas
- [ ] Tab Diagnósticos → crear, editar, dar de baja (solo creador)
- [ ] Tab Actividades → ver asignaciones con estado, intentos y score
- [ ] Tab Roadmap → gestión completa de roadmap
- [ ] Tab Reportes → ver reportes del profesional sobre esta persona

### 3.4 Actividades (catálogo)
- [ ] Listado paginado con filtros (área, categoría, template, estándar/propio)
- [ ] Búsqueda semántica por lenguaje natural → resultados relevantes
- [ ] Crear actividad tipo `SELECT_FIGURE` → wizard completo con ARASAAC
- [ ] Crear actividad tipo `MATCH_IMAGE_WORD` → pares imagen-palabra
- [ ] Crear actividad tipo `ORDER_SEQUENCE` → ítems ordenables
- [ ] Crear actividad tipo `VISUAL_SUM` → suma con bolitas/pictogramas
- [ ] Crear actividad tipo `COMPLETE_LETTER` → huecos y distractores
- [ ] Editar actividad propia → cambios persisten
- [ ] Desactivar actividad sin asignaciones activas → éxito
- [ ] Desactivar actividad con asignaciones activas → error 409

### 3.5 Objetivos / Roadmap
- [ ] Selector de persona → carga roadmap de la persona seleccionada
- [ ] Crear roadmap para persona sin roadmap → éxito
- [ ] Intentar crear segundo roadmap → error (ya existe)
- [ ] Agregar área de habilidad al roadmap
- [ ] Agregar actividad al área → primera actividad crea desbloqueada
- [ ] Segunda actividad → crea bloqueada
- [ ] Reordenar actividades con drag-drop → orden persiste
- [ ] Desbloquear actividad manualmente → estado cambia
- [ ] Eliminar actividad del roadmap → ya no aparece
- [ ] Editar notas del roadmap → cambios persisten

### 3.6 Asignar actividad a persona
- [ ] Desde detalle de persona → asignar actividad → estado Pendiente
- [ ] Intentar asignar misma actividad dos veces → error
- [ ] Cancelar asignación Pendiente → estado Cancelada

### 3.7 Reportes
- [ ] Crear reporte → modal post-creación: "Enviar ahora" o "Revisar después"
- [ ] "Enviar ahora" → estado Enviado de inmediato
- [ ] "Revisar después" → estado Borrador; editable
- [ ] Editar borrador → cambios persisten
- [ ] Enviar borrador → estado Enviado; ya no editable
- [ ] Reporte Rechazado → visible con motivo del admin
- [ ] Dar de baja reporte Borrador → éxito
- [ ] Intentar dar de baja reporte Enviado → bloqueado

### 3.8 Invitaciones
- [ ] Crear invitación para nuevo familiar → email enviado
- [ ] Ver estado de invitaciones (Pendiente / Usada / Expirada)

### 3.9 Mensajes
- [ ] Bandeja de entrada con indicador read/unread
- [ ] Enviar mensaje a familiar → aparece en enviados
- [ ] Abrir mensaje → se marca como leído automáticamente
- [ ] Responder en hilo → respuesta visible para el otro actor
- [ ] Badge en sidebar muestra conteo de no leídos
- [ ] Badge se actualiza al abrir mensaje

---

## 4. Portal AAC (Persona con Discapacidad)

### 4.1 Roadmap visual
- [ ] Ver actividades agrupadas por área
- [ ] Actividades bloqueadas no son seleccionables
- [ ] Actividades desbloqueadas son seleccionables
- [ ] Actividades completadas muestran checkmark / porcentaje
- [ ] Actividades completadas son seleccionables para volver a jugar (re-intento)
- [ ] Al completar o salir de la actividad en el player, se redirige de nuevo a Mi Camino (/app/roadmap)

### 4.2 Resolver actividad — SELECT_FIGURE
- [ ] Pantalla de intro con instrucciones → botón Empezar
- [ ] Seleccionar figura correcta → marca acierto
- [ ] Seleccionar figura incorrecta → marca error
- [ ] Completar todas → pantalla de resultado con score
- [ ] Score ≥ 80% → resultado "Éxito"
- [ ] Score 50–79% → resultado "Parcial"
- [ ] Score < 50% → resultado "Fallido"
- [ ] Botón Reintentar → nueva instancia del player

### 4.3 Resolver actividad — MATCH_IMAGE_WORD
- [ ] Click imagen → queda seleccionada; click palabra → empareja
- [ ] Par correcto → fondo verde, no editable
- [ ] Par incorrecto → fondo rojo, se puede reintentar
- [ ] Completar todos los pares → resultado

### 4.4 Resolver actividad — ORDER_SEQUENCE
- [ ] Botones ▲▼ reordenan ítems
- [ ] Confirmar orden → evalúa resultado
- [ ] Orden correcto → Éxito

### 4.5 Resolver actividad — VISUAL_SUM
- [ ] Visualización de suma con bolitas/pictogramas
- [ ] Seleccionar resultado correcto → Éxito

### 4.6 Resolver actividad — COMPLETE_LETTER
- [ ] Huecos visibles en la palabra
- [ ] Seleccionar letra correcta → hueco se completa
- [ ] Completar todas → resultado

### 4.7 Auto-desbloqueo
- [ ] Completar actividad con score ≥ umbral → siguiente actividad del roadmap se desbloquea automáticamente
- [ ] Score < umbral → siguiente sigue bloqueada

---

## 5. Portal Familiar

### 5.1 Dashboard
- [ ] Ver personas vinculadas con últimas actividades
- [ ] Ver conteo de mensajes no leídos
- [ ] Ver último reporte aprobado por persona

### 5.2 Actividades
- [ ] Selector de persona (si hay más de una)
- [ ] Listado de actividades con filtros: Todas / Pendientes / En progreso / Completadas
- [ ] Contadores por estado en tabs
- [ ] Card muestra: título, estado, fecha asignación, fecha vencimiento, intentos, score último intento
- [ ] Badge "Evaluación" en actividades de evaluación

### 5.3 Progreso
- [ ] Selector de persona
- [ ] Stat cards: completadas/total + barra, score promedio, en curso/pendientes, total intentos
- [ ] Barra de progreso animada
- [ ] Timeline de últimos 15 intentos con fecha, resultado y score
- [ ] Score promedio vacío si no hay completadas → muestra "—"

### 5.4 Reportes
- [ ] Solo ve reportes Aprobados
- [ ] Filtros por tipo y rango de fechas
- [ ] Ver detalle de reporte aprobado

### 5.5 Mensajes
- [ ] Bandeja de entrada con mensajes del profesional
- [ ] Enviar mensaje al profesional
- [ ] Responder en hilo
- [ ] Mensaje se marca leído al abrir

---

## 6. Accesibilidad

- [ ] Panel Alt+A abre selector de perfil
- [ ] Perfil Alto Contraste → colores aplicados en todo el portal
- [ ] Perfil Dislexia → fuente OpenDyslexic
- [ ] Perfil Low Vision → tamaño de fuente aumentado
- [ ] Perfiles Deuteranopia / Protanopia / Tritanopia → filtros de color
- [ ] Modo Oscuro → tema oscuro
- [ ] Combinación perfil + modo oscuro → ambos aplican simultáneamente
- [ ] Preferencia persiste entre sesiones (localStorage)

---

## 7. Seguridad y Permisos

- [ ] Admin no puede acceder a rutas `/pro/**` → redirigido
- [ ] Profesional no puede acceder a rutas `/admin/**` → redirigido
- [ ] Familiar no puede acceder a rutas `/pro/**` ni `/admin/**` → redirigido
- [ ] Persona AAC no puede acceder a portales de otros roles
- [ ] Profesional sin permiso `activities:create` → botón "Nueva Actividad" no visible
- [ ] Acceso a ruta protegida sin token → redirige a login
- [ ] Token expirado → refresh automático transparente o redirige a login

---

## 8. Flujo completo MVP (smoke test E2E)

> **"Un profesional crea una actividad, la organiza en el roadmap de una persona, la persona la visualiza en su portal AAC, la resuelve, el sistema registra el resultado y el profesional puede consultar la respuesta."**

- [ ] **1.** Admin crea profesional → profesional recibe email
- [ ] **2.** Admin aprueba profesional → profesional puede loguearse
- [ ] **3.** Admin crea persona → aparece en Mi Aula del profesional
- [ ] **4.** Admin crea familiar + lo vincula a la persona
- [ ] **5.** Profesional crea actividad (cualquier template)
- [ ] **6.** Profesional arma roadmap: agrega área → agrega actividad → queda desbloqueada
- [ ] **7.** Profesional asigna actividad a la persona → estado Pendiente
- [ ] **8.** Persona ingresa al portal AAC → ve actividad en roadmap
- [ ] **9.** Persona resuelve actividad → resultado registrado
- [ ] **10.** Profesional abre detalle de persona → tab Actividades → ve el intento con score
- [ ] **11.** Familiar ingresa → portal Progreso → ve el intento en el timeline
- [ ] **12.** Profesional crea reporte → lo envía → admin lo aprueba → familiar lo ve

---

## Resumen de cobertura

| Portal | Flujos | Verificados | Fallidos |
|--------|--------|-------------|---------|
| Autenticación | 10 | | |
| Admin | 27 | | |
| Profesional | 33 | | |
| AAC | 18 | | |
| Familiar | 14 | | |
| Accesibilidad | 9 | | |
| Seguridad | 7 | | |
| Smoke test E2E | 12 | | |
| Sprint 10 | 20 | | |
| **Total** | **150** | | |

---

## Sprint 10 — Roadmap Estándar, Players y Modelo de Negocio

### 10.1 Roadmap Estándar — Carga de Players y Reintentos

- [ ] Alumno nuevo recibe roadmap de 10 niveles automáticamente al ser creado
- [ ] Nivel 1 aparece desbloqueado; niveles 2–10 aparecen bloqueados
- [ ] Al abrir el Nivel 1, el player carga correctamente el juego (no muestra "actividad no disponible")
- [ ] Cada uno de los 10 niveles carga su tipo de player correcto (OPTION_SELECT, ORDER_SEQUENCE, CLASSIFY, PICTOGRAM_SELECT, MATCH_IMAGE_WORD, GLOBAL_READING)
- [ ] Alumno completa nivel con ≥60% → el siguiente nivel se desbloquea automáticamente
- [ ] Alumno completa nivel con <60% → el siguiente nivel permanece bloqueado; puede reintentar
- [ ] Actividad completada en el roadmap (Nivel 1) se puede volver a jugar y genera un nuevo intento limpio
- [ ] Al finalizar o salir del reproductor, redirige a Mi Camino (/app/roadmap)

**Casos de borde:**
- [ ] Servidor reiniciado → actividades con `ContentJson = '{}'` se parchean; las que ya tienen contenido no cambian
- [ ] Alumno completa el Nivel 10 → no aparece ningún nivel adicional
- [ ] Tipo de actividad sin player (SOUND_RECOGNITION) → nunca aparece en el roadmap estándar

### 10.2 Animación de Celebración

- [ ] Alumno completa actividad con resultado "Éxito" → aparece overlay con medalla, brillo y confetti
- [ ] Alumno completa con resultado "Parcial" (<60%) → NO aparece la animación de celebración
- [ ] Alumno completa con resultado "Fallido" → NO aparece la animación de celebración
- [ ] Después de la animación, los botones de la pantalla de resultado siguen siendo accesibles

### 10.3 Perfil Persona — Sin Calendario

- [ ] El home del alumno (AAC) NO muestra el botón "Ver Calendario"
- [ ] La barra de navegación inferior del alumno NO muestra el ítem "Calendario"
- [ ] Acceder manualmente a `/app/calendar` redirige o muestra 404
- [ ] El profesional SÍ ve su calendario en `/pro/calendar` sin cambios
- [ ] El familiar SÍ ve su calendario en `/family/calendar` sin cambios

### 10.4 Dashboard Admin — Sin Módulo de Instituciones

- [ ] El sidebar del admin NO muestra el ítem "Instituciones"
- [ ] El sidebar del admin NO muestra el ítem "Mis Instituciones"
- [ ] Acceder manualmente a `/admin/institutions` redirige o muestra 404
- [ ] Acceder manualmente a `/admin/my-institutions` redirige o muestra 404
- [ ] El admin SÍ ve: Dashboard, Familiares, Invitaciones, Personas, Profesionales, Usuarios, Reportes, Catálogos
- [ ] `GET /api/institutions` (backend) sigue respondiendo correctamente
