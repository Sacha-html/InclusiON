# Glosario — Sistema InclusiON

Definición de términos utilizados en la documentación y en la plataforma.

---

## Actores del sistema

| Término | Definición |
|---------|-----------|
| **Administrador Global** | Usuario con acceso total al sistema. Crea instituciones, gestiona roles, configura catálogos y da de alta a todos los tipos de usuario. |
| **Administrador Institucional** | Usuario administrador vinculado a una o más instituciones. Solo ve y gestiona datos dentro del alcance de sus instituciones asignadas. |
| **Profesional** | Docente, terapeuta, psicólogo u otro especialista que trabaja con personas con discapacidad. Evalúa, planifica, crea actividades y monitorea el progreso. |
| **Persona con discapacidad** | Destinatario central del sistema. Realiza actividades educativas y su progreso es monitoreado por profesionales y familiares. En el DOCX original se lo denomina "alumno" o "paciente". |
| **Representante familiar** | Familiar o tutor legal de una persona con discapacidad. Consulta el progreso, recibe reportes y se comunica con el profesional. |

---

## Conceptos del dominio educativo

| Término | Definición |
|---------|-----------|
| **Área de habilidad** | Dominio de competencia que se trabaja con una persona. Ejemplos: Comunicación, Alfabetización, Lógica-Matemática, Conducta, Motricidad. Cada área tiene un color e ícono distintivo. |
| **Perfil de habilidades** | Conjunto de áreas de habilidad asignadas a una persona. Define qué áreas se van a trabajar y aparecen en el roadmap y el radar chart. |
| **Perfil funcional** | Datos que describen las capacidades y necesidades de la persona: nivel de atención, comunicación, motricidad, estilo de aprendizaje, uso de CAA, terapias adicionales, etc. |
| **Diagnóstico funcional** | Evaluación formal registrada por un profesional que documenta el punto de partida de la persona: capacidades, desafíos, apoyos requeridos, objetivos pedagógicos y estrategias recomendadas. |
| **Nivel de autonomía** | Grado de independencia de la persona para interactuar con la plataforma. Determina el método de login: Alta (login visual estándar: identificación por nombre + contraseña), Media (PIN), Baja (login asistido con supervisor). |
| **Plan de trabajo** | Conjunto de actividades organizadas secuencialmente por área de habilidad para una persona. Sinónimo de roadmap. |
| **Reporte de progreso** | Documento formal generado por el profesional que describe los avances de la persona durante un período determinado. Exportable a PDF y visible para la familia. |

---

## Conceptos de la plataforma

| Término | Definición |
|---------|-----------|
| **Actividad** | Ejercicio educativo interactivo creado por un profesional o predefinido por el sistema. Tiene un título, área de habilidad, nivel de complejidad y contenido dinámico basado en una plantilla. |
| **Actividad estándar** | Actividad predefinida por el sistema que cualquier profesional puede usar pero no modificar. |
| **Template (plantilla)** | Tipo de actividad que define su estructura de contenido y comportamiento interactivo. Los 5 tipos son: selección de figuras, suma visual, emparejar imagen-palabra, ordenar secuencia y completar letra. |
| **ContentSchema** | Estructura JSON que define los campos del formulario dinámico de una plantilla. Determina qué datos debe completar el profesional al crear una actividad. |
| **Roadmap** | Plan de trabajo personalizado que organiza actividades en secuencia dentro de cada área de habilidad. La persona lo ve como un camino visual estilo Duolingo con nodos bloqueados, desbloqueados y completados. |
| **Umbral de desbloqueo** | Porcentaje mínimo de éxito que la persona debe alcanzar en una actividad para que la siguiente se desbloquee automáticamente. Por defecto: 60%. |
| **Asignación** | Vinculación de una actividad a una persona. Tiene estados: Pendiente, En Progreso, Completada, Cancelada. |
| **Respuesta** | Registro de un intento de resolución de una actividad asignada. Incluye tiempo, intentos, porcentaje de éxito, patrón de respuesta y nivel de frustración. |
| **Player** | Componente visual interactivo que presenta la actividad a la persona. Hay 5 tipos, uno por cada plantilla. |
| **Dashboard** | Panel principal del profesional o familiar con indicadores resumidos: contadores, últimas actividades, invitaciones. |
| **Mi Aula** | Sección del dashboard del profesional que muestra cards visuales de cada persona asignada con acceso rápido a su detalle. |
| **Radar chart** | Gráfico tipo araña que visualiza el nivel de cada área de habilidad de una persona, calculado como promedio de éxito en actividades completadas. |
| **Invitación** | Mecanismo de registro controlado para familiares. El profesional genera una invitación con email; el familiar recibe un link único para registrarse. Expira a los 7 días. |
| **Catálogo** | Tabla de referencia del sistema que alimenta dropdowns y configuraciones. Los 6 catálogos son: tipos de discapacidad, niveles de autonomía, categorías de actividad, áreas de habilidad, tipos de template y métodos de login. |

---

## Motor de Dificultad Adaptativa (MDA)

| Término | Definición |
|---------|-----------|
| **MDA** | Motor de Dificultad Adaptativa. Sistema automático que ajusta los parámetros de las actividades (dificultad, tiempo, pistas, intentos) según el rendimiento del estudiante. |
| **Estado Estable** | El rendimiento de la persona es consistente. El motor no realiza cambios. |
| **Estado Progresando** | La persona acumula éxitos consecutivos por encima del umbral. El motor sube la dificultad y reduce ayudas. |
| **Estado Dificultad** | La persona acumula fracasos consecutivos. El motor baja la dificultad y aumenta ayudas. |
| **Estado Frustración** | Se detecta un nivel alto de frustración o múltiples abandonos. El motor baja todo al mínimo y envía una alerta al profesional. |
| **Nivel de frustración** | Indicador numérico (1-5) que mide el grado de frustración de la persona durante una actividad. Se incrementa cuando acumula más de 3 intentos fallidos. |
| **Zona de desarrollo próximo** | Concepto pedagógico que refiere al espacio entre lo que la persona puede hacer sola y lo que puede lograr con ayuda. El MDA busca mantener a la persona en esta zona. |

---

## Accesibilidad

| Término | Definición |
|---------|-----------|
| **Perfil de accesibilidad** | Configuración visual de la plataforma adaptada a un tipo de discapacidad. Los 7 perfiles son: estándar, alto contraste, dislexia, baja visión, deuteranopía, protanopía, tritanopía. |
| **CAA** | Comunicación Aumentativa y Alternativa. Conjunto de herramientas y estrategias que complementan o sustituyen el habla para personas con dificultades de comunicación. |
| **CIF / ICF** | Clasificación Internacional del Funcionamiento, de la Discapacidad y de la Salud. Marco de la OMS que clasifica la salud y la discapacidad a nivel corporal, individual y social. |
| **Pictograma** | Imagen esquemática que representa un concepto, objeto o acción. InclusiON usa pictogramas de ARASAAC para las actividades. |
| **ARASAAC** | Portal Aragonés de la Comunicación Aumentativa y Alternativa. Proporciona pictogramas de uso libre que InclusiON integra en las actividades educativas. |
| **WCAG** | Web Content Accessibility Guidelines. Pautas de accesibilidad web del W3C. La plataforma cumple con nivel AA. |
| **Login visual estándar** | Método de autenticación para personas con alta autonomía. La persona se identifica seleccionando su nombre de una lista visual (sin ingresar email), y luego ingresa su contraseña alfanumérica. Endpoint: `/Auth/login/visual-standard`. |
| **Login asistido** | Método de login para personas con baja autonomía. Un profesional con permiso de supervisión autoriza el acceso. |

---

## Términos organizacionales

| Término | Definición |
|---------|-----------|
| **Institución educativa** | Escuela, centro de rehabilitación, consultorio u organización donde trabajan los profesionales con personas con discapacidad. |
| **Filtrado por institución** | Mecanismo que limita la visibilidad de datos para los administradores institucionales. Solo ven profesionales, personas, familiares e invitaciones de sus instituciones asignadas. |
| **Soft-delete** | Desactivación lógica de un registro (se marca como inactivo) sin eliminar los datos históricos. Todas las relaciones del sistema usan soft-delete. |
| **Consentimiento informado** | Autorización formal del representante familiar para el tratamiento de los datos de la persona con discapacidad en la plataforma. |

---

## Gestión de usuarios y soporte

| Término | Definición |
|---------|-----------|
| **Gestión de usuarios (centralizada)** | Administración transversal de cuentas de usuario desde un panel unificado del admin. Incluye reseteo de contraseña, desactivación, reactivación y consulta de actividad. Complementa los CRUDs de dominio (profesionales, familiares, personas). |
| **Onboarding** | Proceso de incorporación de un usuario nuevo al sistema. Incluye el cambio obligatorio de contraseña temporal, el completado de perfil (para profesionales) y un tour guiado del portal. Varía según el rol del usuario. |
| **Centro de ayuda (FAQ)** | Sección de preguntas frecuentes organizadas por categoría (Cuenta y Acceso, Actividades, Reportes, Comunicación, Accesibilidad, General). Gestionadas por el admin, visibles para todos los usuarios autenticados. |
| **Ticket de soporte** | Reporte de problema técnico o consulta creado por un usuario desde la plataforma. Captura automáticamente el contexto (sección, navegador). Tiene estados: Abierto, En Revisión, Respondido, Resuelto, Cerrado. |
| **Guía contextual** | Tooltip o ayuda inline que aparece junto a una funcionalidad específica del portal para explicar su uso. Se implementa como componente frontend sin endpoint. El usuario puede marcar "No volver a mostrar". |
| **Contraseña temporal** | Contraseña generada automáticamente por el sistema al crear o reactivar una cuenta. Se muestra una sola vez al admin y obliga al usuario a cambiarla en el próximo login (`MustChangePassword = true`). |
