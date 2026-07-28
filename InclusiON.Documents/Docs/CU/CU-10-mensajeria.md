# Módulo 10 — Mensajería

---

## CU-45: Enviar mensaje interno

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional / Familiar |
| **Actores secundarios** | — |
| **HU de referencia** | HU-09 |
| **Prioridad** | Alta |

**Precondiciones**
- El remitente está autenticado (Profesional o Familiar).
- Existe un vínculo activo entre el Profesional y el Familiar a través de una Persona compartida (`PersonRepresentative.IsActive = true`).

**Flujo principal**
1. El Actor accede a la sección "Mensajes" y selecciona "Nuevo mensaje".
2. El sistema muestra solo los destinatarios disponibles: Familiares de sus personas asignadas (si es Profesional) o el Profesional de su persona vinculada (si es Familiar).
3. El Actor selecciona el destinatario, opcionalmente vincula el mensaje a una Persona específica y completa asunto y contenido.
4. El Actor envía el mensaje.
5. El sistema crea el mensaje y actualiza el badge de no leídos del destinatario.

**Flujos alternativos**
- **2a. Sin vínculo activo:** El sistema no muestra destinatarios disponibles. "No tenés contactos habilitados para mensajería."
- **2b. Profesional intenta escribir a otro Profesional:** El sistema bloquea. Solo Profesional ↔ Familiar habilitado.
- **2c. Familiar intenta escribir a otro Familiar:** El sistema bloquea.
- **2d. Persona con discapacidad:** No participa en mensajería (sin acceso a la sección).

**Postcondiciones**
- El mensaje queda en la bandeja de entrada del destinatario como no leído.
- El badge del destinatario se incrementa.

---

## CU-46: Responder mensaje (hilo)

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional / Familiar |
| **Actores secundarios** | — |
| **HU de referencia** | HU-09 |
| **Prioridad** | Alta |

**Precondiciones**
- El Actor es remitente o destinatario del mensaje original.

**Flujo principal**
1. El Actor abre un mensaje de su bandeja de entrada.
2. Selecciona "Responder".
3. El sistema precompleta el destinatario y mantiene el hilo de conversación.
4. El Actor escribe el contenido y envía.
5. La respuesta se agrupa en el mismo hilo cronológicamente.

**Postcondiciones**
- El hilo de conversación queda actualizado.
- El destinatario recibe la respuesta como no leída.

---

## CU-47: Consultar bandeja de entrada

| Campo | Detalle |
|-------|---------|
| **Actor principal** | Profesional / Familiar |
| **Actores secundarios** | — |
| **HU de referencia** | HU-09 |
| **Prioridad** | Alta |

**Precondiciones**
- El Actor está autenticado.

**Flujo principal**
1. El Actor accede a la sección "Mensajes".
2. El sistema lista todas las conversaciones donde el Actor es remitente o destinatario.
3. Las conversaciones con mensajes no leídos aparecen resaltadas con un punto azul.
4. El Actor puede filtrar para ver solo mensajes no leídos.
5. Al abrir un mensaje no leído, el sistema lo marca automáticamente como leído.
6. El badge del sidebar se actualiza automáticamente cada 30 segundos. Se detiene si el Actor está en la sección de mensajes.

**Flujos alternativos**
- **Sin mensajes:** El sistema muestra estado vacío "Aún no tenés mensajes."

**Postcondiciones**
- El Actor visualiza todas sus conversaciones.
- Los mensajes abiertos quedan marcados como leídos.
