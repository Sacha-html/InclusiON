# Actividad Interactiva: Rompecabezas (`PUZZLE`) con ARASAAC

Documentación técnica y funcional para la plantilla de actividad interactiva **Rompecabezas** en la plataforma InclusiON.

---

## 1. Resumen y Propósito Pedagógico

La actividad **Rompecabezas** (`PUZZLE`) permite al docente o profesional de la salud/educación crear ejercicios interactivos donde una imagen de pictograma de la biblioteca abierta **ARASAAC** es recortada dinámicamente en piezas desordenadas.

### Objetivos terapéuticos y pedagógicos:
- **Motricidad Fina y Coordinación Óculo-Manual**: Estimula la precisión en el arrastre y colocación de elementos en pantalla.
- **Percepción Visual y Cierre Gestáltico**: Ayuda al estudiante a reconocer una figura global a partir de sus partes.
- **Pensamiento Lógico y Organización Espacial**: Reconocimiento de cuadrantes, relaciones arriba/abajo, izquierda/derecha.
- **Accesibilidad Universal (TEA / Trastornos Motores)**: Ofrece modalidad dual de juego (*Drag & Drop* o *Pulsar para seleccionar y colocar*) y guía visual semitransparente para reducir la frustración.

---

## 2. Definición Técnica en Catálogos (`ActivityTemplateType`)

Para que esta plantilla esté disponible en el sistema, debe registrarse en la tabla `ActivityTemplateTypes` (vía ABM de Administración o script SQL).

### Parámetros del Catálogo:
- **Nombre:** `Rompecabezas`
- **Código (`Code`):** `PUZZLE`
- **Área de habilidad:** `Lógico-matemático` (o `Motricidad Fina` si se encuentra configurada)
- **Usa pictogramas (`UsesPictograms`):** `true`
- **Tiene audio (`HasAudio`):** `true`
- **Nombre de componente (`ComponentName`):** `PuzzlePlayerComponent`
- **Esquema de contenido (`ContentSchema`):**

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "instruction": {
      "type": "string",
      "description": "Consigna verbal o escrita que se presenta al estudiante"
    },
    "pictogramId": {
      "type": "integer",
      "description": "ID numérico oficial del pictograma en la API de ARASAAC"
    },
    "label": {
      "type": "string",
      "description": "Texto descriptivo del pictograma seleccionado"
    },
    "rows": {
      "type": "integer",
      "enum": [1, 2, 3],
      "description": "Cantidad de filas para el recorte de piezas"
    },
    "cols": {
      "type": "integer",
      "enum": [2, 3],
      "description": "Cantidad de columnas para el recorte de piezas"
    },
    "showGhostGuide": {
      "type": "boolean",
      "default": true,
      "description": "Indica si se proyecta la imagen completa tenue como fondo guía"
    }
  },
  "required": ["instruction", "pictogramId", "rows", "cols"]
}
```

---

## 3. Arquitectura en el Frontend

### A. Editor del Profesional (`PuzzleEditorComponent`)
- **Ubicación:** `src/app/views/professional/activities/new/editors/puzzle-editor/`
- **Registro:** [`CONTENT_EDITOR_REGISTRY`](file:///d:/git/InclusiON.Client/src/app/views/professional/activities/new/editors/content-editor-registry.ts) bajo la clave `PUZZLE`.
- **Funcionalidades:**
  1. Campo de consigna con valor sugerido dinámico según el pictograma elegido.
  2. Buscador reactivo conectado con `ArasaacService.search(term)`.
  3. Previsualización inmediata del pictograma con la cuadrícula de corte superpuesta.
  4. Selector de dificultad:
     - **2 piezas (1×2)**: Muy fácil / Inicial (mitades).
     - **4 piezas (2×2)**: Fácil / Cuadrícula estándar.
     - **6 piezas (2×3)**: Nivel intermedio.
     - **9 piezas (3×3)**: Nivel avanzado / Desafiante.
  5. Checkbox para activar/desactivar la guía visual tenue.

### B. Reproductor del Estudiante (`PuzzlePlayerComponent`)
- **Ubicación:** `src/app/views/aac/activities/player/puzzle/`
- **Registro:** [`PLAYER_REGISTRY`](file:///d:/git/InclusiON.Client/src/app/views/aac/activities/player/player-registry.ts) bajo la clave `PUZZLE`.
- **Mecánica de Juego:**
  - **Fase Intro:** Presenta la consigna, permite reproducir el audio con Speech Synthesis y botón accesible de inicio.
  - **Fase Playing:**
    - Tablero central con casilleros numerados y silueta guía al 22% de opacidad.
    - Banco lateral/inferior con las piezas desordenadas aleatoriamente.
    - Recorte visual matemático mediante CSS `background-size` y `background-position` sobre la imagen 500x500 px provista por ARASAAC (`https://static.arasaac.org/pictograms/{id}/{id}_500.png`).
    - **Modalidad A (Táctil / Clic):** Toca una pieza en el banco para seleccionarla (resaltado celeste) y luego toca el casillero destino en el tablero. Tocar una pieza del tablero la devuelve al banco.
    - **Modalidad B (Arrastrar y Soltar):** Soporte nativo para arrastrar con mouse o lápiz óptico hacia el casillero deseado.
    - Cuando una pieza ocupa su posición correcta, muestra un badge verde con tilde (`✓`).
    - Al colocar todas las piezas en su orden correcto, el sistema emite feedback sonoro y transiciona a la fase de resultado.
  - **Fase Result:** Emite 100% de puntuación, tiempo transcurrido, medalla, confetti y opciones de finalizar o reintentar.

---

## 4. Guía Paso a Paso para Dar de Alta en el Catálogo

### Opción 1: Desde la Interfaz de Administración Web (Recomendado)

1. Iniciar sesión en la plataforma con una cuenta de **Administrador**.
2. Dirigirse al menú lateral izquierdo y hacer clic en **Catálogos**.
3. En el selector superior o pestañas de catálogo, elegir **Tipos de plantilla**.
4. Hacer clic en el botón superior derecho **`+ Agregar`**.
5. Completar los campos del modal:
   - **Nombre:** `Rompecabezas`
   - **Código:** `PUZZLE`
   - **Área de habilidad:** Seleccionar `Lógico-matemático` (o `Motricidad Fina`).
   - **Usa pictogramas:** Marcar la casilla (☑).
   - **Tiene audio:** Marcar la casilla (☑).
6. Presionar **`Guardar`**.
7. ¡Listo! A partir de ese momento, cualquier profesional que ingrese a **Crear Actividad** verá la opción *Rompecabezas* en el paso 2 del Wizard.

---

### Opción 2: Mediante Script SQL Directo en PostgreSQL

Si se desea cargar directamente en la base de datos o en scripts de inicialización:

```sql
INSERT INTO "ActivityTemplateTypes" (
    "Name",
    "Code",
    "SkillAreaId",
    "UsesPictograms",
    "HasAudio",
    "ComponentName",
    "ContentSchema",
    "DisplayOrder",
    "IsActive",
    "CreatedAt",
    "CreatedBy"
)
SELECT
    'Rompecabezas',
    'PUZZLE',
    "Id",
    true,
    true,
    'PuzzlePlayerComponent',
    '{"type":"object","properties":{"instruction":{"type":"string"},"pictogramId":{"type":"integer"},"label":{"type":"string"},"rows":{"type":"integer"},"cols":{"type":"integer"},"showGhostGuide":{"type":"boolean"}},"required":["instruction","pictogramId","rows","cols"]}',
    9,
    true,
    NOW(),
    'system_seed'
FROM "SkillAreas"
WHERE "Name" = 'Lógico-matemático'
LIMIT 1
ON CONFLICT ("Code") DO NOTHING;
```
