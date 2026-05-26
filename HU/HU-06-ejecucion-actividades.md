# HU-06 — Ejecución de Actividades

**Proceso relacionado:** 12
**Prioridad:** Crítica

---

## Historia de Usuario

**Como** persona con discapacidad
**Quiero** realizar actividades interactivas con retroalimentación visual y que el sistema registre mi progreso automáticamente
**Para** aprender de forma motivadora y que mi avance desbloquee nuevas actividades

---

## Descripción funcional

Cuando la persona selecciona una actividad desbloqueada de su roadmap, el sistema:

1. **Carga la actividad** con todo su contenido según el tipo de plantilla
2. **Inicia el registro** — Marca el momento de inicio y el primer intento
3. **Presenta el player correspondiente** según el tipo de actividad:

| Tipo | Descripción |
|------|-------------|
| Selección de figuras | Se muestra una instrucción con audio y una grilla de opciones con pictogramas. Feedback verde/rojo al seleccionar. |
| Suma visual | Pictogramas animados representando cantidades + botones numéricos con distractores. |
| Emparejar imagen-palabra | Pictogramas a la izquierda y palabras mezcladas a la derecha. La persona une cada par. |
| Ordenar secuencia | Cards arrastrables que la persona ordena + botón "Verificar". |
| Completar letra | Pictograma + palabra con un espacio en blanco + botones con letras para elegir. |

4. **Registra el progreso** durante la ejecución: intentos, nivel de frustración y patrones de respuesta
5. **Evalúa el resultado** al completar: calcula el porcentaje de éxito
6. **Desbloquea la siguiente actividad** si el porcentaje de éxito supera el umbral definido en el roadmap

### Monitoreo de frustración
Si la persona acumula más de 3 intentos fallidos, el sistema muestra una pausa con estímulo positivo e incrementa el nivel de frustración registrado.

---

## Criterios de Aceptación

- [x] La actividad se carga completa en una sola pantalla sin necesidad de navegación adicional
- [x] El tipo de actividad se determina automáticamente según la plantilla, sin que la persona tenga que elegir
- [x] Si el tipo de actividad no es reconocido, el sistema muestra un mensaje de error amigable
- [x] Durante la ejecución se registran los intentos, tiempos y patrones de respuesta
- [x] Al completar, el sistema evalúa el resultado y lo compara con el umbral de desbloqueo
- [x] Si el resultado supera el umbral, la siguiente actividad del roadmap se desbloquea automáticamente
- [x] Si no supera el umbral, la actividad se marca como completada pero no se desbloquea la siguiente
- [x] La persona solo ve sus propias actividades asignadas
- [x] La interfaz respeta el perfil de accesibilidad de la persona
- [x] Las animaciones respetan la preferencia de movimiento reducido
- [x] Se soporta audio mediante síntesis de voz
