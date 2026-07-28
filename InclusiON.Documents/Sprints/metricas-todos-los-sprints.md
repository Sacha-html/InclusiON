# Métricas de Sprints — InclusiON

> Generado automáticamente desde Jira-CSV.csv
> Fecha: 2026-06-02

**Convención de estados:**

| Estado Jira | Significado |
|-------------|-------------|
| **Desarrollada ✓** | HU implementada y funcional. Código en producción. |
| **Completada ✓** | HU aprobada formalmente. |
| En progreso | Desarrollo en curso. |
| Pendiente | En backlog, no iniciada. |

> ✅ = verificado en checklist de procesos (`State/checklist-procesos.md`)

---

## Sprint 0 — Arranque

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 6 |
| Historias de Usuario | 6 |
| HU completadas | 6 |
| Velocidad (HU) | 100% |
| Tasks completadas | 0 / 0 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Completada ✓ | 6 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mariano Decalli | 2 | 2 |
| Mirko Ivo Wlk | 2 | 2 |
| German Cochis | 1 | 1 |
| Sacha Del Barrio | 1 | 1 |

### Historias de Usuario

- [x] **IN-14** — Definicion de roles del equipo _[Completada ✓]_
- [x] **IN-15** — Eleccion y prueba de herramientas (Teams, GitHub, VS Code, Figma) _[Completada ✓]_
- [x] **IN-16** — Creacion de repositorios GitHub (inclusion-server, inclusion-client) _[Completada ✓]_
- [x] **IN-17** — Elaboracion del Product Backlog inicial _[Completada ✓]_
- [x] **IN-18** — Definicion de ceremonias Scrum (Daily, Planning, Review, Retro) _[Completada ✓]_
- [x] **IN-20** — Modelo de datos base iniciales _[Completada ✓]_

---

## Sprint 1 — Configuración del Sistema

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 15 |
| Historias de Usuario | 15 |
| HU completadas | 15 |
| Velocidad (HU) | 100% |
| Tasks completadas | 0 / 0 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Desarrollada ✓ | 15 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mirko Ivo Wlk | 15 | 15 |

### Historias de Usuario

- [x] **IN-21** ✅ — Registrar institución _[Desarrollada ✓]_
- [x] **IN-22** ✅ — Consultar instituciones _[Desarrollada ✓]_
- [x] **IN-23** ✅ — Editar institución _[Desarrollada ✓]_
- [x] **IN-24** ✅ — Consultar roles _[Desarrollada ✓]_
- [x] **IN-25** ✅ — Asignar permisos por modulo _[Desarrollada ✓]_
- [x] **IN-26** ✅ — Crear administrador institucional _[Desarrollada ✓]_
- [x] **IN-27** ✅ — Asignar institución a administrador _[Desarrollada ✓]_
- [x] **IN-28** ✅ — Filtrar datos por institución _[Desarrollada ✓]_
- [x] **IN-29** ✅ — Enforcement de aislamiento por institucion (InstitutionAccessFilter) _[Desarrollada ✓]_
- [x] **IN-30** ✅ — Confirmar al guardar permisos con aviso de cierre de sesiones _[Desarrollada ✓]_
- [x] **IN-31** ✅ — Revocar tokens al cambiar permisos de un rol _[Desarrollada ✓]_
- [x] **IN-32** ✅ — Invalidar caché de permisos _[Desarrollada ✓]_
- [x] **IN-33** ✅ — Consultar catálogos del sistema (6 tipos) _[Desarrollada ✓]_
- [x] **IN-34** ✅ — Registrar item en catálogo _[Desarrollada ✓]_
- [x] **IN-35** ✅ — Editar item en catálogo _[Desarrollada ✓]_

---

## Sprint 2 — Gestión de Usuarios

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 29 |
| Historias de Usuario | 29 |
| HU completadas | 29 |
| Velocidad (HU) | 100% |
| Tasks completadas | 0 / 0 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Desarrollada ✓ | 29 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mirko Ivo Wlk | 29 | 29 |

### Historias de Usuario

- [x] **IN-36** — Alta de profesional con contrasena temporal y envio de email _[Desarrollada ✓]_
- [x] **IN-37** — Consulta paginada de profesionales con filtros _[Desarrollada ✓]_
- [x] **IN-38** — Edicion de profesional _[Desarrollada ✓]_
- [x] **IN-39** — Desactivacion de profesional _[Desarrollada ✓]_
- [x] **IN-40** ✅ — Alta de persona con perfil funcional _[Desarrollada ✓]_
- [x] **IN-41** ✅ — Consulta paginada de personas con filtros _[Desarrollada ✓]_
- [x] **IN-42** ✅ — Edicion de datos personales y funcionales de persona _[Desarrollada ✓]_
- [x] **IN-43** ✅ — Configuracion del metodo de login con confirm popup _[Desarrollada ✓]_
- [x] **IN-44** ✅ — Desactivacion de persona (soft-delete + revocacion de tokens) _[Desarrollada ✓]_
- [x] **IN-45** ✅ — Alta directa de familiar con selector de persona _[Desarrollada ✓]_
- [x] **IN-46** ✅ — Alta de familiar por invitacion (auto-registro) _[Desarrollada ✓]_
- [x] **IN-47** ✅ — Consulta paginada de familiares con columna Familiar de _[Desarrollada ✓]_
- [x] **IN-48** ✅ — Detalle de familiar con personas vinculadas _[Desarrollada ✓]_
- [x] **IN-49** ✅ — Edicion de familiar _[Desarrollada ✓]_
- [x] **IN-50** ✅ — Desactivacion de familiar _[Desarrollada ✓]_
- [x] **IN-51** ✅ — Vinculacion automatica persona-familiar en alta directa _[Desarrollada ✓]_
- [x] **IN-52** ✅ — Envio de email con contrasena temporal en alta directa de familiar _[Desarrollada ✓]_
- [x] **IN-53** — Crear invitacion y enviar email _[Desarrollada ✓]_
- [x] **IN-54** — Validacion de codigo de invitacion _[Desarrollada ✓]_
- [x] **IN-55** — Aceptacion y registro automatico de invitacion _[Desarrollada ✓]_
- [x] **IN-56** — Consulta de invitaciones por profesional _[Desarrollada ✓]_
- [x] **IN-57** — Consulta de invitaciones por admin _[Desarrollada ✓]_
- [x] **IN-58** ✅ — Asignar profesional a institucion _[Desarrollada ✓]_
- [x] **IN-59** ✅ — Desasignar profesional de institucion _[Desarrollada ✓]_
- [x] **IN-60** ✅ — Asignar persona a profesional _[Desarrollada ✓]_
- [x] **IN-61** ✅ — Desactivar asignacion persona-profesional _[Desarrollada ✓]_
- [x] **IN-62** ✅ — Vinculacion familiar automatica por invitacion _[Desarrollada ✓]_
- [x] **IN-63** ✅ — Configuracion de perfil de habilidades (seleccion multiple) _[Desarrollada ✓]_
- [x] **IN-64** ✅ — Desvinculacion logica (soft-delete) de asignaciones _[Desarrollada ✓]_

---

## Sprint 3 — Autenticación y Accesibilidad

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 16 |
| Historias de Usuario | 16 |
| HU completadas | 16 |
| Velocidad (HU) | 100% |
| Tasks completadas | 0 / 0 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Desarrollada ✓ | 16 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mirko Ivo Wlk | 16 | 16 |

### Historias de Usuario

- [x] **IN-65** ✅ — Login estandar (email + contrasena) _[Desarrollada ✓]_
- [x] **IN-66** ✅ — Login visual estandar (identificacion por nombre + contrasena) _[Desarrollada ✓]_
- [x] **IN-67** ✅ — Login por PIN (4 digitos) _[Desarrollada ✓]_
- [x] **IN-68** ✅ — Login asistido (supervisor autoriza) _[Desarrollada ✓]_
- [x] **IN-69** ✅ — Login familiar _[Desarrollada ✓]_
- [x] **IN-70** ✅ — Identificacion de usuario por nombre _[Desarrollada ✓]_
- [x] **IN-71** ✅ — Refresh de token automatico _[Desarrollada ✓]_
- [x] **IN-72** ✅ — Cambio de contrasena obligatorio en primer login _[Desarrollada ✓]_
- [x] **IN-73** ✅ — Redireccion por rol al portal correspondiente _[Desarrollada ✓]_
- [x] **IN-74** ✅ — Validacion de rol en login admin/profesional (allowedRoles) _[Desarrollada ✓]_
- [x] **IN-75** — 7 perfiles visuales de accesibilidad (alto contraste, dislexia, baja vision, daltonismo) _[Desarrollada ✓]_
- [x] **IN-76** — Modo claro y oscuro (14 combinaciones) _[Desarrollada ✓]_
- [x] **IN-77** — Panel de accesibilidad (Alt+A) _[Desarrollada ✓]_
- [x] **IN-78** — Toasts con colores de accesibilidad _[Desarrollada ✓]_
- [x] **IN-79** — Guards de ruta por rol y permiso con toast de aviso _[Desarrollada ✓]_
- [x] **IN-80** — Directivas de permisos en interfaz _[Desarrollada ✓]_

---

## Sprint 4 — Evaluación, Dashboard y Actividades

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 14 |
| Historias de Usuario | 14 |
| HU completadas | 14 |
| Velocidad (HU) | 100% |
| Tasks completadas | 0 / 0 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Desarrollada ✓ | 14 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mirko Ivo Wlk | 14 | 14 |

### Historias de Usuario

- [x] **IN-81** ✅ — Configuracion del perfil de habilidades _[Desarrollada ✓]_
- [x] **IN-82** ✅ — Edicion del perfil funcional _[Desarrollada ✓]_
- [x] **IN-83** ✅ — Registro de diagnostico funcional _[Desarrollada ✓]_
- [x] **IN-84** ✅ — Consulta de historial de diagnosticos (lista por fecha desc) _[Desarrollada ✓]_
- [x] **IN-85** ✅ — Edicion de diagnostico por su creador _[Desarrollada ✓]_
- [x] **IN-87** ✅ — Dashboard del profesional con contadores reales _[Desarrollada ✓]_
- [x] **IN-88** ✅ — Mi Aula (cards de personas asignadas) _[Desarrollada ✓]_
- [x] **IN-93** ✅ — Listado centralizado de usuarios con filtros (rol, estado, institucion) _[Desarrollada ✓]_
- [x] **IN-94** ✅ — Detalle de usuario con entidad asociada _[Desarrollada ✓]_
- [x] **IN-95** ✅ — Reseteo de password _[Desarrollada ✓]_
- [x] **IN-96** ✅ — Desactivacion de cuenta (soft-delete + revocacion de tokens) _[Desarrollada ✓]_
- [x] **IN-97** ✅ — Reactivacion de cuenta (genera temporal + envio email) _[Desarrollada ✓]_
- [x] **IN-103** — Consulta de tipos de template (catalogo) _[Desarrollada ✓]_
- [x] **IN-104** — Consulta de categorias de actividad (catalogo) _[Desarrollada ✓]_

---

## Sprint 5 — Reportes

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 5 |
| Historias de Usuario | 2 |
| HU completadas | 2 |
| Velocidad (HU) | 100% |
| Tasks completadas | 3 / 3 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Desarrollada ✓ | 5 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mirko Ivo Wlk | 4 | 2 |
| Fernando Aparicio | 1 | 0 |

### Historias de Usuario

- [x] **IN-138** — Consultar reportes como familiar _[Desarrollada ✓]_
- [x] **IN-164** — Consultar reportes como profesional _[Desarrollada ✓]_

---

## Sprint 6 — Seguridad y Features Avanzadas

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 20 |
| Historias de Usuario | 4 |
| HU completadas | 4 |
| Velocidad (HU) | 100% |
| Tasks completadas | 16 / 16 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Completada ✓ | 16 |
| Desarrollada ✓ | 4 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mirko Ivo Wlk | 9 | 4 |
| Fernando Aparicio | 6 | 0 |
| Sacha Del Barrio | 5 | 0 |

### Historias de Usuario

- [x] **IN-86** ✅ — Timeline de diagnosticos en perfil de persona (Profesional) _[Desarrollada ✓]_
- [x] **IN-148** ✅ — Implementar permiso de agrupación del núcleo familiar para el rol Profesional _[Desarrollada ✓]_
- [x] **IN-149** ✅ — Implementar flujo de auto-registro (Sign-up) para el rol Profesional _[Desarrollada ✓]_
- [x] **IN-172** ✅ — Autorizacion por recurso _[Desarrollada ✓]_

---

## Sprint 7 — Actividades, Roadmap y Ejecución

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 23 |
| Historias de Usuario | 16 |
| HU completadas | 16 |
| Velocidad (HU) | 100% |
| Tasks completadas | 7 / 7 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Completada ✓ | 7 |
| Desarrollada ✓ | 16 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mirko Ivo Wlk | 19 | 16 |
| Fernando Aparicio | 1 | 0 |
| German Cochis | 1 | 0 |
| Mariano Decalli | 1 | 0 |
| Sacha Del Barrio | 1 | 0 |

### Historias de Usuario

- [x] **IN-89** ✅ — Detalle de persona con edicion inline _[Desarrollada ✓]_
- [x] **IN-105** — Creacion de actividad con wizard (area, template, contenido, metadatos) _[Desarrollada ✓]_
- [x] **IN-106** — Integracion de pictogramas ARASAAC _[Desarrollada ✓]_
- [x] **IN-107** — Consulta del catalogo de actividades (propias + estandar) _[Desarrollada ✓]_
- [x] **IN-108** — Edicion de actividad propia _[Desarrollada ✓]_
- [x] **IN-109** — Desactivacion de actividad _[Desarrollada ✓]_
- [x] **IN-110** — Creacion del roadmap por persona _[Desarrollada ✓]_
- [x] **IN-111** — Agregar actividades al roadmap por area _[Desarrollada ✓]_
- [x] **IN-112** — Definir orden secuencial y umbral de desbloqueo _[Desarrollada ✓]_
- [x] **IN-113** — Reordenamiento de actividades drag-drop _[Desarrollada ✓]_
- [x] **IN-114** — Desbloqueo manual de actividad _[Desarrollada ✓]_
- [x] **IN-115** — Eliminacion de actividad del roadmap _[Desarrollada ✓]_
- [x] **IN-117** — Visualizacion del roadmap (vista estudiante, estilo Duolingo) _[Desarrollada ✓]_
- [x] **IN-118** ✅ — Carga de asignacion con contenido completo _[Desarrollada ✓]_
- [x] **IN-119** ✅ — Inicio de actividad (registro de respuesta) _[Desarrollada ✓]_
- [x] **IN-135** — Busqueda semantica de actividades por lenguaje natural _[Desarrollada ✓]_

---

## Sprint 8 — Players y Seguimiento

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 23 |
| Historias de Usuario | 10 |
| HU completadas | 10 |
| Velocidad (HU) | 100% |
| Tasks completadas | 13 / 13 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Completada ✓ | 13 |
| Desarrollada ✓ | 10 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mirko Ivo Wlk | 19 | 10 |
| Sacha Del Barrio | 4 | 0 |

### Historias de Usuario

- [x] **IN-98** ✅ — Consulta de actividad reciente del usuario _[Desarrollada ✓]_
- [x] **IN-120** ✅ — Player: Seleccion de figuras _[Desarrollada ✓]_
- [x] **IN-121** ✅ — Player: Suma visual _[Desarrollada ✓]_
- [x] **IN-122** ✅ — Player: Emparejar imagen-palabra _[Desarrollada ✓]_
- [x] **IN-123** ✅ — Player: Ordenar secuencia _[Desarrollada ✓]_
- [x] **IN-124** ✅ — Player: Completar letra _[Desarrollada ✓]_
- [x] **IN-126** ✅ — Completar actividad y evaluar resultado _[Desarrollada ✓]_
- [x] **IN-127** — Desbloqueo automatico de siguiente actividad si supera umbral _[Desarrollada ✓]_
- [x] **IN-136** — Creacion de reporte de progreso (tipo, periodo, contenido) _[Desarrollada ✓]_
- [x] **IN-207** — Recuperar password por parte del usuario _[Desarrollada ✓]_

---

## Sprint 9 — Mensajería y Portal Familiar (En Curso)

### Resumen

| Métrica | Valor |
|---------|-------|
| Total issues | 15 |
| Historias de Usuario | 8 |
| HU completadas | 7 |
| Velocidad (HU) | 88% |
| Tasks completadas | 7 / 7 |

### Por estado

| Estado | Cantidad |
|--------|----------|
| Completada ✓ | 7 |
| Desarrollada ✓ | 7 |
| En progreso | 1 |

### Por miembro

| Miembro | Issues asignadas | HU completadas |
|---------|-----------------|----------------|
| Mirko Ivo Wlk | 11 | 7 |
| Mariano Decalli | 2 | 0 |
| Sacha Del Barrio | 2 | 0 |

### Historias de Usuario

- [x] **IN-91** — Dashboard familiar (ultimas actividades, mensajes, reportes) _[Desarrollada ✓]_
- [x] **IN-92** — Portal familia con progreso completo _[Desarrollada ✓]_
- [x] **IN-140** — Bandeja de entrada de mensajes _[Desarrollada ✓]_
- [x] **IN-141** — Envio de mensajes con asunto y contenido _[Desarrollada ✓]_
- [x] **IN-142** — Hilos de conversacion (respuestas) _[Desarrollada ✓]_
- [x] **IN-143** — Indicador de mensajes no leidos en sidebar _[Desarrollada ✓]_
- [x] **IN-144** — Marcado automatico como leido al abrir _[Desarrollada ✓]_
- [ ] **IN-145** — Notificaciones automaticas de eventos del sistema _[En progreso]_

---


