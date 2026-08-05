# HU-16 — Celebración al Completar Actividad y Ajuste de Perfiles

| Campo | Contenido |
|---|---|
| ID | HU-16 |
| Épica | Resolución de Actividades / Modelo de Negocio |
| Título | Animación de celebración y refinamiento de perfiles de usuario |
| Prioridad | Media |
| Estimación | 5 puntos de historia |
| Sprint asignado | Sprint 10 |
| Estado | Completada |
| Códigos Jira | IN-206, IN-207, IN-212, IN-213 |

---

## Historia de Usuario — Celebración (IN-206)

**Como** alumno
**Quiero** ver una animación de festejo cuando completo exitosamente una actividad
**Para** sentir que mi esfuerzo fue reconocido y mantener mi motivación

---

## Descripción funcional — Medalla (IN-206)

Cuando la persona completa una actividad con resultado `success` (≥60%), el sistema muestra un overlay de celebración en el `player-result.component`:

- Una medalla dorada cae desde arriba con efecto de rebote (`medal-drop`)
- Una explosión de brillo aparece detrás (`burst`)
- 12 partículas de confetti multicolor caen por la pantalla (`confetti`)
- Animaciones implementadas con CSS puro (keyframes), sin dependencias externas
- El overlay aparece sobre la pantalla de resultado durante 2.5 segundos
- Si el resultado NO es `success`, el overlay no aparece

---

## Criterios de Aceptación — Celebración

- [x] Al completar con éxito (≥60%), aparece la animación de medalla
- [x] Si el resultado es Parcial o Fallido, no aparece ninguna animación de celebración
- [x] La animación no bloquea la interacción posterior (botones de volver, etc.)
- [x] La animación respeta la preferencia de movimiento reducido (`prefers-reduced-motion`)
- [x] Sin dependencias externas de JavaScript

## Casos de Borde — Celebración

| Caso | Comportamiento esperado |
|------|------------------------|
| Resultado = Parcial (50–59%) | Sin animación de celebración |
| Resultado = Fallido (<50%) | Sin animación de celebración |
| Resultado = Éxito (≥60%) | Animación completa con medalla y confetti |
| Usuario tiene `sensibilidadMovimiento = true` | Animación reducida o estática (respeta `prefers-reduced-motion`) |

---

## Historia de Usuario — Auto-scroll Chat (IN-207)

**Como** profesional o familiar usando el chat
**Quiero** que la conversación se desplace automáticamente al último mensaje al abrirla o al enviar/recibir uno nuevo
**Para** no tener que hacer scroll manual cada vez

### Criterios de Aceptación

- [x] Al abrir una conversación, el scroll va al último mensaje automáticamente
- [x] Al enviar un mensaje, el scroll baja al mensaje enviado
- [x] Al recibir un nuevo mensaje (refresco), el scroll baja al nuevo mensaje
- [x] El scroll es suave (`behavior: smooth`)

---

## Historia de Usuario — Calendario fuera del perfil Persona (IN-212)

**Como** alumno (persona con discapacidad)
**Quiero** ver en mi portal solo las herramientas que puedo usar de forma autónoma
**Para** no sentirme confundido por funciones que no son para mí

### Descripción funcional

El calendario es una herramienta de gestión de agenda. Las personas con discapacidad tienen autonomía limitada y no pueden coordinar citas ni gestionar horarios por su cuenta — ese es el rol del profesional o familiar.

**Se eliminó del perfil Persona:**
- Botón "Ver Calendario" en el home AAC
- Ítem "Calendario" en la barra de navegación inferior
- Ruta `/app/calendar`

**El calendario sigue disponible en:** Profesional (`/pro/calendar`) y Familiar (`/family/calendar`)

### Criterios de Aceptación

- [x] El alumno NO ve el botón de calendario en su pantalla de inicio
- [x] El alumno NO ve el ítem de calendario en la barra de navegación inferior
- [x] La URL `/app/calendar` no es accesible desde el perfil persona (404 o redirect)
- [x] El profesional y el familiar SÍ ven su calendario sin cambios
- [x] La pantalla de inicio del alumno mantiene: Mi Camino, Mis Actividades y Hablar

### Casos de Borde

| Caso | Comportamiento esperado |
|------|------------------------|
| Alumno ingresa manualmente `/app/calendar` | Redirige a `/app` (home del alumno) |
| Profesional intenta ver `/app/calendar` | No aplica — el profesional usa `/pro/calendar` |
| Familiar con sesión activa | Sigue viendo su calendario en `/family/calendar` |

---

## Historia de Usuario — Módulo de Instituciones fuera del Admin (IN-213)

**Como** administrador del sistema
**Quiero** un dashboard enfocado en gestionar personas, profesionales y reportes
**Para** no ver módulos redundantes que no aportan valor en un modelo de institución única

### Descripción funcional

El administrador del sistema *representa* a la institución — no necesita gestionarse a sí mismo como entidad separada. En el modelo actual (mono-institución por tenant), el CRUD de instituciones en el dashboard es redundante.

**Se eliminó del dashboard Admin:**
- Ítem "Instituciones" del sidebar (CRUD de instituciones — solo visible para Global Admin)
- Ítem "Mis Instituciones" del sidebar (visible para Admin Institucional)
- Rutas `/admin/institutions` y `/admin/my-institutions`

**Los endpoints de backend `/api/institutions` siguen existiendo.** Si en el futuro se necesita gestión multi-institución, se reintroduce la UI.

### Criterios de Aceptación

- [x] El sidebar del admin NO muestra el ítem "Instituciones"
- [x] El sidebar del admin NO muestra el ítem "Mis Instituciones"
- [x] La URL `/admin/institutions` no está disponible (404 o redirect)
- [x] La URL `/admin/my-institutions` no está disponible (404 o redirect)
- [x] Los endpoints de backend `/api/institutions` siguen respondiendo correctamente
- [x] El admin sigue viendo: Dashboard, Familiares, Invitaciones, Personas, Profesionales, Usuarios, Reportes, Catálogos

### Casos de Borde

| Caso | Comportamiento esperado |
|------|------------------------|
| Admin ingresa manualmente `/admin/institutions` | Redirige a `/admin` (dashboard) |
| Global Admin vs Admin Institucional | Ambos ven el mismo sidebar sin instituciones |
| Llamada directa a `GET /api/institutions` | Sigue respondiendo (backend intacto) |

---

## Implementación técnica

**Frontend — Medalla:**
- `player-result.component.html` — markup del overlay y partículas
- `player-result.component.scss` — keyframes `medal-drop`, `burst`, `confetti-N`

**Frontend — Calendario:**
- `views/aac/routes.ts` — eliminada ruta `calendar`
- `views/aac/home/aac-home.component.html` — eliminado botón
- `layout/aac-layout/aac-nav/aac-nav.component.ts` — eliminado ítem nav

**Frontend — Instituciones:**
- `layout/default-layout/_nav.ts` — eliminados ítems nav
- `app.routes.ts` — eliminadas rutas
- `layout/default-layout/default-layout.component.ts` — simplificado filtro
