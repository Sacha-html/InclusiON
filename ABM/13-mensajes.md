# ABM — Mensajes Internos

**Actor:** Profesional / Representante Familiar  
**Justificación:** La comunicación entre el Profesional y la familia es parte esencial del acompañamiento de la persona con discapacidad. El Profesional necesita informar a los familiares sobre el progreso, solicitar información o coordinar acciones. El Familiar necesita reportar situaciones del hogar que afectan el proceso. Sin este ABM, la única vía de comunicación son medios externos (WhatsApp, email personal), que no están integrados al expediente de la persona ni son trazables.

**Entidades:** `Message`

---

## Alta — Mensaje

**Actor:** Profesional / Representante Familiar

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Destinatario | Referencia (`User`) | Sí | Debe existir y estar activo |
| Persona relacionada | Referencia | No | Debe existir y estar activa si se ingresa; el remitente debe tener acceso a esa persona |
| Asunto | Texto (200) | No | — |
| Contenido | Texto largo | Sí | No vacío |
| Mensaje padre | Referencia | No | Debe existir y estar activo (para responder en hilo) |

**Validaciones de integridad:**
- Un Profesional solo puede enviar mensajes a Familiares vinculados a personas bajo su cargo, o a otros Profesionales de su institución.
- Un Familiar solo puede enviar mensajes a Profesionales vinculados a la persona que representa.
- Si se especifica `MensajePadre`, el destinatario debe ser el mismo que el remitente del mensaje padre (o viceversa).

**Resultado:** Se crea `Message` con `Leido = false`, `FechaEnvio = now()`, `Activo = true`.

---

## Baja — Mensaje

**Actor:** No aplica ABM de usuario para baja de mensajes.

Los mensajes no se eliminan. Son parte del registro de comunicación. `Activo = true` permanente. Si en el futuro se requiere archivado, se implementará como estado adicional.

---

## Modificación — Mensaje

Los mensajes no son editables una vez enviados (integridad del registro de comunicación).

---

## Marcar como Leído (operación del sistema)

Cuando el destinatario abre el mensaje, el sistema actualiza automáticamente:
- `Leido = true`
- `FechaLectura = now()`

Esta operación no es un ABM de usuario directo.

---

## Listado — Bandeja de Entrada

**Actor:** Profesional / Representante Familiar

| Columna | Descripción |
|---------|-------------|
| Remitente | Nombre y apellido |
| Asunto | Asunto del mensaje |
| Persona relacionada | Nombre de la persona (si aplica) |
| Fecha de envío | Cuándo se envió |
| Leído | Sí / No |

Ordenado por fecha de envío descendente. Los no leídos aparecen primero.

**Filtros:** leído/no leído, persona relacionada, remitente.

---

## Listado — Mensajes Enviados

**Actor:** Profesional / Representante Familiar

| Columna | Descripción |
|---------|-------------|
| Destinatario | Nombre y apellido |
| Asunto | Asunto del mensaje |
| Persona relacionada | Nombre de la persona (si aplica) |
| Fecha de envío | Cuándo se envió |
| Fue leído | Sí / No |

---

## Vista de Hilo

Los mensajes con `MensajePadre` se agrupan en hilos de conversación. Un hilo muestra todos los mensajes con el mismo ancestro raíz, ordenados cronológicamente.

**Persistencia:** Consulta a `Message` filtrado por `DestinatarioId = usuarioActual` (bandeja) o `RemitenteId = usuarioActual` (enviados). El acceso está restringido por las reglas de autorización de cada actor.
