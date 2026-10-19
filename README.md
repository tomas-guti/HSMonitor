# Hardware Monitor

Aplicación de escritorio para Windows destinada a mostrar métricas reales de
hardware en una ventana principal y, en fases posteriores, en un overlay.

## Estado

La solución contiene las fases 1 y 2: Core independiente, anfitrión WPF,
adaptador de hardware LibreHardwareMonitor y un motor de muestreo cancelable
con snapshots compartidos. El motor todavía no está conectado a la UI; la
ventana no presenta métricas ni datos simulados.

## Requisitos y compilación

- Windows 10 o posterior, x64.
- SDK de .NET 10 LTS (soporte hasta el 14 de noviembre de 2028); el repositorio
  fija `10.0.402` y `global.json` permite bandas de características posteriores
  dentro de .NET 10.
- El destino WPF está declarado explícitamente como `net10.0-windows`.

Desde PowerShell, en la raíz del repositorio:

```powershell
dotnet restore .\HardwareMonitor.sln
dotnet build .\HardwareMonitor.sln --configuration Release
dotnet test .\HardwareMonitor.sln --configuration Release
dotnet run --project .\src\HardwareMonitor.App\HardwareMonitor.App.csproj
```

El servicio se crea con una instancia de `MonitoringService` y una fuente
`LibreHardwareMonitorSource`. `StartAsync` inicia un solo worker y
`CurrentSnapshot` expone el estado inmutable más reciente a todos los
consumidores de esa instancia. `StopAsync` cancela y espera el ciclo activo;
`DisposeAsync` también cierra el adaptador. El muestreo comienza inmediatamente
y espera el intervalo configurado al finalizar cada ciclo. El valor inicial es
1000 ms, con límites de 250–60000 ms. La fuente de hardware es síncrona; el
motor la ejecuta fuera del hilo UI. Cada servicio se inicia una sola vez; para
reanudar tras detenerlo se crea una instancia nueva.

## Sensores y permisos

La infraestructura fija LibreHardwareMonitorLib `0.9.6` (MPL-2.0; grupo
`net10.0` en su nuspec). La aplicación no se eleva por defecto ni solicita
permisos. Si el hardware o sus drivers deniegan acceso, el motor publica un
issue `AccessDenied`/`HardwareUnavailable` sin detener la lectura de otros
dispositivos. La disponibilidad depende del hardware, firmware, drivers y
permisos del equipo; no se garantiza una categoría concreta. Un valor nulo se
marca no disponible; cero es una lectura válida. Ver
[THIRD-PARTY-NOTICES.md](./THIRD-PARTY-NOTICES.md) para el cierre transitivo y
los avisos de licencia.

Las llamadas de LibreHardwareMonitor no admiten cancelación mientras una
operación de driver está en curso. El servicio revisa cancelación entre
dispositivos y espera la operación activa antes de liberar la fuente. La UI
actual no inicia el motor, y esta implementación no ha sido probada contra
sensores físicos.

No se implementan FPS, frametimes ni integración con procesos de juegos. No se
inyecta código en juegos.

## Arquitectura y planificación

Consulta [ARCHITECTURE.md](./ARCHITECTURE.md) para las decisiones técnicas,
[ROADMAP.md](./ROADMAP.md) para el alcance por fases y
[docs/performance-validation.md](./docs/performance-validation.md) para el
protocolo de medición. Los cambios relevantes se registran en
[CHANGELOG.md](./CHANGELOG.md).
