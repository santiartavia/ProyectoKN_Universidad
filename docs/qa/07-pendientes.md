# Pendientes y límites de aceptación

- La matriz conserva los 380 criterios y requisitos. Aprobar los 14 recorridos principales no prueba automáticamente todos sus escenarios negativos, permisos y mensajes individuales.
- No se modificó la base operativa del restaurante. La conexión predeterminada apunta a otro equipo. La migración debe revisarse y aplicarse explícitamente en el entorno que realmente se utilizará.
- Las pruebas usaron un esquema generado por EF en una base nueva. Las variantes históricas de SQL del repositorio pueden requerir reconciliación adicional de columnas, restricciones y procedimientos antes de actualizar una instalación antigua.
- No se ha medido disponibilidad durante una jornada completa, recuperación ante fallos, carga simultánea sostenida ni los límites de dos y tres segundos en equipos reales.
- Los respaldos automáticos requieren definir servidor, carpeta de destino, retención y programación. No se configuró una tarea del sistema sin esos datos.
- Las inconsistencias entre algunos contextos y resultados del Excel se conservan en la matriz. Deben resolverse con el responsable de requisitos antes de certificar esos escenarios contradictorios.
- No se certifica una revisión integral de accesibilidad, dispositivos del restaurante ni despliegue HTTPS de producción.
- Consulte `04-resultados-pruebas.md` y `resultados-91.json` para los bloqueos y fallas concretas de la ejecución más reciente. Ninguna prueba bloqueada se considera aprobada.
