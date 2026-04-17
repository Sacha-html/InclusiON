# HU-08 — Diagnósticos Funcionales y Reportes de Progreso

**Proceso relacionado:** 09, 15
**Prioridad:** Crítica (diagnósticos) / Alta (reportes)

---

## Historia de Usuario

**Como** profesional
**Quiero** registrar diagnósticos funcionales y generar reportes de progreso formales
**Para** documentar la evaluación inicial como base del plan educativo y comunicar avances a la familia de forma estructurada

**Como** administrador
**Quiero** revisar los reportes enviados por los profesionales y aprobarlos o rechazarlos con un comentario
**Para** garantizar la calidad de la información que recibe la familia

**Como** familiar
**Quiero** consultar los reportes de progreso aprobados de mi familiar
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
- **Título y fecha**
- **Contenido** — Texto libre con descripción del progreso
- **Metas alcanzadas**
- **Áreas a reforzar**
- **Recomendaciones futuras**
- **Próximos objetivos**

Los reportes siguen un flujo de aprobación antes de quedar visibles para la familia. El profesional trabaja en un borrador, lo envía al admin, y el admin aprueba o rechaza con comentario. La familia solo accede a reportes aprobados.

---

## Criterios de Aceptación

### Diagnósticos
- [ ] Se pueden registrar múltiples diagnósticos por persona a lo largo del tiempo
- [ ] Solo el creador puede editar su diagnóstico
- [ ] Los diagnósticos de otros profesionales se muestran con indicador "Solo lectura — creado por [nombre]"
- [ ] Solo la fecha y el diagnóstico principal son campos obligatorios
- [ ] El historial se muestra en orden cronológico descendente

### Reportes — Flujo de aprobación
- [x] El profesional puede crear un reporte en estado `Draft`
- [x] El profesional puede editar el reporte solo mientras está en `Draft`
- [x] El profesional puede enviar el reporte al admin (`Draft → Submitted`)
- [x] Un reporte en `Submitted`, `Approved` o `Rejected` no puede editarse (`400 InvalidOperation`)
- [x] El admin puede aprobar un reporte enviado (`Submitted → Approved`)
- [x] El admin puede rechazar un reporte con comentario obligatorio (`Submitted → Rejected`)
- [x] Un reporte rechazado no puede reabrirse; el profesional crea un nuevo `Draft`
- [x] El familiar solo ve reportes `Approved` de sus personas a cargo
- [x] Al aprobar: email a todos los familiares activos vinculados a la persona (background)
- [x] Al rechazar: email al profesional autor con el motivo del rechazo (background)
- [x] Filtros por `dateFrom`, `dateTo` y `reportTypeId` en la vista del familiar
- [x] Al crear un reporte, se muestra un modal preguntando si enviarlo al admin de inmediato o revisarlo primero (backdrop estático, el usuario debe elegir)
- [x] El admin puede ver los reportes de un profesional y aprobar/rechazar directamente desde el tab "Reportes" en el detalle del profesional
- [ ] Los reportes se pueden exportar a PDF
- [ ] Los reportes nuevos se marcan con indicador "Nuevo" que desaparece al abrirlo

---

## Visibilidad por actor

| Estado | Profesional | Admin | Familiar |
|--------|:-----------:|:-----:|:--------:|
| `Draft` | Ve y edita | No ve | No ve |
| `Submitted` | Solo lectura | Ve y decide | No ve |
| `Approved` | Ve | Ve | Ve |
| `Rejected` | Ve (con motivo) | Ve | No ve |
