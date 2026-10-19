# Arquitectura

## Estructura actual

- `src/HardwareMonitor.Core`: modelos y validación independientes de WPF y de
  cualquier biblioteca de hardware.
- `src/HardwareMonitor.App`: anfitrión WPF de Windows. En la fase 1 solo
  muestra el estado explícito de la funcionalidad pendiente.
- `tests/HardwareMonitor.Tests`: pruebas automatizadas para las reglas de
  dominio.

La solución mantiene tres proyectos porque es el mínimo que separa dominio,
presentación y pruebas ejecutables. No se crean proyectos vacíos para
Hardware, UI u Overlay: se extraerán cuando cada módulo tenga implementación
real. En la fase 2, el adaptador de sensores quedará detrás de contratos propios
del dominio; ni la vista principal ni el overlay dependerán de
LibreHardwareMonitor.

## Flujo previsto para el motor de sensores

Una única instancia del motor coordinará el muestreo y publicará snapshots
inmutables compartidos. La ventana principal y el overlay consumirán ese mismo
estado; no abrirán dispositivos ni harán lecturas de hardware por separado.
El trabajo de sensores se ejecutará fuera del hilo de UI y tendrá cancelación,
control de concurrencia e historial acotado.

Las lecturas ausentes se representarán como no disponibles con su motivo, nunca
como cero ni mediante valores inventados. Se conservará la unidad y la marca de
tiempo de cada lectura. Los tipos y contratos del motor se introducirán junto
con su primera implementación en la fase 2, evitando interfaces sin uso.

## UI y overlay

La ventana principal y el futuro overlay consumirán modelos de estado, no
implementaciones de sensores. El overlay se añadirá como una superficie WPF
independiente solo cuando exista el motor compartido. La primera versión usará
las capacidades de renderizado de WPF; una tecnología de dibujo más compleja
requerirá mediciones que la justifiquen.

No se inyectará código en juegos ni se implementará FPS/frametime sin una
fuente mantenida y legalmente utilizable. El overlay puede no aparecer sobre
aplicaciones en pantalla completa exclusiva y algunos anticheat pueden
bloquear overlays externos; no se sortearán esas restricciones.

## Decisiones de plataforma y dependencias

- SDK fijado en `global.json`: .NET 10.0.402; .NET 10 es la rama LTS elegida.
- La UI declara `net10.0-windows`; el dominio y las pruebas apuntan a `net10.0`
  y no dependen del escritorio Windows.
- xUnit se limita al proyecto de pruebas. La configuración inicial usa
  bibliotecas de .NET y no incorpora un contenedor DI, framework MVVM ni
  dependencias de logging.
- El host emite un evento de diagnóstico de inicio mediante `System.Diagnostics.Trace`.
  El almacenamiento y la rotación de logs se decidirán al implementar la
  gestión de errores, sin registrar datos personales.

## Biblioteca candidata

Revisión inicial consultada el 19 de octubre de 2026:

- LibreHardwareMonitorLib `0.9.6` es la versión estable más reciente observada
  en NuGet durante la inspección.
- Su licencia principal upstream es MPL-2.0. El repositorio upstream también
  contiene avisos para componentes de terceros; se revisarán las licencias
  transitivas y los avisos de distribución antes de añadir la dependencia.
- El proyecto de la biblioteca declara `net10.0` como destino compatible.
- El README upstream advierte que algunos sensores requieren permisos de
  administrador. La aplicación seguirá funcionando sin elevación y mostrará
  el motivo de indisponibilidad cuando no pueda acceder a un sensor.

Referencias: [repositorio LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor),
[licencia](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/blob/master/LICENSE),
[paquete NuGet](https://www.nuget.org/packages/LibreHardwareMonitorLib).
