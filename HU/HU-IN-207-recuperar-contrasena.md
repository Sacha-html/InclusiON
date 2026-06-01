# HU IN-207 — Recuperar Contraseña

| Campo | Contenido |
|---|---|
| ID | HU-IN-207 |
| Épica | Acceso al Sistema |
| Título | Recuperación de Contraseña |
| Prioridad | Alta |
| Estimación | 3 puntos de historia |
| Sprint asignado | Backlog PP3 |
| Estado | Pendiente |

**Proceso relacionado:** 02  
**Jira:** IN-207

---

## Historia de Usuario

**Como** Profesional, Administrador o Familiar
**Quiero** poder recuperar el acceso a mi cuenta si olvidé mi contraseña, sin depender de un administrador
**Para** retomar mi actividad de forma autónoma y segura

**Como** Profesional o Familiar
**Quiero** poder resetear el PIN de una Persona con discapacidad desde mi portal
**Para** restablecer su acceso cuando no recuerda el PIN configurado

---

## Descripción Funcional

### Caso 1 — Self-service: Profesional / Admin / Familiar

El usuario con login estándar (email + contraseña) puede iniciar el proceso de recuperación desde la pantalla de login. Ingresa su email, recibe un token de un solo uso con expiración corta (15 min), accede a una pantalla de restablecimiento y define una nueva contraseña. Todas las sesiones activas se revocan al completar el proceso.

### Caso 2 — Reset de PIN: Persona con Discapacidad

La Persona con discapacidad usa PIN o login asistido. No tiene email propio ni acceso self-service. El reset de PIN lo inicia el Profesional asignado o el Familiar representante desde sus respectivos portales, en la sección de perfil/acceso de la Persona.

> **Por qué no aplica self-service para Persona:**
> El login de Persona es por PIN de 4 dígitos o asistido (sin email). La Persona no gestiona su propio acceso — el control está en manos del Profesional y el Familiar, que son sus referentes de confianza en el sistema.

---

## Criterios de Aceptación

### CA-01: Solicitud de recuperación (todos los roles con email)
- [ ] En la pantalla de login estándar existe el link "¿Olvidaste tu contraseña?"
- [ ] Al ingresar un email se muestra siempre el mismo mensaje genérico ("Si el email está registrado, recibirás un enlace."), independientemente de si existe o no (evita enumeración de usuarios)
- [ ] El token de reset tiene expiración de 15 minutos
- [ ] El token es de un solo uso y se invalida tras ser utilizado
- [ ] Si el usuario solicita un nuevo token antes de que expire el anterior, el anterior se invalida

### CA-02: Pantalla de restablecimiento
- [ ] El link del email redirige a `/pages/reset-password?token=...`
- [ ] Si el token es inválido o expirado, se muestra error claro con opción de solicitar uno nuevo
- [ ] El formulario solicita nueva contraseña y confirmación
- [ ] La nueva contraseña debe cumplir: mínimo 8 caracteres, 1 mayúscula, 1 número, distinta a la anterior
- [ ] Al completar, todas las sesiones activas del usuario son revocadas (`RefreshToken` invalidados)
- [ ] Se activa `MustChangePassword = false` si estaba en true
- [ ] Se registra el evento en `AccessAudit`
- [ ] Tras completar, redirige al login con mensaje de éxito

### CA-03: Accesibilidad del flujo (pantallas de recovery)
- [ ] Formularios cumplen WCAG 2.1 AA: contraste, etiquetas, mensajes de error inline
- [ ] Compatible con el panel de accesibilidad del sistema (alto contraste, tamaño de fuente)
- [ ] Funciona correctamente con navegación por teclado y lectores de pantalla
- [ ] No hay timeouts de sesión que interrumpan el flujo antes de los 15 min del token

### CA-04: Reset de PIN para Persona (desde portal Profesional)
- [ ] En el perfil de una Persona asignada, el Profesional puede resetear el PIN
- [ ] El Profesional ingresa un nuevo PIN de 4 dígitos
- [ ] El sistema valida que sea distinto al PIN actual
- [ ] Se registra el cambio en `AccessAudit` con el ID del profesional que lo realizó

### CA-05: Reset de PIN para Persona (desde portal Familiar)
- [ ] El Familiar representante activo puede resetear el PIN de la Persona vinculada
- [ ] Mismo flujo que CA-04

---

## Flujos

### Flujo A — Self-service (Profesional / Admin / Familiar)

```
Login → "¿Olvidaste tu contraseña?"
  → Ingresar email
  → Mensaje genérico (siempre igual)
  → [Email] Link con token (15 min, un solo uso)
  → /pages/reset-password?token=...
    → Token válido: formulario nueva contraseña
      → Guardar hash → revocar sesiones → AccessAudit → redirect login ✅
    → Token inválido/expirado: error + "solicitar nuevo" → volver al paso 2
```

### Flujo B — Reset PIN por Profesional/Familiar

```
Portal Profesional → Personas → [Persona] → Acceso / Seguridad
  → "Resetear PIN"
  → Ingresar nuevo PIN (4 dígitos)
  → Confirmar → Guardar hash → AccessAudit ✅
```

---

## Endpoints

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| POST | `/api/auth/forgot-password` | Público | Solicitar token de reset (siempre 200) |
| POST | `/api/auth/reset-password` | Público (token) | Aplicar nueva contraseña con token |
| PUT | `/api/persons/{id}/reset-pin` | `persons:update` | Reset de PIN por Profesional/Familiar |

---

## Vistas (Frontend)

| Ruta | Componente | Descripción |
|------|------------|-------------|
| `/pages/forgot-password` | `ForgotPasswordComponent` | Formulario de solicitud de recuperación |
| `/pages/reset-password` | `ResetPasswordComponent` | Formulario de nueva contraseña (requiere token en query param) |

**Cambios en vistas existentes:**
- `login.component.html` — Agregar link "¿Olvidaste tu contraseña?" → `/pages/forgot-password`
- `visual-standard-login.component.html` — Reemplazar texto estático por link funcional
- `professional/persons/detail` o `professional/persons/edit` — Agregar acción "Resetear PIN"
- `family/dashboard` o `family/person-detail` — Agregar acción "Resetear PIN"

---

## Modelo de Datos

### Nueva tabla / campos necesarios

```sql
-- PasswordResetToken (nueva tabla)
Id             uuid PK
UserId         uuid FK → Users
Token          varchar(128)  -- hash del token, no el token plano
ExpiresAt      timestamptz
UsedAt         timestamptz NULL
CreatedAt      timestamptz
```

> El email contiene el token en texto plano. En BD se guarda el hash (SHA-256). Al validar: hash del token recibido === hash almacenado && UsedAt IS NULL && ExpiresAt > now().

---

## Email Template

Asunto: `Recuperar acceso — InclusiON`

Contenido mínimo:
- Nombre del usuario
- Link con token (expira en 15 minutos)
- Aviso de que si no solicitó el reset, puede ignorar el email
- Sin revelar si el email existe en el sistema

Template a crear en: `Infrastructure/Templates/Emails/PasswordResetEmail.html`

---

## Restricciones y Seguridad

- Respuesta siempre genérica (evita enumeración de usuarios)
- Rate limiting en `/api/auth/forgot-password`: máx 3 solicitudes por IP cada 15 minutos
- Token de un solo uso con hash en BD (no texto plano)
- Todas las sesiones revocadas al completar el reset
- No aplica self-service para Persona: su login es PIN/asistido sin email propio

---

## Casos NO cubiertos por esta HU

| Caso | Solución existente |
|------|--------------------|
| Admin resetea contraseña de cualquier usuario | HU-11 / IN-95 (`AdminResetPasswordCommand`) |
| Persona con login asistido pierde acceso | El Profesional cambia a otro método de login desde el ABM de Personas |
| Usuario bloqueado por rate limiting | Esperar ventana de tiempo o que el admin reactive la cuenta |
