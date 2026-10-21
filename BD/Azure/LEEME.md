# SQL para Azure SQL Database

En el editor de consultas de la **base de la aplicación ya creada en Azure**, ejecute estos archivos completos y en este orden:

1. `01_esquema_azure.sql`: crea las 42 tablas, sus claves y los índices incluidos. No contiene datos de ejemplo.
2. `02_datos_demo_pequenos.sql`: carga pocos ejemplos relacionados entre sí.

Los archivos no usan `USE`, `CREATE DATABASE`, `GO` ni comandos de SQLCMD. Funcionan dentro de la conexión a la base seleccionada. Cada archivo usa una transacción con reversión si ocurre un error.

El esquema crea tablas faltantes y conserva las existentes. Incluye ajustes aditivos del esquema original y permite `Ventas.id_pedido = NULL`, necesario para ventas rápidas. `Detalle_Pedido.cantidad` es INT, alineado con el modelo corregido. Si encuentra otro tipo en una instalación anterior, cancela la operación: no redondea cantidades ni borra datos para forzar la migración. No es una migración universal para cualquier esquema anterior.

Incluye `Envios_PDV`, `Subcuentas_Venta` y `Subcuenta_Detalle_Venta`, para que esas funciones puedan habilitarse cuando la app se conecte a esta base. Las asignaciones de mesa y las solicitudes repetidas usan las bitácoras existentes, como el código actual.

## Ejemplos incluidos

- Cuatro usuarios con sus empleados: administración, mesero, cocina y caja.
- Cuatro mesas, una caja, un proveedor, tres productos, siete insumos y tres recetas.
- Dos pedidos: uno abierto en AZ-02 y otro finalizado con su venta, detalles y cierre de caja del día anterior.
- El pedido pagado contiene dos casados y dos cappuccinos: total ₡13 600, recibido ₡14 000 y vuelto ₡400. El cierre parte de ₡20 000 y termina en ₡33 600.
- El pedido abierto contiene un casado y una limonada: total ₡6 050.
- El inventario descuenta las recetas de ambos pedidos una sola vez. El limón queda bajo el mínimo para mostrar la alerta.

Son datos **ficticios y verosímiles**, no información personal ni transacciones reales. Los correos usan el dominio reservado `.example`. Los ejemplos están identificados con `(demo)`, usuarios `demo.*` y marcadores `AZ-DEMO-*`. No se rellenan nóminas, vacaciones ni asistencias inventadas.

La caja de ejemplo queda cerrada, con una apertura histórica también cerrada. Para nuevas ventas, abra caja desde la web con el cajero que vaya a operar. Repetir los INSERT no vuelve a abrir la caja ni cambia contraseñas, saldos o estados ya existentes.

## Acceso de demostración

| Rol | Usuario |
| --- | --- |
| Administrador | `demo.admin` |
| Mesero | `demo.mesero` |
| Cocinero | `demo.cocina` |
| Cajero | `demo.caja` |

Contraseña inicial de los cuatro: **`ColibriDemo2026!`**. Los hashes usan el formato actual de PasswordHelper: PBKDF2 SHA256 con 600 000 iteraciones y un salt diferente por cuenta. Son credenciales públicas de demostración; cambie las contraseñas o desactive estas cuentas al terminar las pruebas. No se reemplazan cuentas existentes.

## Conexión de la aplicación

Ejecutar los SQL no cambia Web.config ni mueve la aplicación a Azure. Configure `ColibriDbContext` o la variable de entorno `COLIBRI_CONNECTION_STRING` del servidor web con la conexión de su base. Plantilla:

```text
Server=tcp:TU_SERVIDOR.database.windows.net,1433;Initial Catalog=TU_BASE;User ID=TU_USUARIO;Password=TU_CLAVE;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;Connect Timeout=30;
```

Sustituya los campos con los datos reales de su conexión. No publique la contraseña en Git. El lanzador local `tools/Start-Colibri.ps1` sigue destinado a COLIBRI en SQL Express local y no sirve para arrancar con esta conexión de Azure.

## Validación

Se ejecutaron ambos archivos dos veces sobre una base local temporal, sin tocar COLIBRI ni Azure. Se comprobaron las 42 tablas, los tipos y lecturas EF, siete cargas de controladores, las cuatro contraseñas demo, los conteos y el stock sin cambios al repetir. La base temporal se elimina al terminar. La evidencia está en `validacion-local.txt`.

Esto valida T-SQL y compatibilidad local con la aplicación. La ejecución en Azure depende además de los permisos de la conexión y del esquema previo de la base destino. No se probó contra un servidor Azure real en esta sesión.

Para repetir la validación local con SQL Express: `.\tools\qa\Test-AzureScripts.ps1` desde la raíz del proyecto.
