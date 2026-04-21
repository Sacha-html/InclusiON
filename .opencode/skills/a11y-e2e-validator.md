---
description: Ejecuta tests E2E de accesibilidad completos con Playwright + axe-core
mode: subagent
tools:
  read: true
  write: false
  edit: false
  bash: true
---

# Accessibility E2E Validator

Ejecuta validación E2E completa de accesibilidad del sistema usando Playwright + axe-core.

## Ubicación de tests
Los tests están en: `InclusiON.Testing/tests/frontend/`

## Tests disponibles

| Test | Descripción | Comando |
|------|-------------|---------|
| `accessibility.spec.ts` | WCAG AAA compliance, accessible names, form labels, tab order | `npm run test:a11y` |
| `coreui-validation.spec.ts` | Clases CoreUI, estructura de formularios, accesibilidad de modals/dropdowns | `npm run test:coreui` |
| `input-validation.spec.ts` | IDs en inputs, labels asociados, accessible names | `npm run test:inputs` |
| `responsive.spec.ts` | Viewports (mobile/tablet/desktop), media queries, touch targets | `npm run test:responsive` |
| `light-dark.spec.ts` | Contraste en ambos modos, focus indicators, prefers-color-scheme | `npm run test:theme` |

## Scripts npm disponibles

```bash
# Tests individuales
npm run test:a11y           # WCAG accessibility audit
npm run test:coreui         # CoreUI classes validation
npm run test:inputs         # Input IDs validation
npm run test:responsive     # Responsive design validation
npm run test:theme          # Light/dark mode validation

# Tests combinados
npm run test:all            # Todos los tests de accesibilidad
npm run test:wcag-aaa       # Solo tests WCAG AAA
npm run test:contrast       # Solo tests de contraste

# Modo headed (ver navegador)
npm run test:a11y:headed    # Accessibility con UI
```

## Ejecutar tests

### Paso 1: Asegurar que el servidor está corriendo
El servidor debe estar en `http://localhost:4200`. Puede iniciarse con:
```bash
cd InclusiON.Client && npm start
```

### Paso 2: Ejecutar tests de accesibilidad
```bash
cd InclusiON.Testing && npm run test:all
```

### Paso 3: Ver reporte HTML
```bash
cd InclusiON.Testing && npm run report
```

## Validaciones que realiza

### 1. WCAG AAA (axe-core)
- Contraste WCAG 2.1 AAA (ratio >= 7:1)
- Nombres accesibles en botones, enlaces, inputs
- Labels en todos los formularios
- Imágenes con alt text
- Orden de tabulación lógico
- Indicadores de foco visibles
- Atributos lang y title

### 2. Input IDs
- Todos los inputs tienen ID único
- Labels asociados mediante `for`
- Error messages con `aria-describedby`
- Accessible names en todos los elementos

### 3. CoreUI Classes
- Inputs con clase `form-control`
- Botones con clase `btn`
- Forms con estructura `c-form`
- Modals con `role="dialog"`, `aria-modal`, `aria-labelledby`
- Dropdowns con `aria-expanded`

### 4. Responsive
- Viewports: mobile (375px), tablet (768px), desktop (1920px)
- Sin overflow horizontal
- Textos legibles (>=12px)
- Touch targets >=44px
- Media queries para breakpoints

### 5. Light/Dark Mode
- WCAG AAA contrast en ambos modos
- Focus indicators visibles
- Respeto de `prefers-color-scheme`
- Toggle de theme funcional

## Errores comunes y soluciones

| Error | Solución |
|-------|----------|
| Inputs sin ID | Agregar `id="uniqueId"` al input y `for="uniqueId"` al label |
| Sin clase CoreUI | Agregar `class="form-control"` a inputs |
| Contraste bajo | Usar variables `--a11y-*` de `_accessibility-themes.scss` |
| Labels no asociados | Usar `<label for="inputId">` o `aria-label` |
| Foco no visible | Agregar `:focus-visible { outline: ... }` |

## Output esperado

Éxito:
```
✓ accessibility.spec.ts - 10 tests passed
✓ coreui-validation.spec.ts - 7 tests passed  
✓ input-validation.spec.ts - 5 tests passed
✓ responsive.spec.ts - 15 tests passed
✓ light-dark.spec.ts - 7 tests passed

Total: 44 tests passed
```

Fallo:
```
✗ accessibility.spec.ts
  ✗ form inputs have labels
    Input #123 missing label
```

## Notas
- Los tests usan axe-core con tags `wcag2aaa`, `wcag21aaa`
- Requiere servidor corriendo en localhost:4200
- Para CI, usar `--project=chromium` para un solo navegador
