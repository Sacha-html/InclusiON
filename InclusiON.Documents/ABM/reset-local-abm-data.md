# Preparación de limpieza local para probar los ABM

## Resultado esperado

`InclusiON.Server/Scripts/reset_local_abm_data.sql` prepara la base PostgreSQL local de Docker (`inclusion_dev`) para dejar únicamente el usuario Admin y vaciar los catálogos ABM. El inventario de este documento es una **guía de recarga manual**, no una lista de datos que se conservarán.

**No es una operación para producción.** El script no fue ejecutado durante este trabajo y no se modificaron datos, migraciones ni endpoints. Se modificó `DatabaseSeeder.cs` para desactivar la precarga. Desde este cambio, `SkillAreas` y `ActivityTemplateTypes` no se recrean automáticamente ni por `DatabaseSeeder` ni por `RoadmapInitializer`: quedan bajo carga manual desde el ABM.

## Uso seguro

1. Detener la aplicación y confirmar que se trabaja sobre el PostgreSQL local de Docker.
2. Generar un backup antes de aplicar cualquier cambio.
3. Revisar estáticamente el SQL, sus tablas, el orden de borrado y sus validaciones.
4. Mantener `ROLLBACK` para una prueba controlada. Para aplicar la limpieza, cambiar explícitamente la última instrucción por `COMMIT`, revisar nuevamente el diff y ejecutar solo contra `inclusion_dev`.
5. Verificar que queda el Admin, que los roles/claims siguen presentes y que los catálogos definidos como vaciables están vacíos.

El SQL contiene guardia por `current_database()`, transacción, validación de estructura, validación del Admin y validaciones posteriores. No toca estructuras ni `__EFMigrationsHistory`.

## Alcance de limpieza

### Se conserva

- El usuario Admin con ID `00000000-0000-0000-0000-000000000001`.
- Roles y claims de Identity (`AspNetRoles` y `AspNetRoleClaims`). Los roles del sistema **no son un ABM** y se conservan.
- `LoginMethods` (3 registros): no existe ABM de alta/baja para este catálogo y es necesario para el login y el modelo de personas.
- Tablas técnicas de soporte, como `ActivityAssignmentStatuses`, `BackgroundJobStatuses` y `JobTypes`; se conservan sus definiciones y datos necesarios para el arranque, pero se borran los registros operativos de `BackgroundJobs`.
- Estructuras y migraciones.

### Se limpia

Se eliminan todos los usuarios no Admin y sus dependencias: perfiles, aulas, relaciones profesional-alumno, instituciones asociadas, reportes, diagnósticos, mensajes, auditorías, invitaciones, sesiones, resultados, respuestas, roadmaps, historiales, tokens, dispositivos, trabajos en segundo plano y relaciones de Identity.

También se eliminan **todas** las actividades, contenidos y embeddings. Así no quedan excepciones por actividades estándar y los catálogos de actividades pueden quedar realmente vacíos.

Los catálogos ABM vaciables son: `Specialties`, `DisabilityTypes`, `AutonomyLevels`, `ActivityCategories`, `SkillAreas`, `ActivityTemplateTypes`, `ReportTypes` y `EducationalInstitutions`.

`Diagnoses` no es un catálogo ABM: el inventario observado estaba vacío y sus registros transaccionales se eliminan junto con los perfiles.

## Inventario para recarga manual

Estos conteos fueron relevados antes de la limpieza y sirven para saber qué cargar desde la interfaz. No representan datos que el script conserve.

### Especialidades — 13

| Nombre | Descripción/configuración |
|---|---|
| Educación Especial | Activa |
| Psicología | Activa |
| Psicopedagogía | Activa |
| Fonoaudiología | Activa |
| Terapia Ocupacional | Activa |
| Kinesiología | Activa |
| Trabajo Social | Activa |
| Musicoterapia | Activa |
| Psicomotricidad | Activa |
| Acompañamiento Terapéutico | Activa |
| Docente de Apoyo a la Inclusión (DAI) | Activa |
| Neurología | Activa |
| Pediatría | Activa |

### Tipos de discapacidad — 5

| Nombre | Descripción |
|---|---|
| Sensorial | Incluye discapacidad auditiva y visual (sordera, ceguera) |
| Cognitiva/Intelectual | Relacionada con el aprendizaje y habilidades adaptativas de la vida diaria |
| Motriz | Alteraciones en el funcionamiento motor |
| Mental/Psicosocial | Derivada de diagnósticos vinculados a la salud mental |
| Múltiple | Combinación de dos o más tipos de discapacidad |

### Niveles de autonomía — 3

| Nombre | Descripción | Configuración |
|---|---|---|
| Alta | Puede usar la aplicación de forma independiente con login estándar | No requiere supervisión; orden 1 |
| Media | Requiere login simplificado (PIN o pictogramas), pero puede usar la aplicación solo | No requiere supervisión; orden 2 |
| Baja | Requiere supervisión y login asistido por familiar o profesional | Requiere supervisión; orden 3 |

### Categorías de actividades — 8

| Nombre | Descripción |
|---|---|
| Lectoescritura | Actividades de lectura, escritura, conciencia fonológica y comprensión lectora |
| Numeración y Matemática | Actividades prenuméricas, numeración, secuencias y operaciones básicas |
| Habilidades Socioemocionales | Modificación de conducta, hábitos, rutinas, normas de convivencia e historias sociales |
| Comunicación y Lenguaje | Lengua de señas, sistemas aumentativos y alternativos de comunicación (SAAC) |
| Motricidad y Coordinación | Motricidad fina, motricidad gruesa, coordinación óculo-manual y orientación espacial |
| Creatividad y Expresión Artística | Música, plástica y dramatización |
| Autonomía y Vida Diaria | Vestimenta, higiene, manejo del dinero, noción del tiempo y situaciones cotidianas |
| Estimulación Cognitiva | Memoria, atención, percepción y resolución de problemas |

### Áreas de habilidades — 4 (inventario para recarga manual)

| Nombre | Descripción | Configuración |
|---|---|---|
| Comunicación | Actividades orientadas al desarrollo de habilidades comunicativas mediante pictogramas, selección de opciones y expresión. | Ícono `chat`, color `#2E5FA3`, orden 1 |
| Alfabetización | Actividades de lectura global, reconocimiento de sonidos y construcción de palabras para el desarrollo de la lectoescritura. | Ícono `menu_book`, color `#4CAF50`, orden 2 |
| Lógico-matemático | Actividades de clasificación, ordenamiento y numeración para desarrollar el pensamiento lógico-matemático. | Ícono `calculate`, color `#FF9800`, orden 3 |
| Trayectoria | Camino de aprendizaje estándar anti-frustración. | Ícono `map`, color `#673AB7`, orden 4 |

### Tipos de plantilla — 8

| Nombre | Código | Descripción | Pictogramas / audio |
|---|---|---|---|
| Seleccionar pictograma | `PICTOGRAM_SELECT` | Seleccionar el pictograma correcto entre varias opciones. | Sí / Sí |
| Selección de opciones | `OPTION_SELECT` | Elegir la respuesta correcta entre opciones de texto o imagen. | No / Sí |
| Lectura global | `GLOBAL_READING` | Asociar una palabra completa con su imagen o significado. | No / Sí |
| Reconocer sonidos | `SOUND_RECOGNITION` | Identificar el sonido de una letra o sílaba y asociarlo con su representación. | No / Sí |
| Armar palabras | `BUILD_WORD` | Construir una palabra ordenando letras o sílabas. | No / No |
| Clasificación | `CLASSIFY` | Agrupar elementos según color, forma o categoría. | No / No |
| Ordenamiento | `ORDER_SEQUENCE` | Ordenar elementos en una secuencia lógica o cronológica. | No / No |
| Numeración | `NUMERATION` | Practicar conteo, reconocimiento de números y asociación cantidad-número. | No / No |

Todos tenían `IsActive = true`, `ContentSchema = ""` y `ComponentName = ""`; el orden era 1–2 para Comunicación, 1–3 para Alfabetización y 1–3 para Lógico-matemático.

### Métodos de login — 3 (excepción técnica, no se borran)

| Código | Nombre | Descripción | Configuración relevante |
|---|---|---|---|
| `STANDARD` | Email y contraseña | Login visual con nombre de usuario y contraseña | Autonomía mínima 1; password requerido; orden 1 |
| `PIN` | PIN numérico | Login con nombre de usuario y PIN de 4 dígitos | Autonomía mínima 1; PIN requerido; orden 2 |
| `ASSISTED` | Login asistido | Login asistido donde un familiar o profesional autoriza el acceso | Autonomía mínima 3; supervisor requerido; orden 3 |

### Tipos de reporte — 5

| Nombre | Descripción |
|---|---|
| Evaluación Inicial | Informe de estandarización y diagnóstico inicial del estudiante |
| Seguimiento Mensual | Informe de progreso mensual |
| Informe Trimestral | Evaluación de progreso trimestral |
| PPI | Proyecto Pedagógico Individual para la inclusión |
| Informe Anual | Resumen anual de logros alcanzados y áreas a reforzar |

### Instituciones educativas y diagnósticos

- `EducationalInstitutions`: 0 registros relevados; no hay inventario que recargar.
- `Diagnoses`: 0 registros relevados; no es un catálogo y se elimina como dato transaccional.

## Criterios de aceptación

- [ ] La guardia aborta si la base no se llama `inclusion_dev`.
- [ ] La limpieza es transaccional y queda en `ROLLBACK` por defecto.
- [ ] Queda exactamente el Admin; no quedan otros usuarios, perfiles, aulas, asignaciones ni reportes.
- [ ] No quedan actividades, contenidos, embeddings, roadmaps ni configuraciones adaptativas.
- [ ] Los ocho catálogos ABM vaciables quedan en cero.
- [ ] Se conservan `AspNetRoles` y `AspNetRoleClaims`, sin borrar roles ni claims de roles.
- [ ] No se modifican migraciones ni endpoints.
- [ ] `DatabaseSeeder.cs` deja desactivada la precarga de `SkillAreas` y `ActivityTemplateTypes`.
- [ ] `RoadmapInitializer` no crea automáticamente el área `Trayectoria` ni tipos de plantilla.

## Riesgos y límites

- Un backup incorrecto o un `COMMIT` sobre otra base puede causar pérdida de datos; la guardia de nombre no reemplaza verificar servidor y contenedor.
- `LoginMethods` no se vacía porque no tiene ABM de alta/baja identificado y es necesario para login/personas. Si el backend cambiara esa dependencia, esta excepción debe revisarse antes de modificar el script.
- `SkillAreas` y `ActivityTemplateTypes` quedan bajo carga manual desde el ABM. Los roadmaps requieren que las áreas y los tipos de plantilla necesarios estén cargados previamente; si falta `Trayectoria`, `RoadmapInitializer` no crea el área ni puede generar las actividades estándar.
- La revisión fue estática contra `AppDbContextModelSnapshot`, configuraciones y seeder. No se ejecutó SQL, no se borraron datos y no se validó runtime por instrucción explícita.
