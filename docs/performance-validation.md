# Validación de rendimiento

## Estado de las mediciones

No se han tomado mediciones de CPU, memoria, tiempo de inicio ni frecuencia
real de adquisición. El motor ya está implementado, pero la ventana todavía no
lo inicia y esta validación no realizó una sesión de lectura sobre hardware
físico. No se hacen afirmaciones de consumo frente a NZXT CAM ni otros
productos.

## Protocolo para la fase 6

1. Registrar versión de Windows, CPU, RAM, GPU, resolución, plan de energía,
   versión publicada de la aplicación y configuración de muestreo.
2. Medir una línea base sin la aplicación y después ejecutar la misma versión
   Release durante al menos cinco minutos en reposo; repetir cada escenario
   tres veces y documentar la herramienta y los contadores utilizados.
3. Registrar CPU del proceso, bytes privados, memoria de trabajo, tiempo desde
   inicio del proceso hasta ventana utilizable, intervalos de actualización
   observados, errores y estabilidad durante la sesión.
4. Separar tiempo de inicio en frío y en caliente, y mantener constantes las
   aplicaciones en segundo plano. Informar mediciones y variación, sin fijar
   presupuestos arbitrarios ni extrapolar resultados a otro hardware.
5. Para comparar con otra aplicación, usar la misma máquina, carga, duración,
   sensores disponibles y condiciones; describir diferencias de funcionalidad
   y configuración.

La frecuencia real puede medirse ahora sobre una ejecución Release del motor,
registrando el intervalo configurado, número de ciclos y tiempos de lectura.
Las pruebas de unidad no sustituyen estas mediciones en Windows.
