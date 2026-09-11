# Auditoría de UI — Normalización visual (2026-09-11)

Relevamiento manual de las 4 vistas de la app (Admin, Profesional, Familia, Persona/CAA) corriendo local,
como paso previo a normalizar tipografía, espaciados y consistencia visual — **sin tocar lógica,
colores semánticos ni el panel/mecanismos de accesibilidad, que ya están validados**.

## Hallazgos

### 1. Idioma mezclado (ES/EN) en el mismo concepto
- **Badge de "Estado"**: en `Familiares` y `Usuarios` (admin) aparece en inglés (`Active`); en
  `Personas` y `Profesionales` aparece en español (`Activo`, `Aprobado`). Mismo concepto, dos idiomas
  conviviendo en pantallas hermanas.
- **Página 404**: 100% en inglés ("Oops! You're lost.", "The page you are looking for was not
  found.", "Search") y con el estilo *default* de la plantilla CoreUI, sin ningún esqueleto de la
  app (sidebar, colores, tipografía) — se nota como una pantalla ajena al resto.

### 2. Error tipográfico repetido en el footer
`InclusiON © 2026Institución Cervantes` — falta un espacio entre el año y el nombre de la
institución. Aparece en el footer de **todas** las pantallas de Admin y Familia relevadas.

### 3. Tipografía sin una voz única
- La pantalla de bienvenida (`/login`) usa una tipografía grande y redondeada para "Bienvenido a
  InclusiON"; los formularios de "Acceso Administrativo"/"Acceso Profesional" inmediatamente
  después usan una tipografía plana bastante más chica y menos cuidada para el mismo tipo de
  título. No hay una escala de tamaños de título consistente entre pantallas.

### 4. Tres patrones de login distintos para un mismo propósito
- **Admin / Profesional**: formulario clásico — card blanca, ícono candado, inputs con
  ícono+placeholder, botón sólido.
- **Familia**: flujo conversacional paso a paso — "Escribe tu email" → "Hola, [Nombre]" → contraseña.
- **Persona (CAA)**: flujo conversacional por nombre → teclado numérico de PIN grande, táctil,
  tema oscuro.
  El de Persona está bien resuelto para accesibilidad y **no se toca**. Pero Admin/Profesional y
  Familia cumplen el mismo rol (login "tradicional" con contraseña) y hoy usan dos lenguajes
  visuales distintos sin motivo funcional — candidato claro a unificar.

### 5. Controles nativos sin estilar, mezclados con estilizados
En `Reportes` (admin), el filtro "Personas" es un `<select multiple>` nativo del navegador (sin
ningún estilo), ubicado al lado de los filtros "Profesional", "Estado" y "Tipo" que sí están
estilizados con los componentes de CoreUI. Salta a la vista apenas se entra a la pantalla.

### 6. Nomenclatura inconsistente para el mismo destino
En la home de Persona (CAA): la tarjeta grande dice **"Mis Actividades"**, pero la barra de
navegación inferior que lleva al mismo lugar dice solo **"Actividades"**.

### 7. Botones ya parcialmente unificados (punto de partida)
El Dashboard profesional y Evaluaciones ya quedaron consistentes (botón sólido primario) tras los
fixes de INCNEW-41/44/45. Sirve de plantilla para extender el mismo criterio al resto de la app,
que en general usa los componentes de CoreUI "de fábrica" sin tema propio (`_theme.scss` y
`_variables.scss` están casi vacíos) — lo que hace viable centralizar esto en pocos archivos.

## Qué NO se toca (por pedido explícito)
- El panel/botón de Accesibilidad y sus modos (alto contraste, fuente grande, etc.)
- Colores con significado semántico (estados, niveles de perfil funcional, alertas)
- El diseño de la pantalla de CAA en general (PIN grande, tema oscuro, tarjetas táctiles) — ya
  pensado para su usuario final
- Cualquier lógica funcional

## Prioridad sugerida (de más a menos impacto / menor riesgo)
1. Arreglar el idioma mezclado en badges de "Estado" y el error del footer (cambios acotados, alto
   impacto de percepción de "cuidado").
2. Traducir y re-skinear la página 404.
3. Definir una escala tipográfica única (título grande / título de sección / texto) y aplicarla
   centralizadamente vía variables de tema.
4. Unificar el filtro nativo de Reportes con el resto de los selects.
5. Unificar el lenguaje visual de los logins de Admin/Profesional vs Familia (dejando Persona/CAA
   intacto).
6. Unificar nomenclatura "Actividades" vs "Mis Actividades" en CAA.
