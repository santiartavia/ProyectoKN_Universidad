# Corrección de redirecciones y compatibilidad con COLIBRI

## Reproducción y causa

Se reprodujo la secuencia `/Pedidos -> /Pedidos` mediante solicitudes autenticadas. Cada GET devolvía HTTP 302 a la misma ruta. El controlador base atrapaba también las excepciones de lectura y redirigía a Index, ocultando la causa e impidiendo cargar la pantalla.

Al quitar ese comportamiento circular apareció la causa: `Detalle_Pedido.cantidad` es INT en COLIBRI, mientras que DetallePedido.Cantidad era decimal. La tabla vacía ocultaba el problema; al guardar el primer detalle, su lectura fallaba en Pedidos, Mesero, Cocina y Caja.

Referencia revisada: `D:\Usuario\Downloads\INSTALAR_BASE_DATOS_COMPLETA.sql`, SHA256 `8956CF09B9323A7AEAFCF95642E65AF329EB89B90970CD23DE7BB79318479C25`. Se leyó como definición del esquema; no se ejecutó su instalación ni sus semillas.

## Correcciones

- Cantidad del detalle es int, alineada con la base. Los puntos de entrada rechazan fracciones y cantidades fuera de rango antes de guardarlas.
- Las excepciones GET dejan de redirigir a la misma página. El filtro global devuelve HTTP 500 con una referencia y registra el diagnóstico en App_Data/errores.log, sin exponer el detalle SQL al usuario.
- Se corrigió la vista de Caja que usaba Math.Ceiling sobre una cantidad ahora entera y provocaba una sobrecarga ambigua en Razor.
- En la base, Bitacora_Pedidos.estado_nuevo es obligatorio. SaveChanges completa el estado real del pedido cuando una edición o un pago no cambia su estado; no inventa un estado predeterminado.
- Se quitó del menú de Cocina un enlace a Pedidos para el que ese rol no tiene permisos.
- La comprobación de esquema compara los tipos incluso en tablas vacías, para detectar este fallo antes de insertar el primer registro.

## Validación final

| Comprobación | Resultado |
| --- | --- |
| Compilación de la solución | Correcta |
| Pruebas unitarias, incluida regresión del bucle GET | 15 aprobadas, 0 fallidas |
| Tipos SQL y lectura de los conjuntos EF existentes | 39 conjuntos correctos, 0 errores |
| Carga de datos de controladores principales | 7 correctos |
| Navegación HTTP autenticada, permisos y cierre de sesión | 51 aprobadas, 0 fallidas |
| Interfaz real en Edge, formulario y menú móvil | 7 aprobadas, 0 errores JavaScript |
| Crear, leer, editar y cancelar pedido en COLIBRI | Correcto, con rollback |

Las 51 comprobaciones incluyen administrador (28), mesero (8), cocinero (6) y cajero (9). Se validaron las respuestas finales, la ausencia de bucles, las denegaciones HTTP 403 para Usuarios y la redirección al login después del cierre de sesión. Las pruebas HTTP renderizan las vistas Razor; la prueba de Edge verifica además JavaScript y navegación móvil.

La prueba de escritura usa una transacción explícita que se revierte siempre. Comprueba que el detalle recién creado puede materializarse, que la edición conserva cantidades enteras y que cancelar restaura el inventario. Al terminar comprueba que no quedó un pedido de prueba y que el stock coincide con el anterior. SQL Server puede consumir valores de identidad aun con rollback. No se borró el pedido previo del usuario ni se modificó la estructura de COLIBRI.

Evidencia: resultado-colibri-db.txt, resultado--unit.txt, resultados-colibri-web-{admin,mesero,cocinero,cajero}.json y resultados-colibri-ui.json. Los fallos de diagnóstico previos permanecen en el log local; las tablas anteriores describen la ejecución final.

## Repetición

```powershell
.\tools\qa\Run-Tests.ps1
.\tools\qa\Check-Colibri.ps1 -Pedido
node tools/qa/browser-colibri.cjs
node tools/qa/browser-colibri-ui.cjs
```

Los scripts web usan las cuentas demo y la contraseña documentada en el SQL adjunto. Si se cambiaron, configure COLIBRI_TEST_USER y COLIBRI_TEST_PASSWORD; COLIBRI_TEST_ROLE selecciona admin, mesero, cocinero o cajero. No cambian contraseñas ni crean cuentas. El inicio de sesión normal actualiza su sesión, auditoría y, cuando corresponde, el formato del hash.

Esta evidencia cubre los escenarios ejecutados sobre COLIBRI. No constituye una certificación de todas las combinaciones de entradas ni de todos los procesos contables o laborales. Las funciones de ventas rápidas que requieren tablas ausentes continúan condicionadas a la estructura existente, como se documentó en 08-base-colibri-existente.md.
