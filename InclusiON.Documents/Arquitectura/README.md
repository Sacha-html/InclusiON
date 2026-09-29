# Arquitectura del Sistema — InclusiON

**Definición de Arquitectura en Lenguaje de Negocio / Cliente**  
**Institución Cervantes — Analista de Sistemas — Prácticas Profesionalizantes**  
**Proyecto:** InclusiON — Plataforma Web y Móvil Accesible de Inclusión Educativa  
**Versión del Documento:** 2.0 (Etapa de Desarrollo y Validación de Producto)

---

## 1. Visión General y Propósito

**InclusiON** es un ecosistema digital integral diseñado para acompañar y potenciar la trayectoria educativa de personas con discapacidad, conectando de forma ágil, segura y humana a los cuatro actores centrales del proceso:
- **Estudiantes:** Acceden a un entorno de aprendizaje gamificado, intuitivo y adaptado a sus necesidades sensoriales y cognitivas.
- **Profesionales de la Educación y Salud (Docentes y Terapeutas):** Diseñan actividades, configuran niveles de dificultad, evalúan avances en tiempo real y emiten diagnósticos e informes formales.
- **Familias:** Participan activamente del progreso de sus hijos, consultan informes aprobados y mantienen un canal directo de comunicación con la escuela.
- **Instituciones Educativas y Terapéuticas:** Coordinan sedes, aulas y equipos profesionales bajo estrictos estándares de privacidad.

El presente dossier consolida la **Definición de la Arquitectura del Sistema**, estructurada en **lenguaje del cliente y del negocio** para facilitar su comprensión por parte de equipos directivos, docentes, familias y comités evaluadores:

```
InclusiON.Documents/Arquitectura/
├── README.md                              # Presentación, Propósito y Navegación
├── ARQUITECTURA-DEL-SISTEMA.md            # ⭐ DOCUMENTO MAESTRO CONSOLIDADO (Entrega Académica Oficial)
├── 01-DIAGRAMA-DE-DESPLIEGUE.md           # Distribución de Dispositivos, Servidores y Seguridad
├── 02-AMBIENTE-DE-IMPLEMENTACION.md       # Definición de Tecnologías y Valor para el Usuario
├── 03-DIAGRAMA-TRANSICION-ESTADOS.md      # Ciclos de Vida de Actividades, Alumnos e Informes
├── 04-DIAGRAMAS-BPMN-PROCESOS.md          # 🔄 Modelado de Procesos de Negocio (BPMN 2.0 con Carriles)
├── img/                                   # 🖼️ 19 Diagramas renderizados en imágenes vectoriales SVG directas
└── puml/                                  # 📐 19 Archivos de modelado fuente editables en PlantUML (.puml)
```

---

## 2. Mapa de Navegación del Dossier

| Documento | Enfoque en Lenguaje Cliente | Aspectos Principales Desarrollados |
|---|---|---|
| ⭐ **DOCUMENTO MAESTRO** | [**ARQUITECTURA-DEL-SISTEMA.md**](./ARQUITECTURA-DEL-SISTEMA.md) | **Consolidado oficial completo** con las 4 partes integradas en un solo archivo (Despliegue, Tecnologías, Estados y BPMN 2.0), con todos los diagramas renderizados directamente y explicaciones institucionales. |
| **1. Diagrama de Despliegue** | [01-DIAGRAMA-DE-DESPLIEGUE.md](./01-DIAGRAMA-DE-DESPLIEGUE.md) | Muestra cómo interactúan las computadoras de los docentes, las tablets escolares de los alumnos y los teléfonos de las familias con el servidor central de la escuela, resguardando la privacidad médica y conectándose con el banco de pictogramas ARASAAC. |
| **2. Ambiente de Implementación** | [02-AMBIENTE-DE-IMPLEMENTACION.md](./02-AMBIENTE-DE-IMPLEMENTACION.md) | Explica las tecnologías elegidas (lenguajes, bases de datos y herramientas) en función del beneficio directo que aportan: rapidez táctil en el aula, tipografías para dislexia, alertas sonoras ante frustración, reportes impresos de calidad y costo cero de licencias para la institución. |
| **3. Diagrama de Transición de Estados** | [03-DIAGRAMA-TRANSICION-ESTADOS.md](./03-DIAGRAMA-TRANSICION-ESTADOS.md) | Describe cómo cambian de estado las actividades asignadas, el avance en el camino de aprendizaje ("Mi Camino"), el cuidado emocional contra la frustración (aviso al docente al 4to intento), el circuito de aprobación directiva de informes y las cuentas de usuarios. |
| **4. Diagramas de Procesos (BPMN 2.0)** | [04-DIAGRAMAS-BPMN-PROCESOS.md](./04-DIAGRAMAS-BPMN-PROCESOS.md) | Modela los 11 procesos de negocio principales mediante piscinas y carriles (swimlanes): Proceso Core terapéutico, motor adaptativo, evaluación diagnóstica, alertas en vivo "Mi Aula", onboarding familiar, emisión de reportes oficiales en PDF y autenticación inclusiva. |
| **🖼️ Carpeta de Imágenes** | [img/](./img/) | Contiene los 19 diagramas generados en formato vectorial `.svg` listos para insertar en Word o presentaciones. |
| **📐 Carpeta PlantUML** | [puml/](./puml/) | Contiene los 19 archivos `.puml` originales para edición en herramientas de modelado UML y BPMN. |

---

## 3. Principios Rectores del Producto

1. **Inclusión y Accesibilidad Universal como Norma (WCAG 2.1 AA/AAA):**
   La plataforma no trata la accesibilidad como un accesorio cosmético opcional: cada botón, tipografía (Lexend para dislexia, Atkinson para baja visión), contraste visual y lector en voz alta fue concebido desde el primer día para garantizar la autonomía de las personas con discapacidad.

2. **Cuidado Emocional y Prevención de la Frustración Pedagógica:**
   El sistema protege activamente la autoestima del estudiante. Si detecta 4 intentos fallidos seguidos, pausa la actividad con un mensaje de aliento y le avisa en vivo al docente para que se acerque a brindar apoyo humano antes de que el niño se desanime.

3. **Privacidad Rigurosa y Soberanía de los Datos de Salud:**
   Los expedientes pedagógicos y diagnósticos son confidenciales. Por ello, el motor de recomendación inteligente de actividades funciona dentro del propio servidor institucional, sin enviar jamás información de los menores a servidores de inteligencia artificial en la nube.

4. **Multiplataforma y Adaptabilidad al Aula Real:**
   La solución funciona con fluidez en computadoras de escritorio de la escuela y se puede instalar como aplicación fija de pantalla completa en tablets táctiles Android, evitando distracciones durante las clases.

5. **Colaboración Transparente Escuela-Familia:**
   Los informes no son meras notas internas: cuentan con un circuito formal de revisión directiva y se publican en formato digital e impreso en el portal familiar, fortaleciendo el lazo entre terapeutas y el hogar.
