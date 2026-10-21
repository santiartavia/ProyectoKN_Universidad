# Defectos D-01 a D-09

Los cierres se limitan al código y esquema comprobados en la base QA aislada. La aplicación de la migración a otra base y la aceptación de negocio requieren su propia verificación.

| Defecto | Causa atendida | Implementación | Evidencia |
|---|---|---|---|
| D-01 | Acceso con credenciales de primer ingreso predecibles; hashes malformados y fallback reversible | `PasswordHelper`, `LoginController`, `AutorizacionFilter`, `EmpleadoService` | Pruebas unitarias de contraseña, credenciales correctas/incorrectas de administrador, sesión revocada y acceso por roles |
| D-02 | Selección de cualquier empleado y asignación guardada como texto | `Mesa.IdEmpleadoAsignado`, `MesasController`, `PedidosController`, vista Mesas | Asignación persistente, selección de meseros activos y recorrido C5 |
| D-03 | Guardados parciales y empleado elegido o creado automáticamente | `OperacionController`, `PedidosController`, `Solicitudes_Operacion` | Cabecera, detalle, mesa, inventario y auditoría; reenvío sin duplicación; reversión por falta de existencias |
| D-04 | Estados sin validar; ruta alternativa sin fecha de entrega y acción de auditoría incompatible | `CocinaController`, `MeseroController`, `PedidosController` | Preparación, listo y entrega persistidos; bitácora; recorrido C6 |
| D-05 | Flujo y detalle de cobro incompletos; falta de detalle facturado para reportes | `CajeroController`, vista Cajero | Productos, cantidades, precio unitario, subtotal y total CRC; detalle guardado en la factura |
| D-06 | Caja inventada, empleado incorrecto y cobros sin una unidad transaccional | `CajeroController`, `PuntoVentaController`, `OperacionController`, `ContabilidadService` | Rechazo de monto insuficiente, vuelto, apertura asociada y prevención de doble cobro |
| D-07 | Falta de integración y de garantías en la división de cuentas | `CajeroController`, vista Cajero, `PuntoVentaController` | Dos subcuentas, asignación de cantidades, pagos individuales, suma exacta y cierre de la orden |
| D-08 | Falta de integridad de la salida y truncado silencioso de existencias | `InventarioService`, operaciones Pedidos/PDV | Vaso de Leche: 2 × 0.25 L = 0.50 L; falta de stock revierte la operación; reenvío no rebaja dos veces |
| D-09 | Agrupación/proyección de empleados y errores de consulta poco claros | `BitacoraPedidosController`, `BitacoraPdvController`, consultas de Usuarios | Consulta filtrada y pruebas reales de acceso a las bitácoras en navegador |

Estado: correcciones implementadas y comprobadas mediante los ensayos específicos del ejecutor. Consulte `04-resultados-pruebas.md` para el resultado más reciente, los pasos no aprobados y las limitaciones. No se considera aceptado un caso completo por aprobar únicamente su ensayo de integración.

Durante la nueva ejecución aparecieron dos fallas adicionales: el enlace de decimales HTML con punto bajo cultura es-CR y los campos `type="number"` renderizados con coma decimal. Se corrigieron con `DecimalModelBinder` y valores invariantes en pedidos, cobro, horas extra y nómina. C10 y la reprueba de C14 verifican sus efectos reales; la validación de modelos impide ejecutar operaciones con valores mal formados.
