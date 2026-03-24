# HU-09 — Mensajería Interna

**Proceso relacionado:** 16
**Prioridad:** Alta

---

## Historia de Usuario

**Como** profesional o familiar
**Quiero** enviar y recibir mensajes dentro de la plataforma
**Para** centralizar la comunicación con historial auditable sin depender de canales externos como WhatsApp o email

---

## Descripción funcional

La plataforma ofrece un sistema de mensajería interna entre profesionales y familiares:

### Bandeja de entrada
- Lista de conversaciones con indicador de mensajes no leídos (highlight + punto azul)
- Filtro para ver solo mensajes no leídos
- Al abrir un mensaje no leído se marca automáticamente como leído

### Envío de mensajes
- El remitente selecciona el destinatario y opcionalmente la persona con discapacidad relacionada
- Completa asunto y contenido del mensaje
- Al responder se mantiene el hilo de conversación

### Indicador en sidebar
- Un badge numérico en la barra lateral muestra la cantidad de mensajes no leídos
- Se actualiza automáticamente cada 30 segundos
- El badge deja de actualizarse cuando el usuario está en la sección de mensajes

### Hilos de conversación
Los mensajes se agrupan en hilos cuando son respuestas, permitiendo seguir la conversación completa de forma contextual.

---

## Criterios de Aceptación

- [ ] Cada usuario solo puede ver los mensajes donde es remitente o destinatario
- [ ] Al abrir un mensaje no leído, se marca automáticamente como leído
- [ ] Los mensajes se pueden vincular a una persona con discapacidad para dar contexto
- [ ] Las respuestas mantienen el hilo de conversación
- [ ] El badge de no leídos se actualiza automáticamente
- [ ] La bandeja de entrada permite filtrar solo mensajes no leídos
- [ ] Los hilos agrupan la conversación de forma cronológica
