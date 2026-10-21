# Resultados reales de pruebas

Plan QA: **14/14 casos aceptados; 91/91 pasos aprobados** en el resultado consolidado de la ejecución automatizada local.

Se realizó una reprueba específica de C14 después de corregir la representación de cantidades en los campos HTML numéricos. Los demás resultados proceden de la pasada completa sobre el mismo fixture. No se modificaron los resultados esperados del Excel.

La aceptación corresponde al fixture y al esquema de la base aislada `Colibri_QA_20261020`. No se certifica el cumplimiento de los 380 criterios individuales, el despliegue operativo ni los requisitos de disponibilidad, carga o respaldo.

| Verificación | Resultado | Evidencia |
|---|---|---|
| Compilación | Solución compilada con MSBuild de Visual Studio | `compilacion.txt` |
| Unitarias y seguridad sin BD | 14 aprobadas, 0 fallidas | `resultado--unit.txt` |
| Integración, seguridad y regresión SQL | 25 aprobadas, 0 fallidas | `resultado--integration.txt` |
| Navegación y permisos en Edge | 42 aprobadas de 42 | `resultados-browser.json` |
| Plan original | 14/14 casos, 91/91 pasos | `resultados-91.json` |

Método: inicio de sesión mediante interacción del navegador; lectura de pantallas, selección de opciones, envío HTTP de los formularios reales con sus tokens CSRF y claves de idempotencia, y consultas posteriores para verificar persistencia. No todos los envíos se realizan haciendo clic en el botón: el ejecutor envía los campos del formulario mediante la sesión HTTP del navegador. Los ensayos de controladores verifican adicionalmente la base SQL, los totales, la reversión y los efectos entre módulos.

Los datos «QA» y «E2E» son fixtures exclusivos de la base de pruebas; no se agregaron valores ficticios al dashboard ni a la base operativa.

## Resumen por caso

| Caso | Pasos aprobados | Resultado |
|---|---|---|
| C1 | 7/7 | ACEPTADO |
| C2 | 7/7 | ACEPTADO |
| C3 | 7/7 | ACEPTADO |
| C4 | 7/7 | ACEPTADO |
| C5 | 7/7 | ACEPTADO |
| C6 | 6/6 | ACEPTADO |
| C7 | 5/5 | ACEPTADO |
| C8 | 5/5 | ACEPTADO |
| C9 | 6/6 | ACEPTADO |
| C10 | 6/6 | ACEPTADO |
| C11 | 7/7 | ACEPTADO |
| C12 | 7/7 | ACEPTADO |
| C13 | 7/7 | ACEPTADO |
| C14 | 7/7 | ACEPTADO |

## Evidencia por paso

### C1

Precondiciones originales: El sistema y la base de datos deben estar disponibles. Debe existir un usuario registrado y activo con credenciales válidas y un rol asignado.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Acceder a la pantalla de inicio de sesión del sistema. | El sistema muestra correctamente los campos para ingresar las credenciales. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Ingresar un usuario registrado en el sistema. | El sistema permite ingresar el usuario sin presentar errores. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Ingresar la contraseña correspondiente al usuario. | La contraseña se muestra protegida y puede ser ingresada correctamente. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Presionar el botón "Iniciar sesión". | El sistema valida las credenciales ingresadas. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Verificar el acceso al sistema con las credenciales válidas. | El usuario ingresa correctamente al sistema. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Revision que el usuario acceda a opciones segun su rol | El sistema muestra las funcionalidades autorizadas según el rol asignado al usuario. | Resultado comprobado en la página MVC real | APROBADA |
| 7 | Cerrar la sesión del usuario. | El sistema finaliza la sesión y redirige al usuario a la pantalla de inicio de sesión. | Resultado comprobado en la página MVC real | APROBADA |

### C2

Precondiciones originales: El sistema y la base de datos deben estar disponibles. Deben existir usuarios activos con diferentes roles registrados y permisos previamente asignados.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Iniciar sesión con un usuario registrado. | El sistema permite el acceso con credenciales válidas. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Identificar el rol asignado al usuario. | El sistema reconoce correctamente el rol del usuario. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Acceder al menú principal del sistema. | Se muestran únicamente las opciones autorizadas para el rol. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Ingresar a un módulo autorizado. | El sistema permite el acceso al módulo. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Intentar acceder a una funcionalidad restringida para el rol. | El sistema impide el acceso a la funcionalidad no autorizada. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Iniciar sesión con un usuario de otro rol. | El sistema presenta las opciones correspondientes al nuevo rol. | Resultado comprobado en la página MVC real | APROBADA |
| 7 | Cerrar la sesión. | El sistema finaliza la sesión y restringe nuevamente el acceso. | Resultado comprobado en la página MVC real | APROBADA |

### C3

Precondiciones originales: El sistema y la base de datos deben estar disponibles. El usuario debe haber iniciado sesión y contar con los permisos necesarios para gestionar empleados.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Ingresar al módulo de empleados. | El sistema muestra correctamente la lista de empleados registrados. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Seleccionar la opción para registrar un empleado. | El sistema muestra el formulario de registro. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Completar los datos requeridos del empleado. | El sistema permite ingresar la información solicitada. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Guardar el nuevo empleado. | El sistema registra al empleado en la base de datos. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Buscar y seleccionar el empleado registrado. | El sistema muestra la información correspondiente. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Modificar los datos del empleado y guardar los cambios. | El sistema actualiza correctamente la información. | Resultado comprobado en la página MVC real | APROBADA |
| 7 | Consultar nuevamente el empleado. | El sistema presenta la información actualizada. | Resultado comprobado en la página MVC real | APROBADA |

### C4

Precondiciones originales: Deben existir empleados registrados y activos en el sistema. El usuario encargado debe contar con permisos para administrar turnos, asistencias y vacaciones.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Ingresar al módulo de gestión de personal. | El sistema muestra las opciones de turnos, asistencia y vacaciones. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Seleccionar un empleado registrado. | El sistema carga correctamente la información del empleado. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Asignar un turno al empleado. | El sistema registra el turno asociado al empleado seleccionado. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Registrar la asistencia del empleado. | El sistema almacena correctamente el registro de asistencia. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Registrar una solicitud o período de vacaciones. | El sistema permite registrar las fechas correspondientes. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Consultar la información del empleado. | Se muestran correctamente los turnos, asistencias y vacaciones registrados. | Resultado comprobado en la página MVC real | APROBADA |
| 7 | Verificar la persistencia de los datos registrados. | La información permanece almacenada y relacionada con el empleado correspondiente. | Resultado comprobado en la página MVC real | APROBADA |

### C5

Precondiciones originales: Deben existir mesas y empleados con rol de mesero registrados en el sistema. El usuario debe contar con los permisos necesarios para realizar asignaciones.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Ingresar al módulo de gestión de mesas. | El sistema muestra las mesas registradas y su estado actual. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Seleccionar una mesa disponible. | El sistema permite seleccionar la mesa para su asignación. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Consultar los meseros disponibles. | El sistema muestra los meseros que pueden ser asignados. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Seleccionar un mesero para la mesa. | El sistema permite asociar el mesero con la mesa seleccionada. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Confirmar la asignación. | El sistema registra la relación entre la mesa y el mesero. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Consultar nuevamente la mesa. | El sistema muestra el mesero asignado y el estado actualizado de la mesa. | Resultado comprobado en la página MVC real | APROBADA |
| 7 | Liberar la mesa al finalizar la atención. | La mesa cambia a estado disponible y queda habilitada para una nueva asignación. | Resultado comprobado en la página MVC real | APROBADA |

### C6

Precondiciones originales: El área de cocina debe de tener el permiso de tener visualización de las órdenes y los meseros el permiso para hacer las distintas solicitudes de los clientes.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | El mesero toma la orden del cliente en una mesa | El sistema crea la orden del cliente de forma óptima  | Resultado comprobado en la página MVC real | APROBADA |
| 2 | El mesero agrega los distintos pedidos del cliente para una misma orden | El sistema va agregando los distintos pedidos del mismo cliente a una orden específica si el cliente así lo desea | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Enviar la comanda a la cocina | El mesero envía la comanda a la cocina para que puedan comenzar a realizar los platillos | Resultado comprobado en la página MVC real | APROBADA |
| 4 | El pedido queda en estado de "en proceso" | El sistema pone en estado de "en proceso" el pedido esté terminado | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Cambiar el pedido a "Listo" | El área de cocina avisa a que el pedido está terminado en el sistema | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Registrar el pedido como "Entregado" | Los meseros entregan la comanda al cliente y registran el pedido como "entregado" | Resultado comprobado en la página MVC real | APROBADA |

### C7

Precondiciones originales: Orden registrada como entregada, datos de la orden. 

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | La orden está lista para ser pagada | El sistema muestra la orden como entregada y lista para ser pagada | Resultado comprobado en la página MVC real | APROBADA |
| 2 | El sistema muestra los datos de la orden | El sistema muestra los detalles de la orden, como lo es el nombre del platillo y su monto | Resultado comprobado en la página MVC real | APROBADA |
| 3 | El sistema muestra el monto total de la orden | El sistema muestra el total de pago que debe realizar el cliente por la orden solicitada por el mismo | Resultado comprobado en la página MVC real | APROBADA |
| 4 | El sistema da las distintas opciones de pago | El sistema muestra las distintas formas de pago disponibles  | Resultado comprobado en la página MVC real | APROBADA |
| 5 | El sistema registra la orden como pagada | El sistema registra la orden como pagada al finalizar el proceso de pago | Resultado comprobado en la página MVC real | APROBADA |

### C8

Precondiciones originales: El sistema debe permitir crear cuentas y subcuentas, aparte de permitir más de un producto. 

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | La cuenta tiene varios pedidos  | El sistema permite que una cuenta tenga más de un producto  | Resultado comprobado en la página MVC real | APROBADA |
| 2 | El cliente o clientes desean dividir la cuenta por productos consumidos | El cliente o clientes dividen toda la orden en varias subcuentas  | Resultado comprobado en la página MVC real | APROBADA |
| 3 | La cuenta está dividida en varias subcuentas | El sistema permite crear varias subcuentas para una cuenta general | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Visualización de cada subcuenta en el sistema  | El sistema refleja cada una de las subcuentas de una misma cuenta | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Subcuentas listas para ser pagadas | Cada subcuenta se puede pagar por individual completando al final la cuenta principal | Resultado comprobado en la página MVC real | APROBADA |

### C9

Precondiciones originales: Datos ya en el sistema para poder llevar control del inventario, permisos para usuarios con roles de administrador a empleados autorizados

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Ingresar al módulo de inventario. | El sistema permite el ingreso y muestra los insumos registrados. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Consultar las existencias de los insumos. | El sistema muestra las cantidades disponibles de cada insumo. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Consultar una receta registrada. | El sistema muestra los insumos y cantidades asociados a la receta. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Registrar un pedido con un producto asociado a una receta. | El sistema registra el pedido e identifica los insumos necesarios. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Procesar el pedido y verificar el rebajo de existencias. | El sistema descuenta automáticamente los insumos utilizados según la receta. | Leche: 7.25 → 7 L, receta 0.25 L por vaso | APROBADA |
| 6 | Consultar nuevamente el inventario. | El sistema muestra las existencias actualizadas después del rebajo. | Resultado comprobado en la página MVC real | APROBADA |

### C10

Precondiciones originales: Deben existir insumos registrados en el inventario con cantidades disponibles y niveles mínimos de stock previamente establecidos.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Ingresar al módulo de inventario. | El sistema permite el acceso y muestra los insumos registrados. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Consultar las existencias actuales de los insumos. | El sistema muestra correctamente la cantidad disponible de cada insumo. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Seleccionar un insumo con existencia cercana al stock mínimo. | El sistema muestra la cantidad disponible y el nivel mínimo establecido. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Realizar un movimiento que reduzca la existencia por debajo del mínimo. | El sistema actualiza la cantidad disponible del insumo. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Verificar la detección de bajo stock. | El sistema identifica el insumo que alcanzó o quedó por debajo del stock mínimo. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Consultar el apartado de insumos con bajo stock. | El sistema muestra los insumos que requieren reabastecimiento. | Resultado comprobado en la página MVC real | APROBADA |

### C11

Precondiciones originales: El usuario debe haber iniciado sesión con los permisos correspondientes. La caja debe encontrarse disponible para realizar la apertura y deben existir datos necesarios para registrar los movimientos.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Ingresar al módulo de caja. | El sistema muestra las opciones disponibles para la gestión de caja. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Seleccionar la opción de apertura de caja. | El sistema muestra el formulario para realizar la apertura. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Ingresar el monto inicial y confirmar la apertura. | El sistema registra correctamente la apertura y el monto inicial. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Registrar un egreso de caja. | El sistema permite ingresar el monto, concepto y datos correspondientes al egreso. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Consultar los movimientos de la caja. | El sistema muestra correctamente los movimientos realizados. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Seleccionar la opción de cierre de caja. | El sistema calcula y muestra la información correspondiente al cierre. | Resultado comprobado en la página MVC real | APROBADA |
| 7 | Confirmar el cierre de caja. | El sistema registra el cierre y finaliza las operaciones de la caja. | Resultado comprobado en la página MVC real | APROBADA |

### C12

Precondiciones originales: El usuario debe contar con permisos para acceder a los reportes. Deben existir datos previamente registrados en el sistema para realizar las consultas y generar los reportes.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Ingresar al módulo de reportería. | El sistema muestra las opciones de reportes disponibles. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Seleccionar el tipo de reporte requerido. | El sistema permite configurar los parámetros del reporte. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Ingresar los parámetros o filtros de consulta. | El sistema acepta correctamente los criterios ingresados. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Generar el reporte. | El sistema procesa la solicitud y muestra los datos correspondientes. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Verificar la información presentada. | Los datos mostrados coinciden con la información registrada en el sistema. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Seleccionar la opción de exportación. | El sistema permite seleccionar el formato disponible para exportar. | Resultado comprobado en la página MVC real | APROBADA |
| 7 | Exportar y verificar el archivo generado. | El archivo se genera correctamente y contiene la información del reporte. | Resultado comprobado en la página MVC real | APROBADA |

### C13

Precondiciones originales: Deben existir empleados registrados con la información necesaria para el cálculo de nómina. Los parámetros salariales requeridos deben encontrarse previamente configurados en el sistema.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Ingresar al módulo de nómina. | El sistema muestra las opciones correspondientes al cálculo de nómina. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Seleccionar el período de nómina correspondiente. | El sistema permite seleccionar el período que se desea procesar. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Consultar los empleados incluidos en el cálculo. | El sistema muestra los empleados correspondientes al período. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Verificar los parámetros utilizados para el cálculo. | El sistema utiliza los parámetros previamente configurados. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Ejecutar el cálculo de nómina. | El sistema procesa la información y calcula los montos correspondientes. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Revisar los resultados obtenidos por empleado. | Los montos calculados corresponden con los datos y parámetros registrados. | Resultado comprobado en la página MVC real | APROBADA |
| 7 | Confirmar y guardar el cálculo realizado. | El sistema almacena correctamente la información de la nómina procesada. | Resultado comprobado en la página MVC real | APROBADA |

### C14

Precondiciones originales: El sistema debe estar disponible y contar con usuarios registrados. Deben existir funcionalidades previamente implementadas y acceso autorizado a la consulta de las bitácoras.

| Paso | Acción original | Esperado original | Obtenido | Estado |
|---|---|---|---|---|
| 1 | Iniciar sesión con un usuario autorizado. | El sistema permite el acceso correctamente. | Resultado comprobado en la página MVC real | APROBADA |
| 2 | Realizar una operación registrada por el sistema. | La operación se ejecuta correctamente y genera un registro de auditoría. | Resultado comprobado en la página MVC real | APROBADA |
| 3 | Ingresar al módulo de auditoría o bitácoras. | El sistema muestra los registros disponibles. | Resultado comprobado en la página MVC real | APROBADA |
| 4 | Buscar el registro correspondiente a la operación realizada. | El sistema permite localizar el evento registrado. | Resultado comprobado en la página MVC real | APROBADA |
| 5 | Verificar la información almacenada en la bitácora. | El registro contiene la información correspondiente a la acción realizada. | Resultado comprobado en la página MVC real | APROBADA |
| 6 | Ejecutar funcionalidades previamente implementadas del sistema. | Las funcionalidades continúan operando correctamente después de los cambios. | Resultado comprobado en la página MVC real | APROBADA |
| 7 | Verificar que los cambios realizados no afecten otros módulos. | Los módulos relacionados mantienen su funcionamiento esperado. | Resultado comprobado en la página MVC real | APROBADA |
