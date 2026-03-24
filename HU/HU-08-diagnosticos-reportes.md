# HU-08 — Diagnósticos Funcionales y Reportes de Progreso

**Proceso relacionado:** 09, 15
**Prioridad:** Crítica (diagnósticos) / Alta (reportes)

---

## Historia de Usuario

**Como** profesional
**Quiero** registrar diagnósticos funcionales y generar reportes de progreso formales
**Para** documentar la evaluación inicial como base del plan educativo y comunicar avances a la familia de forma estructurada

**Como** familiar
**Quiero** consultar y descargar los reportes de progreso de mi familiar
**Para** conocer su avance sin depender de reuniones presenciales

---

## Descripción funcional

### Diagnósticos Funcionales
El profesional registra diagnósticos desde el perfil de la persona:
- **Fecha del diagnóstico**
- **Diagnóstico principal** (requerido)
- **Observaciones iniciales**
- **Capacidades identificadas**
- **Desafíos identificados**
- **Apoyos requeridos**
- **Objetivos pedagógicos**
- **Estrategias recomendadas**

Se pueden registrar múltiples diagnósticos a lo largo del tiempo, formando un historial cronológico (timeline). Solo el profesional que creó un diagnóstico puede editarlo; los demás profesionales pueden consultarlo en modo solo lectura.

### Reportes de Progreso
El profesional genera reportes formales para un período determinado:
- **Tipo de reporte** (del catálogo del sistema)
- **Título y período** (fecha inicio - fecha fin)
- **Contenido** — Texto libre con descripción del progreso
- **Metas alcanzadas**
- **Áreas a reforzar**
- **Recomendaciones futuras**
- **Próximos objetivos**

Los reportes son visibles tanto para el profesional como para la familia. Se pueden exportar a PDF.

---

## Criterios de Aceptación

### Diagnósticos
- [ ] Se pueden registrar múltiples diagnósticos por persona a lo largo del tiempo
- [ ] Solo el creador puede editar su diagnóstico
- [ ] Los diagnósticos de otros profesionales se muestran con indicador "Solo lectura — creado por [nombre]"
- [ ] Solo la fecha y el diagnóstico principal son campos obligatorios
- [ ] El historial se muestra en orden cronológico descendente

### Reportes
- [ ] El profesional puede crear reportes seleccionando tipo, período y completando campos de texto
- [ ] La familia puede ver los reportes de su familiar pero no crear ni editar
- [ ] Los reportes se pueden exportar a PDF
- [ ] Los reportes nuevos se marcan con indicador "Nuevo" que desaparece al abrirlo
- [ ] El tipo de reporte se selecciona del catálogo del sistema
