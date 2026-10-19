# Arquitectura

## Proyectos

- `src/HardwareMonitor.Core`: contratos de medición, configuración y motor de
  muestreo. No depende de WPF ni de bibliotecas de hardware.
- `src/HardwareMonitor.Hardware`: adaptador Windows-independiente de UI que usa
  LibreHardwareMonitor y traduce sus resultados a contratos propios.
- `src/HardwareMonitor.App`: anfitrión WPF. En esta fase aún no consume ni
  presenta métricas.
- `tests/HardwareMonitor.Tests`: pruebas de contratos, traducción y ciclo de
  vida del motor con fuentes de lectura controladas.

La solución conserva estos proyectos separados; no se añade un proyecto de UI
o overlay vacío. La capa de hardware apunta a `net10.0`, aunque el anfitrión
WPF requiere `net10.0-windows`.

## Contratos y datos

Cada lectura inmutable contiene un `SensorId` estable del proveedor, `ComponentId`
para distinguir componentes que tengan sensores homónimos, componente y métrica
tipados, valor `double?`, unidad tipada, disponibilidad y marca temporal UTC.
`Available` requiere un valor finito (incluido cero); `Unavailable` requiere un
motivo y no contiene valor; `Error` requiere un error estructurado y tampoco
contiene valor. Las unidades desconocidas permanecen explícitamente como
`Unknown`; no se infieren.

Los IDs se derivan del identificador completo de hardware/sensor que informa
LibreHardwareMonitor. Los snapshots son inmutables, tienen secuencia y
contienen las lecturas y los problemas de una misma vuelta de muestreo. No se
mantiene historial en esta fase.

## Muestreo y distribución del estado

Una instancia de `MonitoringService` es dueña de una única `IMonitoringSource`
y de una única tarea de muestreo. El primer ciclo comienza al iniciar; cada
ciclo posterior espera el intervalo configurado desde que terminó el anterior,
por lo que ciclos lentos no se solapan ni generan consultas más frecuentes que
el intervalo. El valor inicial es 1000 ms y se conserva la validación existente
de 250–60000 ms.

La fuente LibreHardwareMonitor es síncrona; el bucle se ejecuta en un worker y
no en el hilo UI. Los consumidores futuros deben compartir la misma instancia
del servicio y leer `CurrentSnapshot`, no crear lectores propios. El estado
publicado es una referencia inmutable que se reemplaza de forma atómica.

`StartAsync` inicia el worker; `StopAsync` cancela y espera el ciclo activo.
`DisposeAsync` detiene el worker y cierra el adaptador. LibreHardwareMonitor
no acepta un token de cancelación para una llamada de driver en curso: la
cancelación se observa entre dispositivos/lecturas y el cierre espera esa
llamada antes de liberar el origen. Un fallo inesperado del origen publica un
estado `Faulted` con el error y también se propaga a quien espera el worker.
Cada instancia se inicia una sola vez; para reanudar después de detenerla, se
crea una instancia nueva del servicio y de su origen.
Los fallos conocidos de acceso/dispositivo se informan como issues y, cuando
hay sensores asociados, como lecturas `Error` sin reutilizar valores obsoletos.

## LibreHardwareMonitor, permisos y límites

`HardwareMonitor.Hardware` fija `LibreHardwareMonitorLib` `0.9.6`, la última
versión estable que figura en el feed oficial de NuGet consultado el 19 de
octubre de 2026 (`0.9.7` solo figura como prerelease). El nuspec declara grupo
`net10.0` y licencia MPL-2.0. El adaptador habilita las categorías disponibles
en la API comprobada: CPU, GPU, memoria, placa/Super I/O, almacenamiento, red,
batería, PSU y power monitor. Esto no garantiza que una máquina concreta
exponga una categoría ni todos sus sensores.

Las traducciones de tipo y unidad siguen los comentarios del `SensorType` de
la revisión fuente incluida en el paquete. El valor nulo del proveedor se
representa como `Unavailable`; valores no finitos como `Error`. Las categorías
que el adaptador no reconozca explícitamente conservan el valor y usan métrica
`Other`/unidad `Unknown`.

El origen se abre de forma diferida en el primer muestreo. No eleva el proceso
ni solicita privilegios. Si el sistema o un driver deniega acceso, el snapshot
reporta `AccessDenied`/`HardwareUnavailable` y continúa informando los demás
dispositivos. Sensores no expuestos por firmware/driver no pueden mostrarse; un
escaneo vacío se informa como `NoSensorsDetected`. No se ha afirmado cobertura
de hardware específica ni se han medido sensores reales en esta validación.

La dependencia principal es MPL-2.0 y añade dependencias transitivas MPL-2.0,
Apache-2.0 y MIT, además del paquete heredado Mono.Posix.NETStandard con
metadatos de licencia Microsoft/Mono. Las versiones, fuentes y avisos
verificados se registran en [THIRD-PARTY-NOTICES.md](./THIRD-PARTY-NOTICES.md).
Se deben incluir los avisos/licencias indicados al preparar una distribución.

## UI y alcance posterior

La ventana WPF no inicia aún el motor ni muestra lecturas. La futura ventana y
overlay consumirán el mismo servicio y estado; no se implementan aquí gráficos,
overlay, FPS/frametimes ni control de ventiladores. El overlay puede no
aparecer sobre aplicaciones en pantalla completa exclusiva y algunos anticheat
pueden bloquear overlays externos; no se sortearán esas restricciones.

## Plataforma y mediciones

- SDK fijado: .NET 10.0.402; la UI declara `net10.0-windows`, Core, Hardware y
  pruebas apuntan a `net10.0`.
- xUnit se limita al proyecto de pruebas. No se incorpora un contenedor DI ni
  un framework MVVM.
- No hay mediciones de consumo de CPU/memoria ni comparaciones con otros
  productos. Consulte [docs/performance-validation.md](./docs/performance-validation.md).
