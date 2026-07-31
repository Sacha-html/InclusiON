# Registro de Cambios de Desarrollo (Changelog) — InclusiON

Este documento detalla las nuevas funcionalidades, mejoras técnicas, refactorizaciones y correcciones de seguridad aplicadas a la plataforma **InclusiON** (tanto en el Cliente Angular como en el Servidor .NET 10).

---

## 1. Migración del Calendario a Base de Datos

### Backend (.NET 10)
* **Modelo de Datos:** Se implementó la entidad `CalendarEvent` en `InclusiON.Domain` que almacena los eventos de calendario de manera persistente con soporte para:
  - `Id` (Guid)
  - `Title`, `Description`
  - `Type` (Consulta, Tutoría, Clase, Tarea)
  - `Date` y `Time`
  - `TargetScope` (all, student)
  - `StudentId` y `StudentName` (para vincular clases o tareas particulares)
  - `CreatedByProfessionalId`
* **EF Core & Migraciones:** Configurada la tabla `CalendarEvents` con borrado restrictivo. Se generó y aplicó automáticamente la migración `AddCalendarEvents` al iniciar el servidor.
* **Patrón Repositorio:** Creado `ICalendarEventsRepository` y su implementación concreta en `InclusiON.Infrastructure`.
* **Controlador REST:** Creado `CalendarController` con endpoints CRUD (`GET`, `POST`, `DELETE`).
  - **Filtros de Seguridad por Rol:** Los profesionales solo pueden gestionar sus propios eventos. Los familiares/tutores solo pueden visualizar eventos generales (`all`) o los asignados específicamente a sus alumnos vinculados.

### Frontend (Angular)
* **Servicio:** Se creó `calendar.service.ts` para conectar la interfaz con la API.
* **Refactorización de UI:** El componente `calendar.component.ts` se migró del almacenamiento temporal en `localStorage` a llamadas HTTP a la API.
* **Soporte Offline (Caché):** Se conservó una copia local en `localStorage` que actúa como caché de lectura rápida y backup resiliente si la conexión con la API falla.

---

## 2. Trayectoria de Aprendizaje Estándar de 10 Niveles y Diseño Fail-Safe

### Inicialización Automática
* **Roadmap Estándar:** Creado un Roadmap preconfigurado de 10 niveles que abarca áreas de motricidad, cognición y comunicación:
  1. Rompecabezas (Motricidad Fina)
  2. Rutina visual (Cognitiva)
  3. Muchos/Pocos (Matemática Básica)
  4. Explotar burbujas (Coordinación)
  5. Selección de SAAC (Comunicación)
  6. Sonido de vocal (Fonoaudiología)
  7. Colorear libre (Expresión)
  8. Vestirse para el frío (Autonomía)
  9. Ordenar pelotas (Clasificación)
  10. Encuentra el intruso (Atención)
* **Inicializador de Negocio:** Diseñada la interfaz `IRoadmapInitializer` y su clase concreta `RoadmapInitializer` en la capa de Infraestructura para estructurar de manera limpia y consistente las áreas, actividades estándar y relaciones.
* **Auto-Inicialización:** Al registrar a cualquier alumno (ya sea a través de la UI en `CreatePersonCommandHandler` o mediante los datos de prueba iniciales en `DatabaseSeeder`), el sistema crea inmediatamente su roadmap y desbloquea el Nivel 1.
* **Prevención de Acoplamiento:** Se diseñó el puente estático `RoadmapInitializerAccessor` para resolver dependencias circulares entre el proyecto de base de datos (`InclusiON.Data`) y la capa de infraestructura.

### Lógica Anti-Frustración (Fail-Safe)
* **Progresión Continua:** En `CompleteActivityResponseCommandHandler.cs`, al completarse con éxito una actividad del roadmap, el sistema desbloquea automáticamente el siguiente nivel en secuencia y le genera su correspondiente `ActivityAssignment` en estado `Pendiente` sin requerir intervención manual del profesional.
* **Umbral Tolerante al Fallo:** La trayectoria está configurada con un umbral de desbloqueo flexible para evitar la frustración del estudiante al equivocarse.

---

## 3. Rediseño del Canal de Comunicación (WhatsApp Web Layout)

* **Interfaz de Mensajería:** Se reemplazó el listado plano de correos por una interfaz moderna inspirada en WhatsApp Web en `messages.component`:
  - Panel izquierdo con barra de búsqueda rápida de contactos y conversaciones recientes ordenadas cronológicamente.
  - Burbujas de mensajes con alineación interactiva izquierda (recibidos) y derecha (enviados) con su respectiva estampa de hora.
  - Caja inferior para envío instantáneo de mensajes.

---

## 4. Exportación de Reportes a PDF y Compartición con Tutor

* **Exportación de Rendimiento:** Añadido soporte para exportar los detalles y métricas del alumno (DNI, intentos, promedio de éxito) a un PDF limpio y formal listo para imprimir.
* **Integración con Chat:** Se añadió el botón *"Compartir con Tutor"* en el dashboard del profesional. Al hacer clic, el sistema busca a los tutores legales asignados a ese alumno e introduce automáticamente el reporte de rendimiento en la conversación del chat.

---

## 5. Ruteo de Ayuda Contextual en SignalR

* **Comportamiento en Tiempo Real (`RequestHelpCommandHandler`):**
  - Los **familiares y tutores** siempre reciben la alerta visual en tiempo real con redirección directa al panel de familia.
  - El **profesional supervisor** solo recibe la notificación si el alumno ha iniciado sesión en modo **Ingreso Asistido (Visual/SAAC)**, evitando saturar de alertas el dashboard profesional cuando el estudiante ingresa de forma independiente.

---

## 6. Seguridad y Actualización de Dependencias

* **Vulnerabilidad de Criptografía:** Se actualizó la dependencia de `System.Security.Cryptography.Xml` en todos los proyectos del backend a la versión estable no vulnerable `10.0.10`.
* **Consistencia NuGet:** Se resolvió la discrepancia de paquetes mediante la actualización de `QuestPDF` a la versión `2025.4.0` en `InclusiON.Infrastructure`.
