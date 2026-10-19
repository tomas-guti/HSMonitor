# Hoja de ruta

- [x] Fase 0: inspeccionar Windows, herramientas, SDK y espacio de trabajo;
      registrar plataforma, límites y decisiones iniciales.
- [x] Base de fase 1: solución, Core, anfitrión WPF, configuración inicial,
      diagnóstico de arranque, pruebas y documentación.
- [ ] Completar fase 1: ampliar configuración y pruebas cuando se concrete el
      motor, sin implementar sensores antes de su fase.
- [ ] Fase 2: adaptar detección/lectura de sensores mediante contratos propios
      y un único motor cancelable con snapshots compartidos e historial
      limitado.
- [ ] Fase 3: ventana principal, selección de sensores y gráficos ligeros.
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

La solución actual no contiene todavía adquisición de hardware, gráficos,
overlay, persistencia JSON ni publicación; esas capacidades pertenecen a sus
fases indicadas.
