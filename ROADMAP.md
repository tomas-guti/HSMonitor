# Hoja de ruta

- [x] Fase 0: inspeccionar Windows, herramientas, SDK y espacio de trabajo;
      registrar plataforma, límites y decisiones iniciales.
- [x] Base de fase 1: solución, Core, anfitrión WPF, configuración inicial,
      diagnóstico de arranque, pruebas y documentación.
- [x] Fase 2: contratos tipados independientes del proveedor, adaptador
      LibreHardwareMonitor, muestreo cancelable de instancia única, snapshots
      compartidos y pruebas sin hardware real.
- [ ] Fase 3: conectar el servicio a la ventana principal, selección de sensores
      y gráficos ligeros.
- [ ] Fase 4: overlay independiente, configurable y sin lecturas duplicadas.
- [ ] Fase 5: perfiles, atajos, persistencia/migración de configuración y
      gestión explícita de errores recuperables.
- [ ] Fase 6: pruebas de lógica, análisis de estabilidad y mediciones
      reproducibles; no comparar rendimiento con otros productos sin pruebas
      equivalentes.
- [ ] Fase 7: publicación portable autocontenida; evaluar un instalador
      separado cuando la aplicación esté funcional.
- [ ] Fase 8: evaluar fuentes mantenidas y legales para FPS/frametimes como
      ampliación independiente.

La fase 2 no conecta todavía el motor a la ventana WPF ni implementa gráficos,
overlay, persistencia JSON, control de ventiladores o publicación.
