# Documentación Oficial: Sprint 2 — InclusiON
**Espacio Confluence:** Gestión Ágil / Sprints de Desarrollo  
**Marco Académico:** Práctica Profesionalizante II (Institución Cervantes)  
**Criterio Lingüístico:** Lenguaje de Negocio Escolar e Institucional (sin Admin Global)

---

## 1. Ficha del Sprint

* **Nombre del Sprint:** Comunidad Educativa: Docentes, Estudiantes, Familias y Asignaciones
* **Épicas Principales:** IN-4 (Gestión de Usuarios) e IN-5 (Invitaciones y Asignaciones)
* **Procesos del Negocio Cubiertos:** P04 (Matriculación de Alumnos), P05 (Habilitación de Docentes), P06 (Padrón Familiar), P07 (Invitaciones Digitales), P08 (Asignaciones de Aula)
* **Período de Ejecución:** 25 de Marzo de 2025 al 07 de Abril de 2025 (2 semanas / 10 días hábiles)
* **Sprint Goal (Objetivo de Negocio):**  
  "Desarrollar el ecosistema de perfiles y comunidad educativa: proveer los mecanismos para matricular estudiantes con pantalla adaptada y PIN, registrar y habilitar docentes de apoyo, incorporar tutores legales (directo y por invitación) y conformar las asignaciones de trabajo en el aula."

* **Equipo Scrum:**
  * **Mariano Decalli:** Scrum Master / Desarrollador Backend
  * **German Cochis:** Desarrollador Frontend
  * **Sacha Del Barrio:** Relevamiento Institucional & Accesibilidad UI
  * **Fernando Aparicio:** Control de Calidad (QA & Testing)
  * **Mirko Ivo Wlk:** Arquitecto Fullstack & Desarrollador Backend
* **Métricas de Rendimiento:** 29 Historias/Tareas planificadas, 29 completadas (100% de efectividad)

---

## 2. Sprint Backlog Consistente (Jira vs. Procesos del Story Map)

El backlog reúne exactamente las tareas implementadas para habilitar la comunidad educativa y las aulas:

| Código | Tarea / Historia de Usuario | Proceso | Responsable | Estado | Criterio de Aceptación Cumplido |
|:---:|---|:---:|:---:|:---:|---|
| **IN-36** | Alta de docente con clave temporal y correo | P05 | Mirko Wlk | Desarrollada | Registro con matrícula profesional y envío de credenciales seguras. |
| **IN-37** | Consulta paginada de docentes con filtros | P05 | Mirko Wlk | Desarrollada | Grilla de docentes por especialidad, matrícula y estado activo. |
| **IN-38** | Edición de ficha docente | P05 | Mirko Wlk | Desarrollada | Actualización de datos de contacto; matrícula histórica protegida. |
| **IN-39** | Pausa y desactivación de docente | P05 | Mirko Wlk | Desarrollada | Desactivación lógica (soft-delete) preservando autoría de informes. |
| **IN-40** | Matriculación de alumno con perfil funcional | P04 | Mirko Wlk | Desarrollada | Alta de estudiante con selección de apoyos requeridos y datos personales. |
| **IN-41** | Consulta paginada de estudiantes | P04 | Mirko Wlk | Desarrollada | Padrón escolar con filtros por sala, grado y método de acceso. |
| **IN-42** | Edición de datos del estudiante | P04 | Mirko Wlk | Desarrollada | Modificación de sala asignada y configuración visual adaptada. |
| **IN-43** | Configuración de acceso adaptado (PIN / Asistido) | P04 | Mirko Wlk | Desarrollada | Asignación de PIN numérico de 4 dígitos; prohibición de correos al alumno. |
| **IN-44** | Egreso / Pase de alumno (Baja lógica) | P04 | Mirko Wlk | Desarrollada | El legajo pasa a inactivo; preservación permanente del historial legal. |
| **IN-45** | Alta directa de tutor con vínculo a su hijo | P06 | Mirko Wlk | Desarrollada | Registro de padre/madre con selector de estudiante a cargo. |
| **IN-46** | Registro de tutor por invitación digital | P06 | Mirko Wlk | Desarrollada | Pantalla de activación rápida para padres con token seguro. |
| **IN-47** | Consulta paginada de familias | P06 | Mirko Wlk | Desarrollada | Directorio de contactos de emergencia y tutores vinculados. |
| **IN-48** | Detalle de familiar con alumnos vinculados | P06 | Mirko Wlk | Desarrollada | Ficha de tutor que permite ver si tiene más de un hijo en la escuela. |
| **IN-49** | Edición de datos de familiar | P06 | Mirko Wlk | Desarrollada | Actualización urgente de teléfonos y dirección del tutor. |
| **IN-50** | Desvinculación de tutor legal | P06 | Mirko Wlk | Desarrollada | Cese de tutoría legal; bloqueo si es el único tutor activo del alumno. |
| **IN-51** | Asociación automática en alta directa | P06 | Mirko Wlk | Desarrollada | Vinculación inmediata tutor-alumno al completar la matrícula. |
| **IN-52** | Envío de credenciales a la familia | P06 | Mirko Wlk | Desarrollada | Notificación por correo con instructivo de bienvenida. |
| **IN-53** | Generar invitación digital por correo | P07 | Mirko Wlk | Desarrollada | El docente ingresa el email del padre y se envía enlace seguro de 7 días. |
| **IN-54** | Validación de código de invitación | P07 | Mirko Wlk | Desarrollada | Verificación de token no vencido al tocar el enlace desde el celular. |
| **IN-55** | Aceptación y alta automática de cuenta familiar | P07 | Mirko Wlk | Desarrollada | El padre define su contraseña y su cuenta queda creada en un paso. |
| **IN-56** | Consulta de invitaciones del docente | P07 | Mirko Wlk | Desarrollada | El maestro ve cuáles padres aceptaron y cuáles siguen pendientes. |
| **IN-57** | Supervisión directiva de invitaciones | P07 | Mirko Wlk | Desarrollada | Panel general de directivos para monitorear vinculaciones familiares. |
| **IN-58** | Asignar docente a la escuela | P05 | Mirko Wlk | Desarrollada | Vinculación formal de pertenencia del profesional a la institución. |
| **IN-59** | Desvincular docente de la escuela | P05 | Mirko Wlk | Desarrollada | Cese institucional con validación de no dejar aulas sin cobertura. |
| **IN-60** | Asignar alumno a docente en sala | P08 | Mirko Wlk | Desarrollada | El directivo conforma el equipo: el docente ya ve al chico en "Mi Aula". |
| **IN-61** | Cerrar ciclo de asignación docente-alumno | P08 | Mirko Wlk | Desarrollada | Desasignación al culminar el ciclo lectivo preservando informes firmados. |
| **IN-62** | Vinculación familiar automática por código | P06 | Mirko Wlk | Desarrollada | Al registrarse por invitación, el padre se vincula solo a su hijo. |
| **IN-63** | Configuración del perfil de habilidades | P04 | Mirko Wlk | Desarrollada | Activación de áreas prioritarias (Comunicación, Lógica) en la ficha del alumno. |
| **IN-64** | Desvinculación lógica transversal (Soft-delete) | Transv. | Mirko Wlk | Desarrollada | Protección de base de datos: ningún registro educativo o clínico se borra físicamente. |

---

## 3. Registro de Daily Meetings (Minutas Reales del Sprint)

### Daily 05 — Miércoles 26/03/2025 (Inicio de la Iteración)
* **Decalli Mariano (Scrum Master):**  
  *¿Qué hice?* Organizamos el sprint planning del martes. Las tareas quedaron bien repartidas y con estimaciones más realistas que el sprint pasado.  
  *¿Qué voy a hacer?* Hacer el seguimiento del avance y estar atento si alguien se bloquea.  
  *Impedimentos:* Ninguno.
* **Cochis German (Frontend):**  
  *¿Qué hice?* Con los problemas de conexión resueltos, pude conectar el login y el registro. Empecé a trabajar en la pantalla de catálogos.  
  *¿Qué voy a hacer?* Seguir con los catálogos y después arrancar con las actividades.  
  *Impedimentos:* Ninguno.
* **Del Barrio Sacha (Relevamiento y Accesibilidad):**  
  *¿Qué hice?* Tuve una segunda entrevista, esta vez con una fonoaudióloga. Me comentó que es importante poder asociar las actividades a objetivos terapéuticos concretos.  
  *¿Qué voy a hacer?* Documentar esa entrevista y proponer un ajuste en la HU de actividades para incluir ese campo.  
  *Impedimentos:* Ninguno.
* **Aparicio Fernando (QA):**  
  *¿Qué hice?* Terminé de revisar y probar los flujos de registro que hizo Mirko. Todo funciona bien en los casos normales y también cuando algo sale mal.  
  *¿Qué voy a hacer?* Revisar lo que vaya saliendo de este sprint y ayudar donde haga falta.  
  *Impedimentos:* Ninguno.
* **Wlk Mirko (Backend / Arquitectura):**  
  *¿Qué hice?* Terminé el perfil de usuario que quedó pendiente del sprint 1. También levanté un entorno compartido con Docker Compose para que todos puedan usar el mismo ambiente.  
  *¿Qué voy a hacer?* Arrancar con los endpoints de catálogos para que Germán pueda conectarlos.  
  *Impedimentos:* Ninguno.

---

### Daily 06 — Miércoles 02/04/2025 (Cierre Técnico y Conexión de Pantallas)
* **Decalli Mariano (Scrum Master):**  
  *¿Qué hice?* Revisé cómo viene el sprint y actualicé el tablero. Vamos bien, quedó poco para cerrar.  
  *¿Qué voy a hacer?* Preparar la retro del lunes 7 y mandar recordatorio al equipo.  
  *Impedimentos:* Ninguno.
* **Cochis German (Frontend):**  
  *¿Qué hice?* Terminé los catálogos: se pueden ver, crear y editar. También empecé la pantalla de actividades con el campo de objetivo terapéutico que levantó Sacha.  
  *¿Qué voy a hacer?* Terminar el formulario de actividades con sus validaciones y conectarlo con el backend.  
  *Impedimentos:* Ninguno.
* **Del Barrio Sacha (Relevamiento y Accesibilidad):**  
  *¿Qué hice?* Documenté la entrevista con la fonoaudióloga y actualicé la HU de actividades. También definí los criterios de aceptación de las HU de este sprint.  
  *¿Qué voy a hacer?* Coordinar una tercera entrevista con un terapeuta ocupacional para entender mejor cómo se asignan actividades a personas.  
  *Impedimentos:* Ninguno.
* **Aparicio Fernando (QA):**  
  *¿Qué hice?* Revisé lo que hizo Mirko esta semana y fui probando que todo funcione como se espera.  
  *¿Qué voy a hacer?* Seguir revisando a medida que Mirko cierre los endpoints de actividades.  
  *Impedimentos:* Ninguno.
* **Wlk Mirko (Backend / Arquitectura):**  
  *¿Qué hice?* Terminé los endpoints de catálogos y de actividades. El campo de objetivo terapéutico quedó como texto libre por ahora. Abrí los PRs para revisión.  
  *¿Qué voy a hacer?* Revisar los PRs con Fernando y, una vez mergeados, empezar a diseñar lo que viene en el sprint 3.  
  *Impedimentos:* Ninguno.

---

## 4. Retrospectiva del Sprint 2 (Sprint Retrospective)

* **Fecha de Realización:** Lunes 07 de Abril de 2025  
* **Facilitador:** Mariano Decalli (Scrum Master)  
* **Participantes:** Todo el equipo (Mariano Decalli, German Cochis, Sacha Del Barrio, Fernando Aparicio, Mirko Wlk)

### Dinámica: ¿Qué salió bien? ¿Qué podemos mejorar?

#### Comenzar a hacer (Start Doing)
* **Hacer una mini demo al cierre de cada sprint:** Ver todo funcionando junto en una sesión conjunta antes de dar por cerrado el sprint.
* **Incluir las validaciones de los formularios en la misma tarea del formulario:** No dejarlas como tareas separadas para asegurar que la pantalla se entregue lista para usar.
* **Anotar en el tablero cualquier cosa que quede pendiente o incompleta:** Registrar inmediatamente cualquier deuda técnica para no perderla de vista.

#### Dejar de hacer (Stop Doing)
* **Avanzar en pantallas sin saber bien qué datos va a devolver el backend:** Evitar suposiciones que generen retrabajo en los componentes de Angular.
* **Definir criterios de aceptación recién cuando la tarea ya empezó:** Los criterios deben estar formalizados antes de iniciar el desarrollo.
* **Preguntar cosas urgentes y esperar a la daily:** Si la duda es bloqueante o corta, se resuelve directamente por el canal de chat en el momento.

#### Seguir haciendo (Keep Doing)
* **El entorno compartido funcionó perfecto:** Todo el equipo levantó los servicios sin discrepancias de configuración gracias a Docker Compose.
* **El feedback de las entrevistas de Sacha sigue siendo muy útil:** La entrevista con la fonoaudióloga mejoró directamente la definición funcional de las actividades.
* **Fernando revisando lo que hace Mirko:** El control de calidad continuo permitió detectar y corregir errores antes de fusionar el código.

---

### Plan de Acción Concreto para el Sprint 3

| # | Compromiso de Mejora | Responsable | Plazo de Entrega | Estado |
|:---:|---|:---:|:---:|:---:|
| **A-01** | Germán incluirá las validaciones en la misma tarea del formulario desde el sprint 3. | German Cochis | 09/04/2025 | Acordado |
| **A-02** | Mirko y Germán acordarán qué datos necesita cada pantalla antes de que Germán empiece a desarrollarla. | Mirko Wlk y German Cochis | 08/04/2025 | Acordado |
| **A-03** | Mariano organizará una mini demo con el equipo al cierre del sprint 3. | Mariano Decalli | 21/04/2025 | Agendado |
| **A-04** | Todos los integrantes anotarán en el tablero cualquier deuda o pendiente que detecten. | Todo el Equipo | Continuo | En curso |
