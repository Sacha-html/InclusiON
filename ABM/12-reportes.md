# ABM — Reportes de Progreso

**Actor principal:** Profesional (crea y envía) · Administrador (aprueba/rechaza) · Familiar (consulta)  
**Justificación:** El Profesional genera reportes formales de avance para documentar el progreso de la persona. Antes de quedar visibles para la familia, los reportes pasan por revisión administrativa. Sin este ABM no hay documentación oficial del proceso pedagógico ni canal formal de comunicación con la familia.

**Entidades:** `Report`

---

## Máquina de estados — Report

```
             POST /api/reports
                    │
                    ▼
             ┌────────────┐
             │   BORRADOR  │  ← Solo estado editable
             │   (Draft)   │
             └─────┬──────┘
                   │ Profesional envía
                   ▼
             ┌─────────────┐
             │   ENVIADO   │
             │ (Submitted) │  ← Solo lectura para todos
             └──────┬──────┘
          ┌─────────┴──────────┐
          │ Admin aprueba       │ Admin rechaza + comentario
          ▼                    ▼
   ┌─────────────┐      ┌─────────────┐
   │  APROBADO   │      │  RECHAZADO  │
   │ (Approved)  │      │ (Rejected)  │
   └─────────────┘      └─────────────┘
          │                    │
   Email → Familiar     Email → Profesional
```

**Reglas de transición:**
- Solo el autor puede enviar (`Borrador → Enviado`)
- Un reporte rechazado **no puede reabrirse** — el profesional crea un nuevo borrador
- La baja lógica está permitida en `Borrador` y `Rechazado`; en `Enviado` y `Aprobado` requiere confirmación

---

## Alta — Reporte de Progreso

**Actor:** Profesional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Persona con discapacidad | Referencia | Sí | Debe existir y estar activa; el profesional debe tenerla asignada |
| Tipo de reporte | Referencia | Sí | Debe existir y estar activo en el catálogo |
| Título | Texto (200) | Sí | No vacío |
| Contenido | Texto largo | Sí | No vacío |
| Fecha del reporte | Fecha | Sí | No puede ser futura |
| Inicio del período | Fecha | No | Debe ser anterior al fin del período |
| Fin del período | Fecha | No | Debe ser posterior al inicio; no puede ser futura |
| Metas alcanzadas | Texto largo | No | — |
| Áreas a reforzar | Texto largo | No | — |
| Recomendaciones futuras | Texto largo | No | — |
| Próximos objetivos | Texto largo | No | — |

**Resultado:** Se crea `Report` con estado `Borrador`. El profesional autenticado queda registrado como autor.

**Post-creación:** El sistema presenta un modal (no cancelable) con dos opciones:
- **"Enviar al administrador"** — transiciona a `Enviado` de inmediato
- **"Revisar después"** — deja el reporte en `Borrador`

---

## Envío al Administrador

**Actor:** Profesional (solo autor)

- Transiciona de `Borrador` → `Enviado`.
- A partir de este punto el reporte es de **solo lectura** para el profesional.
- **Validación:** Solo el autor puede enviar; solo desde estado `Borrador`.

---

## Aprobación

**Actor:** Administrador

- Transiciona de `Enviado` → `Aprobado`.
- **Efecto colateral:** Email automático a todos los familiares activos vinculados a la persona.
- El familiar puede ver el reporte a partir de este momento.

---

## Rechazo

**Actor:** Administrador

- Transiciona de `Enviado` → `Rechazado`.
- **Campo requerido:** Comentario con el motivo (obligatorio).
- **Efecto colateral:** Email automático al profesional autor con el motivo.
- **Regla:** Un reporte rechazado no puede reabrirse. El profesional debe crear un nuevo `Borrador`.

---

## Modificación — Reporte

**Actor:** Profesional (solo autor · solo en estado `Borrador`)

Todos los campos son editables excepto `PersonaConDiscapacidad` y el `Profesional` autor.

| Campo | Validaciones |
|-------|--------------|
| Fecha del reporte | No puede ser futura |
| Inicio/fin del período | `Inicio < Fin`; fin no futuro |
| Resto | Texto libre |

**Restricción:** Si el reporte está en `Enviado`, `Aprobado` o `Rechazado`, la edición devuelve error.

---

## Baja — Reporte

**Actor:** Profesional (solo reportes propios)

- Se establece `Activo = false` (baja lógica).
- **Restricción:** No se puede dar de baja si está en estado `Enviado` (pendiente de revisión).
- **Advertencia:** Si el reporte fue visto (acceso registrado en `AccessAudit`), se muestra aviso antes de confirmar.

---

## Listado — Reportes

### Profesional (ve sus propios reportes)

| Columna | Descripción |
|---------|-------------|
| Título | Título del reporte |
| Tipo | Del catálogo de tipos de reporte |
| Persona | Persona a quien refiere |
| Fecha | Fecha de emisión |
| Estado | Borrador · Enviado · Aprobado · Rechazado |

**Filtros:** tipo, persona, rango de fechas, estado.  
Ordenado por fecha descendente.

### Administrador (ve todos los reportes de su institución)

| Columna | Descripción |
|---------|-------------|
| Título | Título |
| Profesional | Quién lo generó |
| Persona | Destinatario |
| Estado | Todos los estados visibles |
| Fecha | Fecha de emisión |

**Filtros:** estado, profesional, persona, rango de fechas.  
Vista de cola de `Enviados` para revisión pendiente.

### Familiar (solo ve reportes Aprobados)

| Columna | Descripción |
|---------|-------------|
| Título | Título del reporte |
| Tipo | Tipo de reporte |
| Profesional | Quién lo generó |
| Fecha | Fecha de emisión |
| Período cubierto | Desde — hasta |

**Regla de visibilidad:** El familiar solo accede a reportes en estado `Aprobado` de las personas que representa.  
**Filtros:** tipo, rango de fechas.

---

## Visibilidad por estado y actor

| Estado | Profesional (autor) | Administrador | Familiar |
|--------|:-------------------:|:-------------:|:--------:|
| Borrador | Ve y edita | No ve | No ve |
| Enviado | Solo lectura | Ve y decide | No ve |
| Aprobado | Ve | Ve | Ve |
| Rechazado | Ve (con motivo) | Ve | No ve |
