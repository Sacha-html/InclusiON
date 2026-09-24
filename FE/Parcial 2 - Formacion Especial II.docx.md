# Trabajo Práctico de Investigación y Aplicación

**2do Parcial – Normas y Modelos de Calidad de Software**

*Norma elegida: WCAG 2.2 (Pautas de Accesibilidad para el Contenido Web)*

Sistema analizado: InclusiON (proyecto de tesis)

# Parte I – Investigación

## 1\. Nombre completo de la norma

Se llama Web Content Accessibility Guidelines 2.2, o sea "Pautas de Accesibilidad para el Contenido Web 2.2". Todos la conocen por su sigla: WCAG 2.2.

## 2\. Organismo que la creó

La creó el W3C (World Wide Web Consortium), que es el organismo que define casi todos los estándares que usamos en internet. Dentro del W3C hay un grupo llamado WAI que se dedica específicamente a la accesibilidad, y es el que arma esta norma.

## 3\. Año de publicación

Salió en octubre de 2023\. Antes hubo otras versiones: la 2.0 (2008) y la 2.1 (2018). No se reemplazan entre sí, se van sumando: si cumplís la 2.2, ya estás cumpliendo también las anteriores.

## 4\. Objetivo principal

Que cualquier persona pueda usar una página o app web, tenga o no alguna discapacidad. Para eso da reglas concretas sobre cómo tienen que estar hechos los textos, las imágenes, los botones, los formularios, etc.

## 5\. Problema que intenta resolver

Muchas webs se hicieron pensando solo en un usuario "típico" y se olvidaron de la gente con discapacidad. Algunos ejemplos de barreras comunes:

* Fotos sin ningún texto que las describa, entonces un lector de pantalla no puede "leerlas".

* Páginas que solo funcionan con mouse y no se pueden manejar con teclado.

* Letras con muy poco contraste, difíciles de leer.

* Videos sin subtítulos.

WCAG 2.2 pone reglas claras para que estas cosas no pasen.

## 6\. Ámbito de aplicación

Se aplica a páginas web, apps y contenido multimedia en general. En España es obligatoria para el Estado (organismos públicos) hace rato, y desde 2025 también empieza a ser obligatoria para bancos, seguros, tiendas online y otras empresas privadas. En otros países pasa algo parecido, cada uno con su propia ley basada en WCAG.

## 7\. Principales conceptos

Algunas ideas para entender cómo funciona la norma:

* 4 principios (se los conoce como POUR): el contenido tiene que ser Perceptible, Operable, Comprensible y Robusto. Básicamente: que se pueda percibir, usar, entender y que funcione en distintos dispositivos.

* Niveles A, AA y AAA: A es lo mínimo, AA es lo que normalmente se pide (y lo que exige la ley), y AAA es el nivel más alto, difícil de cumplir al 100%.

* Criterios de éxito: son las reglas puntuales, tipo "las imágenes tienen que tener texto alternativo" o "todo se tiene que poder usar con teclado".

* Lo nuevo de la 2.2: se agregaron 9 reglas nuevas, por ejemplo que el arrastrar y soltar (drag and drop) siempre tenga una forma alternativa de hacerse sin arrastrar, y que el login no dependa solo de acertijos o de memorizar cosas.

## 8\. Ventajas

* Son reglas concretas, se puede chequear si se cumplen o no.

* Mejora la experiencia para todos, no solo para gente con discapacidad.

* Evita problemas legales, porque en varios lugares es obligatoria.

* Ayuda al posicionamiento en Google (SEO).

## 9\. Desventajas

* Puede llevar más tiempo y trabajo si no se piensa desde el principio del proyecto.

* El nivel más alto (AAA) es difícil de cumplir del todo.

* No alcanza con un test automático, a veces hay que revisarlo a mano o probarlo con usuarios reales.

## 10\. Ejemplos de organizaciones que la utilizan

* Organismos públicos de España y de la Unión Europea (es obligación para ellos).

* Bancos, seguros y tiendas online, que ahora también están obligados.

* Empresas grandes como Microsoft, Google y Apple, que la usan como referencia en sus productos.


## 11\. Porque consideramos que es importante esta norma

Las WCAG 2.2 garantizan el derecho básico y universal del acceso equitativo a la información digital, más en un contexto de hiperglobalización donde vemos que cada vez la educación, el empleo, los trámites, etc están completamente digitalizados, un software inaccesible funciona como un mecanismo directo de exclusión social y económica. Esta normativa es de vital importancia porque no solo establece buenas prácticas a nivel de código sino también establece cómo debe establecerse el diseño universal en métricas verificables buscando abarcar y proteger a usuarios con discapacidades cognitivas, visuales y motoras que dependen de interfaces inclusivas para ejercer autonomía.

## 12\. ¿Qué riesgos existirían si no se aplicara? 

Si una organización decide ignorar esta norma, se enfrenta a múltiples riesgos críticos:

**Riesgo institucional y pérdida de contratos (Ley 26.653)**: Si la organización es proveedora del Estado, concesionaria de servicios públicos o una ONG que recibe financiamiento estatal, el incumplimiento de las normativas técnicas de la ONTI (como la Disposición N° 6/19) puede inhabilitarla para renovar licitaciones, cancelar contratos vigentes o suspender la recepción de subsidios nacionales.

**Riesgo de litigios y sanciones por Defensa del Consumidor:** Bajo la Ley 24.240 de Defensa del Consumidor, una interfaz inaccesible impide que los usuarios con discapacidad comprendan los términos y condiciones. Si la información no es "veraz, clara y suficiente" (Art. 4°) o la plataforma incurre en exclusión, la empresa queda expuesta a denuncias que acarrea severas multas económicas (Art. 47\) impuestas por la Dirección Nacional de Defensa del Consumidor, además de la obligación de publicar la resolución sancionatoria en su contra.

**Riesgo social y reputacional**: Daña la imagen pública de la marca al perpetuar barreras digitales, marginando activamente a millones de usuarios, incluyendo a personas mayores y personas con discapacidades.

**Riesgos por discriminación e incumplimiento sectorial**: Los usuarios excluidos podian radicar denuncias formales por discriminación ante organismos como el INADI. Asimismo, en sectores regulados como el financiero, el incumplimiento de las exigencias de accesibilidad digital dictadas por el Banco Central de la República Argentina (BCRA) acarrea graves sanciones operativas para los bancos o billeteras virtuales.

## 13\. ¿Qué beneficios aporta a una organización?

**Blindaje legal e institucional:** Asegura el cumplimiento irrestricto de las leyes nacionales (como la Ley 26.653), mitigando cualquier riesgo de litigio por discriminación o demandas bajo la Ley de Defensa del Consumidor, y garantiza la elegibilidad continua para participar en contrataciones públicas.

**Ampliación del mercado:** Permite capturar y fidelizar a un segmento demográfico con gran poder adquisitivo que frecuentemente es ignorado o excluido por la competencia.

**Mejora de la Experiencia de Usuario (UX):** El diseño universal exigido por WCAG 2.2 no solo ayuda a las personas con discapacidad, sino que reduce la frustración táctil en celulares y disminuye la carga cognitiva para absolutamente todos los usuarios.

**Optimización técnica y SEO:** La obligación de utilizar código HTML semántico, etiquetas correctas y textos alternativos facilita que los motores de búsqueda interpreten e indexen mejor el contenido, lo que mejora significativamente el posicionamiento orgánico del sistema.

## Introducción y contexto normativo (W3C / WAI)

La accesibilidad web no es solo un detalle visual o algo opcional: es la base para que cualquier sistema se pueda usar bien. Si una página o app no es accesible, deja afuera a muchísima gente. En el caso de InclusiON, que trabaja en el área educativa y terapéutica, una barrera digital le quita a las personas con discapacidad la posibilidad de aprender, comunicarse y manejarse de forma autónoma.

Para ordenar todo esto y medirlo bien, el W3C (a través de su grupo WAI) armó la norma WCAG. Esta pauta se apoya en 4 principios básicos conocidos como POUR: Perceptible, Operable, Comprensible y Robusto.

En InclusiON no aplicamos estas reglas como un arreglo de último momento, sino que se integran en todo el desarrollo (tanto en la pantalla que ve el usuario como en la parte interna del sistema) para que la experiencia sea completa:

| Perceptibilidad(Lo que se ve y oye) | Operabilidad(Lo que se toca) | Comprensibilidad(Lo que se entiende) | Robustez(Lo que asegura el sistema) |
| :---- | :---- | :---- | :---- |
| 7 perfiles visuales conmutables Contraste AAA 17.4:1 Pictogramas ARASAAC Lector de voz TTS (Web Speech API) | Botones macro \> 48x48 px Alternativa de botones ▲ y ▼ Panel rápido con Alt \+ A | Carga cognitiva reducida "Mi Camino" en nodos (Duolingo) Login por PIN (4 dígitos) y Asistido | HTML5 semántico nativo Etiquetas ARIA invisibles para lectores de pantalla Navegación rápida en cualquier dispositivo Cifrado médico AES-256 |

## Aspectos observables y verificables en pantalla por el usuario y los evaluadores

## 1\. Perceptibilidad (información disponible por distintas vías sensoriales)

* Selector con 7 perfiles de diseño visual (permite hasta 14 variaciones combinando tonos claros y oscuros):

  * Modo Estándar: vista limpia, moderna y armónica.

  * Modo Alto Contraste: fondo oscuro pleno con tipografía en tonos amarillos y blancos de gran nitidez, ideal para usuarios con disminución visual severa.

  * Modo Dislexia: fuente adaptada con mayor densidad en la base de los caracteres (para evitar que las letras parezcan girar) y un espacio entre palabras más amplio.

  * Modo Visión Reducida: incrementa automáticamente el tamaño tipográfico a 18px manteniendo el orden de los elementos.

  * 3 Variantes para Daltonismo: combinaciones pensadas para Protanopía (dificultad para el rojo), Deuteranopía (dificultad para el verde) y Tritanopía (dificultad para el azul).

* Contraste de nivel elevado AAA (17.4 a 1):

  * Mientras las pautas habituales exigen una proporción de 4.5:1, InclusiON llega a 17.4:1 en el texto principal y 8.6:1 en los botones principales, algo que se puede comprobar fácilmente con un medidor de color.

* Uso de la librería visual ARASAAC:

  * Incluye imágenes claras acompañadas por su texto aclaratorio en español, lo que facilita que los estudiantes que no leen con fluidez comprendan las tareas sin problemas.

* Lector por sintetizador de voz (TTS):

  * Al tocar la opción "Hablar" o los íconos de reproducción de cada consigna, la plataforma lee las indicaciones en español directamente, sin instalar aplicaciones extra.

## 2\. Operabilidad (manejo cómodo y sin trabas de navegación)

* Botones amplios con superficie de clic superior a 48x48 px:

  * Los bloques principales de la pantalla del alumno ("Mi camino", "Actividades", "Hablar") tienen un tamaño holgado mayor a 120 px de alto y colores diferenciados, evitando toques involuntarios en personas con temblores o dificultades motrices.

* Sustituto para actividades de arrastrar usando botones ▲ y ▼:

  * Para ordenar secuencias de pasos, no es necesario mantener apretado y desplazar el ratón. Se puede hacer lo mismo usando simples flechas de subir y bajar.

* Acceso directo mediante teclado (Alt \+ A):

  * Al pulsar Alt \+ A, aparece en un costado las opciones de accesibilidad para ajustar el contraste o el texto rápidamente sin perder el trabajo en curso.

* Resaltado de selección en azul de 3 px:

  * Al navegar usando la tecla Tab, una marca azul muy visible (relación 8.6:1) ubica en qué lugar estamos parados, ajustando la vista para que la barra superior no tape los elementos.

## 3\. Comprensibilidad (estructura clara y menor esfuerzo mental)

* Recorrido por pasos "Mi Camino" (aprendizaje por niveles):

  * Organiza la cursada como una ruta con paradas consecutivas (parecido a Duolingo). El usuario se concentra únicamente en la consigna actual, evitando la confusión que generan los listados extensos.

* Formas de ingreso simplificadas (sin claves difíciles):

  * Acceso con PIN: teclado de números amplio en pantalla para colocar 4 dígitos sencillos.

  * Acceso Acompañado: el tutor o docente valida el equipo una sola vez y el alumno entra seleccionando su foto o nombre, sin redactar contraseñas.

* Alta integrada en tres pasos:

  * El proceso de inscripción resuelve el registro del Alumno, la asociación del Tutor y la asignación del Grupo educativo en una misma pantalla, previniendo la carga repetida de datos.

* Avisos de apoyo pedagógico (para cuidar la motivación):

  * Ante varios intentos fallidos, el sistema no muestra mensajes molestos de error: hace una pausa en el juego y le manda un aviso directo al profesor para que se acerque a ayudarlo.

## 4\. Robustez (compatibilidad duradera y protección de la información)

* Estructura semántica HTML5 e indicativos ARIA:

  * Permite que los softwares asistivos para personas con discapacidad visual (como NVDA o TalkBack) transmitan las secciones, avisos y botones en el orden correcto.

* Resguardo seguro para informes de salud:

  * Las notas clínicas y diagnósticos se guardan con encriptación fuerte (AES-256), protegiendo la privacidad del paciente y respondiendo a las leyes de protección de datos personales.

* Generación directa de documentos impresos o PDF:

  * Creación de reportes de avance organizados y estandarizados, listos para guardar o imprimir para las familias e instituciones.

HASTA ACA DEBERIA EXPONER UNO 

---

\-Las pautas para la accesibilidad van cambiando con el tiempo. En octubre de 2023, el W3C dio un avance clave al lanzar **WCAG 2.2**, estándar que para 2025 logró la máxima categoría internacional transformándose en la norma oficial **ISO/IEC 40500:2025**.

Esta nueva versión incluyó un cambio muy importante: **quitaron el criterio 4.1.1 de análisis sintáctico**. La razón es simple: actualmente los navegadores solucionan solos las fallas menores de marcado. Ya no vale la pena perder tiempo revisando minuciosidades de código cuando la meta principal es asegurar una buena experiencia al usuario.

Por este motivo, la versión 2.2 sumó **9 pautas de cumplimiento obligatorio**, apuntadas a resolver dos puntos pendientes clave: **bajar el esfuerzo de carga cognitiva** y **facilitar el uso en dispositivos táctiles**.

En InclusiON no hizo falta hacer cambios grandes para estar al día, ya que la plataforma se diseñó respetando estas 9 reglas desde el comienzo.

Lo que la pantalla proyecta:

| Criterio WCAG 2.2 | Nivel | Exigencia del Estándar Internacional | Cómo se Demuestra en InclusiON |
| :---- | :---- | :---- | :---- |
| 2.4.11 Foco No Oculto (Mínimo) | AA | El cursor de teclado no debe quedar tapado por barras fijas o cabeceras. | La pantalla mantiene un margen superior automático de 80 px; el menú nunca tapa el botón seleccionado. |
| 2.4.12 Foco No Oculto (Mejorado) | AAA | Cero porcentaje de ocultamiento en cualquier diálogo o ventana. | Las ventanas modulares confinan el foco (focus-trap), garantizando 100% de visibilidad del elemento activo. |
| 2.4.13 Apariencia del Foco | AAA | Indicador de foco de al menos 2px de grosor y contraste mínimo de 3:1. | InclusiON traza un marco azul grueso de 3 píxeles con un contraste sobresaliente de 8.6 a 1\. |
| 2.5.7 Movimientos de Arrastre | AA | Las acciones de arrastre (drag & drop) deben ofrecer alternativa con un clic. | En actividades de secuencia temporal, se incluyen botones ▲ y ▼ para mover tarjetas con un toque simple. |
| 2.5.8 Tamaño del Objetivo (Mínimo) | AA | Objetivos táctiles de al menos 24x24 px para evitar pulsaciones erróneas. | Botones gigantes mayores a 48x48 px en el portal del alumno y espaciados amplios en tablas. |
| 3.2.6 Ayuda Coherente | A | Los canales de ayuda deben estar siempre en el mismo orden relativo. | Los botones de soporte, perfil y menú conservan una posición idéntica y predecible en todos los portales. |
| 3.3.7 Entrada Redundante | A | No exigir reescribir información provista previamente en el mismo trámite. | Registro Unificado en 3 Pasos: Alumno, Tutor y Aula se vinculan en una sola operación sin tipear datos dos veces. |
| 3.3.8 Autenticación Accesible (Mínimo) | AA | No exigir pruebas cognitivas (sin contraseñas complejas ni CAPTCHAs). | Acceso por PIN de 4 números y soporte total para gestores de contraseñas con pegado habilitado. |
| 3.3.9 Autenticación Accesible (Mejorada) | AAA | Cero esfuerzo mental requerido para iniciar sesión. | Login Asistido con Dispositivo Confiable: la tablet queda autorizada y el alumno entra tocando su perfil. |

---

Ante las exigencias legales a nivel global, numerosas organizaciones optaron por soluciones rápidas conocidas como "Accessibility Overlays". Se trata de accesos directos o íconos en pantalla que aseguran resolver la accesibilidad de una web agregando un script simple.

Sin embargo, esto genera una falsa sensación de seguridad. Resulta inadecuado corregir problemas estructurales de base: un complemento de este tipo no logra organizar la jerarquía de un formulario, no asigna sentido a un pictograma educativo ni garantiza el resguardo de la información clínica de un usuario.

La falta de validez de estos complementos se hizo evidente en **abril de 2025**, momento en que la FTC (Comisión Federal de Comercio estadounidense) le impuso una sanción de **1 millón de dólares** a una empresa proveedora por información engañosa, remarcando que el cumplimiento real exige intervenir directamente la arquitectura base del software.

Por esta razón, en InclusiON utilizamos la metodología **Shift-Left**: no dejamos la accesibilidad para la etapa final, sino que la contemplamos de forma integral durante todo el proceso de desarrollo.

**Lo que proyectamos en pantalla:**

| El engaño del widget o botón mágico | La solución nativa de InclusiON |
| :---- | :---- |
| Es un parche cosmético que se cae con facilidad. La FTC multó a sus fabricantes por $1.000.000 de dólares en 1 abr 2025 por engaño. No puede adaptar ejercicios táctiles para niños con temblores o dificultades motoras. No cifra ni protege información confidencial. | Accesibilidad construida desde los cimientos. Código estructurado bajo normativas oficiales internacionales (ISO/IEC 40500:2025). 5 reproductores interactivos con áreas macro y alternativas táctiles de un solo toque. Diagnósticos clínicos protegidos con cifrado de grado bancario (AES-256 en reposo). |

¿Qué pasa en nuestro país? Acá tenemos la **Ley Nacional 26.653 de Accesibilidad de la Información Web**, que se aprobó en 2010 y se reglamentó mediante el Decreto 656 en 2019\. El problema es que hay un desfasaje técnico bastante grave: las resoluciones de la ONTI siguen exigiendo las WCAG 2.0 de 2008\. O sea, nos estamos manejando con pautas de hace casi veinte años, diseñadas cuando todavía no se usaban celulares táctiles ni tablets.

Varios estudios universitarios del último tiempo muestran que las plataformas virtuales de las facultades públicas y las webs oficiales presentan muchísimas trabas, lo que impide que los alumnos ciegos o con problemas de visión puedan estudiar de forma independiente.

Afuera el panorama tampoco es muy distinto. En **1 feb 2026**, el relevamiento internacional **WebAIM Million** revisó el millón de sitios más usados y encontró que el **95.9% de las páginas web del mundo tienen errores graves de accesibilidad**. Esto pasa porque se programa muy rápido con sistemas automáticos y no se revisa la parte funcional.

Lo curioso es que **el 96% de esos 56 millones de fallos guardan relación con apenas 6 descuidos típicos**, cosas que en InclusiON resolvimos desde el primer momento.

| Error / Problema | Prevalencia en sitios | Mitigación en InclusiON |
| :---- | :---- | :---- |
| 1\. Texto con bajo contraste | 83.9% de sitios | Resuelto en InclusiON (AAA 17.4:1) |
| 2\. Falta de texto alternativo (alt) en imágenes | 53.1% de sitios | Resuelto en InclusiON (ARASAAC forzado) |
| 3\. Formularios sin etiquetas (label) | 51.0% de sitios | Resuelto en InclusiON (Etiquetado visible) |
| 4\. Enlaces vacíos sin texto | 46.3% de sitios | Resuelto en InclusiON (Nombres claros) |
| 5\. Botones vacíos no operables | 30.6% de sitios | Resuelto en InclusiON (Botones accesibles) |
| 6\. Omisión del idioma del documento (lang) | 13.5% de sitios | Resuelto en InclusiON (Español declarado) |

Mirando hacia el futuro, el W3C ya está preparando WCAG 3.0 (conocido como 'Project Silver'). El nuevo estándar abandonará el simple 'Pasa o No Pasa' para calificar a los sistemas en niveles Bronce, Plata y Oro, exigiendo para los niveles superiores pruebas reales documentadas con personas con discapacidad.  
InclusiON ya está preparado para este estándar del futuro gracias a su Motor de Dificultad Adaptativa.  
Si un estudiante con autismo o discapacidad cognitiva falla tres veces consecutivas en una actividad, el sistema no lo castiga con un sonido estridente ni una pantalla roja. Amablemente detiene la actividad, adapta la complejidad reduciendo distractores y le emite una alerta pedagógica silenciosa al profesional para que intervenga a tiempo.  
Cada respuesta, tiempo empleado y nivel de frustración queda auditado bajo la escala científica Goal Attainment Scaling (GAS), generando reportes automáticos en formato estándar A4 listos para imprimir para obras sociales o ministerios.

| Requerimientos Futuros de WCAG 3.0 | Capacidades Ya Activas en InclusiON |
| :---- | :---- |
| Evaluación basada en el progreso real del estudiante (no solo en el código estático). | Motor Adaptativo de Dificultad: ajusta el nivel automáticamente según el ritmo de cada persona. |
| Presentación de métricas objetivas de alcance de objetivos terapéuticos y pedagógicos. | Incorporación de la escala Goal Attainment Scaling (GAS) para medir avances de \-2 a \+2. |
| Auditoría de incidentes de estrés o dificultad durante el uso de la plataforma. | Registro automático de tiempos de respuesta, intentos y alertas de frustración en tiempo real. |

## Debilidades del sistema

* En primer lugar, hoy en día si un estudiante tiene parálisis cerebral o una discapacidad motriz severa y no puede  mover los brazos ni las manos, se le complica jugar o contestar porque la plataforma depende de que toque la pantalla o use el teclado. Para resolver esto, la mejora concreta es integrar el control mediante la cámara web de  la tablet o computadora, rastreando la mirada y los gestos de la cara para que el alumno pueda guiar el juego y contestar las actividades simplemente mirando la pantalla, parpadeando o abriendo la boca sin necesidad de tocar nada.  
    
*   Por otro lado, aunque el sistema es excelente leyendo las consignas en voz alta para el estudiante, la comunicación actual es en un solo sentido ya que el alumno no puede responderle hablándole a la  aplicación si le cuesta usar las manos. La mejora propuesta es incorporar reconocimiento de voz para responder hablando, agregando un botón de micrófono que le permita a los niños contestar las consignas simplemente diciendo la palabra o la respuesta en voz alta y que el sistema la procese automáticamente.  
    
*  Además, existe el riesgo de que la lectura en voz alta falle si la computadora o el celular del usuario es antiguo y no tiene instalada la voz en español, provocando que la aplicación no suene o intente leer en inglés con una voz extraña. La solución propuesta es implementar un sistema de voz de respaldo por internet, de modo que si la aplicación detecta que el aparato no tiene una voz clara en español, descargue automáticamente el audio preparado desde nuestro servidor para garantizar que las consignas siempre se escuchen perfectas en cualquier equipo.  
    
*   Finalmente, los profesores o familiares ciegos que utilizan lectores de pantalla pueden escuchar todos los textos del sistema, pero al llegar a los gráficos coloridos de barras o tortas que muestran la evolución del estudiante, el lector de pantalla no logra explicar qué dice el dibujo. Para solucionar esto, proponemos agregar junto a cada gráfico un botón para escuchar el resumen de datos, el cual traduce el gráfico a un texto simple explicativo com por ejemplo que el alumno mejoró un veinte por ciento en lectura este mes, permitiendo que las personas ciegas escuchen el informe completo sin perderse ningún detalle.


  

Propuesta de mejora: 

  Para alinearse plenamente con la norma y llevar el sistema al siguiente nivel, proponemos cinco mejoras concretas de gran impacto. La primera mejora es la integración de control por cámara web mediante seguimiento ocular y gestos faciales, lo que permitirá que los estudiantes con parálisis cerebral o motricidad muy reducida puedan guiar el juego y contestar las actividades simplemente mirando la pantalla, parpadeando o abriendo la boca, sin necesidad de tocar la pantalla ni usar el teclado. La segunda mejora consiste en incorporar reconocimiento de voz para responder hablando, agregando un botón de micrófono para que los niños puedan contestar las consignas simplemente diciendo la palabra en voz alta y que el sistema procese su respuesta hablada automáticamente. La tercera mejora busca llevar el diseño simplificado con botones gigantes y accesibilidad visual también a los portales de los profesores y médicos, logrando que los docentes con alguna dificultad motriz o visual puedan revisar informes y cargar datos de forma igual de fácil desde cualquier celular. La cuarta mejora establece un sistema de voz de respaldo por internet que, al detectar que la computadora o el celular del usuario no tiene instalada una voz clara en español, descarga automáticamente el audio preparado desde nuestro servidor para garantizar que las consignas siempre se escuchen perfectas en cualquier aparato. La quinta mejora propone agregar un botón para escuchar el resumen de datos junto a cada gráfico de evolución, traduciendo las barras y tortas a un texto claro explicativo para que las personas ciegas que usan lectores de pantalla puedan escuchar el informe completo de progreso sin perderse ningún detalle.

Para finalizar:  
InclusiON no es un prototipo experimental ni un portal educativo común con opciones agregadas por compromiso. Es una solución de ingeniería de software robusta, validada y con propósito humano.  
Demostramos que es posible construir tecnología en Argentina que no se resigne al atraso regulatorio de 2014, sino que se plante con orgullo en el estándar internacional más avanzado de la actualidad (WCAG 2.2 e ISO/IEC 40500:2025).  
Logramos un sistema que le devuelve la autonomía al estudiante para que aprenda sin barreras, le ahorra horas de burocracia al terapeuta facilitándole reportes formales, le brinda tranquilidad cotidiana a la familia y le otorga seguridad jurídica a las instituciones educativas.  
La verdadera inclusión ocurre cuando la persona no tiene que forzar sus capacidades para adaptarse a la máquina, sino cuando la tecnología es lo suficientemente inteligente y humana para adaptarse a la persona.  
Muchas gracias. Quedamos a disposición del tribunal para responder sus preguntas.  
