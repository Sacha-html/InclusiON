---
name: wcag-validator
description: Audit autonomo de contrastes WCAG 2.1 AA/AAA en el sistema de temas de accesibilidad de InclusiON.
---

## Objetivo

Analizar `src/scss/_accessibility-themes.scss` y reportar todos los pares de colores que no cumplen WCAG 2.1 AA/AAA.

## Pasos

1. Leer `src/scss/_accessibility-themes.scss` completo (en chunks de ~500 lineas si es necesario)

2. Identificar todas las combinaciones de `data-color-mode` (light/dark) + `data-a11y-profile` (default, high-contrast, color-blind, etc.)

3. Para cada combinacion, extraer y evaluar estos pares foreground/background:

   **Texto principal:**
   - `--a11y-text` / `--a11y-bg`
   - `--a11y-text` / `--a11y-surface`
   - `--a11y-text-secondary` / `--a11y-bg`
   - `--a11y-text-secondary` / `--a11y-surface`

   **Elementos interactivos:**
   - `--a11y-primary-text` / `--a11y-primary`
   - `--a11y-link` / `--a11y-bg`
   - `--a11y-link` / `--a11y-surface`

   **Sidebar:**
   - `--a11y-sidebar-text` / `--a11y-sidebar-bg`
   - `--a11y-sidebar-link` / `--a11y-sidebar-bg`
   - `--a11y-sidebar-active-text` / `--a11y-sidebar-active-bg`

   **Header:**
   - `--a11y-header-text` / `--a11y-header-bg`
   - `--a11y-header-link` / `--a11y-header-bg`

   **Dropdowns:**
   - `--a11y-dropdown-text` / `--a11y-dropdown-bg`
   - `--a11y-dropdown-hover-text` / `--a11y-dropdown-hover-bg`

   **Estados semanticos:**
   - `--a11y-success` / `--a11y-bg`
   - `--a11y-danger` / `--a11y-bg`
   - `--a11y-warning` / `--a11y-bg`

   **Colores por rol:**
   - `--a11y-role-person` / fondo correspondiente
   - `--a11y-role-professional` / fondo correspondiente
   - `--a11y-role-family` / fondo correspondiente

4. Calcular contraste WCAG:
   ```
   Hex -> RGB (0-1)
   Linealizar: c <= 0.04045 -> c/12.92 | sino -> ((c+0.055)/1.055)^2.4
   Luminancia: L = 0.2126*R + 0.7152*G + 0.0722*B
   Ratio = (L_lighter + 0.05) / (L_darker + 0.05)
   ```

5. Clasificar:
   - Texto normal: AAA >= 7:1 | AA >= 4.5:1 | FAIL < 4.5:1
   - Texto grande: AAA >= 4.5:1 | AA >= 3:1 | FAIL < 3:1
   - Elementos UI: AA >= 3:1 | FAIL < 3:1

6. Generar reporte en markdown:
   - Tabla por perfil con cada par y su estado
   - Solo listar failures y pares en el limite (ratio < 5:1)
   - Sugerir color alternativo para cada failure (mantener hue, alcanzar ratio)
   - Verificar que sugerencias sean color-blind safe cuando el perfil lo requiere
   - Resumen final: perfiles evaluados, pares evaluados, AAA/AA/FAIL counts

## Formato de reporte

```markdown
## Validacion WCAG - InclusiON

### [mode: light | profile: high-contrast]

| Par | Foreground | Background | Ratio | Estado | Sugerencia |
|-----|-----------|-----------|-------|--------|------------|
| text/bg | #1a1a1a | #ffffff | 17.4:1 | AAA | - |
| link/surface | #5566aa | #f5f5f5 | 3.8:1 | FAIL | #3d4d8a (5.2:1) |

### Resumen
- Perfiles evaluados: X
- Pares evaluados: Y
- AAA: Z | AA: W | FAIL: N
```
