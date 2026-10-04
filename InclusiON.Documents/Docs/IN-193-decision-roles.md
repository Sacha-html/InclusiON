# IN-193 — Decisión: Estructura de Roles del Sistema

**Tipo:** Registro de Decisión Arquitectónica (ADR)  
**Estado:** Adoptada  
**Fecha:** Mayo 2026  
**Autores:** Equipo InclusiON (Aparicio, Cochis, Decalli, Del Barrio, Wlk)

---

## Contexto

Al diseñar la plataforma, el equipo debía decidir cómo modelar el acceso de los distintos actores del negocio educativo y terapéutico. Las opciones evaluadas fueron:

1. **Un sistema de permisos granulares sin roles fijos (solo claims):** Máxima flexibilidad teórica, pero inmanejable para definir portales e interfaces diferenciadas.
2. **Roles fijos con permisos predeterminados no modificables:** Rígido ante requerimientos operativos de distintas instituciones escolares.
3. **Roles fijos con permisos configurables a nivel institucional ← Opción elegida:** Otorga predictibilidad técnica en interfaces y flujos de autenticación, permitiendo a la dirección escolar gobernar los permisos de su personal.
4. **Roles jerárquicos (admin > profesional > familia > persona):** Incompatible con la realidad del negocio, ya que un rol superior heredaría facultades absurdas o riesgosas (ej. un directivo jugando actividades o un docente configurando la institución).

---

## Decisión

Se adoptó un esquema de **4 roles fijos con permisos configurables** y un mecanismo de alcance institucional estricto delimitado por la sede escolar:

| Rol técnico | Actor de negocio que representa |
|---|---|
| `PersonWithDisability` | Persona con Discapacidad (Estudiante / Alumno) |
| `Professional` | Profesional (Docente de Educación Especial, Terapeuta, Gabinete) |
| `FamilyRepresentative` | Representante Familiar (Padres, Madres, Tutores Legales) |
| `Admin` | Administrador Institucional (Director / Equipo Directivo) |

El rol técnico `Admin` modela a la autoridad escolar y directiva del establecimiento. Su alcance operativo está confinado estrictamente por el claim `institutionId` (delimitación multi-tenant) y se complementa funcionalmente con el catálogo de roles escolares canónicos (`InstitutionalRoles`: Director, Vicedirector, Secretario, Preceptor).

> **Aclaración sobre la no inclusión de "Admin Global":**  
> El sistema no incorpora un rol de Admin Global. El mantenimiento de bases de datos y servidores es una función de soporte e infraestructura externa. Cada institución escolar opera de forma soberana y aislada; ningún usuario del sistema posee privilegios transversales para vulnerar la privacidad médica y pedagógica de alumnos de distintas escuelas.

---

## Por qué cada rol

### `PersonWithDisability` — Rol separado
**Decisión:** No unificar con ningún otro rol.

El estudiante tiene necesidades de interfaz, accesibilidad y autenticación radicalmente distintas al resto: portal adaptativo AAC con pictogramas oficiales de ARASAAC, métodos de login alternativos (PIN numérico visual, acceso Asistido en el aula o acceso Familiar) y navegación simplificada.

Unirlo a cualquier otro rol obligaría a cargar lógica de accesibilidad en vistas que no la requieren, o a duplicar componentes innecesariamente. Asimismo, sus permisos son los más restrictivos del sistema: solo accede a sus propias actividades asignadas y a su sendero de aprendizaje ("Mi Camino").

*Alternativa descartada:* Usar el rol `familia` para el alumno con menor autonomía. Descartado porque el perfil de permisos es disjunto (la familia consulta informes y mensajería; el alumno ejecuta actividades interactivas) y los métodos de autenticación son incompatibles.

---

### `Professional` — Rol separado
**Decisión:** Rol propio con acceso restringido por asignación activa.

El profesional produce el contenido pedagógico y clínico (actividades DUA, plantillas dinámicas, diagnósticos funcionales) y accede a datos altamente sensibles (evaluaciones, evolución, respuestas). Este acceso debe estar estrictamente acotado a las personas que tiene formalmente asignadas a su cargo, no a todo el establecimiento.

Modelar esto dentro del rol `Admin` confundiría responsabilidades: la Dirección gobierna la escuela, mientras que el profesional interviene directamente sobre los estudiantes. Son tareas con naturalezas, interfaces y responsabilidades legales diferenciadas.

*Alternativa descartada:* Darle al profesional un subconjunto de permisos de admin. Descartado porque implicaría que el directivo gestiona el contenido clínico y pedagógico cotidiano, lo cual desvirtúa el rol docente.

---

### `FamilyRepresentative` — Rol separado
**Decisión:** Rol propio con acceso de solo lectura y vínculo explícito por persona.

El familiar tiene un alcance de acompañamiento: únicamente visualiza lo que el equipo directivo y docente aprobó formalmente para su difusión (informes en estado `Approved`), exclusivamente respecto a su hijo o representado, y su alta se produce únicamente por invitación directa.

Separarlo previene que herede permisos de edición pedagógica del docente o facultades de gobierno de la Dirección. Su flujo de ingreso es único (invitación con token de vinculación parental con TTL de 7 días).

*Alternativa descartada:* Que la familia use el mismo rol que el alumno. Descartado porque sus interfaces son distintas: la familia opera desde un portal web responsivo estándar con informes y PDF, no desde el portal AAC adaptativo.

---

### `Admin` — Rol Institucional Escolar (Equipo Directivo)
**Decisión:** Un rol de gobierno institucional centrado en la autonomía de la sede escolar.

La Dirección escolar es responsable de la legalidad institucional, la fiscalización de matrículas del personal docente, la organización de cursos/aulas ("Mi Aula"), la matriculación de alumnos y la auditoría obligatoria de los informes escolares antes de su notificación a las familias.

1. **Simplicidad de autorización:** Las políticas de `[Authorize(Policy = "admin")]` cubren las operaciones de gestión escolar sin dispersión de roles técnicos.
2. **Aislamiento institucional estricto:** El filtro transversal por `institutionId` (a nivel token y base de datos) garantiza que ningún directivo pueda visualizar ni modificar información perteneciente a otra escuela (Ley 25.326).
3. **Mapeo orgánico real:** Se complementa en la capa de negocio con los roles directivos de la escuela (`InstitutionalRoles`: Director, Vicedirector, Secretario, Preceptor).

---

## Por qué no se usó un sistema de permisos puro (sin roles)

Un sistema sin roles fijos (basado únicamente en claims de permisos individuales atomizados) brindaría una flexibilidad aparente, pero presentaría serias desventajas:

- **Imposibilidad de enrutar interfaces:** Haría inviable determinar el portal de inicio tras el login (¿a qué entorno se redirige si no existe la noción de rol?).
- **Complejidad y opacidad en auditoría:** Un registro de *"usuario con permiso X ejecutó actividad"* resulta menos trazable y comprensible que *"docente asignado registró diagnóstico"*.
- **Superficie de error crítica:** Un error de configuración manual al tildar permisos individuales podría otorgar acceso a expedientes médicos confidenciales a un familiar o estudiante.

Los **roles fijos** ofrecen un marco predecible y seguro por diseño; la configuración granular dentro de cada rol permite calibraciones institucionales sin alterar la arquitectura.

---

## Por qué no se usó jerarquía de roles (admin > profesional > familia > persona)

Un modelo jerárquico tradicional (donde el rol superior hereda en cascada todos los permisos del inferior) parecería más simple en código, pero colisiona con el negocio:

- Un **Directivo no debe** ejecutar actividades pedagógicas en el portal AAC — desvirtúa su función y falsearía las métricas del estudiante.
- Un **Profesional no debe** administrar la sede escolar ni otorgar altas docentes — excede su marco laboral.
- La **Persona con Discapacidad no debe** heredar permisos de nadie — su perfil es el más protegido y acotado.

Los perfiles de acceso son cualitativamente distintos, no subconjuntos inclusivos. La jerarquía obligaría a implementar "permisos de negación" para restar facultades heredadas indebidas, introduciendo fragilidad y vulnerabilidades en la seguridad.

---

## Consecuencias

1. **Persistencia de 4 roles fijos:** El sistema mantiene exactamente 4 roles técnicos canónicos (`PersonWithDisability`, `Professional`, `FamilyRepresentative`, `Admin`). Incorporar un nuevo rol requeriría una decisión formal del equipo de arquitectura.
2. **Confinamiento multi-tenant:** El claim `institutionId` delimita transversalmente todas las operaciones del rol `Admin` a su propia sede escolar, garantizando el aislamiento de datos.
3. **Trazabilidad y auditoría integral (`AccessAudit`):** La combinación de rol técnico y autorización por recurso (`[PersonAccess]`) asegura que cada acceso a datos sensibles quede estrictamente justificado por una relación jurídica activa (Ley 25.326).
