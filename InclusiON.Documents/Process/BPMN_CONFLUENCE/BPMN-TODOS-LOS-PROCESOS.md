# Catálogo de Procesos Principales — InclusiON (Formato Confluence)

> **Instrucciones para Confluence:** Copia y pega directamente cada bloque en Confluence utilizando la macro **Mermaid** o el bloque de código Markdown. Todos los diagramas representan el **flujo del proceso principal** (camino exitoso y decisiones clave), con conexiones rectas y en lenguaje claro para clientes, directivos y evaluadores.

---

## Proceso 1 : Gestión de Administradores Institucionales

**Descripción:** Circuito para el alta, asignación de permisos y habilitación del Administrador Institucional (Equipo Directivo) encargado de coordinar docentes, alumnos y familias en la escuela.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S01((●)) --> T01_INPUT["<b>1. Cargar Datos del Administrador</b><br/>Ingresar nombre, email institucional y cargo directivo"]
    T01_INPUT --> T01_VAL["<b>2. Validar Unicidad de Correo</b><br/>Servidor comprueba que el email esté disponible"]
    T01_VAL --> GW01_VAL{"<b>¿Email Libre?</b>"}
    GW01_VAL -->|Sí| T01_CREATE["<b>3. Crear Cuenta Directiva</b><br/>Asignar rol de Administrador Institucional"]
    GW01_VAL -->|No: Ya existe| T01_ERR["<b>Aviso de Duplicado</b><br/>El correo ya pertenece a otro usuario"]
    T01_CREATE --> T01_CREDS["<b>4. Emitir Credenciales Iniciales</b><br/>Generar clave temporal de primer acceso"]
    T01_CREDS --> T01_READY["<b>5. Administrador Habilitado</b><br/>Director accede a gestionar docentes, alumnos y tutores"]
    T01_READY --> E01_OK(((●)))
    T01_ERR --> E01_FAIL(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S01 inicio; class E01_OK,E01_FAIL fin;
    class T01_INPUT accion;
    class T01_VAL,T01_CREATE,T01_CREDS,T01_READY,T01_ERR sistema;
    class GW01_VAL decision;
```

---

## Proceso 2 : Autenticación y Acceso Adaptativo Multi-Método

**Descripción:** Acceso seguro adaptado a la diversidad funcional del usuario (docente por credenciales, estudiante PCD por PIN táctil de 4 dígitos o validación asistida).

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S02((●)) --> GW02_ROL{"<b>¿Tipo de Usuario?</b>"}
    GW02_ROL -->|Docente / Familiar| T02_PASS["<b>Ingreso con Contraseña</b><br/>Email y contraseña personal"]
    GW02_ROL -->|Estudiante PCD| T02_ADAPT["<b>Ingreso Adaptado</b><br/>PIN táctil de 4 dígitos o validación asistida"]
    
    T02_PASS --> T02_VAL["<b>Validar Credenciales</b><br/>Servidor comprueba identidad y permisos"]
    T02_ADAPT --> T02_VAL

    T02_VAL --> GW02_OK{"<b>¿Acceso Válido?</b>"}
    GW02_OK -->|Sí| T02_GRANT["<b>Iniciar Sesión Segura</b><br/>Cargar perfil y ajustes de accesibilidad"]
    GW02_OK -->|No| T02_DENY["<b>Acceso Denegado</b><br/>Aviso sonoro y visual amigable"]
    
    T02_GRANT --> E02_OK(((●)))
    T02_DENY --> E02_FAIL(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S02 inicio; class E02_OK,E02_FAIL fin;
    class T02_PASS,T02_ADAPT accion;
    class T02_VAL,T02_GRANT,T02_DEN sistema;
    class GW02_ROL,GW02_OK decision;
```

---

## Proceso 2b : Gestión de Roles y Permisos Institucionales

**Descripción:** Asignación y control de permisos por sede para garantizar que cada profesional y directivo acceda únicamente a sus alumnos correspondientes.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S02B((●)) --> T02B_SELECT["<b>1. Seleccionar Usuario en Sede</b><br/>Dirección elige al profesional a configurar"]
    T02B_SELECT --> T02B_ROLE["<b>2. Asignar Perfil o Módulos</b><br/>Definir permisos pedagógicos y terapéuticos"]
    T02B_ROLE --> T02B_VAL["<b>3. Validar Alcance Institucional</b><br/>Servidor restringe visibilidad a su escuela"]
    T02B_VAL --> T02B_SAVE["<b>4. Activar Permisos</b><br/>Actualizar credenciales operativas del usuario"]
    T02B_SAVE --> E02B_OK(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    class S02B inicio; class E02B_OK fin;
    class T02B_SELECT,T02B_ROLE accion;
    class T02B_VAL,T02B_SAVE sistema;
```

---

## Proceso 3 : Gestión de Catálogos del Sistema

**Descripción:** Circuito para consultar y enriquecer los catálogos maestros de apoyo (tipos de discapacidad, áreas de habilidad y adaptaciones).

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S03((●)) --> T03_REQ["<b>1. Cargar Nuevo Elemento</b><br/>Definir nombre y descripción del ítem pedagógico"]
    T03_REQ --> T03_VAL["<b>2. Validar Unicidad</b><br/>Servidor comprueba que no exista repetido"]
    T03_VAL --> GW03_UNI{"<b>¿Nombre Libre?</b>"}
    GW03_UNI -->|Sí| T03_SAVE["<b>3. Guardar en Catálogo Global</b><br/>Habilitar ítem en listas desplegables"]
    GW03_UNI -->|No| T03_ERR["<b>Aviso de Duplicado</b><br/>Indicar que el registro ya existe"]
    T03_SAVE --> E03_OK(((●)))
    T03_ERR --> E03_FAIL(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S03 inicio; class E03_OK,E03_FAIL fin;
    class T03_REQ accion;
    class T03_VAL,T03_SAVE,T03_ERR sistema;
    class GW03_UNI decision;
```

---

## Proceso 4 : Gestión de Personas con Discapacidad (Estudiantes)

**Descripción:** Alta pedagógica del estudiante, asignación del nivel de autonomía, selección del método de login adaptado e inicialización de su ruta de aprendizaje.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S04((●)) --> T04_INPUT["<b>1. Cargar Datos del Alumno</b><br/>Nombre, fecha de nacimiento y tutor legal"]
    T04_INPUT --> T04_AUTO["<b>2. Definir Nivel de Autonomía</b><br/>Elegir método de login adaptado (PIN o Asistido)"]
    T04_AUTO --> T04_VAL["<b>3. Validar Coherencia Pedagógica</b><br/>Servidor comprueba perfil y método asignado"]
    T04_VAL --> T04_SAVE["<b>4. Crear Expediente Cifrado (AES-256)</b><br/>Proteger información médica y personal"]
    T04_SAVE --> T04_ROADMAP["<b>5. Inicializar 'Mi Camino'</b><br/>Crear mapa de actividades para el estudiante"]
    T04_ROADMAP --> E04_OK(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    class S04 inicio; class E04_OK fin;
    class T04_INPUT,T04_AUTO accion;
    class T04_VAL,T04_SAVE,T04_ROADMAP sistema;
```

---

## Proceso 5 : Gestión de Profesionales (Aprovisionamiento Institucional)

**Descripción:** Incorporación del equipo docente y terapéutico. La creación es centralizada por la Dirección (sin auto-registro público), verificando matrícula y emitiendo credenciales institucionales.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S05((●)) --> T05_INPUT["<b>1. Cargar Datos del Profesional</b><br/>Dirección ingresa nombre, email y matrícula oficial"]
    T05_INPUT --> T05_VAL["<b>2. Validar Unicidad de Matrícula</b><br/>Servidor verifica que la matrícula esté libre"]
    T05_VAL --> GW05_VAL{"<b>¿Matrícula Válida?</b>"}
    GW05_VAL -->|Sí| T05_CREATE["<b>3. Crear Cuenta Habilitada</b><br/>Generar clave temporal de primer acceso"]
    GW05_VAL -->|No: Ya existe| T05_ERR["<b>Aviso de Duplicidad</b><br/>Matrícula ya registrada previamente"]
    T05_CREATE --> T05_LINK["<b>4. Asignar a la Sede Escolar</b><br/>Profesional listo para recibir alumnos a cargo"]
    T05_LINK --> E05_OK(((●)))
    T05_ERR --> E05_FAIL(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S05 inicio; class E05_OK,E05_FAIL fin;
    class T05_INPUT accion;
    class T05_VAL,T05_CREATE,T05_LINK,T05_ERR sistema;
    class GW05_VAL decision;
```

---

## Proceso 6 : Gestión de Familiares y Tutores Legales

**Descripción:** Vinculación oficial de los tutores legales al expediente escolar del alumno para darles acceso exclusivo al seguimiento pedagógico.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S06((●)) --> T06_ALU["<b>1. Seleccionar Estudiante</b><br/>Institución elige el alumno matriculado"]
    T06_ALU --> T06_FAM["<b>2. Cargar Datos del Tutor Legal</b><br/>Ingresar parentesco, nombre y correo del tutor"]
    T06_FAM --> T06_VAL["<b>3. Comprobar Cuenta en Servidor</b><br/>Verificar si el tutor ya tiene usuario activo"]
    T06_VAL --> GW06_ACC{"<b>¿Tiene Cuenta?</b>"}
    GW06_ACC -->|Sí| T06_LINK["<b>Vincular a Cuenta Existente</b><br/>Asociar nuevo hijo a su perfil actual"]
    GW06_ACC -->|No| T06_INV["<b>Disparar Invitación por Email</b><br/>Enviar enlace seguro de activación (7 días)"]
    T06_LINK --> E06_OK(((●)))
    T06_INV --> E06_INV(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S06 inicio; class E06_OK,E06_INV fin;
    class T06_ALU,T06_FAM accion;
    class T06_VAL,T06_LINK,T06_INV sistema;
    class GW06_ACC decision;
```

---

## Proceso 7 : Gestión de Invitaciones por Correo Electrónico

**Descripción:** Activación digital autónoma del familiar a través de un enlace seguro con vigencia de 7 días naturales.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S07((●)) --> T07_SEND["<b>1. Enviar Invitación por Email</b><br/>Servidor despacha enlace con token seguro"]
    T07_SEND --> T07_OPEN["<b>2. Familiar Abre el Correo</b><br/>Acceder a pantalla de registro familiar"]
    T07_OPEN --> T07_VAL["<b>3. Validar Vigencia del Enlace</b><br/>Servidor comprueba plazo de 7 días"]
    T07_VAL --> GW07_TIME{"<b>¿Enlace Vigente?</b>"}
    GW07_TIME -->|Sí| T07_PASS["<b>4. Definir Contraseña Personal</b><br/>Familiar ingresa su clave privada"]
    GW07_TIME -->|No: Expirado| T07_RETRY["<b>Solicitar Reenvío a la Escuela</b><br/>Generar nuevo enlace seguro"]
    T07_PASS --> T07_ACT["<b>5. Activar Cuenta y Vincular Alumno</b><br/>Habilitar acceso al Portal Familiar"]
    T07_ACT --> E07_OK(((●)))
    T07_RETRY --> E07_EXP(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S07 inicio; class E07_OK,E07_EXP fin;
    class T07_OPEN,T07_PASS accion;
    class T07_SEND,T07_VAL,T07_ACT,T07_RETRY sistema;
    class GW07_TIME decision;
```

---

## Proceso 8 : Asignación Multidisciplinaria de Profesionales a Estudiantes

**Descripción:** Conformación del equipo de apoyo terapéutico y pedagógico para cada alumno dentro de la misma institución.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S08((●)) --> T08_ALU["<b>1. Seleccionar Alumno</b><br/>Dirección identifica al estudiante"]
    T08_ALU --> T08_TEAM["<b>2. Seleccionar Profesionales</b><br/>Elegir fonoaudiólogo, psicopedagogo y docentes"]
    T08_TEAM --> T08_VAL["<b>3. Validar Pertenencia a la Sede</b><br/>Servidor comprueba que compartan institución"]
    T08_VAL --> T08_SAVE["<b>4. Habilitar Acceso al Expediente</b><br/>Otorgar permisos de lectura y prescripción"]
    T08_SAVE --> T08_AULA["<b>5. Incorporar en 'Mi Aula'</b><br/>El alumno aparece en la pantalla de cada profesional"]
    T08_AULA --> E08_OK(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    class S08 inicio; class E08_OK fin;
    class T08_ALU,T08_TEAM accion;
    class T08_VAL,T08_SAVE,T08_AULA sistema;
```

---

## Proceso 9 : Gestión y Curación de Actividades del Catálogo (ARASAAC)

**Descripción:** Creación y curación de actividades lúdicas con integración libre y directa del banco internacional de pictogramas ARASAAC.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S09((●)) --> T09_NEW["<b>1. Crear Nueva Actividad</b><br/>Docente define título, área y consigna"]
    T09_NEW --> T09_SEARCH["<b>2. Buscar Pictogramas ARASAAC</b><br/>Búsqueda de términos visuales en catálogo abierto"]
    T09_SEARCH --> T09_API["<b>3. Descargar Pictogramas Oficiales</b><br/>Conexión online y retorno de imágenes"]
    T09_API --> T09_CONFIG["<b>4. Configurar Dinámica Interactiva</b><br/>Definir opciones correctas y distractores"]
    T09_CONFIG --> T09_SAVE["<b>5. Guardar en Catálogo Institucional</b><br/>Disponible para prescribir en el aula"]
    T09_SAVE --> E09_OK(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    class S09 inicio; class E09_OK fin;
    class T09_NEW,T09_SEARCH,T09_CONFIG accion;
    class T09_API,T09_SAVE sistema;
```

---

## Proceso 9b : Evaluación Diagnóstica Inicial y Perfil Funcional

**Descripción:** Evaluación integral de capacidades iniciales y cálculo automatizado del perfil de actividades recomendadas.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S09B((●)) --> T09B_AREAS["<b>1. Evaluar Áreas de Habilidad</b><br/>Comunicación, lenguaje, cognición y motricidad"]
    T09B_AREAS --> T09B_AUTO["<b>2. Ponderar Nivel de Autonomía</b><br/>Registrar necesidades específicas de apoyo"]
    T09B_AUTO --> T09B_DIAG["<b>3. Cargar Diagnóstico Interdisciplinario</b><br/>Completar observaciones clínicas y metas"]
    T09B_DIAG --> T09B_CIPHER["<b>4. Cifrar Datos Médicos (AES-256)</b><br/>Almacenamiento protegido en el servidor"]
    T09B_CIPHER --> T09B_RECOM["<b>5. Generar Recomendaciones Iniciales</b><br/>Sugerir actividades del catálogo acordes al perfil"]
    T09B_RECOM --> E09B_OK(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    class S09B inicio; class E09B_OK fin;
    class T09B_AREAS,T09B_AUTO,T09B_DIAG accion;
    class T09B_CIPHER,T09B_RECOM sistema;
```

---

## Proceso 10 : PROCESO CORE — Ejecución de Actividades Terapéuticas

**Descripción:** Flujo central del producto. El docente prescribe la actividad, el alumno la resuelve en su tablet adaptada, el sistema evalúa el desempeño (**aprobación $\ge 60\%$**) y desbloquea el siguiente nivel con medalla para celebrar en familia.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S10((●)) --> T10_PRESC["<b>1. Prescribir Actividad</b><br/>Docente asigna ejercicio terapéutico al alumno"]
    T10_PRESC --> T10_OPEN["<b>2. Abrir 'Mi Camino'</b><br/>Estudiante selecciona el nodo en su tablet"]
    T10_OPEN --> T10_PLAY["<b>3. Resolver Consignas Lúdicas</b><br/>Interacción táctil, auditiva y con pictogramas"]
    T10_PLAY --> T10_SUBMIT["<b>4. Enviar Respuestas</b><br/>Pulsar finalizar y transmitir resultados"]
    T10_SUBMIT --> T10_EVAL["<b>5. Evaluar Aciertos en Servidor</b><br/>Calcular porcentaje de éxito obtenido"]
    
    T10_EVAL --> GW10_SCORE{"<b>¿Puntaje $\ge 60\%$?</b>"}
    GW10_SCORE -->|Sí: Aprobado| T10_WIN["<b>Nivel Completado con Medalla</b><br/>Desbloquear siguiente nivel del camino"]
    GW10_SCORE -->|No: En Proceso| T10_RETRY["<b>Habilitar Reintento Amigable</b><br/>Permitir nueva práctica sin penalizaciones"]
    
    T10_WIN --> T10_NOTIF["<b>Notificar Logro en Radar y Familia</b><br/>Docente y tutor observan el avance en vivo"]
    T10_NOTIF --> E10_OK(((●)))
    T10_RETRY --> E10_RETRY(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef alumno fill:#E6FFFA,stroke:#234E52,stroke-width:2px,color:#134E4A;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S10 inicio; class E10_OK,E10_RETRY fin;
    class T10_PRESC accion;
    class T10_OPEN,T10_PLAY,T10_SUBMIT alumno;
    class T10_EVAL,T10_WIN,T10_RETRY,T10_NOTIF sistema;
    class GW10_SCORE decision;
```

---

## Proceso 11 : Roadmap y Motor de Dificultad Adaptativa (MDA)

**Descripción:** Calibración automática de dificultad. Si el estudiante se traba y acumula **4 fallos consecutivos**, la actividad se pausa de forma preventiva y se envía una alerta sonora y visual inmediata a la pantalla del docente.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S11((●)) --> T11_TELE["<b>1. Recibir Telemetría de la Actividad</b><br/>Analizar tiempo, aciertos y reintentos"]
    T11_TELE --> GW11_PERF{"<b>¿Rendimiento del Alumno?</b>"}
    
    GW11_PERF -->|Éxito Sostenido $\ge 80\%$| T11_UP["<b>Aumentar Complejidad</b><br/>Nivel +1 · Reducir pistas visuales"]
    GW11_PERF -->|Ritmo Estable 60-79%| T11_SAME["<b>Mantener Dificultad</b><br/>Consolidar los aprendizajes"]
    GW11_PERF -->|Dificultad Persistente| GW11_FAIL{"<b>¿Alcanzó 4 fallos seguidos?</b>"}
    
    GW11_FAIL -->|1 a 3 fallos| T11_HELP["<b>Otorgar Pistas y Más Tiempo</b><br/>Reforzar apoyos visuales"]
    GW11_FAIL -->|4to fallo| T11_LOCK["<b>Pausar Actividad Preventivamente</b><br/>Bloquear candado para evitar frustración"]
    
    T11_LOCK --> T11_SIG["<b>Disparar Alerta SignalR en Vivo</b><br/>Notificación urgente a pantalla docente"]
    T11_SIG --> T11_DOC["<b>Acompañamiento Humano en Aula</b><br/>Docente acude a brindar contención"]
    
    T11_UP --> E11_OK(((●)))
    T11_SAME --> E11_OK
    T11_HELP --> E11_OK
    T11_DOC --> E11_ALERT(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S11 inicio; class E11_OK,E11_ALERT fin;
    class T11_DOC accion;
    class T11_TELE,T11_UP,T11_SAME,T11_HELP,T11_LOCK,T11_SIG sistema;
    class GW11_PERF,GW11_FAIL decision;
```

---

## Proceso 12 / 15 : Generación y Aprobación Formal de Informes de Evolución

**Descripción:** Circuito documental para redactar, auditar y emitir informes pedagógicos oficiales en formato PDF con firma y sello institucional.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S15((●)) --> T15_DRAFT["<b>1. Redactar Informe en Borrador</b><br/>Profesional carga evolución y métricas"]
    T15_DRAFT --> T15_SEND["<b>2. Enviar a Revisión Directiva</b><br/>Bloqueo temporal de edición"]
    T15_SEND --> T15_AUDIT["<b>3. Auditoría Directiva</b><br/>Dirección revisa contenido y rigor pedagógico"]
    T15_AUDIT --> GW15_DEC{"<b>¿Informe Aprobado?</b>"}
    
    GW15_DEC -->|Requiere Ajustes| T15_FIX["<b>Devolver con Señalamientos</b><br/>Profesional atiende correcciones"]
    T15_FIX --> T15_SEND

    GW15_DEC -->|Aprobado| T15_PDF["<b>4. Compilar Documento PDF Oficial</b><br/>Generación inmutable con gráficos y sello"]
    T15_PDF --> T15_MAIL["<b>5. Notificar a la Familia</b><br/>Aviso por email para descarga en portal"]
    T15_MAIL --> E15_OK(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S15 inicio; class E15_OK fin;
    class T15_DRAFT,T15_SEND,T15_AUDIT,T15_FIX accion;
    class T15_PDF,T15_MAIL sistema;
    class GW15_DEC decision;
```

---

## Proceso 13 / 16 : Comunicación y Mensajería Segura Escuela-Familia

**Descripción:** Cuaderno de comunicaciones digital con cifrado de mensajes, registro cronológico en el legajo y acuse de lectura automático.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S16((●)) --> T16_WRITE["<b>1. Redactar Comunicado</b><br/>Docente escribe mensaje relativo al alumno"]
    T16_WRITE --> T16_SAVE["<b>2. Cifrar y Guardar Mensaje</b><br/>Asociar formalmente a la historia del alumno"]
    T16_SAVE --> T16_NOTIF["<b>3. Notificar al Familiar</b><br/>Aviso visual y por correo electrónico"]
    T16_NOTIF --> T16_READ["<b>4. Familiar Lee el Comunicado</b><br/>Se marca acuse de lectura automático"]
    T16_READ --> GW16_RESP{"<b>¿Requiere Respuesta?</b>"}
    GW16_RESP -->|Sí| T16_REPLY["<b>Responder en la Conversación</b><br/>Familiar envía aclaración al docente"]
    GW16_RESP -->|No| T16_DONE["<b>Constancia de Enterado</b><br/>Registro archivado en el cuaderno"]
    T16_REPLY --> T16_SAVE
    T16_DONE --> E16_OK(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S16 inicio; class E16_OK fin;
    class T16_WRITE,T16_READ,T16_REPLY,T16_DONE accion;
    class T16_SAVE,T16_NOTIF sistema;
    class GW16_RESP decision;
```

---

## Proceso 14 : Seguimiento de Avances y Monitoreo en Tiempo Real ("Mi Aula")

**Descripción:** Supervisión en directo de las tablets de los alumnos desde la pantalla del docente, con semáforos de progreso y alarmas ante dificultades.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S14((●)) --> T14_TOUCH["<b>1. Alumno Resuelve en Tablet</b><br/>Interacción con pictogramas y consignas"]
    T14_TOUCH --> T14_STREAM["<b>2. Transmisión SignalR en Vivo</b><br/>Envío continuo del ritmo de resolución"]
    T14_STREAM --> GW14_EVENT{"<b>¿Dificultad o 4 Fallos?</b>"}
    GW14_EVENT -->|No: Ritmo Normal| T14_GREEN["<b>Semáforo Verde en Tablero</b><br/>Docente visualiza avance satisfactorio"]
    GW14_EVENT -->|Sí: Se Traba| T14_RED["<b>Disparar Alerta Prioritaria</b><br/>Tarjeta parpadeante y aviso sonoro suave"]
    T14_RED --> T14_HELP["<b>Docente Brinda Apoyo Presencial</b><br/>Acercarse al banco a guiar al estudiante"]
    T14_GREEN --> E14_OK(((●)))
    T14_HELP --> E14_OK

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef alumno fill:#E6FFFA,stroke:#234E52,stroke-width:2px,color:#134E4A;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S14 inicio; class E14_OK fin;
    class T14_HELP accion;
    class T14_TOUCH alumno;
    class T14_STREAM,T14_GREEN,T14_RED sistema;
    class GW14_EVENT decision;
```

---

## Proceso 17 : Gestión Centralizada de Usuarios y Sesiones

**Descripción:** Administración de cuentas institucionales, reseteo de claves y desconexión obligatoria de sesiones ante eventualidades de seguridad.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S17((●)) --> T17_FIND["<b>1. Buscar Usuario en Padrón</b><br/>Dirección filtra por rol o nombre"]
    T17_FIND --> GW17_ACTION{"<b>¿Qué Acción?</b>"}
    GW17_ACTION -->|Resetear Clave| T17_RESET["<b>Generar Clave Temporal</b><br/>Obligatorio cambiar en próximo login"]
    GW17_ACTION -->|Suspender| T17_LOCK["<b>Desactivar Acceso</b><br/>Suspender cuenta temporalmente"]
    GW17_ACTION -->|Reactivar| T17_UNLOCK["<b>Restablecer Acceso</b><br/>Habilitar usuario en la sede"]
    
    T17_RESET --> T17_REVOKE["<b>Cerrar Sesiones Activas en Todo Dispositivo</b><br/>Desconexión inmediata por seguridad"]
    T17_LOCK --> T17_REVOKE
    T17_UNLOCK --> E17_OK(((●)))
    T17_REVOKE --> E17_OK

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S17 inicio; class E17_OK fin;
    class T17_FIND accion;
    class T17_RESET,T17_LOCK,T17_UNLOCK,T17_REVOKE sistema;
    class GW17_ACTION decision;
```

---

## Proceso 18 : Onboarding de Nuevos Usuarios

**Descripción:** Primer acceso a la plataforma, cambio de clave provisional obligatoria y visita guiada adaptada según el rol institucional.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S18((●)) --> T18_FIRST["<b>1. Primer Inicio de Sesión</b><br/>Usuario ingresa con clave temporal"]
    T18_FIRST --> T18_FORCE["<b>2. Cambio Obligatorio de Contraseña</b><br/>Definir contraseña personal y segura"]
    T18_FORCE --> GW18_TOUR{"<b>¿Tipo de Perfil?</b>"}
    
    GW18_TOUR -->|Docente / Terapeuta| T18_DOC["<b>Tour Guiado Docente</b><br/>Recorrido por 'Mi Aula', Radar y Actividades"]
    GW18_TOUR -->|Familiar| T18_FAM["<b>Bienvenida Familiar</b><br/>Explicación del portal y avances de su hijo"]
    GW18_TOUR -->|Estudiante PCD| T18_ALU["<b>Prueba Asistida en Tablet</b><br/>Comprobar interacción táctil con el docente"]
    
    T18_DOC --> T18_READY["<b>Perfil Operativo Activo</b><br/>Acceso permanente a las funciones de trabajo"]
    T18_FAM --> T18_READY
    T18_ALU --> T18_READY
    T18_READY --> E18_OK(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S18 inicio; class E18_OK fin;
    class T18_FIRST,T18_FORCE,T18_DOC,T18_FAM,T18_ALU accion;
    class T18_READY sistema;
    class GW18_TOUR decision;
```

---

## Proceso 19 : Centro de Ayuda, FAQ y Tickets de Soporte

**Descripción:** Mesa de ayuda y resolución de dudas con autoservicio en preguntas frecuentes o creación de tickets con captura automática de contexto.

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    S19((●)) --> GW19_OP{"<b>¿Qué Necesita?</b>"}
    GW19_OP -->|Consultar Preguntas| T19_FAQ["<b>Revisar Preguntas Frecuentes (FAQ)</b><br/>Guías rápidas y autoayuda temática"]
    GW19_OP -->|Reportar Incidencia| T19_TICKET["<b>Enviar Ticket de Soporte</b><br/>Describir la duda o problema"]
    
    T19_FAQ --> E19_FAQ(((●)))
    
    T19_TICKET --> T19_CONTEXT["<b>Capturar Contexto Automático</b><br/>Servidor registra pantalla actual y navegador"]
    T19_CONTEXT --> T19_SUPPORT["<b>Equipo de Soporte Analiza y Responde</b><br/>Orientación pedagógica o técnica"]
    T19_SUPPORT --> T19_NOTIF["<b>Notificar Respuesta al Usuario</b><br/>Aviso por correo y en el sistema"]
    T19_NOTIF --> E19_OK(((●)))

    classDef inicio fill:#D5E8D4,stroke:#23A027,stroke-width:2px,color:#274E13;
    classDef fin fill:#F8CECC,stroke:#B85450,stroke-width:3px,color:#783F04;
    classDef accion fill:#DAE8FC,stroke:#2B579A,stroke-width:2px,color:#1A365D;
    classDef sistema fill:#E1D5E7,stroke:#9673A6,stroke-width:2px,color:#4A235A;
    classDef decision fill:#FFFDC3,stroke:#AA7200,stroke-width:2px,color:#7F4F00;
    class S19 inicio; class E19_FAQ,E19_OK fin;
    class T19_FAQ,T19_TICKET,T19_SUPPORT accion;
    class T19_CONTEXT,T19_NOTIF sistema;
    class GW19_OP decision;
```
