# Instrucciones de desarrollo

- Mantén `HardwareMonitor.Core` independiente de WPF y de la biblioteca de
  sensores. Añade proyectos separados cuando exista implementación concreta
  que justifique la separación.
- Conserva nullable reference types y el destino WPF `net10.0-windows`.
- No inventes lecturas: representa explícitamente valores no disponibles,
  unidades y motivos.
- Implementa un único motor cancelable de muestreo y comparte sus snapshots
  entre la ventana principal y el overlay. No leas hardware desde controles.
- Mantén las lecturas fuera del hilo UI, limita el historial y evita consultas
  más frecuentes que el intervalo configurado.
- Usa datos simulados únicamente en pruebas o en un modo de desarrollo
  explícito, separado de producción.
- No añadas FPS/frametimes, inyección en juegos, telemetría, cuentas ni
  dependencias sin una fuente fiable y una justificación documentada.
- Revisa compatibilidad, mantenimiento y licencias de dependencias antes de
  añadirlas; conserva los avisos de licencia necesarios para distribuir.
- Valida cambios con `dotnet build .\HardwareMonitor.sln` y
  `dotnet test .\HardwareMonitor.sln`; añade pruebas para lógica de dominio.
- Documenta limitaciones y mediciones reales. No afirmes rendimiento relativo
  sin benchmarks reproducibles.
