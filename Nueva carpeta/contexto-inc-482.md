# Contexto definido para INC-482

## 1. Desbloqueo del Roadmap y escala GAS

La escala GAS acordada es:

| Éxito | GAS |
|---:|---:|
| 0%–30% | -2 |
| 31%–59% | -1 |
| 60%–69% | 0 |
| 70%–80% | +1 |
| 81%–100% | +2 |

El siguiente nivel del Roadmap se desbloquea cuando:

```text
GAS >= 0
```

Con esta escala, equivale a un éxito mínimo del 60%.

## 2. Alerta de frustración

La alerta se activa exclusivamente cuando el alumno registra cuatro fallas consecutivas en la misma actividad/asignación.

Un GAS negativo aislado no genera una alerta.

Cuando el alumno alcanza el éxito, completa el nivel y avanza al siguiente; no continúa acumulando fallas en ese nivel.

## 3. `HasFrustrationAlert`

El campo no significa simplemente que la última sesión falló.

Debe indicar si existe una alerta real de frustración:

```text
HasFrustrationAlert = true
```

únicamente después de cuatro fallas consecutivas.

Ejemplos:

```text
Falla → Falla → Falla → Falla = alerta
Falla → Éxito = sin alerta
GAS negativo aislado = sin alerta
```

## 4. Nivel actual del Roadmap

El nivel actual se define como el nivel más alto que el alumno haya iniciado.

No se calcula usando el último nivel aprobado, el primer nivel pendiente ni un promedio.

Si el alumno inició el nivel 3, aunque todavía no lo haya completado, el nivel actual es 3.

## 5. Actualización “en tiempo real”

No se implementan WebSockets ni SignalR para los dashboards.

La regla es:

```text
Actividad completada
→ se persisten los datos inmediatamente
→ la siguiente consulta o recarga muestra las métricas actualizadas
```

La actualización automática sin recargar queda fuera del alcance actual.

## Flujo de reintentos

- Una respuesta incorrecta debe llegar a la pantalla de resultado con 0%.
- **Finalizar** guarda el intento y vuelve al dashboard del alumno (`/app`).
- **Intentar de nuevo** guarda primero el fallo y luego crea una nueva respuesta.
- Los fallos mantienen la asignación en progreso hasta alcanzar el máximo de intentos.
- Al cuarto fallo, se bloquea el reintento, se activa la alerta y se redirige al dashboard.
- Una actividad completada exitosamente no puede reiniciarse; para repetirla se necesita una nueva asignación.
