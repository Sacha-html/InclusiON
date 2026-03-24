# HU-10 — Motor de Dificultad Adaptativa (MDA)

**Proceso relacionado:** 11, 13
**Prioridad:** Alta

---

## Historia de Usuario

**Como** profesional
**Quiero** que el sistema ajuste automáticamente la dificultad, tiempo, pistas e intentos de las actividades según el desempeño del estudiante
**Para** mantenerlo en su zona de desarrollo próximo sin frustración ni aburrimiento

**Como** profesional
**Quiero** configurar los rangos del motor adaptativo y ver el historial de ajustes realizados
**Para** personalizar la adaptación según cada persona y tomar decisiones pedagógicas informadas

---

## Descripción funcional

### Ajuste automático
Después de cada actividad completada, el sistema evalúa el rendimiento del estudiante y determina uno de 4 estados:

| Estado | Cuándo se activa | Qué hace el sistema |
|--------|------------------|---------------------|
| **Estable** | Rendimiento consistente | Mantiene los parámetros sin cambios |
| **Progresando** | Varios éxitos consecutivos por encima del umbral | Sube la dificultad, reduce el tiempo, reduce las pistas y los intentos disponibles |
| **Dificultad** | Varios fracasos consecutivos por debajo del umbral | Baja la dificultad, aumenta el tiempo, agrega pistas e intentos |
| **Frustración** | Nivel de frustración alto o 3+ abandonos | Baja todo al mínimo y envía una alerta al profesional |

Los ajustes nunca exceden los rangos configurados por el profesional.

### Configuración por el profesional
Para cada actividad del roadmap, el profesional puede:
- **Activar o desactivar** el motor adaptativo
- **Configurar rangos** de dificultad (mínimo-máximo), tiempo límite (mínimo-máximo)
- **Definir umbrales** de éxitos/fracasos consecutivos para escalar o desescalar, porcentaje de éxito mínimo, umbral de frustración
- **Restaurar valores por defecto**

Si el motor no está configurado o está desactivado, no interviene.

### Historial de ajustes
Cada ajuste realizado por el motor se registra con:
- Tipo de ajuste (escalamiento, desescalamiento, frustración)
- Valores anteriores y nuevos
- Motivo del ajuste
- Fecha y hora

El profesional puede ver este historial en forma de timeline cronológico con colores:
- **Verde** — Escalamiento (progreso)
- **Amarillo** — Desescalamiento (dificultad)
- **Rojo** — Frustración (alerta)

### Búsqueda semántica de actividades
El profesional puede buscar actividades usando lenguaje natural. El sistema encuentra actividades similares por significado semántico, no solo por palabras exactas.

---

## Criterios de Aceptación

### Motor adaptativo
- [ ] Después de cada actividad completada, el sistema evalúa y ajusta automáticamente si el motor está activo
- [ ] Los ajustes nunca exceden los rangos mínimo-máximo configurados por el profesional
- [ ] Si no hay configuración o está desactivada, el sistema no interviene
- [ ] Cada ajuste queda registrado en el historial con tipo, valores y motivo
- [ ] La evaluación y el ajuste se realizan de forma atómica (todo o nada)
- [ ] En estado de frustración se envía una alerta al profesional

### Configuración
- [ ] El profesional puede activar/desactivar el motor por actividad del roadmap
- [ ] Los sliders de rango validan que el mínimo no supere al máximo
- [ ] Se pueden restaurar los valores por defecto con un botón

### Historial
- [ ] El timeline muestra los ajustes en orden cronológico descendente
- [ ] Cada entrada muestra el tipo con color, fecha y descripción legible (ej: "Dificultad subió de 2 a 3")
- [ ] Las alertas de frustración se destacan visualmente con borde rojo
- [ ] Si no hay ajustes, se muestra "El motor aún no ha realizado ajustes"
- [ ] Se puede filtrar por actividad, tipo de ajuste o rango de fechas

### Búsqueda semántica
- [ ] El profesional puede buscar actividades escribiendo en lenguaje natural
- [ ] Los resultados se ordenan por relevancia semántica
- [ ] Si no hay resultados, se muestra un array vacío
