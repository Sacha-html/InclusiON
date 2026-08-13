# HU-17 — Gestión de Aulas y Registro Unificado de Alumno con Tutor

| Campo | Contenido |
|---|---|
| ID | HU-17 |
| Épica | Gestión de Aulas y Onboarding |
| Título | Creación de aulas vacías y registro transaccional unificado de alumnos, tutores y aulas |
| Prioridad | Alta |
| Estimación | 8 puntos de historia |
| Sprint asignado | Sprint 11 |
| Estado | Completada |
| Códigos Jira | IN-301, IN-302, IN-303 |

---

## Historia de Usuario — Creación de Aulas Vacías (IN-301)

**Como** administrador del sistema  
**Quiero** poder crear un aula asociada a un profesional sin asignar alumnos de forma inicial (aula vacía)  
**Para** poder planificar y estructurar los grupos de antemano antes de realizar las matriculaciones de los estudiantes.

### Descripción funcional
El sistema permite al administrador ingresar al perfil del profesional en la pestaña "Personas a cargo" e invocar el evento "Crear Aula". El formulario requiere un nombre de aula único. La adición de alumnos a este aula es opcional, lo que permite que el aula se cree vacía y disponible en el registro.

### Criterios de Aceptación
- [x] El administrador puede crear un aula ingresando únicamente su nombre.
- [x] No se exige seleccionar alumnos de manera obligatoria en la ventana de creación de aulas.
- [x] El aula creada se visualiza de forma consistente en las vistas y tablas del profesional, mostrando 0 alumnos asignados.
- [x] El backend y el frontend no arrojan errores de validación de lista vacía en esta operación.

### Casos de Borde
| Caso | Comportamiento esperado |
|------|------------------------|
| Nombre del aula vacío | El sistema retorna error de validación: *"El nombre del aula no puede estar vacío"*. |
| Sin alumnos en la lista | El aula se crea de manera exitosa y queda en estado activo asociada al profesional. |
| Profesional no está aprobado | El sistema retorna error de negocio: *"El profesional debe estar aprobado para asignarle un aula"*. |
| Encriptación no determinista de IDs (`ENC:...`) | El cliente normaliza el filtrado mediante `classroomName?.toLowerCase()?.trim()` para evitar fallos de coincidencia por hashes dinámicos. |
| Cambio o asignación de aula en tiempo real | El cliente reejecuta `loadClassrooms()` junto a `loadAssignedPersons()` actualizando dinámicamente los contadores `studentCount` de las tarjetas de aula. |

---

## Historia de Usuario — Flujo de Registro Unificado (IN-302)

**Como** administrador del sistema  
**Quiero** registrar a una persona con discapacidad (alumno) mediante un asistente (wizard) de 3 pasos que me obligue a ingresar los datos de su tutor (familiar) y a asignarlo a un aula con su respectivo profesional  
**Para** asegurar que ningún alumno quede registrado sin tutor a cargo ni sin docente asignado, protegiendo la integridad y consistencia del modelo de negocio.

### Descripción funcional
Se rediseñó el flujo de registro de personas en el panel de administrador por un asistente de tres pasos obligatorios:
1. **Paso 1: Datos del Alumno:** Nombre, apellido, fecha de nacimiento, documento, tipo de discapacidad, niveles funcionales, preferencias y configuración de acceso.
2. **Paso 2: Datos del Tutor:** Nombre, apellido, email (obligatorio y único, usado como usuario de login), parentesco/relación, documento y teléfono.
3. **Paso 3: Asignación de Profesional y Aula:** Selección de profesional del cual se cargan sus aulas en tiempo real y asignación del aula correspondiente.

### Integridad Transaccional (Fail-Safe)
La creación ocurre en una sola petición al backend (`POST api/persons/with-tutor`) ejecutándose en una transacción de base de datos. Si se cae la conexión en medio del flujo, o si el tutor contiene datos duplicados (email o documento), la transacción realiza un **Rollback** automático, impidiendo la creación del alumno sin su tutor.

### Criterios de Aceptación
- [x] Interfaz de carga con referencias descriptivas (placeholders) en cada campo para guiar al administrador (ej. *Ej: José*, *Ej: Pérez*).
- [x] Transición automática del Paso 1 al Paso 2 al validar los datos del alumno.
- [x] El Paso 3 exige de manera obligatoria la selección de un profesional aprobado y un aula válida asociada a él.
- [x] Si no se completa el formulario del tutor o la asignación de aula, el alumno no se crea (bloqueo en el cliente y rollback en el servidor).
- [x] El tutor recibe un correo de bienvenida automático con su contraseña temporal tras la creación exitosa de la cuenta.

### Casos de Borde
| Caso | Comportamiento esperado |
|------|------------------------|
| Email de tutor ya registrado | El sistema retorna error *"El email del tutor ya se encuentra registrado"*, y no se crea ni el tutor ni el alumno (transacción revertida). |
| Documento de alumno o tutor duplicado | El sistema detecta la colisión e impide el registro completo mediante rollback de la transacción. |
| Admin abandona o cancela el formulario en el Paso 2 o Paso 3 | No se envía nada al backend; la base de datos queda intacta (ningún alumno huérfano es registrado). |
| Intento de omitir profesional/aula | El botón "Guardar" del Paso 3 permanece deshabilitado hasta seleccionar valores válidos. |

---

## Implementación Técnica

### Backend (API .NET 10)
- **DTO Request:** `CreatePersonWithTutorRequest.cs` que agrupa el alumno, el tutor y el aula.
- **Comando:** `CreatePersonWithTutorCommand.cs` y su correspondiente manejador `CreatePersonWithTutorCommandHandler.cs` que encapsula la lógica transaccional.
- **Controlador:** `PersonsController.cs` con el nuevo endpoint `POST api/persons/with-tutor`.
- **Aulas Vacías:** Modificación en `CreateClassroomRequest.cs`, `CreateClassroomCommand.cs` y `CreateClassroomCommandHandler.cs` para permitir que el campo `PersonIds` sea opcional o nulo.

### Frontend (Angular)
- **Servicio:** `persons.service.ts` con la llamada `createPersonWithTutor`.
- **Componente:** `NewComponent` en `new.component.ts` y `.html` rediseñado a un Wizard de 3 pasos (Alumno, Tutor, Aula y Profesional obligatorios) y placeholders ilustrativos.
- **Componente de Aulas:** `ProfessionalPersonsComponent` (`professional-persons.component.ts` y `.html`) actualizado para permitir crear aulas vacías sin validar la presencia de alumnos en la lista.
