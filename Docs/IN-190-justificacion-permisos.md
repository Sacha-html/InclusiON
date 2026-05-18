# IN-190 — Justificación de Permisos respecto al Negocio

## Contexto

InclusiON opera con un modelo de autorización de tres capas apiladas:

```
[1] Autenticación JWT        → ¿quién es el usuario?              → 401 si falla
[2] Política de rol/permiso  → ¿puede acceder a este módulo?      → 403 si falla
[3] Autorización por recurso → ¿tiene vínculo con este dato?      → 403 / 404 según rol
```

Este documento justifica, desde la perspectiva del negocio, por qué cada rol tiene los permisos que tiene. La fuente legal de referencia es la **Ley 25.326 de Protección de Datos Personales** (Argentina), Art. 8, que establece que los datos sensibles de salud, de menores y de personas con discapacidad solo pueden ser accedidos por quien tenga una relación jurídica directa con el titular.

---

## Principio rector: mínimo privilegio por rol

Cada rol accede únicamente a lo necesario para cumplir su función de negocio. No existe un "acceso general" que luego se restringe; cada rol parte de permisos cero y se habilita explícitamente lo que necesita.

---

## Justificación por rol

### Persona con Discapacidad

**Función de negocio:** realizar actividades educativas interactivas y consultar su propio progreso.

| Permiso | Justificación de negocio |
|---------|--------------------------|
| Acceder al portal AAC y ejecutar actividades | Es la función central del sistema para este actor. Sin este permiso no puede participar del proceso educativo. |
| Ver su propio roadmap | Necesita saber qué actividades tiene pendientes para organizarse y motivarse. |
| Sin acceso a datos de otros usuarios | No tiene relación funcional con otros usuarios. Exponer datos de otros usuarios violaría la privacidad y generaría confusión en una interfaz diseñada para accesibilidad cognitiva. |
| Sin acceso a paneles de gestión | No es responsable de ninguna tarea administrativa. Mostrar esas interfaces sería un error de diseño y un riesgo de operación accidental. |
| Métodos de login alternativos (PIN, asistido, familiar) | La persona puede tener limitaciones en escritura o autonomía. Forzar email+contraseña excluiría a parte del universo de usuarios, contradiciendo el objetivo de inclusión del sistema. |

---

### Profesional

**Función de negocio:** evaluar personas, diseñar planes de trabajo, monitorear progreso y generar reportes clínicos/educativos.

| Permiso | Justificación de negocio |
|---------|--------------------------|
| Crear y editar actividades | Es el actor que produce el contenido educativo. Nadie más tiene el conocimiento clínico/pedagógico para hacerlo. |
| Ver perfil, diagnósticos y respuestas de sus personas asignadas | Necesita los datos clínicos para personalizar la intervención. Restringirlo a las personas **asignadas** (no a toda la institución) evita que un profesional acceda a datos de pacientes que no están bajo su cuidado — exigencia de la Ley 25.326. |
| Generar reportes | El reporte es un documento formal de evolución. Solo el profesional que interviene puede generarlo con validez clínica. |
| Invitar familiares | El profesional conoce a la familia del alumno y es responsable de habilitarles el acceso. Que sea él quien invite garantiza que solo acceden personas verificadas por el equipo profesional. |
| Sin acceso a datos de personas no asignadas | Dos profesionales de la misma institución no tienen por qué ver los datos clínicos del alumnado del otro. El vínculo de asignación activa (`ProfessionalAssignments.IsActive`) es la fuente de verdad. |
| Sin acceso a gestión de instituciones ni catálogos globales | No es su responsabilidad configurar el sistema. Darle esos permisos ampliaría el blast radius de un error operativo. |

---

### Familia / Cuidador

**Función de negocio:** acompañar el proceso educativo de su familiar, consultar su progreso y comunicarse con el profesional.

| Permiso | Justificación de negocio |
|---------|--------------------------|
| Ver progreso y reportes aprobados de su persona a cargo | El familiar necesita información del avance para participar activamente del proceso de inclusión. El acceso se limita a su persona vinculada para proteger la privacidad de otros alumnos. |
| Solo reportes en estado `Aprobado` (no borradores ni enviados) | Un borrador o informe en revisión puede contener información clínica preliminar que aún no fue validada por el profesional. Exponer un borrador podría generar alarma innecesaria o malinterpretación. |
| Mensajería con el profesional | Comunicación es parte del proceso terapéutico. El familiar informa novedades del hogar y el profesional adapta la intervención. |
| Sin acceso a datos clínicos en detalle (diagnósticos, respuestas, perfiles de habilidades) | Esos datos son de uso clínico. El familiar recibe la síntesis (el reporte) pero no los datos brutos que requieren interpretación profesional. |
| Registro solo por invitación | Evita que cualquier persona se registre como familiar de un alumno sin validación previa. El profesional es quien habilitó explícitamente el acceso, garantizando el consentimiento informado. |
| Respuesta 404 (no 403) al intentar acceder a recursos sin vínculo | Desde el negocio: un familiar no debe saber si existe un alumno en el sistema al que no tiene acceso. Un `403` confirmaría la existencia del recurso, lo que podría exponer información de terceros. |

---

### Admin Institucional

**Función de negocio:** operar el sistema dentro de su institución — gestionar cuentas, asignar profesionales y atender soporte.

| Permiso | Justificación de negocio |
|---------|--------------------------|
| Crear y gestionar usuarios de su institución | Es el responsable operativo de la institución. Nadie mejor que él conoce quiénes deben tener acceso. |
| Asignar profesionales a personas | Define qué profesional trabaja con qué alumno, una decisión organizativa de la institución. |
| Resetear contraseñas y desactivar cuentas | Gestión operativa de acceso. Necesario cuando un usuario se va de la institución o pierde sus credenciales. |
| Sin acceso a otras instituciones | Cada institución es una unidad independiente. Un admin de una escuela no tiene ni la responsabilidad ni la autorización de tocar los datos de otra. El claim `institutionId` en el JWT hace este límite técnico, no solo visual. |
| Sin acceso a configuración global (roles, catálogos) | La configuración global afecta a todo el sistema. Permitir que un admin institucional la modifique podría romper el funcionamiento de otras instituciones. |

---

### Admin Global

**Función de negocio:** configurar y mantener el sistema completo, crear instituciones y admins institucionales.

| Permiso | Justificación de negocio |
|---------|--------------------------|
| Crear y configurar instituciones | Alguien debe poder dar de alta nuevas instituciones. Es una tarea de nivel sistémico que solo compete al operador de la plataforma. |
| Asignar y revocar permisos por rol | La configuración de permisos es decisión del dueño del sistema, no de cada institución. |
| Ver y actuar sobre cualquier usuario o institución | Necesario para soporte de nivel 2 y auditorías. No se puede brindar soporte efectivo sin visibilidad total. |
| Todos los accesos auditados (`AccessAudit`) | Aunque tenga bypass total, cada acción queda registrada. Esto responde al principio de responsabilidad: el poder total va acompañado de trazabilidad total. Sin auditoría, no hay forma de demostrar cumplimiento ante un incidente. |

---

## Justificación de decisiones transversales

### Por qué el profesional no puede ver personas de su institución que no tiene asignadas

Dos profesionales de la misma institución atienden alumnos diferentes. Sus datos clínicos son independientes y confidenciales. El filtro por institución no es suficiente: la unidad de acceso correcta es la **asignación activa profesional-persona**, no la membresía institucional.

Fundamento legal: Ley 25.326 Art. 8 — datos sensibles de salud requieren relación jurídica directa.

### Por qué la familia solo ve reportes aprobados

El flujo de aprobación (`Draft → Submitted → Approved`) existe para que el profesional valide el contenido antes de que llegue a la familia. Exponer borradores saltea ese proceso y puede causar alarma o confusión con información no verificada.

### Por qué el registro familiar es solo por invitación

El auto-registro libre permitiría que cualquier persona con el email de un alumno intente vincularse. La invitación garantiza que:
1. El profesional conoce y valida al familiar.
2. El token tiene TTL de 7 días y es de un solo uso.
3. El vínculo queda registrado con el `personId` correcto desde el origen.

### Por qué 404 para familia/persona y 403 para profesional/admin

Desde el negocio, un actor externo (familia, persona) no debe poder inferir la existencia de datos en el sistema que no le corresponden. Un `403` confirmaría que el recurso existe. Un `404` neutraliza esa inferencia. Para actores internos (profesional, admin) el feedback explícito es adecuado y ayuda a operar correctamente.

---

## Matriz de permisos por módulo

| Módulo | Persona | Profesional | Familia | Admin Inst. | Admin Global |
|--------|:-------:|:-----------:|:-------:|:-----------:|:------------:|
| Portal AAC (ejecutar actividades) | ✓ | | | | |
| Crear/editar actividades | | ✓ | | | |
| Roadmap propio | ✓ | | | | |
| Roadmap de persona asignada | | ✓ | | | |
| Diagnósticos y perfil clínico | | ✓ (asignadas) | | | |
| Reportes aprobados (propios) | | | ✓ (vinculadas) | | |
| Reportes (todos los estados) | | ✓ (asignadas) | | | |
| Dashboard y monitoreo | | ✓ | ✓ (lectura) | | |
| Mensajería | | ✓ | ✓ | | |
| Gestión de usuarios | | | | ✓ (institución) | ✓ (global) |
| Gestión de instituciones | | | | | ✓ |
| Configuración de roles/permisos | | | | | ✓ |
| Catálogos (lectura) | | ✓ | | ✓ | ✓ |
| Catálogos (edición) | | | | | ✓ |
| Soporte (tickets) | | ✓ | ✓ | ✓ | ✓ |
