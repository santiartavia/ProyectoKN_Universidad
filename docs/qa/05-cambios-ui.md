# Cambios de interfaz

Se mantuvo Razor y el sistema existente de componentes. La hoja `RestauranteVistas/Content/Site.css` aplica verde profundo, superficies neutras, acentos cálidos, tarjetas, tablas, foco visible y estados coherentes a todos los módulos que usan el layout y a autenticación.

El layout compartido incorpora navegación móvil desplegable y resalta la ruta activa. `Scripts/site.js` añade búsqueda y paginación a tablas extensas, desplazamiento horizontal, estados de envío y bloqueo de envíos repetidos. Las operaciones importantes muestran confirmación. La idempotencia real se valida además en el servidor; desactivar un botón no sustituye esa garantía.

El dashboard consulta ventas completadas del día, pedidos activos, mesas ocupadas y empleados desde SQL Server. No se agregaron cifras estáticas. Las capturas en esta carpeta muestran datos de prueba identificados como QA, obtenidos de la base aislada.

Mesas permite seleccionar un mesero autorizado y confirmar explícitamente una reasignación. La pantalla de cobro muestra precio unitario, cantidades, subtotales, total y estado de pago. La gestión de empleados conserva sus pestañas por tarea, elimina la exclusión artificial del empleado número 1 y entrega una credencial temporal aleatoria, visible una sola vez al administrador. Se mantiene el catálogo de roles principales existente.

Evidencia: `dashboard-desktop.png`, `navegacion-mobile.png` y los resultados del navegador. La revisión visual de dos capturas no equivale a una certificación de accesibilidad o compatibilidad en todos los equipos del restaurante.
