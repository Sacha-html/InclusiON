# Validaciones Justificadas desde Reglas de Negocio

Para cada validación: qué se verifica, la regla concreta, y la razón de dominio que la origina.

---

## 1. Perfiles de usuario

### Professional

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-01 | `DocumentNumber` | Único entre todos los `Professional` | El DNI es emitido por un organismo oficial y pertenece a una sola persona; un duplicado indica error de carga o intento de registro múltiple. |
| V-02 | `LicenseNumber` | Único si se ingresa | El número de matrícula es asignado por un colegio profesional y es único por profesional; dos registros con la misma matrícula representan a la misma persona en el sistema. |
| V-03 | `User.Email` al alta | Único en `User` | El email es el identificador de autenticación; dos cuentas con el mismo email son técnica y semánticamente inmanejables. |
| V-04 | `User.Email` modificación | Solo modificable si `Status = Pending` | El email es la cuenta de autenticación. Cambiarlo después de la activación requiere un flujo separado de cambio de credenciales (con verificación del nuevo email). Antes de la activación el profesional aún no inició sesión, por lo que el email es solo un dato de contacto. |
| V-05 | `Status` al crear `ProfessionalPerson` | `Professional.Status` debe ser `Approved` | Un profesional no validado no tiene habilitación institucional. Asignarlo a personas antes de su aprobación permitiría que alguien sin validar acceda a expedientes clínicos. |

### PersonWithDisability

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-06 | `BirthDate` | Debe ser una fecha pasada | Es la fecha de nacimiento real; un valor futuro no representa a ninguna persona existente. |
| V-07 | `DocumentNumber` | Único entre todas las `PersonWithDisability` | Mismo razonamiento que V-01. Evita duplicar el expediente de una persona. |
| V-08 | `LoginMethod` + `AutonomyLevel` | El método de autenticación debe ser coherente con el nivel de autonomía | Si `AutonomyLevel.RequiresSupervision = true`, el `LoginMethod` también debe requerir supervisor (`RequiresSupervisor = true`). Usar un método autónomo (ej. PIN propio) para alguien que requiere supervisión clínica compromete la seguridad del proceso y los datos registrados. |

### FamilyRepresentative

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-09 | `DocumentNumber` | Único si se ingresa | Mismo razonamiento que V-01; evita duplicar el perfil del familiar. |

---

## 2. Relaciones y vínculos

### ProfessionalPerson

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-10 | Profesional y persona al crear vínculo | Deben pertenecer a la misma institución | La gestión clínica está compartimentada por institución. Un profesional de institución A no tiene visibilidad ni responsabilidad institucional sobre personas de institución B. El cruce de instituciones requeriría acuerdo interinstitucional explícito. |
| V-11 | `ProfessionalPerson` | No puede existir asignación activa duplicada para el mismo par | Dos asignaciones activas del mismo profesional con la misma persona crearían ambigüedad sobre quién es responsable en cada operación. |

### PersonRepresentative

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-12 | `IsPrimary` | Solo un representante primario activo por persona | El representante primario es el punto de contacto principal de la familia. Tener dos primarios crea ambigüedad en notificaciones automáticas y comunicaciones urgentes. |
| V-13 | `ConsentDate` | Obligatoria si `HasInformedConsent = true` | El consentimiento informado es un acto jurídico con fecha; sin fecha, no hay trazabilidad legal de cuándo se obtuvo el consentimiento. |

### Message

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-14 | `SenderId` / `ReceiverId` | Solo se permiten mensajes entre Profesional y Representante Familiar | El canal existe para coordinar el trabajo sobre la persona atendida. Mensajes prof→prof (tienen otros canales institucionales) o familiar→familiar no tienen base clínica dentro del sistema. |
| V-15 | `SenderId` / `ReceiverId` | Remitente y destinatario deben compartir al menos una persona con discapacidad con vínculos activos en ambos lados | Sin una persona en común no existe contexto clínico que justifique la comunicación. Previene contacto entre desconocidos que coinciden en el sistema. |
| V-16 | `SenderId` = `ReceiverId` | Un usuario no puede enviarse mensajes a sí mismo | No existe semánticamente un "mensaje a uno mismo" en el contexto de coordinación clínico-familiar. |
| V-17 | Mensaje una vez enviado | No es editable | Los mensajes forman parte del expediente de comunicación entre equipo clínico y familia. Editarlos post-envío comprometería la integridad del registro y podría alterar el historial de decisiones documentadas. |
| V-18 | `ParentMessageId` | Si se especifica, el destinatario del hilo debe coincidir con el participante de la conversación original | Un hilo une a dos interlocutores específicos. Responder a un mensaje con un destinatario diferente rompería la coherencia del hilo. |

---

## 3. Actividades y contenido

### Activity

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-19 | `ActivityContent.TemplateTypeId` | Inmutable si la actividad fue asignada al menos una vez | Los `ActivityResponse` almacenan datos estructurados según el template. Cambiar la estructura haría incomparables los resultados pasados y presentes, y potencialmente corrompería la lectura del historial clínico. |
| V-20 | `IsStandardActivity` | Las actividades estándar solo pueden ser dadas de baja por el Admin Global | Las actividades estándar son contenido de plataforma compartido entre todas las instituciones. Un profesional individual no puede afectar el catálogo global que usan otros profesionales de otras instituciones. |
| V-21 | Baja de `Activity` | No permitida si hay `ActivityAssignment` con `Status = Pending` o `InProgress` | La persona tiene la actividad en su agenda activa. Desactivarla mid-proceso rompe su sesión y deja asignaciones vivas apuntando a contenido inactivo. |

### ActivityAssignment

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-22 | `DueDate` | Debe ser futura si se ingresa | Una fecha límite ya vencida haría que la asignación nazca expirada, sin utilidad pedagógica. El profesional no puede asignar retroactivamente. |
| V-23 | Par `ActivityId` + `PersonId` | No puede existir una asignación activa duplicada para el mismo par | Dos asignaciones activas de la misma actividad para la misma persona crean ambigüedad sobre a qué asignación pertenece cada `ActivityResponse`. |

---

## 4. Roadmap y aprendizaje

### PersonRoadmap

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-24 | `PersonRoadmap` activo por persona | Solo puede existir uno | El roadmap es el plan de trabajo vigente de la persona. Tener dos activos simultáneos crea ambigüedad sobre cuál es el plan pedagógico actual y fragmenta el historial de progreso. |

### PersonRoadmapArea

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-25 | Baja de `PersonRoadmapArea` | Solo si todas sus actividades fueron dadas de baja primero | El área agrupa actividades con progreso registrado. Eliminarla sin limpiar las actividades dejaría `PersonRoadmapActivity` huérfanas sin área de pertenencia. |
| V-26 | `SkillAreaId` en roadmap | No puede repetirse en el mismo `PersonRoadmap` | Un roadmap no puede tener dos secciones para la misma área de habilidad; el progreso y el radar chart del área serían indeterminados. |

### PersonRoadmapActivity

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-27 | Baja de `PersonRoadmapActivity` | No permitida si es la única actividad desbloqueada del área | Eliminarla dejaría a la persona sin ninguna actividad disponible en esa área, bloqueando completamente su avance. El roadmap perdería su función pedagógica en ese eje. |
| V-28 | `UnlockThresholdPercent` | Entre 0 y 100 | Es un porcentaje; valores fuera del rango no tienen interpretación posible. Un umbral de 0 desbloquea automáticamente. |
| V-29 | `ActivityId` en área | No puede repetirse en la misma `PersonRoadmapArea` | Agregar la misma actividad dos veces en el mismo área crea secuencias duplicadas y haría el progreso ambiguo. |

### AdaptiveEngineConfig

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-30 | `MinDifficultyLevel` vs `MaxDifficultyLevel` | `Min` debe ser estrictamente menor que `Max` | El motor adaptativo necesita espacio para subir o bajar dificultad. Si `Min = Max`, el motor detecta umbrales pero no puede ajustar nada: registra logs vacíos indefinidamente sin efecto real. |
| V-31 | `ConsecutiveSuccessToUpgrade` | Debe ser ≥ 1 | Se necesita al menos un éxito confirmado para decidir subir dificultad. Un valor de 0 provocaría subidas automáticas sin ningún resultado que las respalde. |
| V-32 | `ConsecutiveFailuresToDowngrade` | Debe ser ≥ 1 | Mismo razonamiento inverso: un solo fallo podría no reflejar el patrón real de la persona. |
| V-33 | `SuccessThresholdPercent` | Entre 0 y 100 | Es un porcentaje; representa el piso de score para considerar un intento exitoso. |
| V-34 | `FrustrationThreshold` | Entre 1 y 5 | Debe coincidir con la escala de `FrustrationLevel` usada en `ActivityResponse` (1–5). Un umbral fuera de escala nunca se alcanzaría o se activaría siempre. |

---

## 5. Diagnósticos

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-35 | `DiagnosisDate` | No puede ser futura | Un diagnóstico clínico es el resultado de una evaluación ya realizada; registrar una fecha futura implicaría un diagnóstico que aún no ocurrió, lo cual carece de validez clínica. |
| V-36 | Baja del diagnóstico más reciente | No permitida si existe un `PersonRoadmap` activo | El roadmap puede estar construido sobre las conclusiones de ese diagnóstico. Eliminarlo dejaría el plan de trabajo sin su fundamento clínico-pedagógico documentado. |
| V-37 | Baja o modificación de `Diagnosis` | Solo por el profesional autor (`ProfessionalId`) | El diagnóstico es un registro clínico firmado por un profesional específico; modificarlo implica responsabilidad profesional directa. Otro profesional no puede alterar el criterio diagnóstico de un colega. |

---

## 6. Reportes

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-38 | `AdminComment` en rechazo | Obligatorio al transicionar `Submitted → Rejected` | El profesional necesita saber el motivo concreto para poder producir una revisión adecuada. Un rechazo sin comentario no es accionable y bloquea el flujo de aprobación sin información útil. |
| V-39 | Envío y edición del `Report` | Solo por el profesional autor | El reporte es un documento clínico emitido bajo la responsabilidad del profesional que atiende a la persona. Otro profesional no puede firmar en nombre de un colega ni alterar su evaluación. |

---

## 7. Invitaciones

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-40 | Invitación activa por `Email + ForPersonId` | Solo puede existir una `Pending` a la vez | Si existen múltiples códigos válidos para el mismo destino, el familiar podría registrarse varias veces o compartir el enlace. La invitación única garantiza un único punto de entrada controlado por el profesional. |
| V-41 | TTL de la invitación | 7 días desde la creación | Suficiente para que el familiar actúe. Un plazo mayor permite que se reutilice un link semanas después, cuando el contexto puede haber cambiado: la relación familiar puede haber terminado, la persona puede estar dada de baja, o el profesional puede haber sido reemplazado. |

---

## 8. Sesión y autenticación

| # | Campo / Condición | Regla | Justificación de negocio |
|---|-------------------|-------|--------------------------|
| V-42 | `RefreshToken` al desactivar `User` | Todos los tokens activos deben revocarse | Un usuario dado de baja o suspendido no debe poder continuar sesiones existentes. La revocación forzada garantiza el cierre inmediato de todos los canales de acceso activos al momento de la desactivación. |
