# ABM — Cuentas de Gestión y Supervisión Directiva

**Actor:** Administrador Institucional  
**Justificación:** El Administrador Institucional (Equipo Directivo) requiere gestionar los accesos de supervisión y gestión escolar (vicedirección, secretaría académica, coordinación pedagógica) para la administración operativa y colaborativa de la plataforma dentro del establecimiento.

**Entidades:** `User` (rol directivo/supervisión)

---

## Alta — Usuario Directivo / Coordinador

**Actor:** Administrador Institucional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (100) | Sí | No vacío |
| Apellido | Texto (100) | Sí | No vacío |
| Email | Texto (255) | Sí | Formato válido; único en `User` |
| Rol / Función | Enumerado | Sí | Perfil directivo / administrativo escolar |

**Validaciones de integridad:**
- El email no puede existir previamente en la tabla `User`.
- Se genera con estado activo.

**Resultado:**
- Se crea el usuario en `User` con `MustChangePassword = true` y contraseña temporal.
- Se envía notificación con las credenciales de acceso inicial.

---

## Baja — Usuario Directivo

**Actor:** Administrador Institucional

- Se establece `IsActive = false` en `User` (baja lógica).
- **Validación defensiva:** No se permite que el propio usuario que realiza la operación desactive su propia cuenta activa.

---

## Modificación — Usuario Directivo

**Actor:** Administrador Institucional

Campos editables:

| Campo | Validaciones |
|-------|--------------|
| Nombre | No vacío |
| Apellido | No vacío |
| Email | Formato válido; único (excluyendo registro actual) |

---

## Listado — Cuentas de Gestión Directiva

**Actor:** Administrador Institucional

| Columna | Descripción |
|---------|-------------|
| Nombre y Apellido | Identidad del integrante del equipo directivo |
| Email | Correo institucional |
| Último acceso | Fecha y hora del último login registrado |
| Estado | Activo / Inactivo |

**Persistencia:** Consulta a `User` filtrado por cuentas directivas y de administración escolar.
