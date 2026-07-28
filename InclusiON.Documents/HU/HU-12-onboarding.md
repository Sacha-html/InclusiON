# HU-12 — Onboarding de Usuarios

| Campo | Contenido |
|---|---|
| ID | HU-12 |
| Épica | Onboarding |
| Título | Onboarding de Nuevos Usuarios |
| Prioridad | Alta |
| Estimación | 3 puntos de historia |
| Sprint asignado | Sprint 7 |
| Estado | Completada |

**Proceso relacionado:** 18

---

## Historia de Usuario

**Como** usuario nuevo (profesional o familiar)
**Quiero** ser guiado en mi primer ingreso al sistema para completar mi perfil y conocer las herramientas disponibles
**Para** poder empezar a trabajar rápidamente sin necesitar asistencia externa

---

## Descripción funcional

El flujo de primer ingreso varía según el rol:

### Profesional
1. Login con contraseña temporal → cambio obligatorio de contraseña (ya implementado)
2. Si el perfil tiene campos obligatorios vacíos → wizard de completado (especialidad, teléfono, matrícula)
3. Tour guiado del portal (tooltips sobre las secciones principales, se muestra una sola vez)

### Familiar
1. Login post-registro por invitación
2. Pantalla de bienvenida con resumen de datos y persona vinculada
3. Breve explicación de qué puede hacer en el portal

### Persona con Discapacidad
1. El profesional configura método de login y accesibilidad durante el alta
2. Primer login supervisado por el profesional
3. Pantalla de bienvenida simple con avatar y nombre

---

## Criterios de Aceptación

### Profesional
- [ ] Tras el primer cambio de contraseña, si el perfil está incompleto, se muestra wizard de completado
- [ ] El wizard solicita: especialidad, teléfono, matrícula profesional
- [ ] Tras completar el perfil se muestra un tour guiado del portal
- [ ] El tour se muestra una sola vez (flag `hasCompletedOnboarding`)
- [ ] El profesional puede relanzar el tour desde configuración

### Familiar
- [ ] En el primer login post-registro se muestra pantalla de bienvenida
- [ ] La pantalla muestra datos del familiar y la persona vinculada
- [ ] Se muestra una sola vez

### Persona con Discapacidad
- [ ] Primer login exitoso queda registrado en el sistema
- [ ] Se muestra pantalla de bienvenida simple con avatar y nombre

---

## Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| PUT | `/api/professionals/me/profile` | Completar perfil en onboarding |
| PUT | `/api/professionals/me/onboarding-complete` | Marcar onboarding como completado |
| GET | `/api/professionals/me` | Perfil propio (incluye `isProfileComplete`, `hasCompletedOnboarding`) |
| PUT | `/api/family/me/onboarding-complete` | Marcar onboarding familiar como completado |
| GET | `/api/family/me` | Perfil familiar con persona vinculada y `hasCompletedOnboarding` |

---

## Vistas (FE)

| Ruta | Rol | Descripción |
|------|-----|-------------|
| `/pro/onboarding/profile` | Profesional | Wizard de completado de perfil (2 pasos) |
| `/family/onboarding/welcome` | Familiar | Pantalla de bienvenida |
