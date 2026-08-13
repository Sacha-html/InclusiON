# HU-18 — Refactorización de Autonomía y Métodos de Acceso para Alumnos (PIN y Asistido)

| Campo | Contenido |
|---|---|
| ID | HU-18 |
| Épica | Accesibilidad y Autenticación Adaptativa |
| Título | Restricción de login por email para alumnos, migración a PIN y actualización de UI/Validaciones |
| Prioridad | Alta |
| Estimación | 8 puntos de historia |
| Sprint asignado | Sprint 11 |
| Estado | Completada |
| Códigos Jira | IN-310, IN-311, IN-312 |

---

## Historia de Usuario — Restricción de Login por Email y Migración de Alumnos (IN-310)

**Como** equipo de producto e inclusión  
**Quiero** impedir que las personas con discapacidad (alumnos) inicien sesión utilizando correo electrónico y contraseña, migrando sus cuentas existentes al método PIN con credencial por defecto  
**Para** adaptar la autenticación a sus distintos niveles de autonomía y garantizar un acceso accesible y seguro.

### Descripción funcional
Por definición de negocio, los alumnos no deben gestionar su acceso vía email y contraseña. Se restringe el endpoint y la lógica de autenticación por email para el rol `PersonWithDisability` (`PERSON`). Asimismo, se ejecuta una migración de base de datos que convierte las cuentas de alumnos configuradas con login por email a método **PIN** con el valor `1234` por defecto.

### Criterios de Aceptación
- [x] El handler de autenticación por email (`LoginCommandHandler`) rechaza el acceso si el usuario posee el rol `PersonWithDisability`, retornando `ErrorCode.RoleNotAllowedForLogin` y mensaje explicativo.
- [x] El handler de inicio de sesión visual estándar (`VisualStandardLoginCommandHandler`) retorna un error informando que el login por contraseña ya no está disponible para alumnos.
- [x] El handler de actualización de método de acceso (`UpdateLoginMethodCommandHandler`) prohíbe reasignar el método `STANDARD` a un alumno.
- [x] Se ejecuta la migración EF Core (`20260813190000_MigrateStudentLoginMethodsToPin.cs`) y script SQL (`migrate_students_login_method.sql`) que actualizan a todos los alumnos con `LoginMethodId = 1` o nulo a `LoginMethodId = 2` (PIN) y hash de PIN `1234`.
- [x] El semillador de base de datos (`DatabaseSeeder.cs`) asigna por defecto `LoginMethodId = 2` y PIN `"1234"` a los alumnos de prueba.

---

## Historia de Usuario — Identificación y Login de Familiares por Email (IN-311)

**Como** familiar o tutor  
**Quiero** identificarme en el sistema ingresando mi correo electrónico en lugar de mi nombre  
**Para** agilizar mi inicio de sesión y evitar confusiones con los nombres de los alumnos.

### Descripción funcional
En el flujo de inicio de sesión visual destinado a familiares (`userType === 'FAMILY'`), se actualizó la interfaz de usuario para que el campo de identificación solicite explícitamente el email del tutor.

### Criterios de Aceptación
- [x] El atributo *placeholder* del input de identificación para el rol `FAMILY` dice `"Escribe tu email..."`.
- [x] El título del contenedor (`<h1>`) para el rol `FAMILY` muestra `"Escribe tu email"`.
- [x] La etiqueta accesible del campo (`<label>`) especifica `"Tu email"`.
- [x] El mensaje de validación ante campo vacío indica `"Por favor, escribe tu email"`.

---

## Historia de Usuario — Actualización de Formulario ABM y Validaciones Dinámicas (IN-312)

**Como** profesional o administrador  
**Quiero** disponer de un selector de método de login en el ABM de alumnos que solo permita elegir entre "PIN" y "Asistido", con reglas de validación dinámicas para el PIN  
**Para** evitar la selección errónea del método por email y garantizar que se ingrese un PIN válido cuando este método esté seleccionado.

### Descripción funcional
En los formularios de creación, edición y cambio de método de acceso de alumnos:
1. Se filtra el catálogo de métodos de inicio de sesión eliminando la opción "Email" (`STANDARD`), dejando solo "PIN" y "Asistido".
2. Se implementa una regla lógica reactiva (`Reactive Forms` en Angular):
   - Al seleccionar "PIN", el campo PIN pasa a ser estrictamente obligatorio (`Validators.required` y `Validators.pattern(/^\d{4}$/)`) y se habilita en la vista.
   - Al seleccionar "Asistido", el campo PIN remueve sus validadores, limpia su contenido y se desactiva/oculta.

### Criterios de Aceptación
- [x] El dropdown "Método de Login" en el alta y edición de alumnos no incluye la opción "Email".
- [x] Al seleccionar "PIN", el formulario exige de forma obligatoria el ingreso de un PIN numérico de 4 dígitos para poder avanzar/guardar.
- [x] Al seleccionar "Asistido", el campo PIN se oculta o inhabilita y no interfiere con la validación del formulario.
- [x] El modal de cambio de método de acceso (`ChangeLoginMethodModalComponent`) muestra únicamente las tarjetas de "PIN" y "Asistido".
