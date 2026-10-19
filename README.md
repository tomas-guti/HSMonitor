# Hardware Monitor

Aplicación de escritorio para Windows destinada a mostrar métricas reales de
hardware en una ventana principal y, en fases posteriores, en un overlay.

## Estado

La solución contiene la base compilable de la fase 1: una biblioteca de dominio
sin dependencias de WPF, una aplicación WPF oscura y una suite de pruebas para
la configuración del intervalo de muestreo. **Todavía no lee sensores** y no
presenta datos simulados como reales.

## Requisitos y compilación

- Windows 10 o posterior, x64.
- SDK de .NET 10 LTS (soporte hasta el 14 de noviembre de 2028); el repositorio
  fija `10.0.402` y `global.json` permite bandas de características posteriores
  dentro de .NET 10.
- El destino WPF está declarado explícitamente como `net10.0-windows`.

Desde PowerShell, en la raíz del repositorio:

```powershell
dotnet restore .\HardwareMonitor.sln
dotnet build .\HardwareMonitor.sln --configuration Release --no-restore
dotnet test .\HardwareMonitor.sln --configuration Release --no-build
dotnet run --project .\src\HardwareMonitor.App\HardwareMonitor.App.csproj
```

La ventana inicial indica que todavía no hay adquisición de sensores. El
intervalo de muestreo base es 1000 ms y el modelo rechaza valores fuera de
250–60000 ms para prevenir configuraciones accidentales extremas; el motor que
aplicará esa configuración se implementará en la fase 2.

## Sensores y permisos

LibreHardwareMonitor es el candidato inicial para la fase de hardware: la
versión estable consultada es `0.9.6`, su licencia principal es MPL-2.0 y el
proyecto declara compatibilidad con .NET 10. Algunos accesos a sensores
requieren privilegios de administrador según su documentación. La aplicación
no se elevará por defecto; se identificarán claramente los sensores no
disponibles y los permisos concretos necesarios. La biblioteca y sus
dependencias no forman parte todavía de esta solución.

No se implementan FPS, frametimes ni integración con procesos de juegos. No se
inyecta código en juegos.

## Arquitectura y planificación

Consulta [ARCHITECTURE.md](./ARCHITECTURE.md) para las decisiones técnicas,
[ROADMAP.md](./ROADMAP.md) para el alcance por fases y
[docs/performance-validation.md](./docs/performance-validation.md) para el
protocolo de medición. Los cambios relevantes se registran en
[CHANGELOG.md](./CHANGELOG.md).
