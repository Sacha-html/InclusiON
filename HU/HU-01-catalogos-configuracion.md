# HU-01 — Catálogos y Configuración Inicial del Sistema

**Proceso relacionado:** 01, 02, 03, 04, 08
**Prioridad:** Crítica

---

## Historia de Usuario

**Como** administrador del sistema
**Quiero** configurar las tablas de referencia, registrar profesionales y establecer sus relaciones con instituciones y personas
**Para** que la plataforma esté lista para operar con datos reales

---

## Descripción funcional

El administrador debe poder preparar el sistema antes de que comience el trabajo educativo. Esto implica:

1. **Cargar catálogos de referencia** — Tipos de discapacidad, niveles de autonomía, categorías de actividad, áreas de habilidad, tipos de template de actividad y métodos de login. Estos catálogos alimentan los formularios de toda la plataforma.

2. **Registrar profesionales** — Crear perfiles de profesionales con datos personales y credenciales de acceso. El sistema genera una contraseña temporal que el profesional debe cambiar en su primer ingreso.

3. **Asignar profesionales a instituciones** — Vincular a cada profesional con una o más instituciones educativas donde trabaja.

4. **Asignar personas a profesionales** — Establecer qué personas con discapacidad atiende cada profesional, indicando si es el profesional principal y si puede autorizar el login asistido.

---

## Criterios de Aceptación

### Catálogos
- [ ] El administrador puede consultar, crear y editar items en los 6 catálogos del sistema
- [ ] No se permiten nombres duplicados dentro del mismo catálogo
- [ ] Solo el administrador global puede modificar catálogos; el resto los consulta en modo lectura
- [ ] Los catálogos se reflejan automáticamente en los dropdowns de todos los formularios

### Profesionales
- [ ] Al crear un profesional se genera automáticamente una cuenta de usuario con contraseña temporal
- [ ] El DNI y el email deben ser únicos en el sistema
- [ ] Se puede buscar profesionales por nombre, especialidad, institución o estado
- [ ] Al desactivar un profesional se cierran sus sesiones activas
- [ ] El profesional puede consultar su propio perfil

### Asignaciones
- [ ] Un profesional puede estar vinculado a múltiples instituciones
- [ ] Una persona puede tener múltiples profesionales asignados
- [ ] Se debe indicar quién es el profesional principal de cada persona
- [ ] Se debe indicar si el profesional puede autorizar el login asistido de la persona
- [ ] La desvinculación es lógica (conserva historial)
