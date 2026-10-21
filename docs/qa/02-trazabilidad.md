# Trazabilidad completa

Fuente: las ocho hojas del Excel original, sin sustituir sus códigos por HU-01…HU-14. Los archivos indicados son los puntos de implementación que deben verificarse; su existencia no certifica el cumplimiento. Los ensayos automatizados específicos y sus resultados están en `04-resultados-pruebas.md` y `resultado--integration.txt`.

Estado de esta matriz: inventario completo de criterios; validación individual pendiente salvo la evidencia específica del ejecutor QA. Cada contexto, evento y resultado se conserva textualmente. Las contradicciones documentales no se corrigen alterando el original.

## Gestion de Empleados

### GES 001 — Como usuario Admin quiero registrar nuevos empleados en el sistema para mantener actualizada la informacion del personal del restaurante

Implementación: `RestauranteVistas/Controllers/` (Empleados), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (EmpleadoService, TurnoService, AsistenciaService, VacacionService, HoraExtraService). Casos relacionados: C3, C4, C13, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 3 | Se registra un empleado con datos válidos | En caso que el restaurante contrate un nuevo colaborador y el administrador necesite ingresarlo formalmente al sistema. | Cuando el ADMIN registra idEmpleado(INT), nombreEmpleado(STRING), apellidosEmpleado(STRING), cedulaEmpleado(STRING), telefonoEmpleado(STRING), correoEmpleado(STRING), rolEmpleado(ENUM: Mesero/Cocinero/Cajero/Admin), fechaIngreso(DATE), salarioHora(DECIMAL), estadoEmpleado(ENUM: Activo) | El sistema deberá crear el registro del empleado, almacenar los datos ingresados, asignar estadoEmpleado=Activo y mostrar el mensaje “Empleado registrado correctamente” | PENDIENTE DE VERIFICACION |
| 2.0 / 4 | No se permiten empleados duplicados | En caso que el administrador intente registrar un empleado cuya identificación ya exista | Cuando el ADMIN registra cedulaEmpleado(STRING) y el sistema detecta coincidencia con otro idEmpleado(INT) existente | El sistema deberá bloquear el registro y mostrar el mensaje “La identificación del empleado ya existe en el sistema” | PENDIENTE DE VERIFICACION |
| 3.0 / 5 | Los campos obligatorios deben completarse | En caso que el administrador intente registrar un empleado sin completar información mínima | Cuando el ADMIN registra nombreEmpleado(STRING), cedulaEmpleado(STRING), rolEmpleado(ENUM) o fechaIngreso(DATE) con valores nulos | El sistema deberá bloquear el registro y mostrar el mensaje “Existen campos obligatorios sin completar” | PENDIENTE DE VERIFICACION |
| 4.0 / 6 | El salario debe ser válido | En caso que el administrador ingrese un salario incorrecto | Cuando el ADMIN registra salarioHora(DECIMAL) menor o igual a 0 | El sistema deberá rechazar el registro y mostrar el mensaje “El salario por hora debe ser mayor a cero” | PENDIENTE DE VERIFICACION |
| 5.0 / 7 | El correo debe tener formato válido | En caso que el administrador ingrese un correo inválido | Cuando el ADMIN registra correoEmpleado(STRING) sin formato válido de correo electrónico | El sistema deberá bloquear el registro y mostrar el mensaje “Formato de correo electrónico inválido” | PENDIENTE DE VERIFICACION |
| 6.0 / 8 | Solo ADMIN puede registrar empleados | En caso que un usuario sin privilegios intente registrar empleados | Cuando rolUsuario(ENUM) distinto de ADMIN intenta registrar nombreEmpleado(STRING), cedulaEmpleado(STRING) y rolEmpleado(ENUM) | El sistema deberá bloquear la operación y mostrar el mensaje “Acceso denegado: permisos insuficientes” | PENDIENTE DE VERIFICACION |
| 7.0 / 9 | El registro debe quedar en bitácora | En caso que se cree un nuevo empleado y se requiera trazabilidad | Cuando el ADMIN guarda idEmpleado(INT), usuarioResponsable(STRING), fechaRegistro(DATETIME), tipoOperacion(ENUM: Registro) | El sistema deberá registrar la operación en la bitácora del sistema | PENDIENTE DE VERIFICACION |
### GES 002 — Como usuario ADMIN quiero administrar empleados para actualizar su información laboral

Implementación: `RestauranteVistas/Controllers/` (Empleados), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (EmpleadoService, TurnoService, AsistenciaService, VacacionService, HoraExtraService). Casos relacionados: C3, C4, C13, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 11 | Se actualizan datos del empleado | En caso que el administrador requiera modificar información del empleado | Cuando el ADMIN actualiza idEmpleado(INT), telefonoEmpleado(STRING), correoEmpleado(STRING), rolEmpleado(ENUM), salarioHora(DECIMAL) | El sistema deberá actualizar la información del empleado y guardar fechaModificacion(DATETIME) | PENDIENTE DE VERIFICACION |
| 2.0 / 12 | Se puede inactivar un empleado | En caso que un colaborador deje de laborar en el restaurante | Cuando el ADMIN actualiza idEmpleado(INT), estadoEmpleado(ENUM: Inactivo), motivoInactivacion(STRING), fechaModificacion(DATETIME) | El sistema deberá cambiar el estado del empleado a Inactivo y mantener su historial | PENDIENTE DE VERIFICACION |
| 3.0 / 13 | Se puede reactivar un empleado | En caso que un empleado vuelva a ser contratado | Cuando el ADMIN actualiza idEmpleado(INT), estadoEmpleado(ENUM: Activo), fechaReactivacion(DATETIME) | El sistema deberá reactivar el registro del empleado | PENDIENTE DE VERIFICACION |
| 4.0 / 14 | No se puede editar un empleado inexistente | En caso que se intente modificar un registro inexistente | Cuando el ADMIN intenta editar idEmpleado(INT) que no existe | El sistema deberá bloquear la modificación y mostrar el mensaje “Empleado no encontrado” | PENDIENTE DE VERIFICACION |
| 5.0 / 15 | Datos editados deben ser válidos | En caso que el administrador ingrese datos inválidos | Cuando el ADMIN actualiza salarioHora(DECIMAL) menor a 0 o correoEmpleado(STRING) inválido | El sistema deberá bloquear la actualización | PENDIENTE DE VERIFICACION |
| 6.0 / 16 | Solo ADMIN puede administrar empleados | En caso que un usuario sin permisos intente editar empleados | Cuando rolUsuario(ENUM) distinto de ADMIN intenta editar idEmpleado(INT) | El sistema deberá bloquear la operación | PENDIENTE DE VERIFICACION |
| 7.0 / 17 | Cambios deben registrarse en bitácora | En caso que se modifique información de un empleado | Cuando el ADMIN modifica idEmpleado(INT), usuarioResponsable(STRING), fechaHora(DATETIME), tipoOperacion(ENUM) | El sistema deberá registrar el cambio en bitácora | PENDIENTE DE VERIFICACION |
### GES 003 — Como usuario ADMIN quiero crear turnos de trabajo para organizar la jornada laboral

Implementación: `RestauranteVistas/Controllers/` (Empleados), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (EmpleadoService, TurnoService, AsistenciaService, VacacionService, HoraExtraService). Casos relacionados: C3, C4, C13, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 19 | Se crea turno válido | En caso que el administrador planifique jornadas laborales | Cuando el ADMIN registra idTurno(INT), idEmpleado(INT), fechaTurno(DATE), horaInicio(TIME), horaFin(TIME), tipoTurno(ENUM), jornadaHoras(DECIMAL) | El sistema deberá guardar el turno | PENDIENTE DE VERIFICACION |
| 2.0 / 20 | Horarios válidos | En caso que el administrador planifique jornadas laborales | Cuando horaFin(TIME) sea menor a horaInicio(TIME) | El sistema deberá bloquear el turno | PENDIENTE DE VERIFICACION |
| 3.0 / 21 | No se permiten turnos duplicados | En caso que el administrador planifique jornadas laborales | Cuando idEmpleado(INT), fechaTurno(DATE) y horario coincidan | El sistema deberá bloquear el registro | PENDIENTE DE VERIFICACION |
| 4.0 / 22 | Registrar entrada | En caso que el empleado inicie jornada | Cuando el empleado registra idEmpleado(INT), fechaHoraEntrada(DATETIME) | El sistema deberá registrar asistencia | PENDIENTE DE VERIFICACION |
| 5.0 / 23 | Registrar salida | En caso que finalice la jornada laboral | Cuando el empleado registra fechaHoraSalida(DATETIME), horasTrabajadas(DECIMAL), horasExtra(DECIMAL) | El sistema deberá calcular horas trabajadas | PENDIENTE DE VERIFICACION |
| 6.0 / 24 | Jornada incompleta | En caso que no se registre salida | Cuando exista fechaHoraEntrada(DATETIME) sin fechaHoraSalida(DATETIME) | El sistema deberá marcar asistencia pendiente | PENDIENTE DE VERIFICACION |
| 7.0 / 25 | Solo ADMIN crea turnos | En caso que otro rol intente crear turnos | Cuando rolUsuario(ENUM) distinto de ADMIN registra idTurno(INT) | El sistema deberá bloquear la operación | PENDIENTE DE VERIFICACION |
### GES 004 — Como usuario MESERO quiero registrar las mesas atendidas para llevar control del servicio brindado a los clientes y medir el desempeño del personal del restaurante

Implementación: `RestauranteVistas/Controllers/` (Empleados), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (EmpleadoService, TurnoService, AsistenciaService, VacacionService, HoraExtraService). Casos relacionados: C3, C4, C13, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 27 | Se registra correctamente una mesa atendida | En caso que un mesero atienda una mesa dentro del restaurante y sea necesario registrar la atención realizada como parte del control operativo del servicio | Cuando el MESERO registra idRegistroMesa(INT), idMesa(INT), numeroMesa(INT), idEmpleado(INT), nombreEmpleado(STRING), idOrden(INT), origenMesa(ENUM: Salon/Terraza/Barra), fechaAtencion(DATE) y horaAtencion(DATETIME) | El sistema deberá guardar el registro de la mesa atendida, asociarlo al mesero responsable, registrar la fecha y hora de atención y permitir que esta información sea utilizada posteriormente en reportes de desempeño | PENDIENTE DE VERIFICACION |
| 2.0 / 28 | No se permite registrar la misma mesa dos veces en la misma orden | En caso que se intente registrar nuevamente una mesa que ya fue atendida y asociada a una misma orden dentro del sistema | Cuando el MESERO registra idMesa(INT) e idOrden(INT) y el sistema detecta que ya existe un registro previo con los mismos datos | El sistema deberá bloquear el registro duplicado, evitar inconsistencias en los conteos de mesas atendidas y mostrar el mensaje “La mesa ya se encuentra registrada para esta orden” | PENDIENTE DE VERIFICACION |
| 3.0 / 29 | La orden debe existir para registrar la mesa | En caso que el mesero intente registrar una mesa asociada a una orden inexistente dentro del sistema | Cuando el MESERO registra idOrden(INT) y el sistema no encuentra coincidencia con una orden registrada previamente | El sistema deberá bloquear el registro de la mesa, evitar la generación de datos inconsistentes y mostrar el mensaje “La orden asociada no existe en el sistema” | PENDIENTE DE VERIFICACION |
| 4.0 / 30 | Se debe registrar el origen de la mesa atendida | En caso que el restaurante requiera identificar desde qué área del establecimiento provienen las mesas atendidas por cada mesero | Cuando el MESERO registra origenMesa(ENUM: Salon/Terraza/Barra) junto con idMesa(INT), idEmpleado(INT), idOrden(INT) y fechaAtencion(DATE) | El sistema deberá almacenar el origen de la mesa atendida y permitir posteriormente realizar análisis de productividad por zona de servicio dentro del restaurante | PENDIENTE DE VERIFICACION |
| 5.0 / 31 | Se calcula el total de mesas atendidas por día | En caso que el administrador requiera evaluar el desempeño diario de cada mesero durante la jornada laboral | Cuando el sistema procesa los registros de idEmpleado(INT), fechaAtencion(DATE) y totalRegistrosMesa(INT) correspondientes al día laboral | El sistema deberá calcular totalMesasDia(INT) por empleado, almacenar el resultado y permitir su consulta dentro de los reportes operativos del restaurante | PENDIENTE DE VERIFICACION |
| 6.0 / 32 | Se calcula el total de mesas atendidas por semana | En caso que la administración del restaurante requiera evaluar el desempeño semanal de los meseros para fines de control operativo o incentivos | Cuando el ADMIN consulta idEmpleado(INT), fechaInicio(DATE), fechaFin(DATE) y rangoSemana(STRING) para obtener el historial de atención de mesas | El sistema deberá calcular totalMesasSemana(INT), mostrar el consolidado semanal de atención y permitir revisar el detalle diario de mesas atendidas | PENDIENTE DE VERIFICACION |
| 7.0 / 33 | Solo usuarios autorizados pueden consultar métricas de desempeño | En caso que un usuario sin permisos intente acceder a información de desempeño de los meseros | Cuando rolUsuario(ENUM) distinto de ADMIN intenta consultar idEmpleado(INT), totalMesasDia(INT) o totalMesasSemana(INT) | El sistema deberá restringir el acceso a la información, bloquear la consulta y mostrar el mensaje “No posee permisos para visualizar métricas de desempeño del personal” | PENDIENTE DE VERIFICACION |
### GES 005 — Como usuario ADMIN quiero gestionar las vacaciones de los empleados según los días laborados para controlar su saldo disponible.

Implementación: `RestauranteVistas/Controllers/` (Empleados), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (EmpleadoService, TurnoService, AsistenciaService, VacacionService, HoraExtraService). Casos relacionados: C3, C4, C13, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 35 | El sistema calcula saldo automáticamente | En caso que se controle el derecho a vacaciones | Cuando el sistema calcula: idEmpleado(INT), diasLaborados(INT), politicaVacaciones(ENUM:Empresa/Ley), factorAcumulacion(DECIMAL), fechaCalculo(DATE) | El sistema deberá calcular y actualizar automáticamente el saldoVacacionesDias(DECIMAL), almacenar la fecha del último cálculo y dejar el saldo disponible para futuras solicitudes | PENDIENTE DE VERIFICACION |
| 2.0 / 36 | Se registra solicitud válida | En caso que el empleado solicite vacaciones | uando el ADMIN registra: idSolicitudVacaciones(INT), idEmpleado(INT), fechaInicio(DATE), fechaFin(DATE), diasSolicitados(INT>0), comentarioEmpleado(STRING opcional), fechaSolicitud(DATETIME) | El sistema deberá guardar la solicitud con estado=Solicitada, asociarla al empleado y dejarla disponible para aprobación o rechazo | PENDIENTE DE VERIFICACION |
| 3.0 / 37 | No se excede el saldo disponible | En caso que el empleado solicite vacaciones | Cuando diasSolicitados(INT) > saldoVacacionesDias(DECIMAL) del idEmpleado(INT) | El sistema deberá bloquear el registro de la solicitud, mostrar un mensaje indicando que el saldo es insuficiente y no realizar ningún descuento. | PENDIENTE DE VERIFICACION |
| 4.0 / 38 | Se aprueba solicitud | En caso que el administrador autorice una solicitud válida de vacaciones | Cuando el ADMIN actualiza: idSolicitudVacaciones(INT), estadoSolicitud=ENUM(Aprobada), fechaAprobacion(DATETIME), usuarioResponsable(STRING) | El sistema deberá cambiar el estado de la solicitud a Aprobada, descontar los días solicitados del saldoVacacionesDias y reflejar el período como no laborable para el empleado | PENDIENTE DE VERIFICACION |
| 5.0 / 39 | Se rechaza solicitud | En caso que el administrador determine que la solicitud no puede ser aprobada por razones operativas o administrativas. | Cuando el ADMIN actualiza: idSolicitudVacaciones(INT), estadoSolicitud=ENUM(Rechazada), motivoRechazo(STRING), fechaRechazo(DATETIME), usuarioResponsable(STRING) | El sistema deberá cambiar el estado de la solicitud a Rechazada, conservar el saldo de vacaciones intacto y registrar el motivo del rechazo | PENDIENTE DE VERIFICACION |
| 6.0 / 40 | No se solapan con turnos | En caso que existan turnos activos | Cuando el sistema valida: idEmpleado(INT), fechaInicio(DATE), fechaFin(DATE) y detecta turnosActivos(LIST) | El sistema deberá advertir sobre la existencia de turnos activos y solicitar ajuste o confirmación antes de continuar con la aprobación. | PENDIENTE DE VERIFICACION |
| 7.0 / 41 | Todo queda auditado | En caso que se requiera control sobre las solicitudes y decisiones relacionadas con vacaciones | Cuando se modifica una solicitud registrando: idSolicitudVacaciones(INT), tipoCambio(ENUM:Creación/Aprobación/Rechazo), fechaHora(DATETIME), usuarioResponsable(STRING), detalleCambio(STRING) | El sistema deberá registrar la auditoría completa de la solicitud, permitiendo consultas históricas para control interno y revisiones administrativas. | PENDIENTE DE VERIFICACION |
### GES 006 — Como usuario ADMIN quiero registrar y controlar las horas extra de los empleados para calcular ingresos adicionales correctamente.

Implementación: `RestauranteVistas/Controllers/` (Empleados), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (EmpleadoService, TurnoService, AsistenciaService, VacacionService, HoraExtraService). Casos relacionados: C3, C4, C13, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 43 | Se registran horas extra válidas | En caso que el empleado labore fuera de su horario regular previamente asignado y sea necesario reconocer ese tiempo como ingreso adicional. | Cuando el ADMIN registra: idHorasExtra(INT), idEmpleado(INT), fecha(DATE), horasExtra(DECIMAL>0), tarifaHoraExtra(DECIMAL), motivo(STRING), usuarioResponsable(STRING), fechaRegistro(DATETIME) | El sistema deberá calcular automáticamente el montoExtra = horasExtra × tarifaHoraExtra, guardar el registro y asociarlo al empleado para su pago. | PENDIENTE DE VERIFICACION |
| 2.0 / 44 | No se permiten horas inválidas | En caso que el empleado labore fuera de su horario regular previamente asignado y sea necesario reconocer ese tiempo como ingreso adicional. | Cuando el ADMIN intenta registrar horasExtra(DECIMAL ≤ 0) para un idEmpleado(INT) | El sistema deberá bloquear el registro, mostrar un mensaje indicando que las horas extra deben ser mayores a cero y no guardar información inválida. | PENDIENTE DE VERIFICACION |
| 3.0 / 45 | Debe existir asistencia previa | En caso que el sistema requiera validar que el empleado efectivamente asistió y cumplió una jornada antes de registrar horas extra. | Cuando el sistema valida: idEmpleado(INT), fecha(DATE) y no encuentra registroAsistencia(BOOL=FALSE) | El sistema deberá bloquear el registro de horas extra y mostrar un mensaje indicando que no existe asistencia registrada para ese día. | PENDIENTE DE VERIFICACION |
| 4.0 / 46 | Se reflejan en resumen semanal | En caso que el restaurante consolide la información laboral del empleado para cálculo de planilla y control semanal | Cuando el ADMIN consulta: idEmpleado(INT), semana(INT), rangoFechas(DATE) | El sistema deberá incluir las horas extra registradas en el resumen semanal, sumarlas al total de horas y reflejar el monto adicional correspondiente | PENDIENTE DE VERIFICACION |
| 5.0 / 47 | Ajustes de horas extra requieren justificación | En caso que se detecte un error en el registro de horas extra previamente ingresado y sea necesario corregirlo. | Cuando el ADMIN edita: idHorasExtra(INT), nuevasHorasExtra(DECIMAL), motivoAjuste(STRING), usuarioResponsable(STRING), fechaModificacion(DATETIME) | El sistema deberá actualizar el registro, recalcular el montoExtra, guardar el motivo del ajuste y dejar evidencia para auditoría | PENDIENTE DE VERIFICACION |
| 6.0 / 48 | Solo ADMIN registra horas extra | En caso que otro rol distinto a ADMIN intente registrar o modificar horas extra de un empleado. | Cuando rolUsuario(ENUM) distinto a ADMIN intenta registrar idHorasExtra(INT) | El sistema deberá denegar el acceso, bloquear la operación y mostrar un mensaje indicando falta de permisos. | PENDIENTE DE VERIFICACION |
### GES 007 — Como administrador quiero consultar la bitácora de eventos de empleados

Implementación: `RestauranteVistas/Controllers/` (Empleados), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (EmpleadoService, TurnoService, AsistenciaService, VacacionService, HoraExtraService). Casos relacionados: C3, C4, C13, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 50 | Filtro de eventos por empleado afectado. | En caso de que el administrador necesite revisar quién modificó datos, registró o dio de baja a un empleado por motivos de seguridad. | Cuando el ADMIN accede a la vista de "Bitácora" e inicializa el módulo. | El sistema consulta tabla_bitacora_emp y muestra: idRegistro(INT), idAdminResponsable(INT), accion(ENUM), idEmpleadoAfectado(INT), fechaHora(DATETIME) y detalleCambio(STRING). | PENDIENTE DE VERIFICACION |
| 2.0 / 51 | Filtro por tipo de acción operativa. | En caso de que el administrador necesite rastrear todo el historial de modificaciones que ha sufrido el perfil de un colaborador específico. | Cuando el ADMIN selecciona un idEmpleadoAfectado(INT) desde el panel de búsqueda de la bitácora. | El sistema filtra los logs y despliega únicamente los cambios contractuales o de sistema ejecutados sobre ese empleado en particular. | PENDIENTE DE VERIFICACION |
| 3.0 / 52 | Búsqueda de eventos por rango cronológico. | En caso de que el administrador requiera auditar un tipo de movimiento específico, como despidos o aumentos salariales. | Cuando el ADMIN selecciona el filtro accion(ENUM:Contratacion / Edicion / Despido / AjusteAsistencia). | El sistema actualiza la vista mostrando exclusivamente los registros técnicos que coincidan con la operación seleccionada. | PENDIENTE DE VERIFICACION |
| 4.0 / 53 | Inmutabilidad garantizada de los registros de auditoría. | En caso de que se necesite revisar las acciones administrativas realizadas durante una semana o mes específico. | Cuando el ADMIN ingresa fechaInicio(DATE) y fechaFin(DATE) válidas en el buscador. | El sistema ejecuta el SELECT y despliega todos los rastros de auditoría ocurridos estrictamente dentro de ese periodo de tiempo. | PENDIENTE DE VERIFICACION |
| 5.0 / 54 | Exportación de la bitácora para control de recursos humanos | En caso de que algún usuario intente alterar el historial de RRHH para ocultar una modificación no autorizada. | Cuando cualquier usuario, incluyendo el ADMIN, intenta ejecutar una sentencia UPDATE o DELETE sobre un idRegistro(INT) de la bitácora. | El sistema bloquea la acción a nivel de base de datos, garantizando que el historial sea estrictamente de solo lectura. | PENDIENTE DE VERIFICACION |
| 6.0 / 55 | Restricción estricta de acceso al registro de auditoría. | En caso de que se requiera enviar el historial de movimientos de personal a la gerencia o al contador. | Cuando el ADMIN visualiza los registros filtrados y selecciona la opción exportar(BOOL) con formato(ENUM:PDF/Excel). | El sistema procesa los logs, genera el archivo bitacoraEmpleados(FILE) con los datos técnicos y habilita su descarga segura. | PENDIENTE DE VERIFICACION |
| 7.0 / 56 | Restricción estricta de acceso al registro de auditoría. | En caso de que un empleado intente revisar qué acciones administrativas o salariales están siendo monitoreadas. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero/Cajero) intenta acceder a la ruta de la bitácora de empleados. | El sistema bloquea la renderización de la vista y muestra la alerta “Acceso denegado: El módulo de auditoría es exclusivo para Administradores”. | PENDIENTE DE VERIFICACION |

## Gestion de Usuarios

### GUS 001 — Como usuario admin, quiero agregar usuarios para que puedan iniciar sesion en el sistema

Implementación: `RestauranteVistas/Controllers/` (Usuarios, Login), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (UsuarioService, PasswordHelper, AutorizacionFilter). Casos relacionados: C1, C2, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 3 | Usuario agrega usuarios de manera correctamente | En caso que el administrador desee agregar usuarios al sistema | cuando se ingresan correctamente los atributos nombre (STRING), apellido (STRING), correo(STRING), contraseña(STRING), telefono(INT), Fecha de nacimiento(INT), rol(STRING), direccion(STRING), cedula(INT) y estado(BOOL), ID(INT)<br> | el sistema deberá mostrar un mensaje en pantalla indicacndo que el usuario fue agregado de manera exitosa | PENDIENTE DE VERIFICACION |
| 2.0 / 4 | Usuario ya existente | En caso que el administrador desee agregar usuarios al sistema | Cuando se ingresan los atributos nombre (STRING), apellido (STRING), correo(STRING), contraseña(STRING),  rol(STRING) y , ID(INT)<br>y estos ya estan registrados | el sistema deberá mostrar un mensaje en pantalla indicando que el usuario ya existe | PENDIENTE DE VERIFICACION |
| 3.0 / 5 | campos requeridos o obligatorios | En caso que el administrador desee agregar usuarios al sistema | Cuando no se ingresan los atributos nombre (STRING), apellido (STRING), correo(STRING), contraseña(STRING), telefono(INT), Fecha de nacimiento(INT), rol(STRING), direccion(STRING), cedula(INT) y estado(BOOL), ID(INT)<br> | el sistema deberá mostrar un mensaje en pantalla indicando que faltan campos requeridos | PENDIENTE DE VERIFICACION |
| 4.0 / 6 | Caracteres incorrectos  | En caso que el administrador desee agregar usuarios al sistema | Cuando no se ingresan caracteres incorrectos al dato del atributo | el sistema deberá mostrar un mensaje en pantalla indicando que el atributo no corresponde al tipo de dato | PENDIENTE DE VERIFICACION |
| 5.0 / 7 | Contraseña no cumple con requisitos | En caso que el administrador desee agregar usuarios al sistema | Cuando se ingrese una contraseña que no cumpla con lo requerido: Mayuscula, minimo 12 carácteres, un caracter especial | el sistema deberá mostrar un mensaje en pantalla indicando que no se cumple con los caracteres o tamaño requerido para la contraseña | PENDIENTE DE VERIFICACION |
| 6.0 / 8 | Campos completamente en blanco | En caso que el administrador desee agregar usuarios al sistema | Cuando se dejan todos los campos en blanco | el sistema debera mostrar unmensaje en pantalla indicando que los campos estan en blanco | PENDIENTE DE VERIFICACION |
### GUS 002 — Como usuario quiero iniciar sesión en el sistema para acceder a mi perfil y ver mis funciones

Implementación: `RestauranteVistas/Controllers/` (Usuarios, Login), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (UsuarioService, PasswordHelper, AutorizacionFilter). Casos relacionados: C1, C2, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 10 | inicio de sesión correctamente | en caso de un usuario desee ingresar a su perfil | cuando se ingresa el usuario (STRING), Y contraseña (STRING) | El sistema deberá dar acceso a su perfil y mostrar un mensaje de acceso correcto | PENDIENTE DE VERIFICACION |
| 2.0 / 11 | usuario no creado | en caso de un usuario desee ingresar a su perfil | cuando un usuario no ha sido registrado y se trata de ingresar con su ID (INT) | El sistema deberá mostar un mensaje en pantalla indicando que el usuario no existe  | PENDIENTE DE VERIFICACION |
| 3.0 / 12 | contraseña incorrecta | en caso de un usuario desee ingresar a su perfil | cuando se ingresa la contraseña (String) y está incorrecta | el sistema deberá mostrar un mensaje en pantalla indicando contraseña incorrecta favor intente de nuevo | PENDIENTE DE VERIFICACION |
| 4.0 / 13 | usuario bloqueado por olvidar su contraseña | en caso de un usuario desee ingresar a su perfil | cuando un usuario no recuerda su contraseña o tan solo una parte de ella (String) | El sistema deberá mostar un mensaje en su pantalla indicando que se han realizado muchos intentos de autenticación la cuenta se ha bloqueado | PENDIENTE DE VERIFICACION |
| 5.0 / 14 | campos incompletos | en caso de un usuario desee ingresar a su perfil | cuando un usuario toca el botón de iniciar sesión sin haber puesto la contraseña o el user name  | el sistema debera indicar un mensaje en pantalla que diga que faltan campos para iniciar sesión | PENDIENTE DE VERIFICACION |
| 6.0 / 15 | sesión expirada | en caso de un usuario desee ingresar a su perfil | cuando se ingresan el usuario (STRING) y la contraseña (STRING) correctamente y no puede iniciar sesión | el sistema deberá mostar un mensaje en pantalla indicando que la cuenta del usuario expiro favor solicitar reactivación al administrador | PENDIENTE DE VERIFICACION |
| 7.0 / 16 | olvido de contraseña | en caso de un usuario desee ingresar a su perfil | y coloca una contraseña incorrecta o similar a la suya sin lograr recordar su contraseña correcta | el sistema deberá mostrar un boton que diga "olvidó su contraseña?" a lo que si el usuario lo presiona le da la obción de reestablecer una contraseña nueva | PENDIENTE DE VERIFICACION |
### GUS 003 — como usuario quiero cerrar sesión en el sistema por seguridad y para que nadie externo pueda accerder a mi perfil

Implementación: `RestauranteVistas/Controllers/` (Usuarios, Login), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (UsuarioService, PasswordHelper, AutorizacionFilter). Casos relacionados: C1, C2, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 18 | cierre de sesion correcto | en caso de presionar el boton de cierre de sesión  | Cuando el usuario presiona cerrar sesión registrando:<br>idUsuario(INT), idSesion(INT), usuarioActivo(BOOL=TRUE), fechaHoraSolicitudCierre(DATETIME), dispositivoSesion(STRING), direccionIP(STRING) | el sistema deberá mostar un mensaje en pantalla indicando que la sesión se esta cerrando "cerrando sesión" | PENDIENTE DE VERIFICACION |
| 2.0 / 19 | sesión ya cerrada | en caso que el usuario  quiera salir de su sesión  | Cuando el sistema valida sesión registrando:<br>idUsuario(INT), idSesion(INT), estadoSesion(ENUM:ACTIVA/INACTIVA/CERRADA), fechaHoraVerificacion(DATETIME), dispositivoSesion(STRING) | el sistema deberá mostar un mensaje en pantalla indicando que su sesión se encuentra inactiva | PENDIENTE DE VERIFICACION |
| 3.0 / 20 | cierre automatiamente | en caso que el usuario tenía iniciada su sesión pero ha permanecido inactivo | Cuando el sistema detecta inactividad registrando:<br>idSesion(INT), idUsuario(INT), tiempoInactividadMin(INT), tiempoMaximoPermitido(INT), fechaHoraUltimaActividad(DATETIME), estadoSesion(ENUM) | el sistema se cierra o suspende automaticamente y si la vuelve a encender debera iniciar sesion nuevamente porque ya se cerró | PENDIENTE DE VERIFICACION |
| 4.0 / 21 | cierre de sesion incorrecto por aplicaciones abiertas | en caso que el usuario quiera cerrar su sesión  | Cuando el usuario solicita cierre registrando:<br>idUsuario(INT), idSesion(INT), aplicacionesActivas(INT), listaAplicacionesActivas(STRING), fechaHoraIntentoCierre(DATETIME), usuarioConfirmacion(BOOL) | el sistema deberá mostrar un mensaje en pantalla indicando que tiene aplicaciones en uso y si desea cerrar sesión forzadamente   | PENDIENTE DE VERIFICACION |
| 5.0 / 22 | sistema no permite cerrar sesión por actualización<br> | en caso que el usuario quiera cerrar su sesión  |  el sistema detecta actualización activa registrando:<br>idSistema(INT), idSesion(INT), estadoActualizacion(ENUM:EN_PROCESO/PENDIENTE/FINALIZADA), porcentajeActualizacion(DECIMAL), fechaHoraActualizacion(DATETIME) | el sistema le solicita al usuario no cerrar sesión hasta haber finalizado la actualización | PENDIENTE DE VERIFICACION |
| 6.0 / 23 | cancelar cierre de sesión | en caso que el usuario quiera cerrar su sesión  | Cuando el usuario cancela cierre registrando:<br>idUsuario(INT), idSesion(INT), accionUsuario(ENUM:CANCELAR_CIERRE), fechaHoraAccion(DATETIME), pantallaActual(STRING) | el sistema deberá cancelar la cción de cierre y devolverse a su panta de incio en su perfil porque no se oprimio confirmar  | PENDIENTE DE VERIFICACION |
### GUS 004 — Como usuario admin, quiero editar la información de un usuario para mantener sus datos actualizados

Implementación: `RestauranteVistas/Controllers/` (Usuarios, Login), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (UsuarioService, PasswordHelper, AutorizacionFilter). Casos relacionados: C1, C2, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 25 | edicion correcta | en caso que el admin desee editar la información de un usuario | Cuando se pueden editar o cambiar correctamente los atributos usuarioID(INT), nombre(STRING), apellido(STRING), correo(STRING), teléfono(INT), dirección(STRING) | el sistema deberá mostrar un mensaje indicando que la información fue actualizada exitosamente | PENDIENTE DE VERIFICACION |
| 2.0 / 26 | usuario no encontrado | en caso que el admin desee editar la información de un usuario | cuando intenta editar un usuario con ID (INT) que no existe o no se ha registrado | El sistema deberá mostrar un mensaje en pantalla indicando que el usuario con ese número de ID no existe, que vuelva a poner el número correctamente | PENDIENTE DE VERIFICACION |
| 3.0 / 27 | Correo invalido | en caso que el admin desee editar el correo de un usuario | cuando se requiere actualizar el nuevo correo(STRING) de un usuario pero es un formato incorrecto  | El sistema deberá mostrar un mensaje en pantalla indicando que el correo es invalido mostrando su formato correcto  | PENDIENTE DE VERIFICACION |
| 4.0 / 28 | dirección inexacta | en caso que el admin desee editar la dirección de un usuario | cuando intenta actualizar la dirección (STRING) nueva de un usuario y no cumple con los campos minimos que son, provincia, canton y distrito (STRING) | El sistema deberá mostrar un mensaje en pantalla indicando que la dirección no cumple con los campos minimos requeridos  | PENDIENTE DE VERIFICACION |
| 5.0 / 29 | Información no editable | en caso que el admin desee editar datos ineditables de un usuario | Cuando se intenta actualizar información como Cédula(INT) o fecha de nacimiento (INT) o nombre(STRING) | El sistema deberá mostrar un mensaje en pantalla indicando que esos datos no pueden ser actualizados porque son datos permanentes de una persona  | PENDIENTE DE VERIFICACION |
| 6.0 / 30 | Actualizar un rol | en caso que el admin desee asignar un rol nuevo a un usuario debe actualizar su rol en su información | cuando se cambia un rol (STRING)a un usuario que ya tenia un rol se debe actualizar en su información personal para que aparezca con su nuevo rol | El sistema deberá mostrar un mensaje en pantalla indicando nuevo rol actualizado exitosamente al usuario | PENDIENTE DE VERIFICACION |
| 7.0 / 31 | Error al  editar | En caso que el administrador desee editar la información de un usuario | Cuando se ingresan  los atributos usuarioID(INT), apellido(STRING), correo(STRING), teléfono(INT), dirección(STRING) y no se actualizaron correctamente | El sistema deberá mostar un mensaje en pantalla indicando que vuelva a intentarlo ingresar los datos de nuevo  | PENDIENTE DE VERIFICACION |
### GUS 005 — Como usuario admin, quiero asignar roles a los usuarios para controlar el acceso a los módulos del sistema

Implementación: `RestauranteVistas/Controllers/` (Usuarios, Login), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (UsuarioService, PasswordHelper, AutorizacionFilter). Casos relacionados: C1, C2, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 33 | Rol asignado correctamente | En caso que el administrador desee asignar un rol | Cuando se ingresan correctamente los atributos ID(INT), rol(STRING), estado(BOOL) | el sistema debera mostrar un mensaje indicando que el rol fue asignado correctamente al usuario | PENDIENTE DE VERIFICACION |
| 2.0 / 34 | Usuario inexistente | En caso que el admin desee asignar roles a los usuarios | cuando ingresa un ID (INT) que no está registrado | el sistema debera mostrar un mensaje en pantalla indicando que el usuario no existe | PENDIENTE DE VERIFICACION |
| 3.0 / 35 | Rol invalido | En caso que el admin desee asignar roles a los usuarios | cuando se trata de asignar un rol que no corresponde a los roles ya predeterminados | el sistema deberá  mostrar un mensaje de rol invalido porque no está definido | PENDIENTE DE VERIFICACION |
| 4.0 / 36 | Usuario sin permiso | En caso que el admin desee asignar roles a los usuarios | cuando se trata de asignar un rol a  un usuario con estado (bool) inactivo | el sistemadeberá mostrar un mensaje indicando que el usuario no tiene permisos  | PENDIENTE DE VERIFICACION |
| 5.0 / 37 | Rol ya asignado al usuario  | En caso que el admin desee asignar roles a los usuarios | cuando se intenta asignar un rol(STRING) a un usuario que ya tiene un rol | El sistema deberá mostrar un mensaje indicando que el usuario tiene un ya rol asignado | PENDIENTE DE VERIFICACION |
| 6.0 / 38 | Confirmación de rol asignado correctamente | En caso que el admin desee asignar roles a los usuarios |  cuando se asigne un rol(STRING) y antes de confirmarlo  | El sistema debera mostrar un mensaje en pantalla indicando si ese rol es el correcto para el usuario x | PENDIENTE DE VERIFICACION |
| 7.0 / 39 | El rol no se asigna correctamente | En caso que el admin desee asignar roles a los usuarios | cuando se asigna un rol (STRING) y se confirma el rol pero el usuario no aparece en roles asignados | El sistema deberá dar la opción de asignar un rol al usuario nuevamente | PENDIENTE DE VERIFICACION |
### GUS 006 — Como usuario admin, quiero que los usuarios cambien su contraseña cada 6 meses para tener mayor seguridad con la información de los usuarios

Implementación: `RestauranteVistas/Controllers/` (Usuarios, Login), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (UsuarioService, PasswordHelper, AutorizacionFilter). Casos relacionados: C1, C2, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 41 | cambio de contraseña es obligatorio | en caso que el sistema detecte que han pasado 6 meses  con una misma contraseña | Cuando el sistema valida la vigencia de la contraseña registrando:<br>idUsuario(INT), idSesion(INT), fechaUltimoCambioPassword(DATETIME), fechaActual(DATETIME), tiempoVigenciaMeses(INT=6), estadoPassword(ENUM:VIGENTE/VENCIDA) | el sistema debera solicitar al usuario cambiar a una nueva contraseña | PENDIENTE DE VERIFICACION |
| 2.0 / 42 | contraseña es cambiada correctamente | en caso que el sistema detecte que han pasado 6 meses  con una misma contraseña | Cuando el usuario registra nueva contraseña validando:<br>idUsuario(INT), nuevaPasswordHash(STRING), longitudPassword(INT), cumplePoliticaSeguridad(BOOL), fechaCambioPassword(DATETIME), dispositivoCambio(STRING), direccionIP(STRING) | el sistema deberá mostar un mensaje indicando que la contraseña fue cambiada exitosamente | PENDIENTE DE VERIFICACION |
| 3.0 / 43 | No se permite una contraseña insegura | en caso que el sistema detecte que han pasado 6 meses  con una misma contraseña | Cuando el sistema valida contraseña ingresada registrando:<br>idUsuario(INT), passwordIngresada(STRING), longitudPassword(INT), contieneMayuscula(BOOL), contieneNumero(BOOL), contieneCaracterEspecial(BOOL), cumplePoliticaSeguridad(BOOL) | el sistema debera mostrar unmensaje indicando que su contraeña ya no es segura y deberá una ingresar una nueva contraseña indicandole sus cantidad de carateres | PENDIENTE DE VERIFICACION |
| 4.0 / 44 | contraseña igual a la anterior | en caso que el sistema detecte que han pasado 6 meses  con una misma contraseña | Cuando el sistema compara historial de contraseñas registrando:<br>idUsuario(INT), passwordNuevaHash(STRING), passwordAnteriorHash(STRING), historialPasswords(INT), fechaComparacion(DATETIME) | el sistema debera mostrar un mensaje de que la contraseña debe ser diferente a la pasada | PENDIENTE DE VERIFICACION |
| 5.0 / 45 | usuario sin cambiar la contraseña | en caso que el sistema detecte que han pasado 6 meses  con una misma contraseña | Cuando el usuario intenta iniciar sesión registrando:<br>idUsuario(INT), idSesion(INT), fechaIntentoLogin(DATETIME), estadoPassword(ENUM:VIGENTE/VENCIDA), accesoPermitido(BOOL) | el sistema debera mostrar el mensaje indicando que no podra continuar sin cambiar su contraseña | PENDIENTE DE VERIFICACION |
| 6.0 / 46 | aviso previo al vencimiento de contraseña | en caso que el sistema detecte que han pasado 6 meses  con una misma contraseña | Cuando el sistema evalúa vencimiento de contraseña registrando:<br>idUsuario(INT), fechaUltimoCambioPassword(DATETIME), diasRestantesVencimiento(INT), fechaActual(DATETIME), estadoNotificacion(BOOL) | el sistema debera mostrar el mensaje indicando una alerta de contraseña proxima a vencer y tiene la opcion de cambiarla de una vez o esperar | PENDIENTE DE VERIFICACION |
| 7.0 / 47 | usuario bloqueado por no cambiar contraseña | en caso que el sistema detecte que han pasado 6 meses  con una misma contraseña | Cuando el sistema detecta contraseña vencida registrando:<br>idUsuario(INT), idSesion(INT), estadoPassword(ENUM:VENCIDA), intentosLogin(INT), estadoCuenta(ENUM:ACTIVA/BLOQUEADA), fechaBloqueo(DATETIME) | el sistema debera bloquear su sesión hasta que cambie su contraseña | PENDIENTE DE VERIFICACION |
### GUS 007 — Como usuario admin, quiero eliminar usuarios que ya no trabajan en la empresa para mantener actualizada la cantidad de usuarios 

Implementación: `RestauranteVistas/Controllers/` (Usuarios, Login), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (UsuarioService, PasswordHelper, AutorizacionFilter). Casos relacionados: C1, C2, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 49 | Eliminación correcta | en caso que el admin desee eliminar usuarios | cuando ingresa el Id (INT) del usuario y elimina el resto de datos complenado la eliminación del usuario | El sistema deberá mostrar un mensaje en pantalla indicando que usuario ha sido eliminado exitosamente | PENDIENTE DE VERIFICACION |
| 2.0 / 50 | Usuario inexsistente | en caso que el admin desee eliminar usuarios  | En caso que el administrador intente eliminar un usuario utilizando idUsuario(INT) pero el registro no exista en la base de datos del sistema. | el sistema debera mostrará un mensaje en pantalla que indique usuario no existente | PENDIENTE DE VERIFICACION |
| 3.0 / 51 | depuración de usuarios eliminados | en caso que el admin desee eliminar usuarios  | En caso que el administrador realice un proceso de limpieza del sistema seleccionando uno o varios usuarios mediante idUsuario(INT) para eliminarlos permanentemente. | el sistema debera mostrar un mensaje en pantalla indicando usuarios eliminados permanentemente | PENDIENTE DE VERIFICACION |
| 4.0 / 52 | usuarios que no se pueden eliminar | en caso que el admin desee eliminar usuarios | En caso que el administrador intente eliminar un usuario que tenga un rolSistema(ENUM) con privilegios superiores, como administrador principal o rol jerárquico. | el sistema deberá mostrar un mensaje en pantalla indicando que el usuario no puede ser eliminado | PENDIENTE DE VERIFICACION |
| 5.0 / 53 | No cuenta con privilegios para eliminar | en caso que un usuario desee eliminar usuarios | En caso que un usuario con rolSistema(ENUM) distinto a administrador intente eliminar otro usuario utilizando idUsuario(INT). | el sistema deberá mostrar un mensaje en pantalla indicando que el usuario no puede ser eliminado | PENDIENTE DE VERIFICACION |
| 6.0 / 54 | Eliminación cancelada | en caso que el admin desee eliminar usuarios | En caso que el administrador seleccione un usuario mediante idUsuario(INT) para eliminarlo pero decida cancelar la acción antes de confirmar la operación. | el sistemará mostrará un mensaje que la eliminación fue cancelada | PENDIENTE DE VERIFICACION |
### GUS 008 — como usuario admin quiero  llevar un registro de tipo bitácora para tener un respaldo de cada accion realizada y monitorear 

Implementación: `RestauranteVistas/Controllers/` (Usuarios, Login), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (UsuarioService, PasswordHelper, AutorizacionFilter). Casos relacionados: C1, C2, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 56 | control de los registrosde usuarios | en caso que el usuario admin registre nuevos usuarios | En caso que el administrador registre nuevos usuarios en el sistema, el sistema deberá preparar el registro en bitácora identificando idUsuario(INT), nombreUsuario(STRING), rolSistema(ENUM), estadoCuenta(ENUM:ACTIVO/INACTIVO) y fechaRegistro(DATETIME) para mantener trazabilidad de las cuentas creadas. | El sistema deberá guardar un registro indicnado que se a gregó un nuevo usuario con sus atributos | PENDIENTE DE VERIFICACION |
| 2.0 / 57 | Registro de inicios de sesión | en caso que los usuarios accedan al sistema iniciando sesión | En caso que los usuarios accedan al sistema iniciando sesión, el sistema deberá registrar la actividad identificando idUsuario(INT), nombreUsuario(STRING), idSesion(INT), fechaHoraInicioSesion(DATETIME), direccionIP(STRING) y dispositivoAcceso(STRING) para control de accesos. | la bitacora debera registar el usuario que inicio sesión con su hora y fecha | PENDIENTE DE VERIFICACION |
| 3.0 / 58 | Registro de cierres de sesión | en caso que los usuarios salgan sistema cerrando sesión | En caso que los usuarios finalicen su sesión dentro del sistema, el sistema deberá registrar el cierre identificando idUsuario(INT), idSesion(INT), fechaHoraCierreSesion(DATETIME), duracionSesion(MINUTOS) y estadoSesion(ENUM:CERRADA/EXPIRADA) para control de actividad del usuario. | la bitacora debera registar el usuario que cerró su sesión con su hora y fecha | PENDIENTE DE VERIFICACION |
| 4.0 / 59 | Registro de edicón de informacion de usuarios | en caso que el usuario admin actualice datos de un usuario | En caso que el administrador modifique datos de un usuario existente, el sistema deberá registrar la actualización identificando idUsuario(INT), atributoModificado(STRING), valorAnterior(STRING), valorNuevo(STRING) y fechaModificacion(DATETIME) para mantener historial de cambios. | la bitacora deberá registrar una actualizacion de datos de un usuario<br> | PENDIENTE DE VERIFICACION |
| 5.0 / 60 | Registro de asignamientos de roles | en caso que el usuario admin asigne roles  | En caso que el administrador asigne o modifique roles de acceso en el sistema, el sistema deberá registrar el cambio identificando idUsuario(INT), rolAnterior(ENUM), rolNuevo(ENUM), usuarioAdministrador(STRING) y fechaAsignacionRol(DATETIME) para control de permisos. | la bitacora deberá registrar que se aginó un rol a un usuario en fecha x | PENDIENTE DE VERIFICACION |
| 6.0 / 61 | Registro de cambios de contraseñas | en caso que un usuario realice un cambio de contraseña | En caso que un usuario realice un cambio de contraseña dentro del sistema, el sistema deberá registrar el evento identificando idUsuario(INT), fechaCambioPassword(DATETIME), metodoCambio(ENUM:MANUAL/OBLIGATORIO),  para mantener control de seguridad. | la bitacora debera registrar que hubo un cambio de contraseña  | PENDIENTE DE VERIFICACION |
| 7.0 / 62 | Registro de usuarios eliminados | en caso que el admin elimine usuarios | En caso que el administrador elimine un usuario del sistema, el sistema deberá registrar la operación identificando idUsuario(INT), nombreUsuario(STRING), rolSistema(ENUM), usuarioAdministrador(STRING) y fechaEliminacion(DATETIME) para mantener trazabilidad de cuentas eliminadas. | la bitacora deberá registar el dia y la hora en qué fue eliminado un usuario | PENDIENTE DE VERIFICACION |

## Seguimiento de Pedidos

### SPS 001 — Como mesero quiero crear una cuenta asociada a una mesa para registrar el pedido de los clientes

Implementación: `RestauranteVistas/Controllers/` (Mesas, Pedidos, Cocina, Mesero, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService, PedidoService). Casos relacionados: C5, C6, C7, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 3 | El mesero o administrador puede crear una nueva mesa en el sistema con los datos requeridos. | En caso de que el sistema necesite registrar nuevas mesas disponibles en el restaurante. | Cuando el usuario con rol autorizado selecciona la opción “Crear mesa” e ingresa los datos:<br>idMesa(INT), numeroMesa(INT), capacidad(INT), estadoMesa(ENUM: Disponible/Ocupada/Reservada), ubicación(STRING opcional). | El sistema registra la nueva mesa correctamente, la guarda en la base de datos y la muestra disponible en el sistema para su uso. | PENDIENTE DE VERIFICACION |
| 2.0 / 4 | Si la mesa está disponible y los datos son los correctos, la orden se crea exitosamente | En caso que el mesero atienda una mesa con clientes listos para ordenar | Cuando el mesero selecciona una mesa con estadoMesa=Disponible e ingresa: idMesa(INT), numeroMesa(INT), idMesero(INT), nombreMesero(STRING), cantidadComensales(INT), tipoServicio(ENUM:Restaurante/ParaLlevar), observacionesOrden(STRING opcional) y agrega productos con idProducto(INT), nombreProducto(STRING), cantidad(INT>0), precioUnitario(DECIMAL), observacionesItem(STRING opcional) | Cuando el mesero selecciona una mesa con estadoMesa=Disponible e ingresa: idMesa(INT), numeroMesa(INT), idMesero(INT), nombreMesero(STRING), cantidadComensales(INT), tipoServicio(ENUM:Salón/ParaLlevar), observacionesOrden(STRING opcional) y agrega productos con idProducto(INT), nombreProducto(STRING), cantidad(INT>0), precioUnitario(DECIMAL), observacionesItem(STRING opcional) | PENDIENTE DE VERIFICACION |
| 3.0 / 5 | Si la mesa tiene una orden activa, el sistema debe permitir agregar ítems adicionales a la orden existente | En caso que el mesero atienda una mesa con clientes listos para ordenar | Cuando el meseroselecciona una mesa con estadoMesa=Ocupada y con una orden en estado  "Finalizada" y elige la opción “Agregar productos” ingresando: idOrden(INT), idProducto(INT), nombreProducto(STRING), cantidad(INT>0), precioUnitario(DECIMAL), observacionesItem(STRING opcional) | El sistema agrega los productos a la orden existente, recalcula el totalOrden, mantiene el estadoOrden actual y muestra mensaje “Productos agregados a la orden” | PENDIENTE DE VERIFICACION |
| 4.0 / 6 | Todos los campos obligatorios deben completarse | En caso que el mesero atienda una mesa con clientes listos para ordenar | Cuando el mesero intenta guardar sin productos o sin cantidadComensales (cantidad de personas en una mesa y van a consumir) | El sistema muestra mensaje “Faltan campos obligatorios” | PENDIENTE DE VERIFICACION |
| 5.0 / 7 | La cantidad de productos solicitados debe ser mayor a cero | En caso que el mesero atienda una mesa con clientes listos para ordenar | Cuando el meseroingresa cantidadProducto(INT) menos a 0 al agregar un ítem a la orden | El sistema valida el dato, bloquea el registro del producto y muestra mensaje “La cantidad del producto debe ser mayor a cero” | PENDIENTE DE VERIFICACION |
| 6.0 / 8 | El mesero puede agregar observaciones especiales | En caso que el mesero atienda una mesa con clientes listos para ordenar | Cuando el mesero ingresa observacionesItem(STRING) | El sistema guarda las observaciones asociadas al producto | PENDIENTE DE VERIFICACION |
| 7.0 / 9 | La orden debe enviarse automáticamente a cocina | En caso que el mesero atienda una mesa con clientes listos para ordenar | Cuando el MESERO confirma la orden ingresando: idOrden(INT), idMesa(INT), idMesero(INT) y presiona la opción “Confirmar orden”. | El sistema envía automáticamente la orden a la cocina, registra estadoOrden=Solicitada y deja la orden visible para los cocineros. | PENDIENTE DE VERIFICACION |
| 8.0 / 10 | Solo usuarios con rol mesero pueden crear órdenes | En caso que un usuario sin permisos intente crear una orden | Cuando un usuario con rol diferente a MESERO intenta crear una orden ingresando: idUsuario(INT), rolUsuario(ENUM). | El sistema valida el rol del usuario, bloquea la creación de la orden y muestra un mensaje de acceso denegado. | PENDIENTE DE VERIFICACION |
### SPS 002 — Como cocinero quiero iniciar la preparación de una orden para atender los pedidos

Implementación: `RestauranteVistas/Controllers/` (Mesas, Pedidos, Cocina, Mesero, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService, PedidoService). Casos relacionados: C5, C6, C7, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 12 | El cocinero puede ver órdenes solicitadas | En caso que el cocinero inicie su jornada laboral y necesite revisar los pedidos pendientes que han sido enviados desde el salón o caja para su preparación en cocina. | Cuando al cocinero le llega una orden con sus respectivas idOrden(INT), idProducto(INT), nombreProducto(STRING), cantidad(INT>0) observacionesItem(String) | El sistema muestra órdenes con estadoOrden=Solicitada | PENDIENTE DE VERIFICACION |
| 2.0 / 13 | El cocinero puede seleccionar una orden | En caso que el cocinero inicie su jornada laboral y necesite revisar los pedidos pendientes que han sido enviados desde el salón o caja para su preparación en cocina. | Cuando el COCINERO selecciona una orden ingresando: idOrden(INT). | El sistema muestra detalle con nombreProducto(STRING), cantidad(INT>0) observacionesItem(String) | PENDIENTE DE VERIFICACION |
| 3.0 / 14 | El estado cambia al iniciar preparación | En caso que el cocinero haya revisado los detalles de una orden y decida comenzar activamente la preparación de los productos solicitados por el cliente. | Cuando el COCINERO presiona “Iniciar preparación” ingresando: idOrden(INT). | El sistema cambia estadoOrden=EnPreparacion | PENDIENTE DE VERIFICACION |
| 4.0 / 15 | Cada producto puede cambiar de estado | En caso que la orden contenga múltiples productos que pueden ser preparados de forma independiente dentro de la cocina. | Cuando el COCINERO marca un producto como en preparación ingresando: idProducto(INT), idOrden(INT). | El sistema cambia estadoItem=EnPreparacion para el producto seleccionado. | PENDIENTE DE VERIFICACION |
| 5.0 / 16 | El cocinero puede marcar productos listos | En caso que un producto haya sido preparado completamente conforme a la solicitud del cliente y se encuentre listo para ser servido. | Cuando el COCINERO marca estadoItem=Listo ingresando: idProducto(INT), idOrden(INT). | El sistema actualiza el estado del ítem a Listo. | PENDIENTE DE VERIFICACION |
| 6.0 / 17 | La orden pasa a “Lista” cuando todos los productos están listos | En caso que todos los productos asociados a una orden hayan sido preparados y marcados como listos por el cocinero. | Cuando el último idProducto(INT) se marca como Listo | El sistema cambia estadoOrden=Lista | PENDIENTE DE VERIFICACION |
| 7.0 / 18 | El mesero debe ser notificado | En caso que la orden haya quedado completamente preparada y lista para ser entregada. | Cuando estadoOrden=Lista para la entrega de la idOrden(INT). | El sistema envía notificación al mesero asignado | PENDIENTE DE VERIFICACION |
### SPS 003 — Como mesero quiero entregar el pedido a la mesa cuando esté listo

Implementación: `RestauranteVistas/Controllers/` (Mesas, Pedidos, Cocina, Mesero, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService, PedidoService). Casos relacionados: C5, C6, C7, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 20 | Solo pedidos listos pueden entregarse | En caso que la cocina haya finalizado completamente la preparación del pedido y la orden se encuentre lista para ser servida al cliente en la mesa correspondiente. | Cuando el MESERO visualiza órdenes con estadoOrden=Lista e ingresa: idOrden(INT), numeroMesa(INT). | El sistema permite iniciar el proceso de entrega del pedido. | PENDIENTE DE VERIFICACION |
| 2.0 / 21 | El estado debe cambiar al entregar | En caso que la cocina haya finalizado completamente la preparación del pedido y la orden se encuentre lista para ser servida al cliente en la mesa correspondiente. | Cuando el MESERO presiona la opción “Entregar pedido” ingresando: idOrden(INT), idMesero(INT). | El sistema cambia estadoOrden=Entregada. | PENDIENTE DE VERIFICACION |
| 3.0 / 22 | Se registra la fecha y hora de entrega | En caso que la cocina haya finalizado completamente la preparación del pedido y la orden se encuentre lista para ser servida al cliente en la mesa correspondiente. | Cuando el MESERO confirma la entrega ingresando: idOrden(INT). | El sistema registra fechaHoraEntrega(DATETIME) asociada a la orden. | PENDIENTE DE VERIFICACION |
| 4.0 / 23 | No se puede entregar pedido en preparación | En caso que el mesero intente entregar un pedido que aún se encuentre en proceso de preparación en cocina y no esté listo para ser servido. | Cuando el MESERO intenta entregar ingresando: idOrden(INT) con estadoOrden=EnPreparacion. | El sistema bloquea la acción y evita la entrega del pedido. | PENDIENTE DE VERIFICACION |
| 5.0 / 24 | El pedido queda habilitado para pago | En caso que el pedido haya sido entregado correctamente al cliente y el servicio continúe con el proceso de cobro de la cuenta. | Cuando el sistema detecta el cambio de estado a Entregada para: idOrden(INT). | El sistema habilita la opción de pago para la orden entregada. | PENDIENTE DE VERIFICACION |
| 6.0 / 25 | Solo el mesero asignado puede entregar | En caso que un mesero distinto al asignado a la mesa intente realizar la entrega del pedido al cliente. | Cuando otro MESERO intenta entregar ingresando: idOrden(INT), idMesero(INT). | El sistema deniega la acción por falta de autorización. | PENDIENTE DE VERIFICACION |
### SPS 004 — Como cajero quiero finalizar una orden para cerrar la cuenta del cliente y liberar la mesa

Implementación: `RestauranteVistas/Controllers/` (Mesas, Pedidos, Cocina, Mesero, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService, PedidoService). Casos relacionados: C5, C6, C7, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 27 | Solo órdenes entregadas pueden finalizarse | En caso que el cliente haya consumido todos los productos del pedido y solicite la cuenta para proceder con el pago y cierre de la orden. | Cuando el CAJERO selecciona una orden con estadoOrden=Entregada y visualiza: idOrden(INT), numeroOrden(STRING), detalleProductos(LIST), totalOrden(DECIMAL) | El sistema permite continuar con el proceso de finalización | PENDIENTE DE VERIFICACION |
| 2.0 / 28 | El total debe calcularse automáticamente | En caso que el cliente haya consumido todos los productos del pedido y solicite la cuenta para proceder con el pago y cierre de la orden. | Cuando el CAJERO consulta la orden ingresando: idOrden(INT). | El sistema calcula totalOrden = SUM(cantidad * precioUnitario) | PENDIENTE DE VERIFICACION |
| 3.0 / 29 | El sistema debe registrar el método de pago | En caso que el cliente realice el pago de la cuenta utilizando alguno de los métodos de pago disponibles en el restaurante. | Cuando el CAJERO registra metodoPago(ENUM:Efectivo/SINPE), montoPagado(DECIMAL) | El sistema registra estadoPago=Pagado asociado a la orden. | PENDIENTE DE VERIFICACION |
| 4.0 / 30 | No se permite finalizar sin pago | En caso que el cajero intente cerrar la orden sin que exista un pago previamente registrado en el sistema. | Cuando el CAJERO intenta finalizar una idOrden(INT) sin estadoPago=Pagado | El sistema bloquea la acción y muestra el mensaje “Debe registrarse el pago”. | PENDIENTE DE VERIFICACION |
| 5.0 / 31 | Al finalizar la orden debe cambiar su estado | En caso que el pago haya sido registrado correctamente y el cliente haya cancelado el total de la cuenta. | Cuando el CAJERO confirma la finalización ingresando: idOrden(INT). | El sistema cambia estadoOrden=Finalizada y registra fechaHoraFinalizacion(DATETIME) | PENDIENTE DE VERIFICACION |
| 6.0 / 32 | La mesa debe quedar disponible nuevamente | En caso que la orden haya sido finalizada correctamente y la mesa no tenga pedidos activos asociados. | Cuando el sistema procesa la finalización ingresando: idMesa(INT). | El sistema cambia estadoMesa=Disponible | PENDIENTE DE VERIFICACION |
| 7.0 / 33 | Solo usuarios con rol cajero pueden finalizar órdenes | En caso que un usuario con un rol distinto a cajero intente finalizar una orden desde el sistema. | Cuando un MESERO intenta finalizar ingresando: idOrden(INT), idUsuario(INT). | El sistema deniega el acceso y restringe la operación. | PENDIENTE DE VERIFICACION |
### SPS 005 — Como mesero quiero modificar o cancelar una orden antes de su preparación

Implementación: `RestauranteVistas/Controllers/` (Mesas, Pedidos, Cocina, Mesero, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService, PedidoService). Casos relacionados: C5, C6, C7, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 35 | Solo órdenes no preparadas pueden modificarse | En caso que el cliente solicite modificar su pedido antes de que la cocina haya iniciado la preparación de la orden. | Cuando el MESERO selecciona una orden con: idOrden(INT), numeroOrden(STRING), idMesa(INT), numeroMesa(INT) y valida que estadoOrden(ENUM:Solicitada/EnPreparacion/Lista/Entregada/Finalizada/Cancelada)=Solicitada, luego presiona la opción “Modificar orden”. | El sistema deberá habilitar el modo de edición de la orden únicamente para órdenes en estado Solicitada, mostrando el detalle editable (lista de ítems con idDetalle(INT), idProducto(INT), nombreProducto(STRING), cantidadProducto(INT), observacionesItem(STRING)) y confirmando con un mensaje “Orden disponible para modificación”. | PENDIENTE DE VERIFICACION |
| 2.0 / 36 | El mesero puede agregar productos nuevos | En caso que el cliente solicite modificar su pedido antes de que la cocina haya iniciado la preparación de la orden. | Cuando el MESERO agrega idProducto(INT), cantidad(INT), observacionesItem(STRING) | El sistema deberá agregar el nuevo ítem al detalle de la orden, actualizar el subtotal del ítem y recalcular totalOrden(DECIMAL), registrando fechaHoraModificacion(DATETIME) y usuarioModificador(STRING), y mostrando mensaje “Producto agregado correctamente a la orden”. | PENDIENTE DE VERIFICACION |
| 3.0 / 37 | El mesero puede eliminar productos | En caso que el cliente solicite modificar su pedido antes de que la cocina haya iniciado la preparación de la orden. | Cuando el MESERO elimina un ítem del detalle seleccionando: idOrden(INT), idDetalle(INT), idProducto(INT) y confirma “Eliminar ítem”, indicando motivoEliminacion(STRING opcional). | El sistema deberá eliminar el ítem seleccionado del detalle, recalcular totalOrden(DECIMAL), actualizar fechaHoraModificacion(DATETIME) y mostrar mensaje “Producto eliminado y total actualizado”, manteniendo el estadoOrden en Solicitada. | PENDIENTE DE VERIFICACION |
| 4.0 / 38 | No se puede modificar una orden en preparación | En caso que el mesero intente realizar modificaciones sobre una orden que ya se encuentra en proceso de preparación en cocina. | Cuando el MESERO intenta modificar una orden que ya está en preparación seleccionando: idOrden(INT), numeroOrden(STRING) donde estadoOrden=EnPreparacion y presiona “Modificar orden” o intenta cambiar cantidadProducto(INT) / eliminar ítems. | El sistema deberá bloquear cualquier cambio en la orden, mostrar mensaje “No se puede modificar: orden en preparación”, y registrar el intento con: fechaHoraIntento(DATETIME), usuarioResponsable(STRING), idOrden(INT), accionIntentada(STRING) para control interno. | PENDIENTE DE VERIFICACION |
| 5.0 / 39 | El mesero puede cancelar la orden | En caso que el cliente decida cancelar completamente la orden antes de que la cocina inicie su preparación. | Cuando el MESERO selecciona “Cancelar orden” sobre una orden en estadoOrden=Solicitada e ingresa: idOrden(INT), motivoCancelacion(STRING), comentarioCancelacion(STRING opcional) y confirma la cancelación. | El sistema deberá cambiar estadoOrden=Cancelada, registrar fechaHoraCancelacion(DATETIME) y usuarioResponsable(STRING), dejar la orden marcada como no procesable por cocina, y mostrar mensaje “Orden cancelada correctamente”. | PENDIENTE DE VERIFICACION |
| 6.0 / 40 | Al cancelar una orden la mesa debe liberarse | En caso que una orden haya sido cancelada correctamente y la mesa quede sin pedidos activos asociados. | Cuando el MESERO confirma la cancelación y el sistema verifica que la mesa asociada (idMesa(INT), numeroMesa(INT)) no tenga órdenes activas (estadoOrden  Finalizada/Cancelada). | El sistema deberá cambiar estadoMesa=Disponible, registrar fechaHoraLiberacionMesa(DATETIME) y mostrar mensaje “Mesa liberada y disponible”, asegurando que la mesa quede lista para una nueva orden. | PENDIENTE DE VERIFICACION |
| 7.0 / 41 | La cancelación debe quedar registrada | En caso que se requiera mantener un registro histórico de las órdenes canceladas para control y auditoría administrativa. | Cuando una orden cambia a estadoOrden=Cancelada, el sistema genera el registro de auditoría capturando: idOrden(INT), numeroOrden(STRING), idMesa(INT), idMesero(INT), motivoCancelacion(STRING), usuarioResponsable(STRING), fechaHoraCancelacion(DATETIME). | El sistema deberá guardar la cancelación en el historial/auditoría de órdenes, permitiendo su consulta posterior por ADMIN/CAJERO, y mostrar confirmación “Cancelación registrada para control y auditoría”. | PENDIENTE DE VERIFICACION |
### SPS 006 — Como mesero quiero dar seguimiento al estado del pedido para informar al cliente

Implementación: `RestauranteVistas/Controllers/` (Mesas, Pedidos, Cocina, Mesero, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService, PedidoService). Casos relacionados: C5, C6, C7, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 43 | El mesero puede visualizar el estado de la orden | En caso que el cliente solicite información sobre el estado actual de su pedido mientras se encuentra en la mesa. | Cuando el MESERO consulta la orden ingresando: idOrden(INT), numeroMesa(INT) | El sistema muestra estadoOrden actual | PENDIENTE DE VERIFICACION |
| 2.0 / 44 | El sistema debe reflejar cambios en tiempo real | En caso que el cliente solicite información sobre el estado actual de su pedido mientras se encuentra en la mesa. | Cuando el COCINERO actualiza estados ingresando: idOrden(INT), idProducto(INT), estadoItem(ENUM:Solicitada/EnPreparacion/Listo), fechaHoraCambio(DATETIME) | El sistema le permitirá al mesero ver los cambios inmediatamente en el sistema | PENDIENTE DE VERIFICACION |
| 3.0 / 45 | El mesero puede ver estados intermedios | En caso que el cliente solicite información sobre el estado actual de su pedido mientras se encuentra en la mesa. | Cuando el MESERO revisa la orden ingresando: idOrden(INT) | El sistema muestra: Solicitada / EnPreparacion / Lista | PENDIENTE DE VERIFICACION |
| 4.0 / 46 | El cliente puede ser informado con precisión | En caso que el cliente solicite información sobre el estado actual de su pedido mientras se encuentra en la mesa. | Cuando el MESERO consulta la orden ingresando: idOrden(INT), tipoProducto(STRING) | El sistema muestra tiempos estimados depende del producto | PENDIENTE DE VERIFICACION |
| 5.0 / 47 | El sistema registra cada cambio de estado | En caso que se requiera mantener un registro histórico de los cambios de estado del pedido para control operativo y fines de auditoría. | Cuando cambia el estado de la orden ingresando: idOrden(INT), estadoAnterior(ENUM), estadoNuevo(ENUM), usuarioResponsable(STRING), fechaHoraCambio(DATETIME) | El sistema guarda historialEstados(fecha, usuario, estado) | PENDIENTE DE VERIFICACION |
| 6.0 / 48 | El administrador puede auditar el seguimiento | En caso que el administrador necesite revisar el seguimiento completo de una orden para fines de supervisión o auditoría del servicio. | Cuando el ADMIN accede al seguimiento ingresando: idOrden(INT), rangoFechas(DATE opcional) | El sistema muestra trazabilidad completa del seguimiento del pedido | PENDIENTE DE VERIFICACION |
| 7.0 / 49 | Órdenes cerradas no deben modificarse | En caso que se intente modificar el estado de una orden que ya se encuentra cerrada o finalizada. | Cuando se intenta cambiar el estado ingresando: idOrden(INT), nuevoEstado(ENUM) | El sistema deberá bloquear la acción y pone un mensaje que diga "Orden ya se encuentra finalizada" | PENDIENTE DE VERIFICACION |
### SPS 007 — Como administrador quiero consultar la bitácora de eventos de los pedidos

Implementación: `RestauranteVistas/Controllers/` (Mesas, Pedidos, Cocina, Mesero, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService, PedidoService). Casos relacionados: C5, C6, C7, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 51 | Carga exitosa del registro de auditoría de órdenes. | En caso de que el administrador necesite auditar los cambios de estado, anulaciones o modificaciones en las órdenes de los clientes por control de calidad. | Cuando el ADMIN accede a la vista de "Bitácora de Pedidos" e inicializa el módulo. | El sistema consulta tabla_bitacora_pedidos y muestra: idRegistro(INT), idOrdenAfectada(INT), accionOperativa(ENUM), idUsuarioResponsable(INT) y fechaHora(DATETIME). | PENDIENTE DE VERIFICACION |
| 2.0 / 52 | Filtro de eventos por número de orden específica. | En caso de que un cliente presente una queja y el administrador necesite revisar todo el ciclo de vida y tiempos de ese pedido exacto. | Cuando el ADMIN ingresa un idOrdenAfectada(INT) específico en el panel de búsqueda de la bitácora. | El sistema filtra los logs y despliega únicamente la trazabilidad completa (creación, preparación, entrega, cobro) de esa orden en particular. | PENDIENTE DE VERIFICACION |
| 3.0 / 53 | Filtro por tipo de acción (Anulación, Modificación, Cambio de Estado). | En caso de que el administrador requiera auditar cuántas órdenes fueron canceladas o modificadas después de haber sido enviadas a la cocina. | Cuando el ADMIN selecciona el filtro accionOperativa(ENUM:Anulacion / EdicionItems / CambioEstado). | El sistema actualiza la vista mostrando exclusivamente los registros técnicos que coincidan con el tipo de evento seleccionado. | PENDIENTE DE VERIFICACION |
| 4.0 / 54 | Búsqueda de eventos asociados a un empleado específico | En caso de que el administrador necesite evaluar si un mesero en particular está cometiendo muchos errores o anulaciones de platillos. | Cuando el ADMIN selecciona un idUsuarioResponsable(INT) en los filtros de búsqueda. | El sistema ejecuta el SELECT y despliega todas las alteraciones en los pedidos realizadas únicamente por ese empleado. | PENDIENTE DE VERIFICACION |
| 5.0 / 55 | Búsqueda de eventos por rango cronológico. | En caso de que se necesite revisar las incidencias ocurridas con los pedidos durante un turno, día o semana específica. | Cuando el ADMIN ingresa fechaInicio(DATE) y fechaFin(DATE) válidas en el buscador. | El sistema filtra la base de datos y despliega todos los rastros de auditoría ocurridos estrictamente dentro de ese periodo de tiempo. | PENDIENTE DE VERIFICACION |
| 6.0 / 56 | Exportación de la trazabilidad de pedidos. | En caso de que se requiera documentar las incidencias operativas del salón y cocina para una reunión de mejora continua. | Cuando el ADMIN visualiza los registros filtrados y selecciona la opción exportar(BOOL) con formato(ENUM:PDF/Excel). | El sistema procesa los logs mostrados, genera el archivo bitacoraPedidos(FILE) con los datos técnicos y habilita su descarga segura. | PENDIENTE DE VERIFICACION |
| 7.0 / 57 | Restricción estricta de acceso al registro de auditoría. | En caso de que un empleado intente revisar el historial de auditoría para ver si sus modificaciones fueron detectadas. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero/Cajero) intenta acceder a la ruta de la bitácora de pedidos. | El sistema bloquea la renderización de la vista y muestra la alerta “Acceso denegado: El módulo de auditoría es exclusivo para el Administrador”. | PENDIENTE DE VERIFICACION |

## Inventario

### INV 001 — Como administrador quiero registrar un nuevo insumo

Implementación: `RestauranteVistas/Controllers/` (Inventario, Productos), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (InventarioService, ProductoService). Casos relacionados: C9, C10, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 3 | El sistema registra el insumo exitosamente si los datos son válidos. | En caso de que ingrese un nuevo ingrediente al catálogo de El Colibrí y se deba contabilizar. | Cuando el ADMIN ingresa: nombreInsumo(STRING), idCategoria(INT), unidadMedida(ENUM:Kg/L/Unidad) y stockMinimo(DECIMAL). | El sistema inserta el registro, autogenera idInsumo(INT), asigna estadoActivo(BOOL)=TRUE y muestra “Insumo registrado”. | PENDIENTE DE VERIFICACION |
| 2.0 / 4 | Bloqueo por nombre de insumo duplicado. | En caso de que ingrese un nuevo ingrediente al catálogo de El Colibrí y se deba contabilizar. | Cuando el ADMIN intenta registrar un nombreInsumo(STRING) que ya existe en la base de datos. | El sistema detiene la inserción y muestra el mensaje “Error: Ya existe un insumo registrado con este nombre”. | PENDIENTE DE VERIFICACION |
| 3.0 / 5 | El stock inicial se establece en cero por defecto. | En caso de que ingrese un nuevo ingrediente al catálogo de El Colibrí y se deba contabilizar. | Cuando el ADMIN finaliza la creación del insumo sin registrar una compra inicial. | El sistema asigna automáticamente cantidadActual(DECIMAL)=0 al nuevo idInsumo(INT) creado. | PENDIENTE DE VERIFICACION |
| 4.0 / 6 | Validación de campos obligatorios vacíos. | En caso de que ingrese un nuevo ingrediente al catálogo de El Colibrí y se deba contabilizar. | Cuando el ADMIN intenta guardar dejando nombreInsumo(STRING) o idCategoria(INT) como NULL. | El sistema bloquea el guardado, resalta los campos en rojo y muestra “Complete los campos requeridos”. | PENDIENTE DE VERIFICACION |
| 5.0 / 7 | El stock mínimo no puede ser un valor negativo. | En caso de que ingrese un nuevo ingrediente al catálogo de El Colibrí y se deba contabilizar. | Cuando el ADMIN digita un stockMinimo(DECIMAL) menor a 0. | El sistema bloquea la acción y avisa “El nivel de alerta de stock mínimo debe ser 0 o mayor”. | PENDIENTE DE VERIFICACION |
| 6.0 / 8 | Asignación automática de proveedor principal. | En caso de que ingrese un nuevo ingrediente al catálogo de El Colibrí y se deba contabilizar. | Cuando el ADMIN selecciona idProveedor(INT) de la lista desplegable durante la creación. | El sistema crea la relación en la tabla intermedia insumo_proveedor para futuras órdenes de compra. | PENDIENTE DE VERIFICACION |
| 7.0 / 9 | Restricción de creación por nivel de rol. | En caso que un empleado sin privilegios intente alterar el catálogo. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero) intenta abrir el formulario de "Nuevo Insumo". | El sistema bloquea la vista y muestra “Acceso denegado: Solo Administradores pueden crear insumos”. | PENDIENTE DE VERIFICACION |
### INV 002 — Como administrador quiero consultar el catálogo de insumos

Implementación: `RestauranteVistas/Controllers/` (Inventario, Productos), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (InventarioService, ProductoService). Casos relacionados: C9, C10, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 11 | El sistema despliega la lista completa de existencias. | En caso de que se requiera revisar las cantidades actuales de ingredientes en la bodega o cuarto frío. | Cuando el ADMIN accede al módulo de inventario e inicializa la vista principal | El sistema ejecuta un SELECT a la tabla Insumos y muestra idInsumo(INT), nombre(STRING) y cantidadActual(DECIMAL). | PENDIENTE DE VERIFICACION |
| 2.0 / 12 | Búsqueda dinámica por coincidencia de nombre. | En caso de que se requiera revisar las cantidades actuales de ingredientes en la bodega o cuarto frío. | Cuando el ADMIN ingresa un terminoBusqueda(STRING) en la barra superior. | El sistema filtra la tabla en tiempo real mostrando solo los registros donde el nombre coincida con el texto. | PENDIENTE DE VERIFICACION |
| 3.0 / 13 | Filtrado de insumos por categoría específica. | En caso de que se requiera revisar las cantidades actuales de ingredientes en la bodega o cuarto frío. | Cuando el ADMIN selecciona idCategoria(INT) en el panel de filtros. | El sistema actualiza la vista para mostrar únicamente los insumos asociados a dicha categoría. | PENDIENTE DE VERIFICACION |
| 4.0 / 14 | Indicador visual de alerta para stock bajo. | En caso de que se requiera revisar las cantidades actuales de ingredientes en la bodega o cuarto frío. | Cuando el sistema carga la lista y detecta registros donde cantidadActual(DECIMAL) <= stockMinimo(DECIMAL). | El sistema resalta la fila en color rojo o amarillo y activa el icono de advertencia. | PENDIENTE DE VERIFICACION |
| 5.0 / 15 | Ocultamiento automático de insumos inactivos. | En caso de que se requiera revisar las cantidades actuales de ingredientes en la bodega o cuarto frío. | Cuando el sistema renderiza la tabla de inventario general. | El sistema excluye mediante consulta (WHERE estadoActivo=TRUE) los productos descontinuados para mantener limpia la vista. | PENDIENTE DE VERIFICACION |
| 6.0 / 16 | Paginación de resultados masivos. | En caso de que se requiera revisar las cantidades actuales de ingredientes en la bodega o cuarto frío. | Cuando la cantidad de registros recuperados supera el limitePorPagina(INT=50). | El sistema divide los resultados y habilita los controles de navegación (Siguiente/Anterior). | PENDIENTE DE VERIFICACION |
| 7.0 / 17 | Validación de permisos para lectura de existencias. | En caso que un cajero intente ver cuánto producto queda disponible en bodega. | Cuando un usuario con rolUsuario(ENUM:Cajero) intenta acceder a la ruta del inventario general. | El sistema detiene el enrutamiento y muestra “Acceso denegado a este módulo”. | PENDIENTE DE VERIFICACION |
### INV 003 — Como administrador quiero modificar los datos de un insumo

Implementación: `RestauranteVistas/Controllers/` (Inventario, Productos), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (InventarioService, ProductoService). Casos relacionados: C9, C10, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 19 | Actualización exitosa de los detalles del insumo. | En caso de que el proveedor cambie la presentación o se necesite reubicar el ingrediente de categoría. | Cuando el ADMIN edita un registro e ingresa: nuevoNombre(STRING), nuevaCategoria(INT) y confirma. | El sistema ejecuta el UPDATE, guarda los cambios en la base de datos y muestra “Insumo actualizado”. | PENDIENTE DE VERIFICACION |
| 2.0 / 20 | Restricción de cambio de unidad si hay stock existente. | En caso de que el proveedor cambie la presentación o se necesite reubicar el ingrediente de categoría. | Cuando el ADMIN intenta editar unidadMedida(ENUM) en un idInsumo(INT) donde cantidadActual(DECIMAL) > 0. | El sistema bloquea ese campo específico para evitar errores matemáticos y avisa “No se puede cambiar la unidad con stock mayor a cero”. | PENDIENTE DE VERIFICACION |
| 3.0 / 21 | Bloqueo al intentar renombrar con un nombre ya existente. | En caso de que el proveedor cambie la presentación o se necesite reubicar el ingrediente de categoría. | Cuando el ADMIN cambia el nombreInsumo(STRING) por uno que ya le pertenece a otro idInsumo(INT). | El sistema detiene el proceso y muestra “El nombre ingresado ya está en uso por otro artículo”. | PENDIENTE DE VERIFICACION |
| 4.0 / 22 | Actualización correcta del nivel de stock mínimo. | En caso de que el proveedor cambie la presentación o se necesite reubicar el ingrediente de categoría. | Cuando el ADMIN edita el campo stockMinimo(DECIMAL) con un nuevo valor válido. | El sistema guarda el cambio y recalcula inmediatamente si el insumo debe entrar en estado de alerta. | PENDIENTE DE VERIFICACION |
| 5.0 / 23 | Impedir guardar modificaciones con campos vacíos. | En caso de que el proveedor cambie la presentación o se necesite reubicar el ingrediente de categoría. | Cuando el ADMIN borra el contenido de un campo obligatorio e intenta hacer clic en guardar. | El sistema deshabilita la acción y advierte “No se pueden dejar campos obligatorios en blanco”. | PENDIENTE DE VERIFICACION |
| 6.0 / 24 | Registro en bitácora de la modificación. | En caso de que el proveedor cambie la presentación o se necesite reubicar el ingrediente de categoría. | Cuando se ejecuta exitosamente el UPDATE sobre los datos del insumo. | El sistema inserta un registro de auditoría capturando idUsuario(INT) y fechaModificacion(DATETIME). | PENDIENTE DE VERIFICACION |
| 7.0 / 25 | Restricción de edición por nivel de permisos. | En caso que un empleado intente alterar el stock mínimo para evitar alertas. | Cuando un usuario con rolUsuario(ENUM:Cocinero) intenta presionar el botón de editar en un idInsumo(INT). | El sistema bloquea la acción y muestra “Se requieren privilegios administrativos para modificar el catálogo”. | PENDIENTE DE VERIFICACION |
### INV 004 — Como administrador quiero registrar un ajuste manual de existencias

Implementación: `RestauranteVistas/Controllers/` (Inventario, Productos), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (InventarioService, ProductoService). Casos relacionados: C9, C10, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 27 | Ajuste de entrada (suma) registrado exitosamente. | En caso de que se detecten diferencias por conteo físico en bodega o ingresos de emergencia. | Cuando el ADMIN selecciona idInsumo(INT) e ingresa: tipoAjuste(ENUM:Entrada), cantidadAjuste(DECIMAL), motivo(STRING). | El sistema suma la cantidad al stock actual, guarda el movimiento y muestra “Inventario ajustado correctamente”. | PENDIENTE DE VERIFICACION |
| 2.0 / 28 | Ajuste de salida (resta/merma) registrado exitosamente. | En caso de que un ingrediente se dañe o expire y deba ser retirado físicamente. | Cuando el ADMIN selecciona idInsumo(INT) e ingresa: tipoAjuste(ENUM:Salida), cantidadAjuste(DECIMAL), motivo(STRING). | El sistema resta la cantidad del stock actual, guarda el movimiento y muestra “Merma registrada”. | PENDIENTE DE VERIFICACION |
| 3.0 / 29 | Bloqueo si el ajuste de salida genera stock negativo. | En caso de que se intente retirar más cantidad de la que el sistema indica que existe. | Cuando el ADMIN ingresa una cantidadAjuste(DECIMAL) en salida que es mayor a la cantidadActual(DECIMAL). | El sistema bloquea la resta y muestra la alerta “El ajuste no puede dejar el inventario en números negativos”. | PENDIENTE DE VERIFICACION |
| 4.0 / 30 | Validación obligatoria del motivo del ajuste. | En caso de que se intente alterar las existencias sin justificar la razón (prevención de robos). | Cuando el ADMIN intenta procesar el ajuste dejando el campo motivo(STRING) vacío o nulo. | El sistema detiene el proceso y muestra “Debe escribir una justificación válida para alterar el inventario”. | PENDIENTE DE VERIFICACION |
| 5.0 / 31 | El sistema detona alerta si la salida rompe el stock mínimo. | En caso de que el retiro de producto deje al restaurante sin los insumos necesarios para operar. | Cuando el ajuste de salida se procesa y la nueva cantidadActual(DECIMAL) queda por debajo del stockMinimo(DECIMAL). | El sistema registra el ajuste y dispara inmediatamente una notificación visual de "Stock Crítico". | PENDIENTE DE VERIFICACION |
| 6.0 / 32 | Trazabilidad del movimiento en el Kárdex (Historial). | En caso de que se requiera auditar quién y cuándo modificó los ingredientes a mano. | Cuando el sistema procesa el ajuste y actualiza la base de datos principal. | El sistema inserta los datos en tabla_movimientos incluyendo idUsuario(INT), cantidad(DECIMAL) y fechaHora(DATETIME). | PENDIENTE DE VERIFICACION |
| 7.0 / 33 | Restricción estricta de ajustes manuales. | En caso que un empleado de cocina intente ajustar existencias por su cuenta. | Cuando un usuario con rolUsuario(ENUM:Cocinero) intenta ingresar a la vista de "Ajustes Manuales". | El sistema bloquea el acceso y muestra “Función restringida únicamente para Administradores”. | PENDIENTE DE VERIFICACION |
### INV 005 — Como administrador quiero desactivar un insumo del inventario

Implementación: `RestauranteVistas/Controllers/` (Inventario, Productos), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (InventarioService, ProductoService). Casos relacionados: C9, C10, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 35 | Desactivación exitosa (Borrado lógico). | En caso de que un producto sea descontinuado del menú o ya no se le compre a ningún proveedor. | Cuando el ADMIN selecciona idInsumo(INT) y ejecuta la acción "Desactivar/Eliminar". | El sistema ejecuta un UPDATE cambiando estadoActivo(BOOL)=FALSE y muestra “Insumo desactivado del sistema”. | PENDIENTE DE VERIFICACION |
| 2.0 / 36 | Bloqueo de desactivación si aún hay existencias. | En caso de que un producto sea descontinuado del menú o ya no se le compre a ningún proveedor. | Cuando el ADMIN intenta desactivar un idInsumo(INT) que todavía posee cantidadActual(DECIMAL) > 0. | El sistema impide la acción y advierte “No se puede desactivar un insumo que aún tiene stock disponible, registre una salida primero”. | PENDIENTE DE VERIFICACION |
| 3.0 / 37 | Reactivación de un insumo previamente desactivado. | En caso de que el restaurante decida volver a utilizar un ingrediente que se había descontinuado meses atrás. | Cuando el ADMIN consulta la lista de inactivos y cambia estadoActivo(BOOL)=TRUE. | El sistema restaura el registro, lo vuelve a mostrar en las listas principales y avisa “Insumo reactivado”. | PENDIENTE DE VERIFICACION |
| 4.0 / 38 | Exclusión automática en compras y recetas. | En caso de que un producto sea descontinuado del menú o ya no se le compre a ningún proveedor. | Cuando el estadoActivo(BOOL) de un insumo cambia a FALSE en la base de datos. | El sistema oculta inmediatamente ese idInsumo(INT) de las listas desplegables de ajustes, compras y formulación de platillos. | PENDIENTE DE VERIFICACION |
| 5.0 / 39 | Conservación del historial financiero tras desactivar. | En caso de que se necesiten consultar reportes de meses anteriores donde el insumo sí se utilizó. | Cuando se generan reportes de egresos y facturas antiguas que contienen un idInsumo(INT) desactivado. | El sistema muestra los datos históricos correctamente sin lanzar errores de llave foránea (integridad referencial). | PENDIENTE DE VERIFICACION |
| 6.0 / 40 | Registro de auditoría por desactivación. | En caso de que un producto sea descontinuado del menú o ya no se le compre a ningún proveedor. | Cuando el sistema procesa la solicitud de baja lógica del registro. | El sistema captura el idUsuario(INT) responsable y la fechaHora(DATETIME), guardándolo en la bitácora de seguridad. | PENDIENTE DE VERIFICACION |
| 7.0 / 41 | Restricción jerárquica para dar de baja insumos. | En caso que un usuario intente remover elementos del catálogo sin autorización. | Cuando un usuario con rolUsuario(ENUM:Mesero/Cajero) intenta accionar el borrado de un idInsumo(INT). | El sistema bloquea la orden y muestra “Privilegios insuficientes para eliminar registros del catálogo”. | PENDIENTE DE VERIFICACION |
### INV 006 — Como administrador quiero consultar la bitácora de eventos del inventario

Implementación: `RestauranteVistas/Controllers/` (Inventario, Productos), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (InventarioService, ProductoService). Casos relacionados: C9, C10, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 43 | Carga exitosa del registro de auditoría general. | En caso de que el administrador necesite auditar las acciones, modificaciones o accesos realizados en el módulo de inventario por motivos de seguridad. | Cuando el ADMIN accede a la vista de "Bitácora" e inicializa el módulo. | El sistema consulta la tabla_bitacora y muestra: idRegistro(INT), idUsuario(INT), accionOperativa(ENUM), modulo(STRING), fechaHora(DATETIME) y detalleCambio(STRING). | PENDIENTE DE VERIFICACION |
| 2.0 / 44 | Filtro de eventos por usuario específico. | En caso de que el administrador necesite auditar las acciones, modificaciones o accesos realizados en el módulo de inventario por motivos de seguridad. | Cuando el ADMIN selecciona un idUsuario(INT) desde el panel de búsqueda de la bitácora. | El sistema filtra los logs y despliega únicamente los eventos y modificaciones ejecutadas por ese empleado en particular. | PENDIENTE DE VERIFICACION |
| 3.0 / 45 | Filtro por tipo de acción (Creación, Edición, Eliminación). | En caso de que el administrador necesite auditar las acciones, modificaciones o accesos realizados en el módulo de inventario por motivos de seguridad. | Cuando el ADMIN selecciona el filtro accionOperativa(ENUM:INSERT/UPDATE/DELETE/AJUSTE). | El sistema actualiza la vista mostrando exclusivamente los registros técnicos que coincidan con la operación seleccionada. | PENDIENTE DE VERIFICACION |
| 4.0 / 46 | Búsqueda de eventos por rango cronológico. | En caso de que el administrador necesite auditar las acciones, modificaciones o accesos realizados en el módulo de inventario por motivos de seguridad. | Cuando el ADMIN ingresa fechaInicio(DATE) y fechaFin(DATE) válidas en el buscador. | El sistema ejecuta el SELECT y despliega todos los rastros de auditoría ocurridos estrictamente dentro de ese periodo. | PENDIENTE DE VERIFICACION |
| 5.0 / 47 | Inmutabilidad de los registros de la bitácora. | En caso de que se intente alterar el historial de auditoría para ocultar un movimiento irregular. | Cuando cualquier usuario, incluyendo el ADMIN, intenta ejecutar una sentencia de edición (UPDATE) o borrado (DELETE) sobre un idRegistro(INT) de la bitácora. | El sistema bloquea la acción a nivel de base de datos, garantizando que el historial sea estrictamente de solo lectura (inmutable). | PENDIENTE DE VERIFICACION |
| 6.0 / 48 | Exportación de la bitácora para auditoría externa. | En caso de que el administrador necesite auditar las acciones, modificaciones o accesos realizados en el módulo de inventario por motivos de seguridad. | Cuando el ADMIN visualiza los registros filtrados y selecciona la opción exportar(BOOL) con formato(ENUM:PDF/Excel). | El sistema procesa los logs, genera el archivo bitacoraInventario(FILE) con los datos técnicos y habilita su descarga segura. | PENDIENTE DE VERIFICACION |
| 7.0 / 49 | Restricción estricta de acceso al registro de auditoría. | En caso de que un empleado intente revisar qué acciones están siendo monitoreadas por el sistema. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero/Cajero) intenta acceder a la ruta de la bitácora de inventario. | El sistema bloquea la renderización de la vista y muestra la alerta “Acceso denegado: El módulo de auditoría es exclusivo para Administradores”. | PENDIENTE DE VERIFICACION |

## Contabilidad 

### CNT 001 — Como administrador quiero realizar el cierre de caja diario

Implementación: `RestauranteVistas/Controllers/` (Contabilidad, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (ContabilidadService, OperacionController). Casos relacionados: C7, C8, C11, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 3 | El sistema procesa el cierre exitosamente si los montos coinciden. | En caso que el administrador finalice la jornada laboral y deba liquidar los ingresos del día. | Cuando el ADMIN selecciona "Cerrar Caja" e ingresa: idCierre(INT), fechaCierre(DATE), idUsuario(INT), montoApertura(DECIMAL), totalVentasEfectivo(DECIMAL), totalVentasSINPE(DECIMAL), totalEgresos(DECIMAL) y saldoReal(DECIMAL) siendo igual al saldoEsperado(DECIMAL). | El sistema cambia estadoCaja=Cerrada, guarda el registro, genera reporteCierre(FILE) y muestra el mensaje “Cierre diario completado con éxito”. | PENDIENTE DE VERIFICACION |
| 2.0 / 4 | El sistema registra un cierre con faltante de dinero. | En caso que el administrador finalice la jornada laboral y deba liquidar los ingresos del día. | Cuando el ADMIN ingresa: saldoReal(DECIMAL) menor al saldoEsperado(DECIMAL), detalleObservacion(STRING) y confirma la diferencia. | El sistema guarda el cierre, marca descuadre=Faltante, registra montoDiferencia(DECIMAL) y notifica al usuario del descuadre detectado. | PENDIENTE DE VERIFICACION |
| 3.0 / 5 | El sistema debe permitir agrupar los cierres diarios para generar el cierre semanal | En caso que el administrador finalice la jornada laboral y deba liquidar los ingresos del día. | Cuando el ADMIN ingresa: saldoReal(DECIMAL) mayor al saldoEsperado(DECIMAL), detalleObservacion(STRING) y confirma el excedente. | El sistema guarda el cierre, marca descuadre=Sobrante, registra montoDiferencia(DECIMAL) y actualiza la bitácora contable. | PENDIENTE DE VERIFICACION |
| 4.0 / 6 | Bloqueo del cierre si existen órdenes activas sin pagar. | En caso que el administrador finalice la jornada laboral y deba liquidar los ingresos del día. | Cuando el ADMIN intenta procesar el cierre pero el sistema detecta registros en la tabla Pedidos con estadoOrden(ENUM:Pendiente/EnPreparacion/Listo). | El sistema bloquea el botón de "Confirmar Cierre" y muestra el mensaje “Error: Existen órdenes abiertas que deben ser finalizadas o canceladas”. | PENDIENTE DE VERIFICACION |
| 5.0 / 7 | Impedir la duplicación de cierres para una misma fecha. | En caso que el administrador necesite un reporte fiscal y financiero de todo el año | Cuando el ADMIN intenta guardar un cierre ingresando una fechaCierre(DATE) que ya posee un idCierre(INT) con estadoCierre=Completado. | El sistema rechaza la transacción y muestra la alerta “Ya existe un cierre contable registrado para el día seleccionado”. | PENDIENTE DE VERIFICACION |
| 6.0 / 8 | Desglose automático de ventas por método de pago. | En caso que el administrador finalice la jornada laboral y deba liquidar los ingresos del día. | Cuando el ADMIN accede a la pantalla de liquidación y el sistema carga: sumatoriaFacturasEfectivo(DECIMAL) y sumatoriaFacturasSINPE(DECIMAL). | El sistema muestra el comparativo visual entre las ventas del sistema y el dinero reportado, calculando automáticamente el saldoEsperado(DECIMAL). | PENDIENTE DE VERIFICACION |
| 7.0 / 9 | Restricción de acceso a la liquidación por nivel de rol. | En caso que otro usuario diferente a administrador finalice la jornada laboral y deba liquidar los ingresos del día. | Cuando un usuario con rolUsuario(ENUM:Mesero/Cocinero) intenta ingresar al módulo de "Cierres Contables" con su idUsuario(INT). | El sistema bloquea la interfaz de contabilidad y muestra el mensaje “Acceso denegado: Se requieren permisos de nivel Administrador”. | PENDIENTE DE VERIFICACION |
### CNT 002 — Como usuario cajero quiero registrar el pago de una orden

Implementación: `RestauranteVistas/Controllers/` (Contabilidad, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (ContabilidadService, OperacionController). Casos relacionados: C7, C8, C11, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 11 | Registro de pago exitoso con monto exacto en efectivo. | En caso que el cliente solicite la cuenta y proceda a realizar el pago de su consumo en la caja. | Cuando el CAJERO selecciona idOrden(INT) e ingresa: idFactura(INT), idCaja(INT), metodoPago(ENUM:Efectivo), montoTotal(DECIMAL), montoRecibido(DECIMAL) igual al montoTotal. | El sistema registra el pago, genera idTransaccion(INT), cambia estadoFactura=Pagada y muestra el mensaje “Pago en efectivo procesado con éxito”. | PENDIENTE DE VERIFICACION |
| 2.0 / 12 | Registro de pago exitoso mediante transferencia SINPE | En caso que el cliente solicite la cuenta y proceda a realizar el pago de su consumo en la caja. | Cuando el CAJERO selecciona idOrden(INT) e ingresa: idFactura(INT), metodoPago(ENUM:SINPE), numComprobanteSINPE(STRING), montoTotal(DECIMAL), montoRecibido(DECIMAL) igual al montoTotal. | El sistema vincula el número de comprobante al registro, cambia estadoFactura=Pagada y muestra “Transacción SINPE registrada correctamente”. | PENDIENTE DE VERIFICACION |
| 3.0 / 13 | Cálculo automático de vuelto en pagos con efectivo. | En caso que el cliente solicite la cuenta y proceda a realizar el pago de su consumo en la caja. | Cuando el CAJERO selecciona idFactura(INT) e ingresa: metodoPago(ENUM:Efectivo), montoTotal(DECIMAL), montoRecibido(DECIMAL) mayor al montoTotal. | El sistema calcula la variable vuelto(DECIMAL) = montoRecibido - montoTotal, registra el pago y muestra el monto exacto a entregar al cliente. | PENDIENTE DE VERIFICACION |
| 4.0 / 14 | Bloqueo de transacción por monto recibido insuficiente. | En caso que el cliente solicite la cuenta y proceda a realizar el pago de su consumo en la caja. | Cuando el CAJERO intenta procesar el pago ingresando: montoRecibido(DECIMAL) menor al montoTotal(DECIMAL) de la factura idFactura(INT). | El sistema bloquea el botón de "Confirmar Pago" y muestra el mensaje de error “Monto insuficiente para cubrir el total de la factura”. | PENDIENTE DE VERIFICACION |
| 5.0 / 15 | Validación obligatoria de selección de método de pago. | En caso que el cliente solicite la cuenta y proceda a realizar el pago de su consumo en la caja. | Cuando el CAJERO intenta guardar la factura idFactura(INT) dejando el campo metodoPago(ENUM) como NULL o vacío. | El sistema detiene la acción, resalta el campo en rojo y muestra la alerta “Debe seleccionar Efectivo o SINPE para finalizar”. | PENDIENTE DE VERIFICACION |
| 6.0 / 16 | Restricción de doble pago para facturas ya canceladas. | En caso que se intente procesar un pago sobre una orden que ya fue liquidada previamente. | Cuando el CAJERO selecciona un idOrden(INT) cuyo estadoFactura ya es igual a Pagada e intenta ingresar un nuevo montoRecibido(DECIMAL). | El sistema bloquea los campos de edición y muestra el mensaje informativo “Esta orden ya cuenta con un pago registrado y se encuentra cerrada”. | PENDIENTE DE VERIFICACION |
| 7.0 / 17 | Validación de permisos para la gestión de cobros. | En caso que un usuario sin los permisos requeridos intente acceder al módulo de facturación y cobro. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero) intenta abrir la pantalla de pagos ingresando su idUsuario(INT). | El sistema deniega el acceso a la interfaz de cobro y muestra el mensaje “Acceso denegado: Su rol no cuenta con permisos para procesar pagos”. | PENDIENTE DE VERIFICACION |
### CNT 003 — Como administrador quiero generar una nota de crédito asociada a una factura registrada para corregir errores contables sin eliminar el documento original.

Implementación: `RestauranteVistas/Controllers/` (Contabilidad, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (ContabilidadService, OperacionController). Casos relacionados: C7, C8, C11, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 19 | La nota de crédito se genera correctamente cuando se registran los datos requeridos. | En caso que el administrador detecte un error en una factura ya procesada y necesite realizar una corrección contable del documento. | Cuando el ADMIN registra idFactura(INT), motivoNotaCredito(STRING), idUsuario(INT) y fechaHoraRegistro(DATETIME) para generar la nota de crédito. | El sistema genera idNotaCredito(INT), vincula la factura original, actualiza estadoFactura=Corregida y muestra el mensaje “Nota de crédito registrada correctamente”. | PENDIENTE DE VERIFICACION |
| 2.0 / 20 | El sistema valida que el motivo de la nota de crédito sea obligatorio. | En caso que el administrador requiera emitir una nota de crédito para justificar una corrección contable. | Cuando el ADMIN intenta registrar idFactura(INT) pero deja motivoNotaCredito(STRING) vacío o NULL. | El sistema bloquea la operación, resalta el campo motivoNotaCredito y muestra la alerta “Debe ingresar el motivo de la nota de crédito”. | PENDIENTE DE VERIFICACION |
| 3.0 / 21 | El sistema impide generar notas de crédito en periodos contables cerrados. | En caso que el administrador intente realizar ajustes contables sobre una factura perteneciente a un periodo contable cerrado. | Cuando el ADMIN intenta generar notaCredito utilizando idFactura(INT) que pertenece a idCierreContable(INT) con estadoCierre(ENUM)=Completado. | El sistema bloquea la transacción y muestra el mensaje “No se pueden generar notas de crédito en periodos contables cerrados”. | PENDIENTE DE VERIFICACION |
| 4.0 / 22 | El sistema actualiza automáticamente los ingresos al emitir una nota de crédito. | En caso que se genere una nota de crédito para revertir total o parcialmente el monto de una factura registrada. | Cuando el ADMIN confirma la nota de crédito ingresando idFactura(INT), montoNotaCredito(DECIMAL) y metodoPago(ENUM:Efectivo/SINPE/Tarjeta). | El sistema descuenta montoNotaCredito del totalIngresos(DECIMAL) del turno activo y actualiza el balance de caja en tiempo real. | PENDIENTE DE VERIFICACION |
| 5.0 / 23 | El sistema evita registrar notas de crédito duplicadas para una misma factura. | En caso que el administrador intente registrar nuevamente una nota de crédito sobre una factura que ya fue corregida previamente. | Cuando el ADMIN intenta generar notaCredito utilizando idFactura(INT) que ya posee estadoNotaCredito(ENUM)=Generada. | El sistema bloquea la operación y muestra el mensaje “La factura ya posee una nota de crédito registrada en el sistema”. | PENDIENTE DE VERIFICACION |
| 6.0 / 24 | El sistema registra auditoría de las notas de crédito generadas. | En caso que se requiera mantener trazabilidad de las correcciones contables realizadas en el sistema. | Cuando el sistema registra idNotaCredito(INT), idFactura(INT), idUsuarioResponsable(INT), motivoNotaCredito(STRING) y fechaHoraRegistro(DATETIME). | El sistema guarda la información en la tabla bitacoraNotasCredito permitiendo su consulta posterior en el módulo de auditoría. | PENDIENTE DE VERIFICACION |
| 7.0 / 25 | El sistema restringe la generación de notas de crédito a usuarios autorizados. | En caso que un usuario sin permisos administrativos intente realizar correcciones contables en el sistema. | Cuando un usuario con rolUsuario(ENUM:Mesero/Cajero) intenta generar notaCredito ingresando idFactura(INT). | El sistema bloquea el acceso a la funcionalidad y muestra el mensaje “Acceso denegado: solo el Administrador puede generar notas de crédito”. | PENDIENTE DE VERIFICACION |
### CNT 004 — Como administrador quiero registrar un egreso de dinero

Implementación: `RestauranteVistas/Controllers/` (Contabilidad, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (ContabilidadService, OperacionController). Casos relacionados: C7, C8, C11, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 27 | El egreso se registra correctamente restando el monto de la caja activa. | En caso de que el administrador necesite retirar dinero de la caja para pagos de insumos o servicios menores. | Cuando el ADMIN ingresa: idEgreso(INT), montoEgreso(DECIMAL), metodoPago(ENUM:Efectivo/SINPE), descripcion(STRING), idUsuario(INT), fechaHora(DATETIME) y idCajaActiva(INT). | El sistema descuenta montoEgreso(DECIMAL) del totalEfectivo(DECIMAL) o totalGeneral(DECIMAL) de la caja, guarda el registro y muestra “Egreso registrado con éxito”. | PENDIENTE DE VERIFICACION |
| 2.0 / 28 | Validación obligatoria de descripción y número de comprobante. | En caso de que el administrador necesite retirar dinero de la caja para pagos de insumos o servicios menores. | Cuando el ADMIN intenta guardar el egreso pero deja los campos descripcion(STRING) o numComprobante(STRING) vacíos. | El sistema bloquea la acción, resalta los campos obligatorios y muestra la alerta “Debe ingresar una descripción y número de respaldo para el gasto”. | PENDIENTE DE VERIFICACION |
| 3.0 / 29 | Bloqueo de egreso en efectivo por fondos insuficientes en caja. | En caso de que el administrador necesite retirar dinero de la caja para pagos de insumos o servicios menores. | Cuando el ADMIN ingresa montoEgreso(DECIMAL) con metodoPago(ENUM:Efectivo) y este valor es superior al saldoActualEfectivo(DECIMAL) en la gaveta. | El sistema impide el retiro, muestra el saldo disponible actual y emite el mensaje “Error: Fondos insuficientes en efectivo para procesar este egreso”. | PENDIENTE DE VERIFICACION |
| 4.0 / 30 | El sistema debe clasificar el egreso según una categoría predefinida. | En caso de que el administrador necesite retirar dinero de la caja para pagos de insumos o servicios menores. | Cuando el ADMIN selecciona categoriaEgreso(ENUM:Proveedores / Servicios / Planilla / Reparaciones / Otros) antes de confirmar. | El sistema asocia el idEgreso(INT) a la categoría seleccionada para su posterior desglose en el reporte de cierres contables. | PENDIENTE DE VERIFICACION |
| 5.0 / 31 | Restricción de montos negativos o iguales a cero. | En caso de que el administrador necesite retirar dinero de la caja para pagos de insumos o servicios menores. | Cuando el ADMIN digita un montoEgreso(DECIMAL) menor o igual a 0. | El sistema inhabilita el botón de confirmación y muestra el mensaje de validación “El monto del egreso debe ser una cifra positiva”. | PENDIENTE DE VERIFICACION |
| 6.0 / 32 | Generación automática de comprobante interno de salida de efectivo. | En caso de que el administrador necesite retirar dinero de la caja para pagos de insumos o servicios menores. | Cuando el sistema procesa el egreso exitosamente y el ADMIN selecciona la opción imprimirVale(BOOL). | El sistema genera un documento valeCaja(FILE) que incluye: idEgreso, monto, concepto, nombreUsuario y espacio para firma de recibido. | PENDIENTE DE VERIFICACION |
| 7.0 / 33 | Validación de permisos jerárquicos para salidas de capital. | En caso de que el administrador necesite retirar dinero de la caja para pagos de insumos o servicios menores. | Cuando un usuario con rolUsuario(ENUM:Mesero/Cajero) intenta acceder a la funcionalidad de Egresos ingresando su idUsuario(INT). | El sistema deniega el acceso al formulario de gastos y muestra el mensaje “Acceso restringido: Función disponible únicamente para el rol Administrador”. | PENDIENTE DE VERIFICACION |
### CNT 005 — Como usuario cajero quiero realizar la apertura de caja

Implementación: `RestauranteVistas/Controllers/` (Contabilidad, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (ContabilidadService, OperacionController). Casos relacionados: C7, C8, C11, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 35 | El sistema permite la apertura exitosa con un monto inicial válido. | En caso de que el cajero inicie su turno y deba habilitar la caja registradora con el fondo de dinero inicial para dar vueltos. | Cuando el CAJERO selecciona la opción "Apertura" e ingresa: idApertura(INT), idCaja(INT), idUsuario(INT), montoInicial(DECIMAL) mayor a 0 y fechaHoraApertura(DATETIME). | El sistema registra el monto, cambia estadoCaja=Abierta, asigna el idUsuario(INT) al turno actual y muestra el mensaje “Caja habilitada correctamente”. | PENDIENTE DE VERIFICACION |
| 2.0 / 36 | El sistema permite iniciar la caja con un monto base de cero. | En caso de que el cajero inicie su turno y deba habilitar la caja registradora con el fondo de dinero inicial para dar vueltos. | Cuando el CAJERO ingresa: montoInicial(DECIMAL) igual a 0 y confirma la acción. | El sistema permite la apertura, registra saldoBase(DECIMAL) en 0, cambia estadoCaja=Abierta y genera una alerta informativa de "Caja iniciada sin efectivo". | PENDIENTE DE VERIFICACION |
| 3.0 / 37 | Bloqueo de apertura si se intenta ingresar un monto negativo. | En caso de que el cajero inicie su turno y deba habilitar la caja registradora con el fondo de dinero inicial para dar vueltos. | Cuando el CAJERO digita un montoInicial(DECIMAL) menor a 0. | El sistema inhabilita el botón de "Confirmar Apertura" y muestra el mensaje de error “El monto inicial no puede ser un valor negativo”. | PENDIENTE DE VERIFICACION |
| 4.0 / 38 | Impedir la apertura si la caja ya se encuentra en estado activa. | En caso de que el cajero inicie su turno y deba habilitar la caja registradora con el fondo de dinero inicial para dar vueltos. | Cuando el CAJERO intenta procesar una apertura pero el sistema detecta que la idCaja(INT) tiene estadoCaja=Abierta. | El sistema bloquea el acceso al formulario de apertura y muestra el mensaje “Error: La caja ya se encuentra abierta por un usuario activo”. | PENDIENTE DE VERIFICACION |
| 5.0 / 39 | Restricción de apertura si existe un cierre pendiente del turno anterior. | En caso de que el cajero inicie su turno y deba habilitar la caja registradora con el fondo de dinero inicial para dar vueltos. | Cuando el CAJERO intenta abrir la caja pero existe un registro previo con estadoCierre=Pendiente. | El sistema impide la apertura y muestra la notificación “Debe realizar el cierre del turno anterior antes de iniciar uno nuevo”. | PENDIENTE DE VERIFICACION |
| 6.0 / 40 | Generación automática del comprobante de apertura de turno. | En caso de que el cajero inicie su turno y deba habilitar la caja registradora con el fondo de dinero inicial para dar vueltos. | Cuando el sistema confirma la apertura exitosa y el CAJERO activa la variable imprimirComprobante(BOOL). | El sistema genera el documento comprobanteApertura(FILE) detallando idApertura, montoInicial, idUsuario y fechaHora, enviándolo a la cola de impresión. | PENDIENTE DE VERIFICACION |
| 7.0 / 41 | Validación de permisos para la gestión de apertura de fondos. | En caso de que el cajero inicie su turno y deba habilitar la caja registradora con el fondo de dinero inicial para dar vueltos. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero) intenta acceder al módulo de apertura ingresando su idUsuario(INT). | El sistema deniega el acceso a la pantalla de configuración de caja y muestra “Acceso denegado: Se requieren permisos de Cajero o Administrador”. | PENDIENTE DE VERIFICACION |
### CNT 006 — Como administrador quiero generar los cierres contables periódicos

Implementación: `RestauranteVistas/Controllers/` (Contabilidad, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (ContabilidadService, OperacionController). Casos relacionados: C7, C8, C11, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 43 | Generación exitosa de cierre semanal basado en cierres diarios. | En caso que el administrador necesite consolidar el rendimiento financiero de la semana para reportes internos. | Cuando el ADMIN selecciona tipoCierre(ENUM:Semanal) e ingresa: fechaInicio(DATE), fechaFin(DATE), idUsuario(INT). | El sistema suma ingresosEfectivo(DECIMAL), ingresosSINPE(DECIMAL) y egresos(DECIMAL) de los cierres diarios del rango, genera idCierrePeriodico(INT) y muestra los totales. | PENDIENTE DE VERIFICACION |
| 2.0 / 44 | Consolidación de datos para el cierre contable mensual. | En caso que el administrador necesite el balance general de ingresos y egresos al finalizar el mes calendario. | Cuando el ADMIN selecciona tipoCierre(ENUM:Mensual) e ingresa: mesCierre(INT), anioCierre(INT) y idUsuario(INT). | El sistema totaliza todos los movimientos del mes, calcula utilidadNeta(DECIMAL), cambia estadoPeriodo=Cerrado y genera reporteMensual(FILE). | PENDIENTE DE VERIFICACION |
| 3.0 / 45 | Procesamiento del cierre anual para fines fiscales. | En caso que el administrador necesite el informe de ingresos totales del año para la declaración de impuestos. | Cuando el ADMIN selecciona tipoCierre(ENUM:Anual) e ingresa: anioCierre(INT) y confirma el procesamiento. | El sistema consolida los 12 cierres mensuales, genera el resumen de ingresosAnuales(DECIMAL) y guarda el histórico en la tabla cierres_anuales. | PENDIENTE DE VERIFICACION |
| 4.0 / 46 | Bloqueo de cierre periódico por falta de cierres diarios. | En caso que el administrador intente cerrar una semana o mes pero falte información de algún día. | Cuando el ADMIN intenta generar un cierre periódico pero el sistema detecta fechas dentro del rango con estadoCaja(ENUM:Pendiente/Abierta). | El sistema bloquea la acción y muestra la alerta “Error: Existen días sin cierre de caja procesado en el rango seleccionado”. | PENDIENTE DE VERIFICACION |
| 5.0 / 47 | Restricción de modificación de periodos ya finalizados. | En caso de que se intente alterar datos contables de un mes que ya fue cerrado oficialmente. | Cuando un usuario intenta editar un ingreso o egreso cuya fecha pertenece a un idCierrePeriodico(INT) con estadoPeriodo=Cerrado. | El sistema bloquea los campos de edición y muestra el mensaje “Periodo contable cerrado: No se permiten modificaciones en registros históricos”. | PENDIENTE DE VERIFICACION |
| 6.0 / 48 | Exportación técnica de los estados financieros del periodo. | En caso que el administrador necesite enviar el respaldo de los cierres al contador externo del restaurante. | Cuando el sistema confirma el cierre y el ADMIN selecciona formatoExportacion(ENUM:PDF/Excel) | El sistema genera el archivo balanceContable(FILE) incluyendo el desglose de totalEfectivo(DECIMAL), totalSINPE(DECIMAL) y detalleEgresos(ARRAY). | PENDIENTE DE VERIFICACION |
| 7.0 / 49 | Validación de jerarquía para el cierre de ciclos contables. | En caso de que un usuario con permisos limitados intente ejecutar un cierre mensual o anual. | Cuando un usuario con rolUsuario(ENUM:Cajero/Mesero) intenta acceder a la configuración de cierres periódicos con su idUsuario(INT). | El sistema deniega el acceso al módulo de consolidación y muestra “Acceso denegado: Función restringida exclusivamente para el Administrador”. | PENDIENTE DE VERIFICACION |
### CNT 007 — Como administrador quiero consultar la bitácora de eventos contables y financieros

Implementación: `RestauranteVistas/Controllers/` (Contabilidad, Cajero), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (ContabilidadService, OperacionController). Casos relacionados: C7, C8, C11, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 51 | Carga exitosa del registro de auditoría contable. | En caso de que el administrador necesite revisar las alteraciones, anulaciones o movimientos financieros realizados en el sistema por control antifraude. | Cuando el ADMIN accede a la vista de "Bitácora Financiera" e inicializa el módulo. | El sistema consulta tabla_bitacora_contable y muestra: idRegistro(INT), idUsuario(INT), accionOperativa(ENUM), idDocumento(INT), montoInvolucrado(DECIMAL) y fechaHora(DATETIME). | PENDIENTE DE VERIFICACION |
| 2.0 / 52 | Filtro de eventos por cajero o usuario responsable. | En caso de que se detecte un descuadre continuo en caja y el administrador necesite auditar los movimientos de un empleado específico. | Cuando el ADMIN selecciona un idUsuarioResponsable(INT) desde el panel de búsqueda de la bitácora. | El sistema filtra los logs y despliega únicamente las acciones financieras (aperturas, cobros, anulaciones, egresos) ejecutadas por ese cajero en particular. | PENDIENTE DE VERIFICACION |
| 3.0 / 53 | Filtro por tipo de acción u operación contable. | En caso de que el administrador requiera auditar cuántas facturas han sido anuladas o cuántos egresos manuales se han registrado. | Cuando el ADMIN selecciona el filtro accionOperativa(ENUM:AnulacionFactura / RegistroEgreso / CierreCaja). | El sistema actualiza la vista mostrando exclusivamente los registros técnicos que coincidan con la operación financiera seleccionada. | PENDIENTE DE VERIFICACION |
| 4.0 / 54 | Búsqueda de eventos contables por rango cronológico. | En caso de que se necesite revisar todos los movimientos anómalos o críticos ocurridos durante un fin de semana específico. | Cuando el ADMIN ingresa fechaInicio(DATE) y fechaFin(DATE) válidas en el buscador. | El sistema ejecuta el SELECT y despliega todos los rastros de auditoría contable ocurridos estrictamente dentro de ese periodo. | PENDIENTE DE VERIFICACION |
| 5.0 / 55 | Inmutabilidad estricta de los registros financieros. | En caso de que un usuario intente alterar el historial para ocultar la anulación de una factura cobrada o un retiro de efectivo. | Cuando cualquier usuario, incluyendo el ADMIN, intenta ejecutar una sentencia UPDATE o DELETE sobre un idRegistro(INT) de la bitácora. | El sistema bloquea la acción a nivel de base de datos, garantizando que el historial financiero sea estrictamente de solo lectura. | PENDIENTE DE VERIFICACION |
| 6.0 / 56 | Exportación de la auditoría para el contador. | En caso de que se requiera enviar el respaldo de movimientos y anulaciones al departamento contable externo para la declaración de impuestos. | Cuando el ADMIN visualiza los registros filtrados y selecciona la opción exportar(BOOL) con formato(ENUM:PDF/Excel). | El sistema procesa los logs mostrados, genera el archivo bitacoraFinanciera(FILE) con los datos técnicos y habilita su descarga segura. | PENDIENTE DE VERIFICACION |
| 7.0 / 57 | Restricción jerárquica de acceso a la auditoría. | En caso de que un cajero intente revisar el historial de auditoría para comprobar si sus anulaciones están siendo monitoreadas. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero/Cajero) intenta acceder a la ruta de la bitácora contable. | El sistema bloquea la renderización de la vista y muestra la alerta “Acceso denegado: El módulo de auditoría financiera es exclusivo para el Administrador”. | PENDIENTE DE VERIFICACION |

## Reportería

### RPT 001 — Como administrador quiero generar un reporte de ventas

Implementación: `RestauranteVistas/Controllers/` (Reporteria), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (Reportes/ReporteService, Reportes/ExportadorReportes). Casos relacionados: C12, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 3 | Generación exitosa de reporte con montos consolidados. | En caso que el administrador necesite analizar el rendimiento financiero del restaurante en un periodo específico. | Cuando el ADMIN selecciona "Reporte de Ventas" e ingresa: idUsuario(INT), fechaInicio(DATE), fechaFin(DATE) y presiona "Generar". | El sistema consulta las facturas con estadoFactura=Pagada, calcula totalVentas(DECIMAL), desgloseMetodos(totalEfectivo, totalSINPE) y muestra la información en pantalla. | PENDIENTE DE VERIFICACION |
| 2.0 / 4 | Notificación de ausencia de registros en el rango seleccionado. | En caso que el administrador necesite analizar el rendimiento financiero del restaurante en un periodo específico. | Cuando el ADMIN ingresa un rango de fechaInicio(DATE) y fechaFin(DATE) donde no existen registros en la tabla Facturas. | El sistema no genera gráficos y muestra el mensaje informativo “No se encontraron ventas procesadas en las fechas indicadas”. | PENDIENTE DE VERIFICACION |
| 3.0 / 5 | Validación de coherencia cronológica en las fechas. | En caso que el administrador necesite analizar el rendimiento financiero del restaurante en un periodo específico. | Cuando el ADMIN intenta generar el reporte ingresando una fechaFin(DATE) que es cronológicamente anterior a la fechaInicio(DATE). | El sistema bloquea el procesamiento y muestra la alerta de validación “Error: La fecha final no puede ser menor a la fecha de inicio”. | PENDIENTE DE VERIFICACION |
| 4.0 / 6 | Exportación del reporte a formato de documento portátil (PDF). | En caso que el administrador necesite analizar el rendimiento financiero del restaurante en un periodo específico. | Cuando el ADMIN visualiza el reporte y selecciona la opción exportar(BOOL) con formato(ENUM:PDF). | El sistema procesa los datos, genera el archivo reporteVentas(FILE) en formato PDF y habilita la descarga inmediata. | PENDIENTE DE VERIFICACION |
| 5.0 / 7 | Exportación del reporte a formato de hoja de cálculo (Excel). | En caso que el administrador necesite analizar el rendimiento financiero del restaurante en un periodo específico. | Cuando el ADMIN visualiza el reporte y selecciona la opción exportar(BOOL) con formato(ENUM:Excel). | El sistema procesa los datos, genera el archivo reporteVentas(FILE) en formato XLSX con el desglose de idFactura, fecha y monto, e inicia la descarga. | PENDIENTE DE VERIFICACION |
| 6.0 / 8 | Bloqueo por omisión de parámetros obligatorios | En caso que el administrador necesite analizar el rendimiento financiero del restaurante en un periodo específico. | Cuando el ADMIN intenta generar el reporte dejando los campos fechaInicio(DATE) o fechaFin(DATE) como NULL o vacíos. | El sistema detiene la ejecución, resalta los campos obligatorios en la interfaz y muestra el mensaje “Debe completar el rango de fechas para continuar”. | PENDIENTE DE VERIFICACION |
| 7.0 / 9 | Restricción de acceso a información financiera sensible. | En caso que un usuario con permisos limitados intente consultar el volumen de ventas del negocio. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero) intenta acceder al módulo RPT 001 ingresando su idUsuario(INT). | El sistema deniega el acceso a la vista de reportes y muestra el mensaje de seguridad “Acceso denegado: Se requieren privilegios de Administrador”. | PENDIENTE DE VERIFICACION |
### RPT 002 — Como administrador quiero generar un reporte de productos más vendidos

Implementación: `RestauranteVistas/Controllers/` (Reporteria), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (Reportes/ReporteService, Reportes/ExportadorReportes). Casos relacionados: C12, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 11 | El reporte se genera correctamente ordenado de mayor a menor rotación. | En caso que el administrador necesite identificar los artículos con mayor éxito de venta para ajustar el stock o el menú. | Cuando el ADMIN selecciona la opción "Top Ventas" e ingresa: idUsuario(INT), fechaInicio(DATE), fechaFin(DATE) y confirma la consulta. | El sistema agrupa los registros, calcula cantidadVendida(INT) y totalGenerado(DECIMAL) por cada idProducto(INT), mostrando la lista en orden descendente. | PENDIENTE DE VERIFICACION |
| 2.0 / 12 | El sistema permite filtrar los productos más vendidos por categoría. | En caso que el administrador necesite identificar los artículos con mayor éxito de venta para ajustar el stock o el menú. | Cuando el ADMIN selecciona categoriaProducto(ENUM:Entradas/Platos/Bebidas/Postres) e ingresa el rango de fechaInicio(DATE) y fechaFin(DATE). | El sistema filtra la tabla de ventas y muestra únicamente el ranking de los productos pertenecientes a la categoría seleccionada. | PENDIENTE DE VERIFICACION |
| 3.0 / 13 | Notificación de ausencia de ventas de productos en el periodo. | En caso que el administrador necesite identificar los artículos con mayor éxito de venta para ajustar el stock o el menú. | Cuando el ADMIN ingresa fechas donde no existen registros asociados a productos en la tabla Pedidos_Items. | El sistema no genera la lista y muestra el mensaje informativo “No se registran movimientos de productos en el rango de fechas seleccionado”. | PENDIENTE DE VERIFICACION |
| 4.0 / 14 | Validación de integridad cronológica en la selección de fechas. | En caso que el administrador necesite identificar los artículos con mayor éxito de venta para ajustar el stock o el menú. | Cuando el ADMIN intenta generar el reporte ingresando una fechaFin(DATE) que es cronológicamente anterior a la fechaInicio(DATE). | El sistema bloquea el procesamiento del reporte y muestra la notificación de error “Rango inválido: La fecha de inicio debe ser menor a la fecha final”. | PENDIENTE DE VERIFICACION |
| 5.0 / 15 | Limitación de la cantidad de registros mostrados en el reporte (Top N). | En caso que el administrador necesite identificar los artículos con mayor éxito de venta para ajustar el stock o el menú. | Cuando el ADMIN selecciona la variable limiteResultados(INT) con valores predefinidos de 5, 10 o 20. | El sistema recorta el listado de resultados y presenta únicamente los N productos con mayor cantidadVendida(INT) en la interfaz. | PENDIENTE DE VERIFICACION |
| 6.0 / 16 | Exportación de estadísticas de productos a archivos externos. | En caso que el administrador necesite identificar los artículos con mayor éxito de venta para ajustar el stock o el menú. | Cuando el ADMIN visualiza los datos consolidados y selecciona la opción exportar(BOOL) con formato(ENUM:PDF/Excel). | El sistema procesa la información, genera el archivo reporteMasVendidos(FILE) con el desglose de cantidades y montos, y habilita su descarga. | PENDIENTE DE VERIFICACION |
| 7.0 / 17 | Restricción de acceso a estadísticas de ventas por rol. | En caso que un usuario con permisos limitados intente consultar el volumen de ventas del negocio. | Cuando un usuario con rolUsuario(ENUM:Cajero/Mesero) intenta acceder a la funcionalidad de RPT 002 ingresando su idUsuario(INT). | El sistema deniega el ingreso al módulo de estadísticas y muestra el mensaje de seguridad “Acceso denegado: Se requiere rol de Administrador”. | PENDIENTE DE VERIFICACION |
### RPT 003 — Como administrador quiero generar un reporte de ingresos por método de pago

Implementación: `RestauranteVistas/Controllers/` (Reporteria), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (Reportes/ReporteService, Reportes/ExportadorReportes). Casos relacionados: C12, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 19 | Generación exitosa del reporte con el desglose de ingresos totales. | En caso que el administrador necesite conciliar el dinero físico y digital para la auditoría de caja. | Cuando el ADMIN selecciona "Reporte por Métodos" e ingresa: idUsuario(INT), fechaInicio(DATE), fechaFin(DATE) y presiona "Consultar". | El sistema suma ingresosEfectivo(DECIMAL) e ingresosSINPE(DECIMAL), genera idReporte(INT) y muestra los totales comparativos en pantalla. | PENDIENTE DE VERIFICACION |
| 2.0 / 20 | El sistema permite filtrar resultados por un método de pago específico. | En caso que el administrador necesite conciliar el dinero físico y digital para la auditoría de caja. | Cuando el ADMIN selecciona metodoPago(ENUM:Efectivo/SINPE) e ingresa el rango de fechaInicio(DATE) y fechaFin(DATE). | El sistema filtra la tabla de transacciones y muestra únicamente el listado de facturas asociadas al método de pago seleccionado. | PENDIENTE DE VERIFICACION |
| 3.0 / 21 | Notificación cuando no existen ingresos en el rango establecido. | En caso que el administrador necesite conciliar el dinero físico y digital para la auditoría de caja. | Cuando el ADMIN ingresa fechas donde la sumatoria de totalVentas(DECIMAL) es igual a 0. | El sistema no despliega gráficos y muestra el mensaje informativo “Sin registros: No se encontraron movimientos para los métodos de pago en estas fechas”. | PENDIENTE DE VERIFICACION |
| 4.0 / 22 | Validación de errores en la cronología de las fechas seleccionadas. | En caso que el administrador necesite conciliar el dinero físico y digital para la auditoría de caja. | Cuando el ADMIN intenta generar el reporte ingresando una fechaFin(DATE) anterior a la fechaInicio(DATE). | El sistema bloquea la ejecución de la consulta y muestra la alerta de validación “Error: El rango de fechas seleccionado es inválido”. | PENDIENTE DE VERIFICACION |
| 5.0 / 23 | Exportación de los datos conciliados para control externo. | En caso que el administrador necesite conciliar el dinero físico y digital para la auditoría de caja. | Cuando el ADMIN visualiza los totales y selecciona la opción exportar(BOOL) con formato(ENUM:PDF/Excel). | El sistema procesa los datos, genera el archivo reporteMetodosPago(FILE) y habilita la descarga del documento con el desglose técnico. | PENDIENTE DE VERIFICACION |
| 6.0 / 24 | Bloqueo por falta de selección de parámetros de búsqueda. | En caso que el administrador necesite conciliar el dinero físico y digital para la auditoría de caja. | Cuando el ADMIN intenta procesar el reporte dejando los campos de fechaInicio(DATE) o fechaFin(DATE) vacíos. | El sistema detiene la acción, marca los campos obligatorios en rojo y muestra el mensaje “Debe completar los criterios de búsqueda”. | PENDIENTE DE VERIFICACION |
| 7.0 / 25 | Validación de permisos para la visualización de datos contables. | En caso que el administrador necesite conciliar el dinero físico y digital para la auditoría de caja. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero) intenta ingresar a la funcionalidad RPT 003 con su idUsuario(INT). | El sistema deniega el acceso al panel de reportes financieros y muestra el mensaje de seguridad “Acceso denegado: Se requiere nivel de Administrador”. | PENDIENTE DE VERIFICACION |
### RPT 004 — Como administrador quiero generar un reporte de desempeño de meseros

Implementación: `RestauranteVistas/Controllers/` (Reporteria), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (Reportes/ReporteService, Reportes/ExportadorReportes). Casos relacionados: C12, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 27 | El sistema genera el resumen de ventas y atención por empleado exitosamente. | En caso que el administrador necesite evaluar la eficiencia y el volumen de ventas generado por el personal de salón para la gestión de incentivos. | Cuando el ADMIN selecciona la opción "Rendimiento" e ingresa: idUsuario(INT), fechaInicio(DATE), fechaFin(DATE) y presiona el botón "Consultar". | El sistema agrupa los pedidos con estadoOrden=Finalizada por idMesero(INT), calcula cantidadMesas(INT), suma totalVentas(DECIMAL) y muestra el ranking en la interfaz. | PENDIENTE DE VERIFICACION |
| 2.0 / 28 | El sistema permite filtrar el rendimiento de un mesero específico. | En caso que el administrador necesite evaluar la eficiencia y el volumen de ventas generado por el personal de salón para la gestión de incentivos. | Cuando el ADMIN ingresa: fechaInicio(DATE), fechaFin(DATE) y selecciona un idMesero(INT) de la lista desplegable de personal activo. | El sistema filtra los registros de la base de datos y despliega únicamente el desglose de facturación y mesas atendidas correspondientes al idMesero(INT) seleccionado. | PENDIENTE DE VERIFICACION |
| 3.0 / 29 | Notificación de ausencia de datos de atención en el periodo. | En caso que el administrador necesite evaluar la eficiencia y el volumen de ventas generado por el personal de salón para la gestión de incentivos. | Cuando el ADMIN ingresa un rango de fechas donde no existen órdenes registradas en la tabla Pedidos para ningún usuario. | El sistema inhabilita la generación de gráficos y muestra el mensaje informativo “No se encontraron registros de servicios para el rango de fechas seleccionado”. | PENDIENTE DE VERIFICACION |
| 4.0 / 30 | Validación de errores en la secuencia cronológica de fechas. | En caso que el administrador necesite evaluar la eficiencia y el volumen de ventas generado por el personal de salón para la gestión de incentivos. | Cuando el ADMIN intenta procesar el reporte ingresando una fechaFin(DATE) cronológicamente menor a la fechaInicio(DATE). | El sistema bloquea el procesamiento de la consulta, resalta los selectores de fecha y muestra la alerta “Error: La fecha final debe ser posterior a la fecha inicial”. | PENDIENTE DE VERIFICACION |
| 5.0 / 31 | El reporte incluye el cálculo del tiempo promedio de servicio. | En caso que el administrador necesite evaluar la eficiencia y el volumen de ventas generado por el personal de salón para la gestión de incentivos. | Cuando el sistema procesa el reporte y calcula la diferencia entre horaApertura(TIME) y horaCierre(TIME) para cada idOrden(INT) finalizada. | El sistema muestra la columna tiempoPromedio(TIME) por mesero, permitiendo identificar la velocidad de rotación de mesas del personal. | PENDIENTE DE VERIFICACION |
| 6.0 / 32 | Exportación de métricas de desempeño a archivos externos. | En caso que el administrador necesite evaluar la eficiencia y el volumen de ventas generado por el personal de salón para la gestión de incentivos. | Cuando el ADMIN visualiza los resultados consolidados y selecciona la opción exportar(BOOL) con formatoExportacion(ENUM:PDF/Excel). | El sistema procesa los KPIs de ventas y tiempos, genera el archivo reporteMeseros(FILE) y habilita la descarga inmediata del documento técnico. | PENDIENTE DE VERIFICACION |
| 7.0 / 33 | Restricción de acceso a la evaluación de personal por rol. | En caso que el administrador necesite evaluar la eficiencia y el volumen de ventas generado por el personal de salón para la gestión de incentivos. | Cuando un usuario con rolUsuario(ENUM:Mesero/Cocinero/Cajero) intenta acceder a la funcionalidad RPT 004 ingresando su idUsuario(INT). | El sistema bloquea la carga del módulo de métricas y muestra el mensaje de seguridad “Acceso denegado: Se requieren privilegios de Administrador para ver el desempeño”. | PENDIENTE DE VERIFICACION |
### RPT 005 — Como administrador quiero generar un reporte de inventario

Implementación: `RestauranteVistas/Controllers/` (Reporteria), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (Reportes/ReporteService, Reportes/ExportadorReportes). Casos relacionados: C12, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 35 | El sistema genera la lista completa de insumos con sus saldos actuales. | En caso que el administrador necesite supervisar los niveles de existencias de los ingredientes y productos para garantizar la operación continua. | Cuando el ADMIN selecciona "Inventario General" e ingresa: idUsuario(INT), fechaConsulta(DATE) y presiona "Ver Reporte". | El sistema consulta la tabla Insumos, extrae idInsumo(INT), nombreInsumo(STRING), cantidadActual(DECIMAL), stockMinimo(DECIMAL) y muestra el listado total. | PENDIENTE DE VERIFICACION |
| 2.0 / 36 | El sistema permite filtrar únicamente los productos con stock bajo. | En caso que el administrador necesite supervisar los niveles de existencias de los ingredientes y productos para garantizar la operación continua. | Cuando el ADMIN selecciona el filtro estadoStock(ENUM:Bajo) donde cantidadActual(DECIMAL) sea menor o igual a stockMinimo(DECIMAL). | El sistema filtra la base de datos y despliega solo los insumos que requieren reabastecimiento urgente, resaltándolos en color rojo. | PENDIENTE DE VERIFICACION |
| 3.0 / 37 | El sistema permite filtrar el inventario por categoría de producto. | En caso que el administrador necesite supervisar los niveles de existencias de los ingredientes y productos para garantizar la operación continua. | Cuando el ADMIN selecciona categoriaInsumo(ENUM:Vegetales/Carnes/Lácteos/Bebidas/Otros) y presiona "Filtrar". | El sistema actualiza la vista del reporte mostrando únicamente los artículos asociados al idCategoria(INT) seleccionado. | PENDIENTE DE VERIFICACION |
| 4.0 / 38 | Búsqueda por coincidencia de nombre de insumo. | En caso que el administrador necesite supervisar los niveles de existencias de los ingredientes y productos para garantizar la operación continua. | Cuando el ADMIN ingresa un terminoBusqueda(STRING) en el campo de texto del reporte de inventario. | El sistema realiza una búsqueda parcial en la columna nombreInsumo(STRING) y muestra los resultados que coinciden con el texto ingresado. | PENDIENTE DE VERIFICACION |
| 5.0 / 39 | Notificación de reporte vacío si no existen insumos registrados | En caso que el administrador necesite supervisar los niveles de existencias de los ingredientes y productos para garantizar la operación continua. | Cuando el ADMIN intenta generar el reporte pero el conteo total de la tabla Insumos es igual a 0. | El sistema no despliega la tabla de datos y muestra el mensaje de advertencia “No se registran insumos en el inventario actual”. | PENDIENTE DE VERIFICACION |
| 6.0 / 40 | Exportación de los niveles de stock a formatos externos. | En caso que el administrador necesite supervisar los niveles de existencias de los ingredientes y productos para garantizar la operación continua. | Cuando el ADMIN visualiza los datos y selecciona la opción exportar(BOOL) con formato(ENUM:PDF/Excel). | El sistema procesa la lista actual, genera el archivo reporteInventario(FILE) con los encabezados técnicos y permite su descarga. | PENDIENTE DE VERIFICACION |
| 7.0 / 41 | Restricción de acceso a la gestión de inventarios por rol. | En caso que el administrador necesite supervisar los niveles de existencias de los ingredientes y productos para garantizar la operación continua. | Cuando un usuario con rolUsuario(ENUM:Mesero/Cajero) intenta abrir el módulo RPT 005 ingresando su idUsuario(INT). | El sistema deniega el acceso a la información de existencias y muestra el mensaje “Acceso denegado: Se requiere rol de Administrador”. | PENDIENTE DE VERIFICACION |
### RPT 006 — Como administrador quiero generar un reporte de egresos

Implementación: `RestauranteVistas/Controllers/` (Reporteria), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (Reportes/ReporteService, Reportes/ExportadorReportes). Casos relacionados: C12, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 43 | Generación exitosa de reporte con el total de gastos acumulados. | En caso que el administrador necesite analizar las salidas de dinero y gastos operativos del restaurante en un periodo definido. | Cuando el ADMIN selecciona "Reporte de Gastos" e ingresa: idUsuario(INT), fechaInicio(DATE), fechaFin(DATE) y presiona "Generar". | El sistema consulta la tabla Egresos, suma totalEgresos(DECIMAL), agrupa por categoriaEgreso(ENUM) y muestra el resumen financiero en pantalla. | PENDIENTE DE VERIFICACION |
| 2.0 / 44 | El sistema permite filtrar los gastos por una categoría específica. | En caso que el administrador necesite analizar las salidas de dinero y gastos operativos del restaurante en un periodo definido. | Cuando el ADMIN selecciona categoriaEgreso(ENUM:Proveedores/Servicios/Planilla/Otros) e ingresa el rango de fechas. | El sistema filtra los registros y despliega únicamente el listado de gastos asociados a la categoría seleccionada con su montoEgreso(DECIMAL). | PENDIENTE DE VERIFICACION |
| 3.0 / 45 | Notificación de ausencia de egresos en el rango de fechas | En caso que el administrador necesite analizar las salidas de dinero y gastos operativos del restaurante en un periodo definido. | Cuando el ADMIN ingresa un rango de fechas donde no existen registros cargados en la tabla Egresos. | El sistema inhabilita la generación de gráficos y muestra el mensaje informativo “No se registran egresos de dinero en el periodo consultado”. | PENDIENTE DE VERIFICACION |
| 4.0 / 46 | Validación de integridad en el rango cronológico de búsqueda. | En caso que el administrador necesite analizar las salidas de dinero y gastos operativos del restaurante en un periodo definido. | Cuando el ADMIN intenta procesar el reporte ingresando una fechaFin(DATE) menor a la fechaInicio(DATE). | El sistema bloquea el procesamiento de la consulta y muestra la alerta técnica “Error: La fecha final debe ser posterior a la fecha inicial”. | PENDIENTE DE VERIFICACION |
| 5.0 / 47 | El sistema permite filtrar egresos por método de pago (Efectivo/SINPE). | En caso que el administrador necesite analizar las salidas de dinero y gastos operativos del restaurante en un periodo definido. | Cuando el ADMIN selecciona metodoPago(ENUM:Efectivo/SINPE) para identificar el origen de los fondos. | El sistema actualiza la vista y muestra el totalEgresos(DECIMAL) pagado exclusivamente mediante el método de pago seleccionado. | PENDIENTE DE VERIFICACION |
| 6.0 / 48 | Exportación de los datos de egresos a formatos de auditoría. | En caso que el administrador necesite analizar las salidas de dinero y gastos operativos del restaurante en un periodo definido. | Cuando el ADMIN visualiza el reporte consolidado y selecciona la opción exportar(BOOL) con formato(ENUM:PDF/Excel). | El sistema procesa los datos, genera el archivo reporteEgresos(FILE) con el desglose de idEgreso, descripción y monto, habilitando la descarga. | PENDIENTE DE VERIFICACION |
| 7.0 / 49 | Restricción de acceso a información de gastos por jerarquía. | En caso que el administrador necesite analizar las salidas de dinero y gastos operativos del restaurante en un periodo definido. | Cuando un usuario con rolUsuario(ENUM:Cajero/Mesero/Cocinero) intenta acceder a la funcionalidad RPT 006 con su idUsuario(INT). | El sistema deniega el acceso a la interfaz de gastos y muestra el mensaje de seguridad “Acceso restringido: Se requiere rol de Administrador”. | PENDIENTE DE VERIFICACION |
### RPT 007 — Como administrador quiero realizar el Cierre de caja de un turno para consolidar los ingresos registrados durante la jornada.

Implementación: `RestauranteVistas/Controllers/` (Reporteria), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (Reportes/ReporteService, Reportes/ExportadorReportes). Casos relacionados: C12, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 51 | El cierre de caja se realiza correctamente cuando los datos del turno son válidos. | En caso que el administrador necesite consolidar las transacciones registradas durante el turno activo de caja. | Cuando el ADMIN selecciona idCaja(INT), idTurno(INT) y fechaCierre(DATETIME) para ejecutar el proceso de cierre. | El sistema calcula totalIngresos(DECIMAL), totalTransacciones(INT), actualiza estadoTurno= Cerrado y muestra “Cierre de caja realizado correctamente”. | PENDIENTE DE VERIFICACION |
| 2.0 / 52 | El sistema valida que exista un turno activo antes de realizar el cierre. | En caso que el administrador intente cerrar una caja sin que exista un turno operativo activo. | Cuando el ADMIN intenta ejecutar cierreCaja utilizando idCaja(INT) pero estadoTurno(ENUM)=Cerrado o NULL. | El sistema bloquea la operación y muestra “No existe un turno activo para realizar el cierre de caja”. | PENDIENTE DE VERIFICACION |
| 3.0 / 53 | El sistema calcula automáticamente los ingresos registrados durante el turno. | En caso que el administrador confirme el cierre del turno para obtener el resumen financiero del periodo. | Cuando el sistema procesa idTurno(INT) y consulta montoEfectivo(DECIMAL), montoSINPE(DECIMAL)  | El sistema calcula totalIngresos(DECIMAL), genera el resumen del turno y actualiza el balance contable del día. | PENDIENTE DE VERIFICACION |
| 4.0 / 54 | El sistema impide realizar cierres duplicados sobre un mismo turno. | En caso que el administrador intente cerrar nuevamente un turno que ya fue finalizado anteriormente. | Cuando el ADMIN intenta ejecutar cierreCaja usando idTurno(INT) que posee estadoTurno(ENUM)=Cerrado. | El sistema bloquea la operación y muestra “El turno seleccionado ya fue cerrado previamente”. | PENDIENTE DE VERIFICACION |
| 5.0 / 55 | El sistema registra auditoría del proceso de cierre de caja. | En caso que se requiera mantener trazabilidad de quién ejecutó el cierre financiero del turno. | Cuando el sistema registra idCierre(INT), idCaja(INT), idTurno(INT), idUsuarioResponsable(INT) y fechaHoraCierre(DATETIME). | El sistema guarda el registro en la tabla bitacoraCierres permitiendo su consulta en el módulo de auditoría. | PENDIENTE DE VERIFICACION |
| 6.0 / 56 | El sistema restringe el proceso de cierre a usuarios autorizados. | En caso que un usuario sin permisos administrativos intente ejecutar el cierre financiero de una caja. | Cuando un usuario con rolUsuario(ENUM:Cajero/Mesero) intenta ejecutar cierreCaja ingresando idCaja(INT) y idTurno(INT). | El sistema bloquea la acción y muestra “Acceso denegado: solo el Administrador puede realizar cierres de caja”. | PENDIENTE DE VERIFICACION |
### RPT 008 — Como administrador quiero consultar la bitácora de generación de reportes

Implementación: `RestauranteVistas/Controllers/` (Reporteria), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (Reportes/ReporteService, Reportes/ExportadorReportes). Casos relacionados: C12, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 58 | Carga exitosa del registro de auditoría de consultas. | En caso de que el administrador necesite monitorear quién ha estado consultando o extrayendo información sensible y financiera del restaurante. | Cuando el ADMIN accede a la vista de "Bitácora de Reportes" e inicializa el módulo. | El sistema consulta tabla_bitacora_reportes y muestra: idRegistro(INT), idUsuarioConsulta(INT), tipoReporte(ENUM), parametrosBusqueda(STRING) y fechaHoraConsulta(DATETIME). | PENDIENTE DE VERIFICACION |
| 2.0 / 59 | Filtro de eventos por usuario consultante. | En caso de que se sospeche de fuga de información y se necesite revisar cuántas veces un empleado específico ha generado reportes. | Cuando el ADMIN selecciona un idUsuarioConsulta(INT) desde el panel de búsqueda de la bitácora. | El sistema filtra los logs y despliega únicamente el historial de los reportes generados o descargados por ese usuario en particular. | PENDIENTE DE VERIFICACION |
| 3.0 / 60 | Filtro por tipo de reporte generado. | En caso de que el administrador requiera auditar exclusivamente quién ha estado consultando las ganancias (Ventas y Egresos). | Cuando el ADMIN selecciona el filtro tipoReporte(ENUM:Ventas / Egresos / Inventario / Meseros). | El sistema actualiza la vista mostrando exclusivamente los registros técnicos que coincidan con la categoría del reporte seleccionado. | PENDIENTE DE VERIFICACION |
| 4.0 / 61 | Búsqueda de consultas por rango cronológico. | En caso de que se necesite revisar la actividad de extracción de datos ocurrida durante el último mes o semana. | Cuando el ADMIN ingresa fechaInicio(DATE) y fechaFin(DATE) válidas en el buscador. | El sistema ejecuta el SELECT y despliega todos los rastros de auditoría de consultas ocurridas estrictamente dentro de ese periodo. | PENDIENTE DE VERIFICACION |
| 5.0 / 62 | Inmutabilidad de los registros de consulta. | En caso de que un usuario intente borrar la evidencia de que descargó el reporte de ventas totales del mes. | Cuando cualquier usuario, incluyendo el ADMIN, intenta ejecutar una sentencia UPDATE o DELETE sobre un idRegistro(INT) de esta bitácora. | El sistema bloquea la acción a nivel de base de datos, garantizando que el historial de visualizaciones sea estrictamente de solo lectura. | PENDIENTE DE VERIFICACION |
| 6.0 / 63 | Registro de exportaciones (Descargas de archivos). | En caso de que se necesite diferenciar entre alguien que solo "vio" el reporte en pantalla vs. alguien que lo guardó en su computadora. | Cuando el sistema lee el log y detecta que la variable formatoExportacion(ENUM) es diferente a NULL (ej. PDF o Excel). | El sistema resalta ese registro en la tabla con un icono de "Archivo Descargado" para alertar al administrador de la extracción física de datos. | PENDIENTE DE VERIFICACION |
| 7.0 / 64 | Restricción jerárquica de acceso a la auditoría de reportes. | En caso de que un empleado intente revisar si el sistema está registrando sus consultas. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero/Cajero) intenta acceder a la ruta de la bitácora de reportería. | El sistema bloquea la renderización de la vista y muestra la alerta “Acceso denegado: El control de auditoría de información es exclusivo para el Administrador”. | PENDIENTE DE VERIFICACION |

## Punto De Venta

### PDV 001 — Como usuario CAJERO quiero abrir una venta rápida para registrar el consumo inmediato de un cliente sin necesidad de crear una orden previa.

Implementación: `RestauranteVistas/Controllers/` (PuntoVenta, OperacionPdv, BitacoraPdv), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService). Casos relacionados: C7, C8, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 3 | Se crea venta rápida correctamente | En caso que el cliente realice una compra directa en el mostrador. | Cuando el CAJERO registra idVenta(INT), tipoVenta(ENUM=RAPIDA), fechaHora(DATETIME), idUsuario(INT), estadoVenta(ENUM=ABIERTA) | El sistema deberá crear la venta y habilitar la pantalla de registro de productos. | PENDIENTE DE VERIFICACION |
| 2.0 / 4 | La venta queda asociada al cajero | En caso que varios cajeros operen en el mismo punto de venta. | Cuando el sistema registra idUsuario(INT), nombreUsuario(STRING), idCaja(INT) | El sistema deberá bloquear la apertura e informar que ya existe una sesión de caja activa. | PENDIENTE DE VERIFICACION |
| 3.0 / 5 | La venta inicia sin productos | En caso que el cliente aún no haya seleccionado artículos. | Cuando se crea idVenta(INT) sin detalleVenta(LIST) | El sistema deberá permitir agregar productos posteriormente. | PENDIENTE DE VERIFICACION |
| 4.0 / 6 | La venta puede cancelarse antes del cobro | En caso que el cliente decida no comprar. | Cuando estadoVenta(ENUM=ABIERTA) y el CAJERO cancela idVenta(INT) | El sistema deberá eliminar la venta sin afectar registros. | PENDIENTE DE VERIFICACION |
| 5.0 / 7 | No se permite cobrar una venta vacía | En caso que el cajero intente cobrar sin productos. | Cuando detalleVenta(LIST) vacío | El sistema deberá bloquear el cobro. | PENDIENTE DE VERIFICACION |
| 6.0 / 8 | La venta queda visible en pantalla | En caso que el cajero continúe agregando productos. | Cuando idVenta(INT) activa | El sistema deberá mostrar el resumen de la venta en curso. | PENDIENTE DE VERIFICACION |
| 7.0 / 9 | Solo cajeros crean ventas rápidas | En caso que otro rol intente iniciar una venta. | Cuando rolUsuario(ENUM)  distinto a CAJERO | El sistema deberá restringir acceso. | PENDIENTE DE VERIFICACION |
### PDV 002 — Como usuario CAJERO quiero dividir una cuenta en varias partes para permitir que varios clientes paguen por separado si es necesario.

Implementación: `RestauranteVistas/Controllers/` (PuntoVenta, OperacionPdv, BitacoraPdv), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService). Casos relacionados: C7, C8, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 11 | La cuenta puede dividirse | En caso que en una mesa hayan  cobros por separado | Cuando el CAJERO selecciona dividirCuenta(idVenta INT) | El sistema deberá generar subcuentas independientes. | PENDIENTE DE VERIFICACION |
| 2.0 / 12 | Se asignan productos a cada subcuenta | En caso que cada cliente pague artículos específicos. | Cuando el CAJERO mueve idProducto(INT), cantidad(INT), idSubCuenta(INT) | El sistema deberá actualizar cada subcuenta. | PENDIENTE DE VERIFICACION |
| 3.0 / 13 | Se recalcula el total por subcuenta | En caso que se distribuyan productos. | Cuando subtotalSubCuenta(DECIMAL) | El sistema deberá recalcular el monto individual. | PENDIENTE DE VERIFICACION |
| 4.0 / 14 | Las subcuentas pueden pagarse por separado | En caso que cada cliente pague individualmente. | Cuando idSubCuenta(INT) se envía a cobro | El sistema deberá permitir pago independiente. | PENDIENTE DE VERIFICACION |
| 5.0 / 15 | La cuenta principal se cierra al pagar todas las subcuentas | En caso que todas las subcuentas se liquiden. | Cuando subCuentasPagadas=TRUE | El sistema deberá cerrar la venta principal. | PENDIENTE DE VERIFICACION |
| 6.0 / 16 | No se permite dividir una cuenta ya pagada | En caso que la venta esté cerrada. | Cuando estadoVenta=Pagada | El sistema deberá bloquear la acción. | PENDIENTE DE VERIFICACION |
| 7.0 / 17 | La división queda registrada | En caso que se requiera trazabilidad. | Cuando idVenta(INT), idSubCuenta(INT), fechaOperacion(DATETIME) | El sistema deberá registrar la operación. | PENDIENTE DE VERIFICACION |
### PDV 003 — Como usuario CAJERO quiero validar y confirmar correctamente los productos de una venta para reducir errores antes de enviar el ticket o la cuenta final.

Implementación: `RestauranteVistas/Controllers/` (PuntoVenta, OperacionPdv, BitacoraPdv), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService). Casos relacionados: C7, C8, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 19 | El sistema valida correctamente los datos del detalle de venta antes de confirmar | En caso que el cajero registre una venta en el punto de venta y necesite confirmar que los productos, cantidades y precios sean correctos antes de generar el cobro al cliente. | Cuando el CAJERO revisa y confirma: idVenta(INT), idDetalleVenta(INT), idProducto(INT), nombreProducto(STRING), cantidad(INT>0), precioUnitario(DECIMAL>0), subtotalItem(DECIMAL), fechaHoraRevision(DATETIME) | El sistema deberá validar el detalle completo de la venta, comprobar que no existan inconsistencias en cantidades o precios, y mostrar la venta como lista para cobro. | PENDIENTE DE VERIFICACION |
| 2.0 / 20 | No se permite confirmar una venta con productos sin cantidad válida | En caso que el cajero registre una venta en el punto de venta y necesite confirmar que los productos, cantidades y precios sean correctos antes de generar el cobro al cliente. | Cuando el sistema detecta cantidad(INT) <= 0 en uno o más productos del idVenta(INT) | El sistema deberá bloquear la confirmación de la venta y mostrar un mensaje indicando que existen cantidades inválidas en el detalle. | PENDIENTE DE VERIFICACION |
| 3.0 / 21 | No se permite confirmar una venta con precios faltantes o inválidos | En caso que el cajero registre una venta en el punto de venta y necesite confirmar que los productos, cantidades y precios sean correctos antes de generar el cobro al cliente. | Cuando el sistema detecta precioUnitario(DECIMAL) <= 0, subtotalItem(DECIMAL) vacío o totalVenta(DECIMAL) inconsistente | El sistema deberá impedir la confirmación de la venta y alertar que existen errores en el precio de uno o más productos. | PENDIENTE DE VERIFICACION |
| 4.0 / 22 | El sistema debe advertir si hay productos duplicados en la venta | En caso que el cajero registre una venta en el punto de venta y necesite confirmar que los productos, cantidades y precios sean correctos antes de generar el cobro al cliente. | Cuando el sistema detecta el mismo idProducto(INT) repetido en varias líneas de detalle sin consolidación | El sistema deberá mostrar una advertencia al cajero para revisar si corresponde unificar o corregir los productos repetidos. | PENDIENTE DE VERIFICACION |
| 5.0 / 23 | El sistema debe permitir corregir productos antes de confirmar | En caso que durante la revisión el cajero detecte un error de digitación en el ticket o en la venta. | Cuando el CAJERO edita: idDetalleVenta(INT), cantidad(INT), precioUnitario(DECIMAL), observacionItem(STRING), usuarioResponsable(STRING), fechaHoraEdicion(DATETIME) | El sistema deberá actualizar el detalle de la venta, recalcular automáticamente los subtotales y el total general, y dejar trazabilidad de la corrección realizada. | PENDIENTE DE VERIFICACION |
| 6.0 / 24 | La venta confirmada debe quedar marcada como validada | En caso que todos los datos de la venta hayan sido verificados correctamente y el cajero esté listo para continuar con el proceso de cobro. | Cuando el CAJERO confirma: idVenta(INT), estadoVenta(ENUM:EnRevision/Validada), idUsuario(INT), fechaHoraConfirmacion(DATETIME) | El sistema deberá cambiar el estado de la venta a Validada y dejarla lista para su cobro sin errores operativos. | PENDIENTE DE VERIFICACION |
| 7.0 / 25 | Solo roles autorizados pueden validar ventas | En caso que un usuario distinto al cajero o administrador intente confirmar una venta desde el punto de venta. | Cuando rolUsuario(ENUM) distinto a CAJERO o ADMIN intenta confirmar: idVenta(INT), idUsuario(INT) | El sistema deberá denegar la acción, impedir la validación de la venta y registrar el intento de acceso no autorizado. | PENDIENTE DE VERIFICACION |
### PDV 004 — Como usuario CAJERO quiero enviar rápidamente la información del pedido o venta al área correspondiente para agilizar la atención del cliente y reducir tiempos de espera.

Implementación: `RestauranteVistas/Controllers/` (PuntoVenta, OperacionPdv, BitacoraPdv), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService). Casos relacionados: C7, C8, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 27 | La información se envía correctamente al destino correspondiente | En caso que el cajero haya terminado de registrar la venta o pedido y necesite que la información llegue de forma inmediata al área operativa correspondiente. | Cuando el CAJERO envía: idVenta(INT), idOrden(INT opcional), idDestino(ENUM:Cocina/Bar/Caja), fechaHoraEnvio(DATETIME), prioridad(ENUM:Normal/Urgente), observacionesGenerales(STRING opcional) | El sistema deberá transmitir la información al destino correspondiente de forma inmediata, registrar el envío y mostrar confirmación al usuario. | PENDIENTE DE VERIFICACION |
| 2.0 / 28 | El envío debe incluir el detalle completo del pedido o venta | En caso que el cajero haya terminado de registrar la venta o pedido y necesite que la información llegue de forma inmediata al área operativa correspondiente. | Cuando el sistema envía: detalleVenta(LIST), idProducto(INT), nombreProducto(STRING), cantidad(INT), observacionItem(STRING), subtotalItem(DECIMAL) | El sistema deberá incluir todo el detalle de productos en el envío, evitando pérdida de información entre el punto de venta y el área operativa. | PENDIENTE DE VERIFICACION |
| 3.0 / 29 | No se permite enviar una venta vacía o incompleta | En caso que el cajero intente acelerar la operación sin haber completado toda la información necesaria del ticket. | Cuando idVenta(INT) no tenga detalleVenta(LIST), totalVenta(DECIMAL) o estadoVenta(ENUM) válido | El sistema deberá bloquear el envío y mostrar un mensaje indicando que la venta está incompleta. | PENDIENTE DE VERIFICACION |
| 4.0 / 30 | El sistema debe registrar la hora exacta de envío | En caso que el restaurante necesite medir la rapidez operativa desde el punto de venta hasta la atención interna. | Cuando el sistema registra fechaHoraEnvio(DATETIME), idUsuario(INT), idDestino(ENUM) | El sistema deberá almacenar la hora exacta del envío para control de tiempos operativos. | PENDIENTE DE VERIFICACION |
| 5.0 / 31 | El sistema debe indicar si el envío fue exitoso o fallido | En caso que la operación dependa de una comunicación inmediata y sea necesario conocer si la información llegó correctamente. | Cuando el sistema procesa estadoEnvio(ENUM:Enviado/Fallido/Pendiente), codigoConfirmacion(STRING opcional), mensajeSistema(STRING) | El sistema deberá informar al cajero si el envío fue exitoso o si se requiere reenviar la información. | PENDIENTE DE VERIFICACION |
| 6.0 / 32 | El sistema debe permitir reenvío cuando exista un fallo | En caso que ocurra un error de comunicación y la información no llegue al área correspondiente. | Cuando el CAJERO selecciona reenviar: idVenta(INT), idOrden(INT), motivoReenvio(STRING), fechaHoraReenvio(DATETIME) | El sistema deberá reenviar la información, actualizar el estado del envío y dejar trazabilidad del reintento. | PENDIENTE DE VERIFICACION |
| 7.0 / 33 | Solo roles autorizados pueden realizar envíos operativos | En caso que un usuario sin permisos intente enviar información desde el punto de venta a otras áreas del restaurante. | Cuando rolUsuario(ENUM) distinto a CAJERO o ADMIN intenta enviar: idVenta(INT), idDestino(ENUM) | El sistema deberá bloquear la operación y registrar el intento no autorizado. | PENDIENTE DE VERIFICACION |
### PDV 005 — Como usuario CAJERO quiero visualizar el estado de las ventas y cobros en tiempo real para atender con mayor rapidez y tomar decisiones inmediatas durante la operación.

Implementación: `RestauranteVistas/Controllers/` (PuntoVenta, OperacionPdv, BitacoraPdv), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService). Casos relacionados: C7, C8, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 35 | El sistema muestra ventas activas en tiempo real | En caso que el cajero necesite conocer qué ventas están abiertas, pendientes o pagadas mientras atiende a varios clientes de forma simultánea. | Cuando el CAJERO consulta panelOperativo con: fechaHoraConsulta(DATETIME), idCaja(INT), filtroEstado(ENUM:Abierta/Pendiente/Pagada/Anulada) | El sistema deberá mostrar en tiempo real el listado actualizado de ventas según su estado operativo. | PENDIENTE DE VERIFICACION |
| 2.0 / 36 | El sistema muestra el total acumulado del turno actual | En caso que el cajero necesite visualizar rápidamente cuánto dinero se ha movido en su turno sin ingresar al módulo de reportería. | Cuando el sistema consolida: idSesionCaja(INT), totalVentasTurno(DECIMAL), totalPagosTurno(DECIMAL), totalPendientes(DECIMAL), fechaHoraActualizacion(DATETIME) | El sistema deberá mostrar el acumulado actual del turno en pantalla de operación, sin generar un reporte formal. | PENDIENTE DE VERIFICACION |
| 3.0 / 37 | El sistema muestra ventas pendientes de cobro | En caso que el cajero necesite identificar de inmediato cuáles clientes o tickets aún no han sido pagados. | Cuando el sistema filtra: estadoVenta=PendientePago, idCaja(INT), idUsuario(INT) | El sistema deberá mostrar claramente las ventas pendientes para dar prioridad a su atención. | PENDIENTE DE VERIFICACION |
| 4.0 / 38 | El sistema actualiza automáticamente los cambios sin recargar manualmente | En caso que la operación del punto de venta requiera agilidad y no se deba perder tiempo refrescando pantallas manualmente. | Cuando el sistema detecta cambios en: estadoVenta(ENUM), idPago(INT), idSesionCaja(INT), fechaHoraEvento(DATETIME) | El sistema deberá refrescar automáticamente la información visible en el panel operativo del punto de venta. | PENDIENTE DE VERIFICACION |
| 5.0 / 39 | El sistema debe mostrar alertas visuales para ventas demoradas | En caso que una venta permanezca abierta o pendiente más tiempo del esperado y se requiera atención inmediata del cajero. | Cuando tiempoVentaAbierta(MINUTOS) supera umbralConfigurado(INT), idVenta(INT), estadoVenta(ENUM) | El sistema deberá resaltar visualmente la venta y mostrar una alerta para agilizar su atención. | PENDIENTE DE VERIFICACION |
| 6.0 / 40 | El sistema debe permitir buscar ventas activas rápidamente | En caso que el cajero necesite localizar una venta específica mientras atiende varios clientes. | Cuando el CAJERO busca por: idVenta(INT), numeroOrden(INT), numeroMesa(INT), nombreCliente(STRING opcional), fecha(DATE) | El sistema deberá devolver de inmediato la coincidencia y mostrar la venta buscada en pantalla. | PENDIENTE DE VERIFICACION |
| 7.0 / 41 | Solo roles autorizados pueden acceder al panel operativo en tiempo real | En caso que un usuario sin permisos intente visualizar la información operativa del punto de venta. | Cuando rolUsuario(ENUM) distinto a CAJERO o ADMIN intenta acceder a panelOperativo | El sistema deberá denegar el acceso y bloquear la visualización de la información en tiempo real. | PENDIENTE DE VERIFICACION |
### PDV 006 — Como usuario CAJERO quiero visualizar indicadores operativos inmediatos del punto de venta para reducir errores, atender más rápido y mejorar el control durante el turno.

Implementación: `RestauranteVistas/Controllers/` (PuntoVenta, OperacionPdv, BitacoraPdv), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService). Casos relacionados: C7, C8, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 43 | El sistema muestra productos mas vendidos del turno actual | En caso que el cajero ocupe identificar rapidamente que productos se estan moviendo más durante la operacion actual para agilizar la atención y confirmar cobros frecuentes | Cuando el sistema consolida: idSesionCaja(INT), idProducto(INT), nombreProducto(STRING), cantidadVendida(INT), fechaHoraActualizacion(DATETIME) | El sistema deberá mostrar en tiempo real los productos más vendidos del turno actual dentro del panel operativo del punto de venta. | PENDIENTE DE VERIFICACION |
| 2.0 / 44 | El sistema muestra el número de ventas realizadas en el turno | En caso que el cajero necesite conocer cuántas transacciones ha procesado durante su jornada para controlar el flujo de atención. | Cuando el sistema calcula: idSesionCaja(INT), cantidadVentas(INT), fechaHoraActualizacion(DATETIME) | El sistema deberá mostrar la cantidad de ventas registradas en el turno actual de manera actualizada. | PENDIENTE DE VERIFICACION |
| 3.0 / 45 | El sistema muestra el tiempo promedio de atención por venta | En caso que el restaurante busque agilizar la atención al cliente y el cajero necesite ver si el proceso de cobro está siendo eficiente. | Cuando el sistema calcula: idSesionCaja(INT), tiempoPromedioAtencion(DECIMAL), totalVentas(INT), fechaHoraActualizacion(DATETIME) | El sistema deberá mostrar el tiempo promedio de atención por venta dentro del panel de operación. | PENDIENTE DE VERIFICACION |
| 4.0 / 46 | El sistema alerta sobre ventas corregidas o editadas con frecuencia | En caso que se requiera reducir errores repetitivos durante el registro y cobro en el punto de venta. | Cuando el sistema detecta: idVenta(INT), cantidadEdiciones(INT), fechaHoraUltimaEdicion(DATETIME), usuarioResponsable(STRING) | El sistema deberá mostrar una alerta operativa sobre ventas con múltiples correcciones para prevenir errores recurrentes. | PENDIENTE DE VERIFICACION |
| 5.0 / 47 | El sistema muestra el ingreso acumulado del turno actual | En caso que el cajero necesite tener visibilidad rápida del dinero ya cobrado durante su sesión sin generar reportes formales. | Cuando el sistema consolida: idSesionCaja(INT), totalCobrado(DECIMAL), totalPendiente(DECIMAL), fechaHoraActualizacion(DATETIME) | El sistema deberá mostrar el monto cobrado acumulado del turno y el saldo pendiente aún en proceso. | PENDIENTE DE VERIFICACION |
| 6.0 / 48 | El sistema actualiza los indicadores sin salir del módulo de caja | En caso que el cajero deba mantener la operación continua sin cambiar a otras pantallas o módulos. | Cuando el sistema detecta nuevos eventos de venta, cobro, edición o anulación en: idSesionCaja(INT), fechaHoraEvento(DATETIME) | El sistema deberá refrescar automáticamente los indicadores operativos visibles dentro del punto de venta. | PENDIENTE DE VERIFICACION |
| 7.0 / 49 | Solo roles autorizados pueden visualizar indicadores operativos | En caso que se requiera proteger la información operativa del punto de venta durante el turno. | Cuando rolUsuario(ENUM) distinto a CAJERO o ADMIN intenta visualizar indicadores operativos | El sistema deberá restringir el acceso a estos indicadores y mantener la seguridad operativa del módulo. | PENDIENTE DE VERIFICACION |
### PDV 007 — Como administrador quiero consultar la bitácora de eventos del punto de venta

Implementación: `RestauranteVistas/Controllers/` (PuntoVenta, OperacionPdv, BitacoraPdv), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` (OperacionController, InventarioService). Casos relacionados: C7, C8, C14.

| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |
|---|---|---|---|---|---|
| 1.0 / 51 | Carga exitosa del registro de auditoría de caja. | En caso de que el administrador necesite revisar las acciones operativas, aperturas o anulaciones realizadas en las terminales de cobro por seguridad. | Cuando el ADMIN accede a la vista de "Bitácora PDV" e inicializa el módulo. | El sistema consulta tabla_bitacora_pdv y muestra: idRegistro(INT), idCaja(INT), idUsuarioCajero(INT), accionOperativa(ENUM), idVenta(INT) y fechaHora(DATETIME). | PENDIENTE DE VERIFICACION |
| 2.0 / 52 | Filtro de eventos por cajero responsable. | En caso de que se detecten movimientos inusuales durante un turno y se deba investigar a un cajero específico. | Cuando el ADMIN selecciona un idUsuarioCajero(INT) desde el panel de búsqueda de la bitácora. | El sistema filtra los logs y despliega únicamente las acciones de apertura, cobro, o cancelación de ventas ejecutadas por ese cajero en particular. | PENDIENTE DE VERIFICACION |
| 3.0 / 53 | Filtro por tipo de acción operativa. | En caso de que el administrador requiera auditar cuántas ventas rápidas fueron canceladas o cuántos descuentos manuales se aplicaron en mostrador. | Cuando el ADMIN selecciona el filtro accionOperativa(ENUM:AperturaCaja / CancelacionVenta / CobroRapido / EdicionVenta). | El sistema actualiza la vista mostrando exclusivamente los registros técnicos que coincidan con la operación de caja seleccionada. | PENDIENTE DE VERIFICACION |
| 4.0 / 54 | Búsqueda de eventos por terminal específica. | En caso de que el restaurante tenga múltiples puntos de cobro y se necesite auditar los eventos ocurridos solo en la "Caja Principal". | Cuando el ADMIN selecciona un idCaja(INT) específico en los filtros de búsqueda. | El sistema ejecuta el SELECT y despliega todas las alteraciones y transacciones registradas únicamente en esa terminal física. | PENDIENTE DE VERIFICACION |
| 5.0 / 55 | Búsqueda de eventos por rango cronológico. | En caso de que se necesite revisar la actividad operativa del punto de venta ocurrida durante un turno, día o semana específica. | Cuando el ADMIN ingresa fechaInicio(DATE) y fechaFin(DATE) válidas en el buscador. | El sistema filtra la base de datos y despliega todos los rastros de auditoría ocurridos estrictamente dentro de ese periodo de tiempo. | PENDIENTE DE VERIFICACION |
| 6.0 / 56 | Inmutabilidad de los registros transaccionales. | En caso de que un cajero intente alterar el historial de acciones para ocultar una venta anulada de forma indebida. | Cuando cualquier usuario, incluyendo el ADMIN, intenta ejecutar una sentencia UPDATE o DELETE sobre un idRegistro(INT) de esta bitácora. | El sistema bloquea la acción a nivel de base de datos, garantizando que el historial de auditoría de ventas sea estrictamente de solo lectura. | PENDIENTE DE VERIFICACION |
| 7.0 / 57 | Restricción jerárquica de acceso a la auditoría. | En caso de que un empleado intente revisar si el sistema está registrando las cancelaciones de ventas rápidas que realiza. | Cuando un usuario con rolUsuario(ENUM:Cocinero/Mesero/Cajero) intenta acceder a la ruta de la bitácora del punto de venta. | El sistema bloquea la renderización de la vista y muestra la alerta “Acceso denegado: El control de auditoría operativa es exclusivo para el Administrador”. | PENDIENTE DE VERIFICACION |

## Requerimientos No Funcionales

### RQNF-001

El tiempo máximo de respuesta para registrar una venta en el módulo de punto de venta no debe superar los 2 segundos bajo condiciones normales de operación.

Solución documentada: Implementar optimización en consultas a la base de datos mediante índices en campos críticos como productos, ventas y pedidos. Además, utilizar consultas optimizadas y limitar la carga innecesaria de datos para garantizar tiempos de respuesta rápidos en las transacciones del punto de venta.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-002

El acceso al sistema debe realizarse únicamente mediante autenticación con credenciales válidas previamente registradas.

Solución documentada: Integrar un sistema de autenticación basado en usuarios y contraseñas almacenadas en la base de datos. El proceso de inicio de sesión debe validar las credenciales ingresadas comparándolas con los registros almacenados antes de permitir el acceso al sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-003

Los usuarios del sistema deben poder operar las funciones principales del sistema después de un breve periodo de capacitación.

Solución documentada: Diseñar una interfaz de usuario clara con menús organizados, botones identificables y procesos guiados para las operaciones principales. También se puede incluir una pequeña guía de uso o capacitación inicial para los usuarios del sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-004

El sistema debe mantenerse disponible durante el horario completo de operación del restaurante para permitir el registro continuo de transacciones.

Solución documentada: Implementar el sistema en un servidor estable que permanezca activo durante el horario de operación del restaurante. El acceso al sistema debe realizarse a través de una aplicación web o local que permita mantener el servicio disponible mientras el servidor esté activo.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-005

La información almacenada en la base de datos debe mantenerse consistente durante las operaciones de inserción, actualización y eliminación de registros.

Solución documentada: Aplicar restricciones de integridad en la base de datos mediante claves primarias, claves foráneas y reglas de validación que eviten inconsistencias entre tablas relacionadas.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-006

La aplicación debe ejecutarse correctamente en los sistemas operativos utilizados en los equipos del restaurante.

Solución documentada: Configurar el sistema para que sea compatible con los sistemas operativos utilizados por la empresa, asegurando que el software y las dependencias necesarias puedan ejecutarse correctamente en dichos equipos.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-007

El sistema debe minimizar la probabilidad de fallos durante la ejecución de operaciones críticas como ventas o registros de pedidos.

Solución documentada: Diseñar el sistema utilizando una arquitectura estable que gestione adecuadamente errores y excepciones, evitando que fallos menores interrumpan la operación completa del sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-008

Las contraseñas de los usuarios deben almacenarse utilizando mecanismos de cifrado que protejan la información sensible.

Solución documentada: Implementar algoritmos de cifrado para almacenar las contraseñas en la base de datos, de modo que la información no pueda ser interpretada directamente en caso de acceso no autorizado a la base de datos.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-009

Las consultas de información en módulos como inventario, pedidos y reportes deben ejecutarse en un tiempo inferior a 3 segundos.

Solución documentada: Optimizar las consultas utilizadas en los módulos de inventario y reportes mediante técnicas de filtrado eficiente y uso adecuado de índices en la base de datos.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-010

Todas las acciones relevantes realizadas por los usuarios deben registrarse en una bitácora de actividad del sistema.

Solución documentada: Crear una tabla de registro de actividad o bitácora donde se almacenen eventos importantes del sistema, incluyendo operaciones realizadas por los usuarios dentro de cada módulo.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-011

La interfaz del sistema debe permitir identificar claramente las funciones principales de cada módulo para facilitar su utilización.

Solución documentada: Aplicar principios de diseño de experiencia de usuario que permitan identificar claramente las funciones disponibles en cada módulo, utilizando etiquetas, iconos y estructuras visuales consistentes.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-012

Solo los usuarios con permisos autorizados podrán acceder a los módulos administrativos del sistema.

Solución documentada: Implementar un sistema de roles y permisos que determine qué funcionalidades puede utilizar cada tipo de usuario dentro del sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-013

El sistema debe permitir aumentar su capacidad para soportar un mayor número de usuarios sin afectar su funcionamiento.

Solución documentada: Diseñar la arquitectura del sistema de forma modular para permitir ampliar la capacidad del sistema agregando recursos o usuarios adicionales cuando sea necesario.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-014

El sistema debe garantizar continuidad operativa durante el horario laboral del negocio.

Solución documentada: Utilizar infraestructura tecnológica confiable que garantice el funcionamiento continuo del sistema durante las horas de operación del negocio.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-015

La información ingresada por los usuarios debe ser validada antes de almacenarse para evitar inconsistencias en la base de datos.

Solución documentada: Implementar validaciones en formularios y controles de entrada que revisen los datos antes de enviarlos a la base de datos para evitar registros inválidos o incompletos.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-016

El sistema debe mantener estabilidad operativa incluso cuando múltiples usuarios acceden simultáneamente.

Solución documentada: Diseñar el sistema con manejo de sesiones y control de concurrencia para permitir el uso simultáneo de múltiples usuarios sin conflictos en las operaciones.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-017

Durante periodos de alta demanda el sistema debe mantener tiempos de respuesta estables en las operaciones principales.

Solución documentada: Implementar control de carga en el sistema para asegurar que las operaciones críticas mantengan tiempos de respuesta adecuados incluso en periodos de mayor uso.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-018

El sistema debe funcionar correctamente en navegadores web modernos utilizados por la organización.

Solución documentada: Utilizar tecnologías web compatibles con navegadores modernos y realizar pruebas de compatibilidad para asegurar una correcta visualización del sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-019

Las transacciones realizadas dentro del sistema deben quedar registradas para permitir su posterior análisis o verificación.

Solución documentada: Registrar todas las transacciones relevantes en la base de datos, permitiendo posteriormente revisar el historial de operaciones realizadas dentro del sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-020

Los intentos fallidos de acceso al sistema deben registrarse para permitir auditorías de seguridad.

Solución documentada: Configurar el sistema para que registre automáticamente los intentos fallidos de inicio de sesión en una tabla de seguridad o auditoría.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-021

La navegación entre los distintos módulos del sistema debe ser clara y sencilla para facilitar el trabajo del personal.

Solución documentada: Diseñar la navegación del sistema mediante menús organizados y accesos directos que faciliten el desplazamiento entre módulos.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-022

El sistema debe permitir realizar actualizaciones o mejoras sin afectar significativamente la operación del sistema.

Solución documentada: Implementar una arquitectura de software modular que permita realizar actualizaciones o mejoras sin afectar el funcionamiento general del sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-023

El sistema debe permitir recuperar rápidamente el servicio en caso de interrupciones menores.

Solución documentada: Establecer mecanismos de recuperación que permitan reiniciar rápidamente el sistema en caso de interrupciones menores del servicio.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-024

Las operaciones que involucren modificaciones de datos deben garantizar la consistencia de la información almacenada.

Solución documentada: Utilizar transacciones de base de datos que aseguren que las modificaciones de información se realicen completamente o no se realicen en caso de errores.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-025

El sistema debe proteger la información confidencial relacionada con usuarios, ventas y transacciones.

Solución documentada: Implementar controles de acceso y protección de datos que limiten el acceso a la información confidencial del sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-026

La arquitectura del sistema debe permitir la incorporación de nuevos módulos o funcionalidades en el futuro.

Solución documentada: Diseñar el sistema con una estructura flexible que permita integrar nuevas funcionalidades sin modificar significativamente los módulos existentes.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-027

El sistema debe reducir al mínimo la pérdida de información ante fallos inesperados del sistema.

Solución documentada: Utilizar mecanismos de manejo de errores y recuperación automática que minimicen la pérdida de información durante fallos del sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-028

Los registros de actividad deben incluir información como usuario, fecha, hora y tipo de operación realizada.

Solución documentada: Diseñar la bitácora del sistema para almacenar información detallada de las actividades realizadas por los usuarios.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-029

El sistema debe operar utilizando tecnologías compatibles con la infraestructura tecnológica definida para el proyecto.

Solución documentada: Utilizar tecnologías compatibles con la infraestructura tecnológica definida para el proyecto, garantizando estabilidad y compatibilidad.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-030

El registro de un pedido estándar de cliente no debe tomar más de un minuto para un usuario capacitado.

Solución documentada: Optimizar el proceso de registro de pedidos mediante formularios simplificados y selección rápida de productos.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-031

El sistema debe soportar múltiples transacciones simultáneas sin afectar la velocidad de respuesta.

Solución documentada: Diseñar el sistema para gestionar múltiples transacciones simultáneas mediante control de sesiones y manejo adecuado de recursos.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-032

El sistema debe evitar modificaciones no autorizadas en la información almacenada.

Solución documentada: Implementar controles de seguridad que restrinjan las modificaciones de datos únicamente a usuarios autorizados.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-033

La estructura del sistema debe facilitar el mantenimiento y evolución del software a lo largo del tiempo.

Solución documentada: Diseñar el código del sistema siguiendo buenas prácticas de desarrollo que faciliten su mantenimiento y actualización.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-034

Solo usuarios autorizados podrán realizar modificaciones sobre información crítica del sistema.

Solución documentada: Implementar controles de autorización que verifiquen los permisos del usuario antes de permitir modificaciones en información crítica.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

### RQNF-035

Deben realizarse copias de seguridad periódicas de la base de datos para garantizar la recuperación de la información ante fallos.

Solución documentada: Implementar procesos automáticos de respaldo de la base de datos que permitan recuperar la información en caso de fallos del sistema.

Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.

