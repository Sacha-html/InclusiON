# Justificación por rol

### Persona con Discapacidad

**Función de negocio:** realizar actividades educativas interactivas y consultar su propio progreso.

| Permiso | Justificación de negocio |
|---|---|
| Acceder al portal AAC y ejecutar actividades | Es la función central del sistema para este actor. Sin este permiso no puede participar del proceso educativo. |
| Ver su propio roadmap | Necesita saber qué actividades tiene pendientes para organizarse y motivarse. |
| Sin acceso a datos de otros usuarios | No tiene relación funcional con otros usuarios. Exponer datos de otros usuarios violaría la privacidad y generaría confusión en una interfaz diseñada para accesibilidad cognitiva. |
| Sin acceso a paneles de gestión | No es responsable de ninguna tarea administrativa. Mostrar esas interfaces sería un error de diseño y un riesgo de operación accidental. |
| Métodos de login alternativos (PIN, asistido, familiar) | La persona puede tener limitaciones en escritura o autonomía. Forzar email+contraseña excluiría a parte del universo de usuarios, contradiciendo el objetivo de inclusión del sistema. |

---

### Profesional

**Función de negocio:** evaluar personas, diseñar planes de trabajo, monitorear progreso y generar reportes clínicos/educativos.

| Permiso | Justificación de negocio |
|---|---|
| Crear y editar actividades | Es el actor que produce el contenido educativo. Nadie más tiene el conocimiento clínico/pedagógico para hacerlo. |
| Ver perfil, diagnósticos y respuestas de sus personas asignadas | Necesita los datos clínicos para personalizar la intervención. Restringirlo a las personas asignadas (no a toda la institución) evita que un profesional acceda a datos de pacientes que no están bajo su cuidado — exigencia de la Ley 25.326. |
| Generar reportes | El reporte es un documento formal de evolución. Solo el profesional que interviene puede generarlo con validez clínica. |
| Invitar familiares | El profesional conoce a la familia del alumno y es responsable de habilitarles el acceso. Que sea él quien invite garantiza que solo acceden personas verificadas por el equipo profesional. |
| Sin acceso a datos de personas no asignadas | Dos profesionales de la misma institución no tienen por qué ver los datos clínicos del alumnado del otro. El vínculo de asignación activa (`ProfessionalAssignments.IsActive`) es la fuente de verdad. |
| Sin acceso a configuración de la institución | No es su responsabilidad la administración de la sede escolar. Confinar sus permisos reduce el riesgo de errores operativos. |

---

### Familia / Cuidador

**Función de negocio:** acompañar el proceso educativo de su familiar, consultar su progreso y comunicarse con el profesional.

| Permiso | Justificación de negocio |
|---|---|
| Ver progreso y reportes aprobados de su persona a cargo | El familiar necesita información del avance para participar activamente del proceso de inclusión. El acceso se limita a su persona vinculada para proteger la privacidad de otros alumnos. |
| Solo reportes en estado Aprobado (no borradores ni enviados) | Un borrador o informe en revisión puede contener información clínica preliminar que aún no fue validada por la dirección escolar. Exponer un borrador podría generar alarma innecesaria o malinterpretación. |
| Mensajería con el profesional | La comunicación es parte del proceso terapéutico. El familiar informa novedades del hogar y el profesional adapta la intervención. |
| Sin acceso a datos clínicos en detalle (diagnósticos, respuestas, perfiles de habilidades) | Esos datos son de uso clínico. El familiar recibe la síntesis cualitativa (el reporte) pero no los datos brutos que requieren interpretación profesional. |
| Registro solo por invitación | Evita que cualquier persona se registre como familiar de un alumno sin validación previa. El profesional es quien habilitó explícitamente el acceso, garantizando el consentimiento informado. |
| Respuesta 404 (no 403) al intentar acceder a recursos sin vínculo | Desde el negocio: un familiar no debe saber si existe un alumno en el sistema al que no tiene acceso. Un 403 confirmaría la existencia del recurso, lo que podría exponer información de terceros. |

---

### Admin Institucional

**Función de negocio:** operar y gobernar el sistema dentro de su institución escolar — gestionar cuentas del personal docente, asignar alumnos y aulas, auditar y aprobar informes oficiales y atender soporte.

| Permiso | Justificación de negocio |
|---|---|
| Crear y gestionar usuarios de su institución | Es el responsable legal y operativo del establecimiento. Conoce quiénes forman parte de la planta docente y fiscaliza sus títulos y matrículas. |
| Asignar profesionales a personas y aulas | Define qué profesional trabaja con qué alumno y organiza las salas escolares, una decisión organizativa central de la institución. |
| Resetear contraseñas y desactivar cuentas | Gestión de seguridad y acceso operativo. Necesario cuando un docente olvida sus credenciales o se desvincula de la institución. |
| Auditar y aprobar o rechazar reportes escolares | Control de calidad institucional. Ningún informe oficial se publica a las familias sin el visto bueno de la Dirección escolar. |
| Sin acceso a otras instituciones | Cada institución es una unidad autónoma. El claim `institutionId` en el JWT confina las operaciones estrictamente a su colegio, garantizando el aislamiento de datos. |
| Sin acceso a configuración de infraestructura | La administración de servidores y base de datos es tarea de soporte/DevOps del proveedor, no un rol de usuario dentro de la escuela. |

---

## Justificación de decisiones transversales

### Por qué el profesional no puede ver personas de su institución que no tiene asignadas
Dos profesionales de la misma institución atienden alumnos diferentes. Sus datos clínicos son independientes y confidenciales. El filtro por institución no es suficiente: la unidad de acceso correcta es la **asignación activa profesional-persona**, no la membresía institucional.  
*Fundamento legal:* Ley 25.326 Art. 8 — datos sensibles de salud requieren relación jurídica directa.

### Por qué la familia solo ve reportes aprobados
El flujo de aprobación (`Draft` → `Submitted` → `Approved`) existe para que la Dirección escolar valide el contenido pedagógico antes de que llegue a la familia. Exponer borradores saltea ese proceso y puede causar alarma o confusión con información no verificada.

### Por qué el registro familiar es solo por invitación
El auto-registro libre permitiría que cualquier persona con el email de un alumno intente vincularse. La invitación garantiza que:
1. El profesional conoce y valida al familiar previamente.
2. El token tiene TTL de 7 días y es de un solo uso.
3. El vínculo queda registrado con el `personId` correcto desde el origen.

### Por qué 404 para familia/persona y 403 para profesional/admin
Desde el negocio, un actor externo (familia, persona) no debe poder inferir la existencia de datos en el sistema que no le corresponden. Un 403 confirmaría que el recurso existe. Un 404 neutraliza esa inferencia. Para actores internos (profesional, admin) el feedback explícito es adecuado y ayuda a operar correctamente.

---

## Matriz de permisos por módulo

| Módulo | Persona | Profesional | Familia | Admin Inst. |
|---|:---:|:---:|:---:|:---:|
| Portal AAC (ejecutar actividades) | ✓ | | | |
| Crear/editar actividades | | ✓ | | |
| Roadmap propio | ✓ | | | |
| Roadmap de persona asignada | | ✓ | | |
| Diagnósticos y perfil clínico | | ✓ (asignadas) | | ✓ (consulta) |
| Reportes aprobados (propios) | | | ✓ (vinculadas) | |
| Reportes (circuito de redacción y aprobación) | | ✓ (redacta) | | ✓ (aprueba/rechaza) |
| Dashboard y monitoreo | | ✓ | ✓ (lectura) | ✓ (institucional) |
| Mensajería | | ✓ | ✓ | ✓ |
| Gestión de usuarios del colegio | | | | ✓ (institución) |
| Configuración de sede escolar | | | | ✓ (institución) |
| Catálogos escolares (lectura/activación) | | ✓ | | ✓ |
| Soporte (tickets y ayuda) | | ✓ | ✓ | ✓ |
