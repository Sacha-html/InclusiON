# Caso Borde: CB-10 — Docente que se anota solo e intenta habilitarse a sí mismo [DEPRECADO / ELIMINADO]

| Campo | Contenido |
|---|---|
| **Identificador** | CB-10 (Histórico / QA) |
| **Título original** | Docente que se anota solo e intenta habilitarse a sí mismo |
| **Descripción original** | Un profesional entra a la web pública, se registra con su número de matrícula e intenta "aprobar" su cuenta para empezar a ver expedientes de inmediato. La cuenta queda congelada en espera hasta que un directivo comprueba su matrícula y presiona "Aprobar". |
| **Estado actual** | **DEPRECADO Y ELIMINADO POR DISEÑO ARQUITECTÓNICO** |
| **Fecha de baja** | 17 de Septiembre de 2026 |
| **Módulos relacionados** | `HU-IN-149`, `HU-IN-150`, `CU-05`, `CU-06`, `ProfessionalsController`, `CreateProfessionalCommandHandler` |

---

## 1. Justificación Técnica de la Baja (¿Por qué se elimina?)

El caso de borde original asumía un flujo de **auto-registro público abierto** para profesionales. En la evolución de la arquitectura y el modelo de seguridad de InclusiON, **dicho flujo fue completamente desestimado y reemplazado por un aprovisionamiento institucional centralizado**:

1. **Eliminación del Endpoint de Auto-Registro:**  
   El endpoint `POST /api/professionals/register` fue dado de baja en `ProfessionalsController.cs`. No existe ruta pública en el backend que admita solicitudes de alta abiertas para profesionales.
2. **Eliminación del Acceso en el Frontend:**  
   En la pantalla de acceso (`login.component.html`), se eliminó cualquier enlace o botón hacia formularios de registro docente.
3. **Gobierno y Seguridad de Datos Médicos/Pedagógicos:**  
   InclusiON gestiona información altamente sensible de personas con discapacidad (diagnósticos, terapias, progresos). Por estándares de privacidad y seguridad institucional, **ninguna persona externa puede solicitar cuentas docentes desde internet**.
4. **Modelo Vigente: Alta Directiva Centralizada (`professionals:create`):**  
   - Los profesionales son dados de alta **exclusivamente por el Administrador / Equipo Directivo** de la institución (`POST /api/professionals`).
   - El profesional nace directamente en estado `Approved` (`Status = ProfessionalStatusEnum.Approved`) y activo (`IsActive = true`).
   - El sistema le genera una contraseña temporal (`PasswordGenerator.GenerateTemporary()`) y activa la directiva `MustChangePassword = true`.
   - En su primer inicio de sesión, el docente debe cambiar obligatoriamente su contraseña antes de poder operar en la plataforma (`HU-12 Onboarding`).

---

## 2. Impacto en la Matriz de Casos de Borde

* Al **no existir la precondición** (un profesional no puede registrarse por su cuenta ni quedar en estado `Pending`), el intento de "auto-visto-bueno" es una imposibilidad fáctica y lógica dentro del sistema actual.
* En la documentación de diagramas de estado (`casos-borde.md`), el identificador **CB-10** fue reasignado formalmente al caso borde de *`Report Approved sin PersonRepresentative activos`* para mantener la consistencia del modelo de estados de reportes pedagógicos.

---

## 3. Trazabilidad de Cambios

- [x] Reglas de negocio actualizadas en [`reglas-negocio.md`](../reglas-negocio.md) (reglas de auto-registro marcadas como deprecadas).
- [x] Casos de uso [`CU-02-gestion-usuarios.md`](../CU/CU-02-gestion-usuarios.md) (`CU-05` y `CU-06`) catalogados como deprecados/eliminados con aviso formal.
- [x] Historias de usuario `HU-IN-149` y `HU-IN-150` archivadas como históricas con nota de reemplazo por `HU-01` / `HU-17`.
