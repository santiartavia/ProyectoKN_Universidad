# Conexión a COLIBRI existente — 2026-10-20

La aplicación local se inicia con `tools/Start-Colibri.ps1`, usando la conexión de Web.config a COLIBRI en SQLEXPRESS local. El lanzador elimina la conexión QA del entorno del nuevo proceso y reinicia solo el IIS Express de este proyecto en el puerto 51235. No modifica la base.

Validación realizada: compilación correcta; comprobación de solo lectura de los 39 conjuntos EF presentes, sin errores; carga de datos de Home, Mesas, Pedidos, Cocina, Cajero, PuntoVenta e Inventario correcta. La comprobación invoca las acciones Index directamente: no certifica el renderizado de todas las vistas ni ejecuta cobros o cambios sobre los registros existentes. Los resultados están en `.qa-runtime/check-colibri/resultado.txt`.

COLIBRI tenía cinco usuarios existentes, 13 mesas y cuatro productos. No se agregaron usuarios, datos QA, saldos, columnas ni tablas. Las credenciales de QA corresponden exclusivamente a la base aislada anterior; para COLIBRI se usan las cuentas y contraseñas existentes.

La asignación de mesero usa Bitacora_Usuarios_RRHH y los pedidos activos. Las nuevas asignaciones registran el identificador del empleado en valor_nuevo. El control transaccional de solicitudes repetidas usa Bitacora_Acceso. No se requiere aplicar BD/qa_integridad.sql.

La base no contiene Envios_PDV, Subcuentas_Venta ni Subcuenta_Detalle_Venta. La aplicación detecta esas ausencias, omite las consultas y oculta/rechaza esas operaciones de venta rápida. Se conservan Pedidos y la división de cuentas de pedidos en Caja usando las tablas existentes. Esta adaptación respeta la estructura disponible; no habilita mediante datos ficticios las funciones que requieren esas tablas.

Los resultados históricos de las pruebas QA documentados anteriormente corresponden a la base aislada y al código de esa ejecución. No deben presentarse como una ejecución de operaciones sobre COLIBRI ni como una certificación completa de esta adaptación.
