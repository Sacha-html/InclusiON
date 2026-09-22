# 🎓 Trabajo Práctico de Investigación y Aplicación
## 2do Parcial — Normas y Modelos de Calidad de Software
**Institución Cervantes — Analista de Sistemas**

* **Norma Elegida**: WCAG 2.2 (Pautas de Accesibilidad para el Contenido Web)
* **Sistema Analizado**: **InclusiON** (Proyecto de Tesis / Plataforma Web Inclusiva)

---

## 📑 Parte I — Investigación Teórica

### 1. Nombre completo de la norma
Se denomina **Web Content Accessibility Guidelines 2.2** (*Pautas de Accesibilidad para el Contenido Web 2.2*). Es mundialmente reconocida por su sigla: **WCAG 2.2**.

### 2. Organismo creador
Fue elaborada por el **W3C** (*World Wide Web Consortium*), la organización internacional encargada de definir las normas y estándares de la Web. Dentro del W3C, el desarrollo técnico está a cargo del grupo especializado **WAI** (*Web Accessibility Initiative*).

### 3. Año de publicación
Publicada formalmente en **octubre de 2023**. 
* Versiones previas: WCAG 2.0 (2008) y WCAG 2.1 (2018).
* **Evolución acumulativa**: Las versiones no se reemplazan, sino que se suman de manera retrospectiva (quien cumple WCAG 2.2 cumple automáticamente 2.0 y 2.1). En 2025 fue elevada al máximo estándar internacional como **ISO/IEC 40500:2025**.

### 4. Objetivo principal
Garantizar que cualquier persona pueda percibir, comprender, navegar e interactuar con el contenido y aplicaciones web de forma autónoma, independientemente de si posee o no alguna discapacidad.

### 5. Problema que intenta resolver
Eliminar las barreras digitales causadas por interfaces diseñadas exclusivamente para usuarios promedio o no discapacitados.
* **Barreras comunes**:
  * Imágenes sin descripción alt para lectores de pantalla.
  * Formularios y navegación operables únicamente mediante ratón (sin soporte de teclado).
  * Textos con contraste de color insuficiente.
  * Contenido multimedia sin subtítulos o descripciones de audio.

### 6. Ámbito de aplicación
Aplica a sitios web, aplicaciones web, portales transaccionales y contenido digital. 
* En la **Unión Europea y España**, es de cumplimiento obligatorio para organismos públicos y, a partir de 2025 (European Accessibility Act), para el sector privado regulado (banca, e-commerce, seguros).
* En **Argentina**, rige bajo la Ley Nacional 26.653 de Accesibilidad de la Información Web.

### 7. Principales conceptos
* **4 Principios Universales (POUR)**:
  1. **Perceptible**: La información y la interfaz deben ser presentables a los usuarios en formas que puedan percibir.
  2. **Operable**: Los componentes de la interfaz y la navegación deben ser utilizables por cualquier método de entrada.
  3. **Comprensible**: La información y el manejo de la interfaz deben ser comprensibles y predecibles.
  4. **Robusto**: El contenido debe ser lo suficientemente robusto para ser interpretado de manera fiable por una amplia variedad de agentes de usuario (incluidos lectores de pantalla).
* **Niveles de Conformidad**:
  * **Nivel A**: Requisitos mínimos obligatorios para evitar la exclusión total.
  * **Nivel AA**: Estándar recomendado internacionalmente y exigido por los marcos legales.
  * **Nivel AAA**: Nivel máximo de accesibilidad y especialización.
* **Novedades en WCAG 2.2**: Incorporó 9 nuevos criterios de éxito orientados a mejorar la experiencia en dispositivos móviles táctiles y reducir la carga cognitiva.

### 8. Ventajas
* Establece criterios métricos y verificables.
* Mejora la usabilidad universal (UX) para todos los usuarios.
* Asegura blindaje legal y regulatorio.
* Potencia la indexación orgánica en motores de búsqueda (SEO).

### 9. Desventajas / Desafíos
* Exige planificar la arquitectura de software desde las etapas iniciales (*Shift-Left*).
* El nivel AAA resulta complejo de alcanzar de manera homogénea en todos los flujos.
* Las herramientas de auditoría automática no son suficientes; requiere validaciones funcionales humanas y pruebas con usuarios reales.

### 10. Organizaciones que la utilizan
* Organismos gubernamentales e instituciones públicas de la UE y EE.UU.
* Sector bancario, plataformas educativas globales y servicios de salud.
* Empresas tecnológicas líderes como Microsoft, Google y Apple.

### 11. Importancia estratégica y social
Las WCAG 2.2 garantizan el derecho básico y universal del acceso equitativo a la información digital. En un contexto de hiperglobalización donde la educación, el empleo y los trámites ciudadanos están digitalizados, un software inaccesible funciona como un mecanismo directo de exclusión social y económica. Esta norma no solo establece buenas prácticas de código, sino métricas de Diseño Universal para proteger a personas con discapacidades cognitivas, visuales y motoras.

### 12. Riesgos por incumplimiento
* **Riesgo Institucional y Licitaorio (Ley 26.653)**: Inhabilitación para contratar con el Estado, suspensión de licitaciones o cancelación de subsidios públicos ante la ONTI (Disposición N° 6/19).
* **Sanciones por Defensa del Consumidor (Ley 24.240)**: Exposición a severas multas económicas (Art. 47) por falta de información clara y veraz (Art. 4°).
* **Riesgo Reputacional**: Pérdida de imagen pública y discriminación activa de clientes.
* **Acciones ante INADI y Reguladores Sectoriales**: Sanciones operativas en sectores regulados (banca BCRA, educación).

### 13. Beneficios organizacionales
* **Blindaje Legal**: Cumplimiento irrestricto de las leyes nacionales e internacionales.
* **Ampliación de Mercado**: Acceso y fidelización de un segmento demográfico históricamente desatendido.
* **Optimización de UX**: Reducción del esfuerzo táctil y menor fatiga mental para el 100% de los usuarios.
* **Calidad Técnica**: Código semántico estructurado, de fácil mantenimiento y alta compatibilidad.

---

## 💻 Parte II — Aplicación Práctica en InclusiON

### 1. Introducción y Enfoque en InclusiON
InclusiON es un ecosistema digital inclusivo orientado al ámbito educativo y terapéutico. Para evitar que una barrera digital prive a un estudiante con discapacidad de su autonomía, la plataforma integra WCAG 2.2 de forma nativa desde la arquitectura (*Shift-Left*).

```
                  ┌─────────────────────────────────────────────────┐
                  │          PRINCIPIOS ACCESIBLES (POUR)           │
                  └────────────────────────┬────────────────────────┘
                                           │
         ┌──────────────────┬──────────────┴───────┬──────────────────┐
         ▼                  ▼                      ▼                  ▼
  PERCEPTIBILIDAD        OPERABILIDAD        COMPRENSIBILIDAD     ROBUSTEZ
  • 7 Perfiles Visuales  • Botones Macro      • "Mi Camino"      • HTML5 Semántico
  • Contraste AAA 17.4:1   (>48px)             nodos gamificados • ARIAInvisible
  • ARASAAC Pictogramas  • Botones ▲ y ▼     • Login por PIN y   • Cifrado AES-256
  • Sintetizador TTS     • Alt + A Accesible   Asistido            • PDF A4 Estándar
```

---

### 2. Aspectos Verificables por Principio (POUR)

#### 👁️ A. Perceptibilidad
* **7 Perfiles Visuales Conmutables (14 combinaciones)**:
  * **Estándar**: Vista moderna y armónica.
  * **Alto Contraste**: Fondo oscuro pleno con tipografía en blanco/amarillo de gran nitidez.
  * **Dislexia**: Fuente adaptada con mayor densidad en la base de caracteres e interlineado amplio.
  * **Visión Reducida**: Incremento tipográfico automático a 18px.
  * **3 Variantes para Daltonismo**: Ajustes cromáticos para Protanopía, Deuteranopía y Tritanopía.
* **Contraste de Nivel AAA (17.4:1)**: Supera la exigencia estándar de 4.5:1, alcanzando 17.4:1 en texto principal y 8.6:1 en botones.
* **Integración ARASAAC**: Pictogramas con texto alternativo descriptivo en español.
* **Sintetizador de Voz (TTS)**: Lector de consignas integrado vía Web Speech API sin requerir extensiones de terceros.

#### 🎮 B. Operabilidad
* **Áreas Táctiles Macro (> 48x48 px)**: Botones principales ("Mi camino", "Actividades", "Hablar") con altura superior a 120px para prevenir pulsaciones erróneas en personas con temblores o baja motricidad.
* **Alternativa a Arrastrar (Drag & Drop)**: Reemplazo del arrastre con mouse/dedo mediante botones de desplazamiento simple **▲** y **▼**.
* **Menú Rápido por Teclado (`Alt + A`)**: Despliega el panel de ajustes de accesibilidad de forma inmediata.
* **Foco de Selección Azul (3px)**: Marcador visual de alta nitidez (8.6:1) al navegar con la tecla `Tab`, manteniendo un margen superior de 80px para evitar el solapamiento con barras fijas.

#### 🧠 C. Comprensibilidad
* **Ruta de Aprendizaje Gamificada ("Mi Camino")**: Presentación secuencial de actividades nodo por nodo para minimizar la carga cognitiva.
* **Métodos de Autenticación Simplificados**:
  * **Login por PIN**: Teclado numérico amplio de 4 dígitos.
  * **Login Asistido**: Autorización previa por dispositivo confiable; el estudiante ingresa seleccionando su foto o avatar.
* **Asistente de Registro Unificado en 3 Pasos**: Creación simultánea de Alumno, Tutor y Aula sin duplicar tipeo de datos.
* **Avisos de Apoyo Pedagógico (Pausas Preventivas)**: Tras 4 intentos fallidos, el sistema detiene el juego de forma amigable y notifica en tiempo real al profesional vía SignalR.

#### 🔒 D. Robustez
* **Estructura HTML5 Semántica y Marcas ARIA**: Compatibilidad con lectores de pantalla (NVDA, JAWS, TalkBack).
* **Cifrado de Grado Médico (AES-256)**: Protección de diagnósticos, informes y datos sensibles de salud.
* **Exportación Estándar A4 / PDF**: Generación de reportes limpios y sanitizados para la institución y las familias.

---

### 3. Matriz de Cumplimiento de las 9 Reglas de WCAG 2.2 en InclusiON

| Criterio WCAG 2.2 | Nivel | Exigencia Internacional | Demostración en InclusiON |
| :--- | :---: | :--- | :--- |
| **2.4.11 Foco No Oculto (Mínimo)** | AA | El foco de teclado no debe ser tapado por barras fijas o cabeceras. | Margen superior automático de 80 px en la vista; el encabezado fijo nunca solapa el elemento activo. |
| **2.4.12 Foco No Oculto (Mejorado)** | AAA | Cero porcentaje de ocultamiento en cualquier diálogo o ventana. | Modal con atrapado de foco (*focus-trap*) que garantiza 100% de visibilidad del elemento activo. |
| **2.4.13 Apariencia del Foco** | AAA | Indicador de foco de al menos 2px de grosor y contraste $\ge 3:1$. | Marco azul grueso de 3 píxeles con contraste sobresaliente de 8.6:1. |
| **2.5.7 Movimientos de Arrastre** | AA | Las acciones de arrastre (*drag & drop*) deben ofrecer alternativa de un solo clic. | Tarjetas con botones simples **▲** y **▼** para reordenar secuencias sin arrastrar. |
| **2.5.8 Tamaño del Objetivo (Mínimo)** | AA | Objetivos táctiles de al menos 24x24 px. | Botones principales $> 48\times 48\text{ px}$ en el portal del estudiante y espaciado holgado en tablas. |
| **3.2.6 Ayuda Coherente** | A | Los canales de ayuda deben conservar el mismo orden relativo. | Botones de soporte, perfil y menú en posición idéntica y predecible en todos los portales. |
| **3.3.7 Entrada Redundante** | A | No solicitar reescribir información provista previamente en el mismo trámite. | Registro unificado en 3 pasos: Alumno, Tutor y Aula se vinculan en una sola operación. |
| **3.3.8 Autenticación Accesible (Mínimo)**| AA | No exigir pruebas cognitivas (contraseñas complejas o CAPTCHAs). | Ingreso por PIN numérico de 4 dígitos con pegado de credenciales habilitado. |
| **3.3.9 Autenticación Accesible (Mejorada)**| AAA | Cero esfuerzo mental requerido para iniciar sesión. | Login Asistido por dispositivo confiable: el estudiante ingresa con un toque en su foto. |

---

### 4. Metodología Shift-Left vs. El Engaño de los "Accessibility Overlays"

Ante la presión legal internacional, surgieron soluciones rápidas conocidas como *Accessibility Overlays* o *widgets mágicos* (scripts flotantes que prometen arreglar la accesibilidad automáticamente). 

> [!WARNING]
> **Sanción Histórica de la FTC (1 de Abril de 2025)**: La Comisión Federal de Comercio de EE.UU. multó por **USD 1.000.000** a una empresa proveedora de overlays por publicidad engañosa, sentando el precedente de que los parches cosméticos no corrigen la accesibilidad estructural.

| Aspecto | Parche Cosmético / Widget FLotante | Solución Nativa en InclusiON (*Shift-Left*) |
| :--- | :--- | :--- |
| **Arquitectura** | Parche cosmético superficial sobre el DOM. | Construida de forma nativa desde los cimientos del código. |
| **Normativa** | Rechazada por comités y sancionada por la FTC por engaño. | Conforme a ISO/IEC 40500:2025 y WCAG 2.2. |
| **Dispositivos Táctiles** | Incapaz de adaptar ejercicios para usuarios con temblores. | 5 reproductores interactivos con áreas macro y alternativas táctiles. |
| **Seguridad de Datos** | No cifra ni protege información confidencial. | Diagnósticos clínicos protegidos con cifrado bancario AES-256. |

---

### 5. Diagnóstico Nacional e Internacional de Accesibilidad Web

#### A. Panorama Nacional (Argentina)
* **Ley Nacional 26.653 (2010)** y **Decreto 656/2019**: Marco regulatorio obligatorio para el sector público.
* **Desfasaje Técnico**: La ONTI (Disposición N° 6/19) continúa tomando como parámetro la versión **WCAG 2.0 (2008)**, generando una brecha técnica de casi dos décadas frente al diseño móvil actual.
* **Realidad Universitaria**: Diversos estudios evidencian que gran parte de los campus virtuales de universidades públicas presentan barreras críticas para estudiantes con discapacidad visual.

#### B. Panorama Internacional (Estudio WebAIM Million)
El estudio anual **WebAIM Million** sobre el millón de sitios más visitados reveló que el **95.9% de las webs presentan errores graves de accesibilidad**. Sorprendentemente, el 96% de esos 56 millones de fallos se concentran en apenas **6 descuidos recurrentes**, todos resueltos en InclusiON desde el diseño:

| Error / Problema Recurrente | Prevalencia Global | Solución Implementada en InclusiON |
| :--- | :---: | :--- |
| 1. Texto con bajo contraste | 83.9% | Resuelto mediante Contraste Nivel AAA (17.4:1). |
| 2. Falta de texto alternativo (`alt`) | 53.1% | Resuelto mediante integración forzada de ARASAAC. |
| 3. Formularios sin etiquetas (`label`) | 51.0% | Resuelto con etiquetado semántico visible y explícito. |
| 4. Enlaces vacíos o ambiguos | 46.3% | Resuelto con nombres descriptivos de destino. |
| 5. Botones vacíos o inaccesibles | 30.6% | Resuelto con botones macro y etiquetas aria-label. |
| 6. Omisión del idioma (`lang`) | 13.5% | Resuelto con declaración explícita de idioma español (`lang="es"`). |

---

### 6. Proyección Futura: Preparación para WCAG 3.0 (*Project Silver*)

El W3C trabaja en la especificación **WCAG 3.0**, la cual reemplazará el modelo binario ("Pasa / No Pasa") por un sistema de calificación por niveles (**Bronce, Plata y Oro**), exigiendo métricas objetivas de progreso y auditorías de comportamiento en tiempo real. InclusiON ya cuenta con arquitectura preparada para esta futura norma:

| Requerimiento Futuro WCAG 3.0 | Capacidad Ya Activa en InclusiON |
| :--- | :--- |
| **Evaluación basada en progreso real** | Motor Adaptativo de Dificultad: ajusta el nivel pedagógico al ritmo del estudiante. |
| **Métricas objetivas de avance** | Incorporación de la escala científica *Goal Attainment Scaling* (GAS) para medir progresos (-2 a +2). |
| **Auditoría de eventos de frustración** | Registro automático de tiempos de respuesta, reintentos y alertas preventivas en tiempo real (SignalR). |

---

## 🗣️ Parte IV — Debilidades y Mejoras en Idioma Cliente (Guión de Exposición Oral)

Para explicar las oportunidades de mejora ante un tribunal, clientes o usuarios finales sin recurrir a modismos excesivamente técnicos, se formulan los siguientes puntos en **lenguaje claro, empático y de alto impacto**:

---

### 🔴 1. Falta de Control por Cámara (Seguimiento Ocular / Gestos) para Alumnos con Movilidad Reducida Severa
* **Debilidad (Idioma Cliente)**:
  > *"Hoy en día, si un estudiante tiene parálisis cerebral o una discapacidad motriz severa y no puede mover los brazos ni las manos, se le complica tocar los botones en la pantalla o usar las flechas del teclado. El sistema depende de que el usuario haga algún tipo de toque o clic físico."*
* **Mejora Concreta (Idioma Cliente)**:
  > *"**Conexión y control mediante la Cámara Web**: Integrar una función de seguimiento ocular y gestos faciales usando la cámara de la tablet o computadora. De esta forma, el alumno puede jugar y seleccionar las respuestas guiando el cursor solo con la mirada, parpadeando o abriendo la boca, sin necesidad de tocar físicamente el dispositivo."*
* **Norma WCAG 2.2**: Cumple con **2.1.1 (Teclado/Entradas Alternativas)** y la extensión de **Control por Puntero Guiado**.

---

### 🔴 2. Imposibilidad de Responder o Dictar Consignas por Voz ("Responder Hablando")
* **Debilidad (Idioma Cliente)**:
  > *"El sistema es excelente leyendo las consignas en voz alta para el alumno, pero la comunicación es en un solo sentido: el alumno no puede 'responderle hablándole' a la app. Si a un niño le cuesta pulsar la pantalla pero puede hablar, hoy no tiene esa vía rápida para contestar."*
* **Mejora Concreta (Idioma Cliente)**:
  > *"**Reconocimiento de Voz ('Responder Hablando')**: Agregar un botón de micrófono para que los niños puedan responder las actividades simplemente diciendo la palabra en voz alta (ejemplo: decir 'Gato' o 'Cuatro') y que el sistema procese su respuesta hablada automáticamente."*
* **Norma WCAG 2.2**: Alineado con **3.3.8 (Autenticación e Interacción sin Esfuerzo Cognitivo/Motor)**.

---

### 🔴 3. Los Reportes para Profesores y Médicos se ven más Difíciles en Pantallas de Celular
* **Debilidad (Idioma Cliente)**:
  > *"La parte donde juegan los alumnos tiene botones gigantes y letras super claras. Pero cuando el profesor o el terapeuta entra desde su celular a ver los gráficos o editar actividades, la pantalla se ve más apretada y con botones más pequeños, lo que puede dificultar el uso si el docente también tiene alguna dificultad visual o motriz."*
* **Mejora Concreta (Idioma Cliente)**:
  > *"**Diseño Simplificado Universal para Profesionales**: Llevar los botones grandes, menús simplificados y accesibilidad visual también a las pantallas de los adultos y docentes, para que todo el sistema sea igual de cómodo y accesible sin importar quién lo esté usando."*
* **Norma WCAG 2.2**: Cumple con **2.5.8 (Tamaño de Botones Táctiles)** y **1.4.10 (Adaptación de Pantalla)**.

---

### 🔴 4. Riesgo de que la App se quede "Muda" si el Dispositivo no tiene Voces en Español
* **Debilidad (Idioma Cliente)**:
  > *"La lectura en voz alta depende de lo que tenga instalado el celular o la computadora del usuario. Si es una computadora del colegio antigua que no tiene descargada la voz en español, el sistema no suena o intenta leer en inglés con una voz extraña."*
* **Mejora Concreta (Idioma Cliente)**:
  > *"**Sistema de Voz de Respaldo por Internet**: Si la app detecta que el celular no tiene una voz clara en español, descarga automáticamente un audio preparado desde nuestro servidor para garantizar que las consignas siempre se escuchen perfectas en cualquier aparato."*
* **Norma WCAG 2.2**: Cumple con **3.1.2 (Pronunciación e Idioma Correcto)**.

---

### 🔴 5. Los Gráficos con las Notas y Evolución no se "Leen" en Voz Alta para Personas Ciegas
* **Debilidad (Idioma Cliente)**:
  > *"Los profesores o familiares ciegos que usan un lector de pantalla pueden escuchar todos los textos de la página. Pero al llegar a los gráficos coloridos de barras o tortas que muestran el avance del alumno, el lector de pantalla no logra interpretar ni explicar qué dice el dibujo."*
* **Mejora Concreta (Idioma Cliente)**:
  > *"**Botón 'Escuchar Resumen de Datos' en Gráficos**: Agregar junto a cada gráfico un botón que lo traduzca a un texto explicativo (ejemplo: 'El alumno mejoró un 20% en lectura este mes'), permitiendo que las personas ciegas escuchen el informe completo sin perderse nada."*
* **Norma WCAG 2.2**: Cumple con **1.1.1 (Contenido No Textual)** y **4.1.2 (Compatibilidad con Lectores de Pantalla)**.

---

## 🎯 Conclusión Final

> *"InclusiON no es un prototipo experimental ni un portal educativo común con opciones agregadas por compromiso. Es una solución de ingeniería de software robusta, validada y con propósito humano.*
>
> *Demostramos que es posible construir tecnología en Argentina que no se resigne al atraso regulatorio de 2014, sino que se plantee con orgullo en el estándar internacional más avanzado de la actualidad (WCAG 2.2 e ISO/IEC 40500:2025).*
>
> *Logramos un sistema que le devuelve la autonomía al estudiante para que aprenda sin barreras, le ahorra horas de burocracia al terapeuta facilitándole reportes formales, le brinda tranquilidad cotidiana a la familia y le otorga seguridad jurídica a las instituciones educativas.*
>
> *La verdadera inclusión ocurre cuando la persona no tiene que forzar sus capacidades para adaptarse a la máquina, sino cuando la tecnología es lo suficientemente inteligente y humana para adaptarse a la persona."*
