# HU IN-149 — Auto-registro de Profesionales (Sign-up)

**Proceso relacionado:** 04
**Prioridad:** Alta
**Estado:** ✅ Completada

---

## Historia de Usuario

**Como** profesional del sistema (docente, terapeuta, psicólogo, etc.)
**Quiero** poder registrarme en la plataforma creando mis propias credenciales de acceso
**Para** solicitar mi alta y poder acceder al sistema una vez que un administrador valide mi identidad

---

## Descripción funcional

El profesional puede acceder a un formulario público de registro sin necesidad de estar autenticado. Completa sus datos personales, selecciona opcionalmente la institución a la que pertenece y envía la solicitud. El profesional queda en estado `Pending` hasta que un administrador valide su solicitud.

### Flujo de registro

1. El profesional accede a la página pública de registro (`/register-professional`)
2. Completa el formulario con:
   - Nombre, Apellido, Email, Documento (obligatorios)
   - Teléfono (opcional)
   - Especialidad (obligatoria)
   - Matrícula/Número de licencia (opcional)
   - Fecha de nacimiento (obligatoria, mayor de 18 años)
   - Institución (opcional)
3. El sistema valida que el email y la matrícula no estén en uso (validación asíncrona con debounce)
4. Al enviar, se crea el `User` (inactivo) y el `Professional` (estado `Pending`)
5. Si seleccionó institución, se crea la relación `ProfessionalInstitution`
6. Se muestra un modal de confirmación de éxito
7. El profesional es redirigido al login

---

## Criterios de Aceptación

- [x] El formulario de registro es accesible públicamente sin autenticación
- [x] Los campos obligatorios se validan en el frontend (nombre, apellido, email, documento, especialidad, fecha de nacimiento)
- [x] La fecha de nacimiento valida formato dd/mm/aaaa, no es futura y el profesional es mayor de 18 años
- [x] El email se valida en tiempo real contra usuarios y profesionales existentes (debounce 800ms)
- [x] La matrícula se valida en tiempo real contra profesionales existentes (debounce 800ms)
- [x] Si el email o la matrícula ya existen, se muestra error y se bloquea el envío
- [x] Al registrarse exitosamente, se muestra un modal de confirmación con botón "Aceptar" que redirige al login
- [x] El profesional se crea con `Status = Pending` y `User.IsActive = false`
- [x] Si se selecciona institución, se crea la relación `ProfessionalInstitution` con `IsActive = true`
- [x] El endpoint de instituciones es público para permitir la selección en el registro

---

## Endpoints

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| POST | `/api/Professionals/register` | Público | Registro público de profesional |
| GET | `/api/Institutions` | Público | Listado de instituciones (para el select) |
| GET | `/api/ProfessionalValidation/email` | Público | Validación asíncrona de email |
| GET | `/api/ProfessionalValidation/license-number` | Público | Validación asíncrona de matrícula |

---

## Componentes Frontend

| Componente | Ruta | Descripción |
|------------|------|-------------|
| `RegisterProfessionalComponent` | `/register-professional` | Formulario público de registro |
| `uniqueEmailValidator` | `@shared/utils` | Validador asíncrono de email |
| `uniqueLicenseValidator` | `@shared/utils` | Validador asíncrono de matrícula |

---

## Modelos de datos

### Professional (nuevos campos usados)
- `Email` — Email del profesional (guardado antes de crear el usuario)
- `Status` — `ProfessionalStatusEnum.Pending` al registrarse
- `ProfessionalInstitutions` — Relación con institución si se seleccionó

### User
- `IsActive = false` — El usuario no puede acceder hasta ser aprobado
- `EmailConfirmed = false` — Pendiente de validación

---

## Validaciones implementadas

### Frontend (síncronas)
- `Validators.required` en nombre, apellido, email, documento, especialidad, fecha de nacimiento
- `Validators.email` en email
- `Validators.minLength(2)` y `maxLength(100)` en nombre y apellido
- `validDate` — formato dd/mm/aaaa válido
- `notFutureDate` — la fecha no puede ser futura
- `minAge(18)` — debe ser mayor de 18 años

### Frontend (asíncronas)
- `uniqueEmailValidator` — verifica que el email no exista en Users ni Professionals
- `uniqueLicenseValidator` — verifica que la matrícula no exista en Professionals
- Debounce de 800ms en ambas validaciones

### Backend
- Verificación de email duplicado (en Users)
- Verificación de documento duplicado (en Professionals)
- Creación de User + Professional en transacción
