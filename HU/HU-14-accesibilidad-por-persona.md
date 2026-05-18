# HU-14 — Perfil de Accesibilidad Configurable por Persona

**Proceso relacionado:** 5, 3 (Sprint 3 — Accesibilidad)
**Prioridad:** Alta
**Creada:** 2026-05-08
**Jira (propuesto):** IN-186

---

## Historia de Usuario

**Como** profesional
**Quiero** configurar el perfil de accesibilidad visual de cada persona con discapacidad que atiendo
**Para** que cuando esa persona acceda al portal el sistema ya tenga las adaptaciones correctas aplicadas sin que ella tenga que configurarlas manualmente

**Como** persona con discapacidad
**Quiero** que la plataforma ya esté configurada con mi perfil visual desde el primer momento
**Para** no depender de ajustar opciones que pueden ser difíciles de encontrar o entender

---

## Descripción funcional

### Contexto actual

El sistema ya tiene 14 combinaciones de accesibilidad (7 perfiles × 2 modos de color) gestionadas por `AccessibilityService`. El panel de accesibilidad (Alt+A) permite al usuario ajustar la configuración en sesión, pero:

- La configuración se guarda en `localStorage` del navegador — se pierde al cambiar de dispositivo
- No hay configuración por persona en el backend
- Si la persona accede desde un dispositivo compartido o nuevo, empieza con la configuración por defecto

### Solución

El profesional configura, desde el perfil de cada persona asignada, los parámetros de accesibilidad que le corresponden. Esta configuración se persiste en el backend y se carga automáticamente cuando esa persona inicia sesión.

### Vista del profesional — Tab "Accesibilidad" en perfil de persona

El profesional ve una nueva pestaña "Accesibilidad" en la pantalla de detalle de persona con:

| Parámetro | Opciones | Default |
|-----------|----------|---------|
| Perfil visual | default, alto-contraste, dislexia, baja-visión, deuteranopía, protanopía, tritanopía | default |
| Modo de color | claro, oscuro | claro |
| Tamaño de fuente | pequeño (90%), normal (100%), grande (115%), muy grande (130%) | normal |
| Guía de lectura | activada / desactivada | desactivada |
| Texto a voz | activado / desactivado | desactivado |

El profesional puede:
- **Previsualizar** cómo se verá la pantalla con esa configuración (aplica al panel en tiempo real)
- **Guardar** la configuración para esa persona
- **Restaurar valores por defecto** con un botón

### Carga automática al iniciar sesión

Cuando la persona inicia sesión (por cualquier método: PIN, asistido, familiar, estándar visual):
1. El backend incluye la configuración de accesibilidad en la respuesta del login (o en un endpoint separado)
2. El frontend la aplica via `AccessibilityService.applyConfig(config)` antes de renderizar el portal
3. La persona puede ajustar manualmente en el panel (Alt+A), pero sus cambios duran solo la sesión a menos que el profesional los actualice

### Prioridad de aplicación

```
Config guardada por profesional (backend)
  → La persona ajusta manualmente en sesión (localStorage, temporal)
    → Si no hay ninguna: valores por defecto del sistema
```

---

## Criterios de Aceptación

### Configuración por el profesional

- [ ] La pantalla de detalle de persona tiene una pestaña "Accesibilidad" visible solo para el profesional asignado
- [ ] El profesional puede seleccionar perfil visual, modo de color, tamaño de fuente, guía de lectura y texto a voz
- [ ] Al hacer clic en "Previsualizar" la configuración se aplica en tiempo real en la pantalla actual del profesional (sin guardar)
- [ ] Al guardar, la configuración se persiste en el backend asociada a esa persona
- [ ] Se muestra toast de confirmación al guardar
- [ ] El botón "Restaurar" vuelve a los valores por defecto del sistema
- [ ] Si la persona no tiene configuración guardada, se muestran los valores por defecto del sistema

### Carga automática al iniciar sesión

- [ ] Al completar el login, el frontend carga la configuración de accesibilidad de la persona desde el backend
- [ ] La configuración se aplica antes de mostrar el portal (sin flash visual del tema por defecto)
- [ ] Funciona para todos los métodos de login: PIN, asistido, familiar, estándar visual
- [ ] Si el backend no devuelve configuración, se aplica el perfil por defecto sin error

### Backend

- [ ] Existe endpoint `GET /api/Persons/{id}/accessibility` que devuelve la configuración guardada (o 404 si no hay)
- [ ] Existe endpoint `PUT /api/Persons/{id}/accessibility` para guardar/actualizar la configuración
- [ ] Solo el profesional asignado a esa persona puede leer y escribir su configuración (validación de asignación)
- [ ] La respuesta de login incluye o referencia la configuración de accesibilidad de la persona

### Accesibilidad de la feature

- [ ] La pestaña y controles respetan el perfil de accesibilidad activo del profesional mientras configura
- [ ] Los labels de las opciones son descriptivos (no solo íconos de color)
- [ ] El formulario es completamente navegable por teclado

---

## Modelo de datos (backend)

Nueva tabla `PersonAccessibilityConfigs`:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PersonId` | `Guid` (FK) | Persona a quien pertenece la configuración |
| `Profile` | `string` | Código del perfil: `default`, `high-contrast`, `dyslexia`, `low-vision`, `deuteranopia`, `protanopia`, `tritanopia` |
| `ColorMode` | `string` | `light` o `dark` |
| `FontSizePercent` | `int` | 90, 100, 115 o 130 |
| `ReadingGuideEnabled` | `bool` | Guía de lectura activa |
| `TextToSpeechEnabled` | `bool` | Texto a voz activo |
| `UpdatedAt` | `DateTime` | Fecha de última actualización |
| `UpdatedByUserId` | `Guid` (FK) | Usuario que realizó el último cambio |

Relación: `Person` 1-0..1 `PersonAccessibilityConfig`

---

## Endpoints propuestos

```
GET  /api/Persons/{id}/accessibility
     → 200 PersonAccessibilityConfigResponse | 404 si no configurado
     → Requiere: profesional asignado a la persona O admin institucional

PUT  /api/Persons/{id}/accessibility
     → Body: UpdatePersonAccessibilityConfigRequest
     → 204 No Content
     → Requiere: profesional asignado a la persona

PATCH login response (todas las rutas /Auth/login/*)
     → Agregar campo opcional AccessibilityConfig en AuthResponse
```

---

## Notas de implementación

### Frontend — AccessibilityService

Agregar método `loadFromBackend(config: PersonAccessibilityConfigDto)` que recibe la config del servidor y la aplica vía los métodos existentes (`setProfile()`, `setColorMode()`, `setFontSize()`, etc.).

Invocar en el `AuthService` después de login exitoso y antes de la redirección al portal.

### Relación con HU-05

El criterio de HU-05: *"La interfaz respeta el perfil de accesibilidad configurado"* depende de esta HU. Al implementar HU-14, HU-05 queda automáticamente cubierto.
