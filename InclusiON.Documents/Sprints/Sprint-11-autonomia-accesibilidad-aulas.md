# Sprint 11 — Gestión de Aulas, Registro Unificado y Autonomía de Alumnos (PIN/Asistido)

**Período:** Agosto 2026

**Objetivo:** Permitir la creación de aulas vacías y el registro unificado transaccional (Alumno + Tutor + Aula), implementar la refactorización de accesibilidad y autonomía eliminando el login por email para alumnos, migrar credenciales a PIN por defecto (1234), actualizar la UI de identificación de familiares a email y aplicar validaciones dinámicas en el ABM de personas.

---

## Tareas

### Gestión de Aulas y Registro Unificado (HU-17)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-301 | Creación de aulas vacías sin alumnos obligatorios | `CreateClassroomCommandHandler.cs` | `professional-persons.component.ts` | ✅ DONE |
| IN-302 | Endpoint transaccional de registro unificado Alumno + Tutor + Aula | `CreatePersonWithTutorCommandHandler.cs` | `persons.service.ts` | ✅ DONE |
| IN-303 | Rediseño de Asistente de Registro (Wizard) de 3 Pasos | — | `new.component.ts` / `.html` | ✅ DONE |
| IN-304 | Normalización de filtrado por nombre de aula para resolver encriptación dinámmica | `AssignmentsRepository.cs` | `list.component.ts` / `professional-persons.component.ts` | ✅ DONE |

---

### Autonomía de Alumnos y Métodos de Acceso (HU-18)

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-310 | Restricción de inicio de sesión por email para rol `PersonWithDisability` | `LoginCommandHandler.cs`, `VisualStandardLoginCommandHandler.cs` | — | ✅ DONE |
| IN-311 | Restricción de reasignación del método Email/STANDARD en ABM de alumnos | `UpdateLoginMethodCommandHandler.cs` | `change-login-method-modal.component.ts` | ✅ DONE |
| IN-312 | Migración de base de datos a PIN por defecto (1234) para alumnos existentes | `20260813190000_MigrateStudentLoginMethodsToPin.cs` + `migrate_students_login_method.sql` | — | ✅ DONE |
| IN-313 | Actualización de UI/Copy para Login de Familiares (`userType === 'FAMILY'`) a Email | — | `identify-user.component.ts` / `.html` | ✅ DONE |
| IN-314 | Filtrado de opción Email en dropdown "Método de Login" en ABM de Alumnos | — | `new.component.ts`, `change-login-method-modal.component.ts` | ✅ DONE |
| IN-315 | Validación dinámica de PIN obligatorio cuando `loginMethodId == 2` (PIN) y desactivación cuando es Asistido | — | `new.component.ts` / `.html` | ✅ DONE |

---

## Resumen del Sprint

1. **Restricción de Login por Email en Alumnos:** Por definición del producto, los alumnos no gestionan su acceso vía correo electrónico. El backend bloquea los intentos de autenticación por email para el rol `PersonWithDisability`.
2. **Migración de Datos:** Se actualizaron todos los alumnos de la base de datos con método Email a `PIN` (ID 2) con el valor `1234`.
3. **Login de Familiares por Email:** En la vista de identificación visual, la solapa o tipo de usuario `FAMILY` solicita explícitamente "Escribe tu email...".
4. **Formulario ABM Adaptado:** El selector de método de login solo muestra "PIN" y "Asistido". Si se selecciona "PIN", el PIN es estrictamente obligatorio (`Validators.required`). Si se selecciona "Asistido", el campo PIN se oculta o deshabilita.
