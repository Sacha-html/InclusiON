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

## 3. Rediseño del Canal de Comunicación (WhatsApp Web Layout) y Auto-Scroll

* **Interfaz de Mensajería:** Se reemplazó el listado plano de correos por una interfaz moderna inspirada en WhatsApp Web en `messages.component`:
  - Panel izquierdo con barra de búsqueda rápida de contactos y conversaciones recientes ordenadas cronológicamente.
  - Burbujas de mensajes con alineación interactiva izquierda (recibidos) y derecha (enviados) con su respectiva estampa de hora.
  - Caja inferior para envío instantáneo de mensajes.
* **Auto-Scroll UX:** Se implementó una referencia de plantilla (`#chatHistory`) combinada con un efecto reactivo de Angular (`effect`). Cada vez que la lista de mensajes (`chatMessages`) se actualiza por la carga de un chat, el envío de una respuesta o la recepción de un nuevo mensaje, el contenedor realiza automáticamente un desplazamiento suave hacia el final de la ventana de conversación.

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

---

## 7. Corrección de Players de Actividades y Animación de Celebración

### Diagnóstico y Corrección de Bug Crítico

Las 10 actividades estándar del roadmap mostraban la pantalla del player pero sin ningún juego cargado. La causa raíz eran dos problemas simultáneos:
* **`ContentJson` vacío (`'{}'`):** El `RoadmapInitializer` inicializaba los registros de `ActivityContent` con un JSON vacío, por lo que ningún player (CLASSIFY, ORDER_SEQUENCE, OPTION_SELECT, PICTOGRAM_SELECT) tenía datos del juego.
* **`SOUND_RECOGNITION` sin player implementado:** La actividad 6 (Conciencia fonológica) usaba un `TemplateTypeCode` para el que no existe componente Angular en el `PLAYER_REGISTRY`, mostrando "Este tipo de actividad no está disponible" en lugar del juego.

### Backend (.NET 10)
* **`PatchStandardActivitiesContentAsync` en `DatabaseSeeder`:** Nuevo método de parche que corre automáticamente en cada arranque del backend. Detecta actividades estándar con `ContentJson = '{}'` y las actualiza con el JSON correcto compatible con cada player del frontend.
* **Corrección de `TemplateTypeId`:** Actividades 3 (`Muchos/Pocos`), 6 (`Conciencia fonológica`) y 10 (`Encuentra el intruso`) migradas de `CLASSIFY`/`SOUND_RECOGNITION` a `OPTION_SELECT`, que sí tiene player disponible.
* **`RoadmapInitializer`:** Se actualizó para que nuevos alumnos reciban `ContentJson` correcto desde el primer registro. Se añadió lógica de actualización de actividades existentes con JSON vacío.

### Frontend (Angular)
* **Fix de `InvalidStateError` con HMR:** `withViewTransitions()` en Angular 20 entra en conflicto con Hot Module Replacement en desarrollo. Se deshabilita condicionalmente en entornos `!environment.production` dentro de `app.config.ts`.
* **Animación de medalla de celebración:** Al completar exitosamente una actividad del roadmap, el `player-result.component` muestra un overlay con animaciones CSS puras: caída de medalla dorada (`medal-drop`), explosión de brillo (`burst`) y 12 partículas de confetti multicolor. Sin dependencias externas.

---

## 8. Decisiones de Modelo de Negocio — Ajuste de Perfiles de Usuario

### Contexto
Revisión del modelo de negocio que derivó en la eliminación de funcionalidades de UI que no sumaban valor para los perfiles correspondientes. Los cambios son exclusivamente de frontend; el backend y la base de datos no se modificaron.

### Frontend (Angular)

**Eliminación del Calendario del perfil Persona (`/app/calendar`)**
* Las personas con discapacidad tienen autonomía limitada y no necesitan gestionar su propio calendario — ese es un rol del profesional o familiar.
* Se eliminó la ruta `/app/calendar` de `aac/routes.ts`.
* Se eliminó el botón "Ver Calendario" del home del perfil Persona (`aac-home.component.html`).
* Se eliminó el ítem "Calendario" de la barra de navegación inferior del layout AAC (`aac-nav.component.ts`).
* El calendario sigue disponible sin cambios en los perfiles **Profesional** y **Familiar**.

**Eliminación del módulo de Instituciones del dashboard Admin (`/admin/institutions`, `/admin/my-institutions`)**
* El administrador del sistema *es* la institución. Gestionar instituciones desde adentro del dashboard no tiene sentido en el modelo de negocio actual (sistema mono-institución por tenant).
* Se eliminaron los items `Instituciones` y `Mis Instituciones` del sidebar de navegación (`_nav.ts`).
* Se eliminaron las rutas `/admin/institutions` y `/admin/my-institutions` de `app.routes.ts`.
* Se simplificó la lógica de filtrado de navegación en `default-layout.component.ts`.
* **Los endpoints de backend `/api/institutions` y el modelo de datos siguen existiendo** — solo se removió la UI de gestión.

---

## 9. Mejoras en "Mi Camino" (Roadmap) — Reintentos y Redirección de Flujo (Agosto 2026)

### Backend (.NET 10)
* **Re-ejecución de Actividades Completadas:** Se modificó `StartActivityResponseCommandHandler.cs` para permitir iniciar un nuevo intento (`StartResponse`) en asignaciones que ya tienen el estado `Completada`. Al hacerlo, la asignación se actualiza a `EnProgreso` y se genera un nuevo intento para guardar el progreso nuevo de manera limpia.

### Frontend (Angular)
* **Habilitación de Clic en Nodos Completados:** En `aac-roadmap.component.ts`, se eliminó la restricción en la acción `onNodeClick` que bloqueaba el clic en actividades completadas (`node.status === 'completed'`). Ahora el alumno puede hacer clic en cualquier actividad desbloqueada del mapa (sea pendiente, en progreso o completada) para jugarla de nuevo.
* **Redirección al Roadmap al finalizar:** En `activity-player-shell.component.ts`, se cambió el destino de redirección en `onCompleted()` para que, al finalizar el juego o presionar volver en caso de error, el alumno regrese a la pantalla de "Mi Camino" (`/app/roadmap`) en lugar del catálogo general de actividades (`/app/activities`).

---

## 10. Robustez en Semillado de Base de Datos y Tutores del Aula (Agosto 2026)

### Backend (.NET 10)
* **Robustez en `DatabaseSeeder`:** Se refactorizó el método `SeedFamilyAsync` para buscar usuarios, perfiles y relaciones existentes antes de crearlos. Esto previene violaciones de clave primaria y duplicación de datos al ejecutar el semillador de base de datos múltiples veces.
* **Fortaleza de Contraseñas:** Se actualizó la contraseña por defecto del familiar de prueba a `Familia123!` para satisfacer la restricción de longitud mínima de 8 caracteres exigida por Identity.
* **Asociación de Tutores Faltantes:** Se registraron y vincularon los familiares de prueba `anatu@test.com` (Patricia Martínez) para la estudiante Ana Martínez, y `carlostu@test.com` (Roberto Rodríguez) para Carlos Rodríguez. Esto resolvió un problema donde el profesional Pedro Martínez solo podía visualizar a 6 tutores de los 9 alumnos a cargo en el listado de mensajería (ahora visualiza correctamente a los 9 tutores).

---

## 11. Sistema y Panel de Notificaciones (Mensajes, Actividades y Calendario) (Agosto 2026)

### Backend (.NET 10)
* **Carga Eager en Actividades Completadas:** Se actualizó `ActivityAssignmentRepository.cs` para incluir la relación `.Include(a => a.Person)` en `GetByIdAsync`. Esto permite que el handler de finalización (`CompleteActivityResponseCommandHandler.cs`) cargue el nombre real del estudiante y el título de la actividad en las notificaciones enviadas, en lugar de un mensaje genérico.
* **Notificaciones de Calendario en Tiempo Real:** Modificado `CalendarController.cs` para inyectar `IBackgroundJobRepository` y disparar un trabajo en segundo plano del tipo `Push` cuando el profesional guarda o edita un evento específico para un alumno. Esto notifica en tiempo real a todos los tutores/familiares activos vinculados a dicho estudiante.

### Frontend (Angular)
* **Dropdown de Notificaciones Multirrol:** Se rediseñó el componente de la campana de notificaciones (`NotificationBellComponent`) en la cabecera del sistema. Al hacer clic, ahora despliega una lista interactiva de notificaciones recientes en lugar de redirigir inmediatamente a mensajería.
* **Habilitación de Notificaciones para el Administrador:** Se actualizó `default-header.component.ts` para que la campana de notificaciones también se muestre en el perfil de Administrador (`Admin`), sumándose a Profesional y Familiar.
* **Semillado de Notificaciones Personalizadas por Rol:** Al cargar el panel por primera vez (o si el almacenamiento local está vacío), se precargan notificaciones simuladas acordes al rol logueado para enriquecer la experiencia de usuario:
  - **Profesional:** Nuevos mensajes de tutores, confirmación de actividades completadas por alumnos y recordatorios de sesiones.
  - **Familiar:** Nuevos mensajes de terapeutas, avisos de actividades nuevas asignadas a sus representados y recordatorios de eventos del calendario.
  - **Administrador:** Alertas del sistema como registro de nuevos profesionales pendientes de aprobación, reportes semanales enviados y avisos de mantenimiento de servidor.
* **Integración en Tiempo Real y Persistencia:** Conectada la campana al flujo `notification$` del `SignalrService` para recibir eventos en tiempo real. La lista se persiste localmente en `localStorage` y permite navegar directamente al recurso asociado al hacer clic en cada notificación (mensajes, calendario, evaluaciones, etc.), además de marcar la notificación como leída o limpiar la bandeja completamente.

---

## 12. Creación de Aulas Vacías y Asistente de Registro Unificado (Alumno + Tutor + Aula) (Agosto 2026)

### Backend (.NET 10)
* **Semillado de Datos Robustos (`DatabaseSeeder.cs`):** 
  - Se agregó el método `SeedCustomClassroomsAndStudentsAsync` que asegura que los profesionales Sacha Del Barrio (nuevo), Sofía Gutiérrez y Pedro Martinez estén aprobados en el sistema.
  - Genera 6 aulas de prueba de manera automática al iniciar la API.
  - Genera y vincula transaccionalmente a 39 alumnos (`student1@inclusion.com` a `student39@inclusion.com` con clave `Student123!`) y sus respectivos 39 tutores (`tutor1@inclusion.com` a `tutor39@inclusion.com` con clave `Tutor123!`) enlazados por parentesco en la base de datos y distribuidos en las aulas (entre 5 y 8 alumnos por aula).
* **Endpoint de Registro Bulk (`POST api/persons/with-tutor`):**
  - Creado `CreatePersonWithTutorRequest.cs` y `CreatePersonWithTutorCommand.cs`.
  - Implementado `CreatePersonWithTutorCommandHandler.cs` que ejecuta toda la creación (Usuario Alumno, Persona con Discapacidad, Usuario Tutor, Familiar Representante, Enlace de Parentesco, Asignación de Aula obligatoria e Inicialización de Roadmap) de forma transaccional usando `IUnitOfWork.ExecuteInTransactionAsync`, garantizando revertir todo en caso de cualquier error (Rollback).
* **Flexibilización de Creación de Aulas:**
  - Modificado `CreateClassroomRequest.cs`, `CreateClassroomCommand.cs` y `CreateClassroomCommandHandler.cs` para admitir que la lista de IDs de alumnos sea opcional (`null` o vacía), permitiendo registrar aulas vacías.
  - Añadidos controles defensivos contra valores nulos en `AssignmentsController.cs` al validar permisos sobre listas vacías de alumnos.

### Frontend (Angular)
* **Asistente de Registro (Wizard) de 3 Pasos:**
  - Rediseñado por completo el componente de registro `NewComponent` (`new.component.ts` y `.html`) en la administración de Personas.
  - Implementado asistente visual (Wizard) que transiciona automáticamente del Paso 1 (Datos del Alumno con placeholders descriptivos y guías de ingreso) al Paso 2 (Datos del Tutor a cargo) y finalmente al Paso 3 (Asignación obligatoria de Profesional y Aula).
* **Servicios de API:**
  - Creado e integrado el modelo `CreatePersonWithTutorRequest` y el método `createPersonWithTutor()` en `persons.service.ts` para conectar con el nuevo endpoint transaccional del backend.
* **Creación de Aulas Vacías:**
  - Se eliminó la validación en `professional-persons.component.ts` y `.html` que forzaba a seleccionar alumnos antes de guardar un aula, permitiendo al administrador crear aulas vacías directamente desde la pestaña "Personas a cargo" del perfil del docente.

---

## 13. Sincronización de Aulas y Filtrado Robusto por Nombre de Aula (Agosto 2026)

### Diagnóstico de Encriptación No Determinista
* **Causa Raíz:** El backend en .NET utiliza encriptación AES con Vector de Inicialización (IV) dinámico (`AesGcmEncryptionService`), generando hashes `ENC:...` distintos para el mismo GUID en peticiones o serializaciones independientes.
* **Impacto:** La comparación estricta por ID (`p.classroomId === targetId`) resultaba falsa en el cliente de Angular, devolviendo un arreglo vacío al filtrar alumnos por aula.

### Backend (.NET 10)
* **Recarga de Navegación de Aula:** 
  - En `AssignmentsRepository.cs` (`MovePersonToClassroomAsync`), al cambiar el foreign key `ClassroomId`, se recarga explícitamente la entidad `Classroom` para asegurar que `ProfessionalPersonResponse` contenga siempre la propiedad `ClassroomName` poblada.
  - En `AssignPersonCommandHandler.cs`, se incluye la propiedad de navegación `Classroom` antes del mapeo del DTO al asignar o reactivar alumnos.

### Frontend (Angular)
* **Filtrado por Texto Plano Normalizado (`classroomName`):** 
  - Se actualizó la propiedad calculada `filteredPersons` tanto en `list.component.ts` (Vista del Profesional) como en `professional-persons.component.ts` (Panel de Administración) para filtrar utilizando `classroomName?.toLowerCase()?.trim()`.
* **Sincronización Dinámica de Contadores:**
  - En `professional-persons.component.ts`, se incorporó la llamada a `loadClassrooms()` en todas las acciones de asignación, cambio de aula, transferencia y desasignación, manteniendo las tarjetas de aulas superiores sincronizadas en tiempo real con sus contadores de alumnos.
* **Depuración y UI:**
  - Removida la opción redundante *"Todas las Aulas"* en la vista del profesional.
  - Agregados logs descriptivos de consola (`console.log`) para diagnosticar la estructura de datos en runtime.

---

## 14. Refactorización de Accesibilidad y Autonomía de Alumnos (PIN y Asistido) (Agosto 2026)

### Backend (.NET 10)
* **Restricción de Login por Email en Alumnos:**
  - `LoginCommandHandler.cs`: Agregada validación que rechaza el acceso por email/contraseña para el rol `PersonWithDisability`, devolviendo `ErrorCode.RoleNotAllowedForLogin` con el mensaje *"Los alumnos no pueden iniciar sesión con email y contraseña."*.
  - `VisualStandardLoginCommandHandler.cs`: Inhabilitado el acceso visual estándar por contraseña para personas con discapacidad.
  - `UpdateLoginMethodCommandHandler.cs`: Prohibida la reasignación del método `STANDARD` (`LoginMethodId = 1`) para alumnos.
  - `DatabaseSeeder.cs`: Configurado el alumno de prueba Juan con `LoginMethodId = 2` (PIN) y PIN `"1234"`.
* **Migración de Base de Datos:**
  - Creada la migración EF Core `20260813190000_MigrateStudentLoginMethodsToPin.cs` y el script SQL `migrate_students_login_method.sql` para actualizar automáticamente a todos los alumnos existentes que tenían login por email (`LoginMethodId = 1` o `NULL`) hacia `LoginMethodId = 2` (PIN) y PIN por defecto `1234`.

### Frontend (Angular)
* **Login de Familiares:**
  - `identify-user.component.ts` e `.html`: Actualizado el flujo para `userType === 'FAMILY'` para solicitar explícitamente el correo electrónico ("Escribe tu email...", "Escribe tu email", "Tu email").
* **Formulario ABM de Alumnos y Validaciones Dinámicas:**
  - `new.component.ts`: Filtrado el selector de métodos de inicio de sesión eliminando la opción "Email" (`STANDARD`), dejando disponibles únicamente "PIN" y "Asistido".
  - **Regla Reactiva de Validación:** Al seleccionar "PIN" (`id === 2`), el campo PIN pasa a ser estrictamente obligatorio (`Validators.required` y `Validators.pattern(/^\d{4}$/)`) y se habilita. Al elegir "Asistido", se remueven las reglas de validación, se limpia el valor y se deshabilita/oculta.
  - `change-login-method-modal.component.ts` y `login-method-selector.component.ts`: Excluida la opción "Email" del listado de métodos seleccionables en el modal de cambio de método de acceso.

