# Guía de reinicialización local y carga manual de catálogos

## Objetivo

Esta guía permite preparar la base de datos local de Docker para probar los ABM de catálogos desde cero.

El resultado esperado es:

- Conservar únicamente el usuario Administrador.
- Eliminar alumnos, profesionales, familiares y sus datos relacionados.
- Eliminar actividades y datos operativos de prueba.
- Vaciar los catálogos ABM definidos en esta guía.
- Recargar los catálogos manualmente desde la interfaz.
- Verificar que los ABM permitan consultar, crear, editar y desactivar registros.

> **Alcance:** esta guía es exclusivamente para la base local de desarrollo `inclusion_dev` ejecutada en Docker. No debe utilizarse en producción ni en una base que contenga datos reales que deban conservarse.

## Archivos involucrados

Script de limpieza:

```text
InclusiON.Server/Scripts/reset_local_abm_data.sql
```

Documentación funcional de los ABM:

```text
InclusiON.Documents/ABM/03-catalogos.md
```

## 1. Verificaciones previas

Antes de ejecutar cualquier operación:

1. Confirmar que se está trabajando sobre el repositorio y la rama correctos.
2. Detener el backend y cualquier proceso que esté utilizando la base.
3. Confirmar que el contenedor local de PostgreSQL está activo:

   ```powershell
   docker ps
   ```

4. Confirmar que la base utilizada es `inclusion_dev`.
5. Confirmar que no existen datos reales que deban conservarse.

El script contiene una guardia que aborta si la base actual no se llama `inclusion_dev`, pero esta protección no reemplaza la verificación manual del contenedor y la base.

## 2. Crear un backup

Desde PowerShell, ejecutar:

```powershell
docker exec postgres pg_dump -U postgres -d inclusion_dev > "$env:TEMP\inclusion_dev_backup.sql"
```

Verificar que el archivo exista:

```powershell
Test-Path "$env:TEMP\inclusion_dev_backup.sql"
```

El resultado esperado es:

```text
True
```

Si el backup falla, no continuar con la limpieza.

## 3. Ejecutar una prueba sin guardar cambios

El script finaliza intencionalmente con `ROLLBACK`. Esto permite comprobar el orden de eliminación y las validaciones sin conservar ningún cambio.

Si la consola está ubicada en la raíz del repositorio:

```powershell
Get-Content ".\InclusiON.Server\Scripts\reset_local_abm_data.sql" -Raw |
  docker exec -i postgres psql -U postgres -d inclusion_dev -v ON_ERROR_STOP=1
```

Si la consola está ubicada en `InclusiON.Server\Scripts`:

```powershell
Get-Content ".\reset_local_abm_data.sql" -Raw |
  docker exec -i postgres psql -U postgres -d inclusion_dev -v ON_ERROR_STOP=1
```

La salida debe finalizar con:

```text
ROLLBACK
```

No debe aparecer ningún error. Si aparece un error, no cambiar `ROLLBACK` por `COMMIT` y no volver a ejecutar el script sin analizar la causa.

## 4. Aplicar la limpieza definitivamente

Solo después de verificar el backup y una prueba sin errores:

1. Abrir el script:

   ```powershell
   notepad .\reset_local_abm_data.sql
   ```

2. Modificar únicamente la última instrucción:

   ```sql
   ROLLBACK;
   ```

   por:

   ```sql
   COMMIT;
   ```

3. Guardar el archivo.
4. Revisar nuevamente que la base de destino sea `inclusion_dev`.
5. Ejecutar el mismo comando de la prueba anterior.

La salida debe finalizar con:

```text
COMMIT
```

Si aparece cualquier error, detener el proceso y conservar la salida completa para su análisis. No hacer reintentos ciegos.

## 5. Qué elimina el script

El script elimina:

- Todos los usuarios excepto el Admin.
- Alumnos, profesionales y familiares.
- Aulas y asignaciones.
- Relaciones entre alumnos, profesionales y familiares.
- Actividades, contenidos y embeddings.
- Reportes, diagnósticos, mensajes, invitaciones y eventos de calendario.
- Roadmaps, sesiones, respuestas y resultados de actividades.
- Configuraciones adaptativas y trabajos pendientes.
- Historiales, auditorías, tokens y dispositivos asociados.
- Instituciones educativas.
- Los ocho catálogos ABM vaciables.

## 6. Qué conserva el script

Se conservan deliberadamente:

- El usuario Admin con ID `00000000-0000-0000-0000-000000000001`.
- Los roles de Identity:
  - `Admin`
  - `Professional`
  - `FamilyRepresentative`
  - `PersonWithDisability`
- Los claims de los roles.
- Las estructuras de la base y el historial de migraciones.
- `LoginMethods`, porque no posee ABM de alta/baja y es necesario para los flujos de autenticación.
- Estados y tipos técnicos utilizados por el backend, como estados de asignaciones y trabajos.

Los roles y tablas técnicas no son catálogos de negocio y no deben eliminarse durante esta tarea.

## 7. Orden de carga manual

Después de aplicar la limpieza y reiniciar el backend, cargar los datos en este orden:

1. Áreas de habilidad.
2. Tipos de plantilla asociados a las áreas.
3. Especialidades.
4. Tipos de discapacidad.
5. Niveles de autonomía.
6. Categorías de actividad.
7. Tipos de reporte.
8. Instituciones educativas, si se cuenta con datos reales.

Las áreas deben cargarse antes que los tipos de plantilla porque cada tipo de plantilla requiere asociarse a un área activa.

## 8. Datos para cargar en los ABM

### 8.1 Áreas de habilidad

| Nombre | Descripción | Ícono | Color RGB | Orden |
|---|---|---|---|---:|
| Comunicación | Actividades orientadas al desarrollo de habilidades comunicativas mediante pictogramas, selección de opciones y expresión. | `chat` | 46, 95, 163 | 1 |
| Alfabetización | Actividades de lectura global, reconocimiento de sonidos y construcción de palabras para el desarrollo de la lectoescritura. | `menu_book` | 76, 175, 80 | 2 |
| Lógico-matemático | Actividades de clasificación, ordenamiento y numeración para el desarrollo del pensamiento lógico-matemático. | `calculate` | 255, 152, 0 | 3 |
| Trayectoria | Camino de aprendizaje estándar anti-frustración. | `map` | 103, 58, 183 | 4 |

Los colores RGB corresponden respectivamente a:

- Comunicación: `#2E5FA3`.
- Alfabetización: `#4CAF50`.
- Lógico-matemático: `#FF9800`.
- Trayectoria: `#673AB7`.

### 8.2 Tipos de plantilla

| Área | Nombre | Código | Pictogramas | Audio |
|---|---|---|---|---|
| Comunicación | Seleccionar pictograma | `PICTOGRAM_SELECT` | Sí | Sí |
| Comunicación | Selección de opciones | `OPTION_SELECT` | No | Sí |
| Alfabetización | Lectura global | `GLOBAL_READING` | No | Sí |
| Alfabetización | Reconocer sonidos | `SOUND_RECOGNITION` | No | Sí |
| Alfabetización | Armar palabras | `BUILD_WORD` | No | No |
| Lógico-matemático | Clasificación | `CLASSIFY` | No | No |
| Lógico-matemático | Ordenamiento | `ORDER_SEQUENCE` | No | No |
| Lógico-matemático | Numeración | `NUMERATION` | No | No |

En el formulario actual deben completarse:

- Nombre.
- Código.
- Área de habilidad.
- Soporta pictogramas.
- Soporta audio.

`Trayectoria` no tiene tipos de plantilla precargados en el inventario original.

### 8.3 Especialidades

Cargar las siguientes especialidades, todas activas:

1. Educación Especial
2. Psicología
3. Psicopedagogía
4. Fonoaudiología
5. Terapia Ocupacional
6. Kinesiología
7. Trabajo Social
8. Musicoterapia
9. Psicomotricidad
10. Acompañamiento Terapéutico
11. Docente de Apoyo a la Inclusión (DAI)
12. Neurología
13. Pediatría

### 8.4 Tipos de discapacidad

| Nombre | Descripción |
|---|---|
| Sensorial | Incluye discapacidad auditiva y visual (sordera, ceguera). |
| Cognitiva/Intelectual | Relacionada con el aprendizaje y habilidades adaptativas de la vida diaria. |
| Motriz | Alteraciones en el funcionamiento motor. |
| Mental/Psicosocial | Derivada de diagnósticos vinculados a la salud mental. |
| Múltiple | Combinación de dos o más tipos de discapacidad. |

### 8.5 Niveles de autonomía

| Nombre | Descripción | Requiere supervisión | Orden |
|---|---|---:|---:|
| Alta | Puede usar la aplicación de forma independiente con login estándar. | No | 1 |
| Media | Requiere login simplificado —PIN o pictogramas—, pero puede utilizar la aplicación de forma independiente. | No | 2 |
| Baja | Requiere supervisión y login asistido por familiar o profesional. | Sí | 3 |

### 8.6 Categorías de actividad

| Nombre | Descripción |
|---|---|
| Lectoescritura | Actividades de lectura, escritura, conciencia fonológica y comprensión lectora. |
| Numeración y Matemática | Actividades prenuméricas, numeración, secuencias y operaciones básicas. |
| Habilidades Socioemocionales | Modificación de conducta, hábitos, rutinas, normas de convivencia e historias sociales. |
| Comunicación y Lenguaje | Lengua de señas, sistemas aumentativos y alternativos de comunicación (SAAC). |
| Motricidad y Coordinación | Motricidad fina, motricidad gruesa, coordinación óculo-manual y orientación espacial. |
| Creatividad y Expresión Artística | Música, plástica y dramatización. |
| Autonomía y Vida Diaria | Vestimenta, higiene, manejo del dinero, noción del tiempo y situaciones cotidianas. |
| Estimulación Cognitiva | Memoria, atención, percepción y resolución de problemas. |

### 8.7 Tipos de reporte

| Nombre | Descripción |
|---|---|
| Evaluación Inicial | Informe de estandarización y diagnóstico inicial del estudiante. |
| Seguimiento Mensual | Informe de progreso mensual. |
| Informe Trimestral | Evaluación de progreso trimestral. |
| PPI | Proyecto Pedagógico Individual para la inclusión. |
| Informe Anual | Resumen anual de logros alcanzados y áreas a reforzar. |

### 8.8 Métodos de login

Estos registros se conservan automáticamente y no se cargan desde el ABM porque el panel no permite crear ni eliminar métodos de login:

| Código | Nombre | Configuración |
|---|---|---|
| `STANDARD` | Email y contraseña | Requiere contraseña; orden 1. |
| `PIN` | PIN numérico | Requiere PIN; orden 2. |
| `ASSISTED` | Login asistido | Requiere supervisor; orden 3. |


