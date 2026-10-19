# Registro de cambios

## 0.2.0 - Fase 2

- Se añadieron contratos inmutables de sensores con IDs de hardware y sensor,
  métrica, valor nullable, unidad, disponibilidad, marca temporal y errores
  estructurados.
- Se añadió `HardwareMonitor.Hardware` con LibreHardwareMonitorLib `0.9.6` y
  traducción de tipos/unidades a contratos propios.
- Se añadió un servicio de muestreo configurable, cancelable y sin ciclos
  solapados, que publica un snapshot consistente de solo lectura.
- Se añadieron pruebas de contratos, normalización, sensores sin valor, errores,
  muestreo, cancelación y ciclo de vida usando fuentes controladas.
- Se documentaron dependencias, licencias, permisos y limitaciones detectadas.
  La UI aún no muestra las lecturas.

## 0.1.0 - Base de fase 1

- Se creó una solución .NET 10 con Core independiente, anfitrión WPF y pruebas
  xUnit.
- Se añadió la configuración tipada y validable del intervalo de muestreo,
  con valor inicial de 1000 ms.
- La ventana inicial indica que la lectura de sensores aún no está habilitada;
  no se generan métricas simuladas.
- Se documentaron decisiones de arquitectura y el protocolo de medición de
  rendimiento pendiente.
