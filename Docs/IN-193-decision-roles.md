# IN-193 — Decisión: Estructura de Roles del Sistema

**Tipo:** Registro de Decisión Arquitectónica (ADR)
**Estado:** Adoptada
**Fecha:** Mayo 2026
**Autores:** Equipo InclusiON (Aparicio, Cochis, Decalli, Del Barrio, Wlk)

---

## Contexto

Al diseñar la plataforma, el equipo debía decidir cómo modelar el acceso de los distintos actores del negocio. Las opciones evaluadas eran:

1. Un sistema de permisos granulares sin roles fijos (solo claims)
2. Roles fijos con permisos predeterminados no modificables
3. Roles fijos con permisos configurables por el admin global ← **opción elegida**
4. Roles jerárquicos (admin > profesional > familia > persona)

---

## Decisión

Se adoptó un esquema de **4 roles fijos con permisos configurables** y un mecanismo de alcance institucional para el rol admin:

| Rol técnico | Actor de negocio que representa |
|-------------|----------------------------------|
| `PersonWithDisability` | Persona con Discapacidad |
| `Professional` | Profesional (docente, terapeuta, etc.) |
| `FamilyRepresentative` | Familia / Cuidador |
| `Admin` | Admin Institucional y Admin Global |

El rol `Admin` se subdivide en práctica mediante el claim `isGlobalAdmin` (booleano) y `institutionId` (IDs asignadas), sin crear un quinto rol.

---

## Por qué cada rol

### `PersonWithDisability` — rol separado

**Decisión:** no unificar con ningún otro rol.

El actor persona con discapacidad tiene necesidades de interfaz y autenticación radicalmente diferentes al resto: portal AAC con pictogramas, métodos de login alternativos (PIN, asistido, familiar), navegación simplificada. Unirlo a cualquier otro rol obligaría a cargar lógica de accesibilidad en vistas que no la necesitan, o a duplicar pantallas. Además, sus permisos son los más restrictivos del sistema: solo accede a sus propias actividades y roadmap.

**Alternativa descartada:** usar el rol `familia` para la persona con menor autonomía. Descartado porque el perfil de permisos es diferente (familia accede a reportes y mensajería; la persona solo a actividades) y porque los métodos de login son incompatibles.

---

### `Professional` — rol separado

**Decisión:** rol propio con acceso restringido por asignación activa.

El profesional produce contenido clínico/educativo y accede a datos sensibles (diagnósticos, perfiles de habilidades, respuestas). Ese acceso debe estar acotado a las personas que tiene asignadas, no a toda la institución. Modelar esto dentro del rol `admin` confundiría responsabilidades: el admin gestiona el sistema, el profesional interviene sobre personas. Son tareas con motivaciones, interfaces y riesgos distintos.

**Alternativa descartada:** darle al profesional un subconjunto de permisos de admin. Descartado porque implicaría que el admin gestiona el contenido clínico, lo cual no corresponde a su función de negocio.

---

### `FamilyRepresentative` — rol separado

**Decisión:** rol propio con acceso de solo lectura y vínculo explícito por persona.

El familiar tiene el alcance más acotado después de la persona: solo ve lo que el profesional decidió compartir (reportes aprobados), solo de su familiar vinculado, y solo accede por invitación. Separarlo evita que herede permisos de edición del profesional o de gestión del admin. Su modo de registro es único (invitación con token), lo que hace necesario diferenciarlo en el flujo de autenticación.

**Alternativa descartada:** que la familia use el mismo rol que la persona con discapacidad. Descartado porque sus interfaces y permisos son diferentes: la familia consulta desde una vista web estándar, no desde el portal AAC.

---

### `Admin` con flag `isGlobalAdmin` — un rol, dos alcances

**Decisión:** un único rol técnico `admin`, diferenciado por claim, no por rol separado.

Las operaciones de un admin institucional y un admin global son las mismas en naturaleza (gestionar usuarios, asignar profesionales, resetear contraseñas); lo que varía es el alcance (una institución vs todo el sistema). Modelar esto con un flag en el JWT en lugar de dos roles separados tiene tres ventajas:

1. **Simplicidad de autorización:** todas las políticas de `[Authorize(Policy="admin")]` aplican a ambos. El filtro de alcance es responsabilidad del middleware y del repositorio, no de la capa de autorización.
2. **Escalabilidad:** si en el futuro se agregan jerarquías (por ejemplo, admin regional), el modelo de claims es extensible sin crear nuevos roles.
3. **Sin duplicación de código:** los controllers, handlers y políticas no necesitan variantes por subtipo de admin.

**Alternativa descartada:** roles `AdminGlobal` y `AdminInstitutional` separados. Descartado porque duplicaría todas las políticas de autorización y los tests de integración sin agregar valor diferencial.

---

## Por qué no se usó un sistema de permisos puro (sin roles)

Un sistema sin roles fijos (solo claims de permisos individuales) ofrecería máxima flexibilidad pero:

- Haría imposible definir interfaces por tipo de usuario (¿a qué portal redirige el login si no hay rol?)
- Complicaría la auditoría (un log de "usuario con permiso X accedió" es menos legible que "profesional accedió")
- Aumentaría la superficie de error de configuración: asignar mal un permiso individual podría dar acceso clínico a un familiar

Los roles fijos dan un marco predecible; los permisos configurables dentro de cada rol permiten ajustes operativos sin cambiar la arquitectura.

---

## Por qué no se usó jerarquía de roles (admin > profesional > familia > persona)

Un modelo jerárquico (`admin` hereda todos los permisos de `profesional`, que hereda los de `familia`, etc.) parecería más simple pero:

- Un admin **no debe** poder ejecutar actividades del portal AAC — no es su función
- Un profesional **no debe** poder gestionar instituciones — excede su responsabilidad
- La persona **no debe** heredar permisos de nadie — su acceso es el más restrictivo

Los perfiles de acceso son cualitativamente distintos, no una superset. La jerarquía habría requerido negar permisos heredados, lo que es más complejo y propenso a errores que asignarlos desde cero por rol.

---

## Consecuencias

- Cuatro roles fijos persisten en el sistema. Agregar un quinto requeriría una decisión del equipo (no es operación rutinaria).
- El claim `isGlobalAdmin` es parte del contrato del JWT y no puede eliminarse sin migrar tokens activos.
- La autorización por recurso (capa 3, HU-IN-172) complementa este esquema: el rol determina qué módulos son accesibles; el vínculo de asignación determina qué datos específicos son accesibles.
