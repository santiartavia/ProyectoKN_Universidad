-- 02 - DATOS PEQUEÑOS Y COHERENTES PARA DEMOSTRACIÓN
-- Ejecutar DESPUÉS de 01_esquema_azure.sql, dentro de la misma base de Azure.
-- Datos ficticios, con nombres y montos verosímiles; moneda CRC y fechas Costa Rica (UTC-6).
-- Usuarios: demo.admin / demo.mesero / demo.cocina / demo.caja
-- Contraseña de demostración para los cuatro: ColibriDemo2026!
-- Hashes PBKDF2 SHA256, 600000 iteraciones, salt individual; no cambian cuentas existentes.
-- Repetible: no duplica estos ejemplos ni vuelve a descontar sus insumos.
-- No ejecutar como datos reales de producción: incluye operaciones contables de ejemplo.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 51100, 'Seleccione la base de la aplicación.', 1;
IF OBJECT_ID(N'dbo.PasswordHistorial', N'U') IS NULL OR OBJECT_ID(N'dbo.Envios_PDV', N'U') IS NULL
    THROW 51101, 'Primero ejecute 01_esquema_azure.sql.', 1;
BEGIN TRY
BEGIN TRANSACTION;
DECLARE @ahora DATETIME2(0)=DATEADD(HOUR,-6,SYSUTCDATETIME());
DECLARE @ayer DATE=DATEADD(DAY,-1,CAST(@ahora AS DATE));
DECLARE @inicio DATETIME2(0)=DATEADD(HOUR,10,CAST(@ayer AS DATETIME2));
DECLARE @ventaHora DATETIME2(0)=DATEADD(HOUR,3,@inicio);
DECLARE @fin DATETIME2(0)=DATEADD(HOUR,8,@inicio);

INSERT dbo.Roles(nombre_rol,descripcion,estado)
SELECT r.rol,r.descripcion,1 FROM (VALUES
 (N'Administrador',N'Administración del restaurante'),(N'Mesero',N'Atención de mesas y pedidos'),
 (N'Cocinero',N'Preparación de pedidos'),(N'Cajero',N'Cobros y caja')) r(rol,descripcion)
WHERE NOT EXISTS(SELECT 1 FROM dbo.Roles t WHERE t.nombre_rol=r.rol);

DECLARE @personas TABLE(usuario NVARCHAR(80),rol NVARCHAR(60),cedula NVARCHAR(20),nombre NVARCHAR(80),apellidos NVARCHAR(120),telefono NVARCHAR(20),salario DECIMAL(10,2),hash NVARCHAR(255));
INSERT @personas VALUES
 (N'demo.admin',N'Administrador',N'109990101',N'María Fernanda',N'Vargas Solís',N'8888-0101',2500,N'pbkdf2-sha256$600000$FdxxN0DZPbq/YOI4mi9T7633Auoc/+PBLpEUcqxfiyN3jmR85koQoqSv4chVw+Uh'),
 (N'demo.mesero',N'Mesero',N'209990102',N'Daniel',N'Rojas Méndez',N'8888-0102',1800,N'pbkdf2-sha256$600000$9fKAVbQNKPCBsC/X95We3Th64gnU3EF/NLWTpbZWmmDkmEvhP2T6JSDx0jp87p1o'),
 (N'demo.cocina',N'Cocinero',N'309990103',N'Laura',N'Castro Mora',N'8888-0103',2200,N'pbkdf2-sha256$600000$zwxmZCwuqRvh7emWlcy51eI1ArZfJ7AmQ4/ySJPywDVjyWvr7/5sxFyM5a90aQo4'),
 (N'demo.caja',N'Cajero',N'409990104',N'Andrés',N'Jiménez Soto',N'8888-0104',1900,N'pbkdf2-sha256$600000$qFQvk/EGpAHS0uNeMkowCRKke1pUpkKynu+xcbPQFV2bAU9x0Z22zx4nSavycA+B');
IF EXISTS(SELECT 1 FROM @personas p JOIN dbo.Usuarios u ON u.nombre_usuario=p.usuario JOIN dbo.Roles r ON r.id_rol=u.id_rol WHERE r.nombre_rol<>p.rol)
    THROW 51102, 'Un usuario demo ya existe con otro rol. No se modificó su cuenta.', 1;
INSERT dbo.Usuarios(id_rol,nombre_usuario,correo,password_hash,cambio_password_requerido,intentos_fallidos,bloqueado,estado,fecha_creacion,fecha_password,direccion)
SELECT r.id_rol,p.usuario,p.usuario+N'@colibri.example',p.hash,0,0,0,1,@ahora,@ahora,N'San José, Montes de Oca'
FROM @personas p JOIN dbo.Roles r ON r.nombre_rol=p.rol
WHERE NOT EXISTS(SELECT 1 FROM dbo.Usuarios u WHERE u.nombre_usuario=p.usuario);
INSERT dbo.Empleados(id_usuario,cedula,nombre,apellidos,telefono,correo_personal,direccion,salario_hora,dias_vacaciones_disponibles,fecha_ingreso,estado)
SELECT u.id_usuario,p.cedula,p.nombre,p.apellidos,p.telefono,p.usuario+N'@colibri.example',N'San José, Montes de Oca',p.salario,12,DATEADD(MONTH,-8,CAST(@ahora AS DATE)),1
FROM @personas p JOIN dbo.Usuarios u ON u.nombre_usuario=p.usuario
WHERE NOT EXISTS(SELECT 1 FROM dbo.Empleados e WHERE e.id_usuario=u.id_usuario);
DECLARE @admin INT=(SELECT id_usuario FROM dbo.Usuarios WHERE nombre_usuario=N'demo.admin');
DECLARE @mesero INT=(SELECT TOP(1) e.id_empleado FROM dbo.Empleados e JOIN dbo.Usuarios u ON u.id_usuario=e.id_usuario WHERE u.nombre_usuario=N'demo.mesero' ORDER BY e.id_empleado);
DECLARE @cajero INT=(SELECT TOP(1) e.id_empleado FROM dbo.Empleados e JOIN dbo.Usuarios u ON u.id_usuario=e.id_usuario WHERE u.nombre_usuario=N'demo.caja' ORDER BY e.id_empleado);

INSERT dbo.Mesas(numero_mesa,capacidad,estado_mesa,estado)
SELECT m.numero,m.capacidad,N'disponible',1 FROM (VALUES(N'AZ-01',4),(N'AZ-02',4),(N'AZ-03',2),(N'AZ-04',6))m(numero,capacidad)
WHERE NOT EXISTS(SELECT 1 FROM dbo.Mesas t WHERE t.numero_mesa=m.numero);
IF NOT EXISTS(SELECT 1 FROM dbo.Cajas WHERE nombre_caja=N'Caja demo Azure')
    INSERT dbo.Cajas(nombre_caja,estado_caja,estado) VALUES(N'Caja demo Azure',N'cerrada',1);
DECLARE @caja INT=(SELECT TOP(1) id_caja FROM dbo.Cajas WHERE nombre_caja=N'Caja demo Azure' ORDER BY id_caja);

INSERT dbo.Categorias_Insumo(nombre_categoria,estado)
SELECT n,1 FROM (VALUES(N'Carnes'),(N'Granos'),(N'Bebidas'),(N'Lácteos'))c(n)
WHERE NOT EXISTS(SELECT 1 FROM dbo.Categorias_Insumo t WHERE t.nombre_categoria=c.n);
INSERT dbo.Categorias_Producto(nombre_categoria,estado)
SELECT n,1 FROM (VALUES(N'Platos fuertes'),(N'Bebidas'))c(n)
WHERE NOT EXISTS(SELECT 1 FROM dbo.Categorias_Producto t WHERE t.nombre_categoria=c.n);
IF NOT EXISTS(SELECT 1 FROM dbo.Proveedores WHERE cedula_juridica=N'3-101-999901')
    INSERT dbo.Proveedores(cedula_juridica,nombre_empresa,contacto_nombre,telefono,correo,estado)
    VALUES(N'3-101-999901',N'Distribuidora Valle Central (demo)',N'Elena Campos',N'2222-0199',N'pedidos@vallecentral.example',1);
DECLARE @proveedor INT=(SELECT id_proveedor FROM dbo.Proveedores WHERE cedula_juridica=N'3-101-999901');
DECLARE @insumos TABLE(nombre NVARCHAR(120),categoria NVARCHAR(100),unidad NVARCHAR(30),minimo DECIMAL(10,2),stock DECIMAL(10,2),costo DECIMAL(10,2));
INSERT @insumos VALUES
 (N'Pechuga de pollo (demo)',N'Carnes',N'kg',2,10,3200),
 (N'Arroz blanco (demo)',N'Granos',N'kg',3,20,950),
 (N'Frijoles negros (demo)',N'Granos',N'kg',2,15,1200),
 (N'Café molido (demo)',N'Bebidas',N'kg',0.50,1,6500),
 (N'Leche entera (demo)',N'Lácteos',N'litro',2,8,900),
 (N'Limón (demo)',N'Bebidas',N'kg',0.50,0.50,1800),
 (N'Azúcar (demo)',N'Granos',N'kg',1,5,850);
INSERT dbo.Insumos(id_categoria,nombre_insumo,unidad_medida,stock_minimo,stock_actual,costo_unitario,estado)
SELECT c.id_categoria,i.nombre,i.unidad,i.minimo,i.stock,i.costo,1 FROM @insumos i JOIN dbo.Categorias_Insumo c ON c.nombre_categoria=i.categoria
WHERE NOT EXISTS(SELECT 1 FROM dbo.Insumos t WHERE t.nombre_insumo=i.nombre);
INSERT dbo.Insumos_Proveedores(id_insumo,id_proveedor,fecha_asoc)
SELECT i.id_insumo,@proveedor,@ahora FROM dbo.Insumos i JOIN @insumos s ON s.nombre=i.nombre_insumo
WHERE NOT EXISTS(SELECT 1 FROM dbo.Insumos_Proveedores t WHERE t.id_insumo=i.id_insumo AND t.id_proveedor=@proveedor);

DECLARE @productos TABLE(nombre NVARCHAR(120),categoria NVARCHAR(100),descripcion NVARCHAR(500),precio DECIMAL(10,2));
INSERT @productos VALUES
 (N'Casado de pollo (demo)',N'Platos fuertes',N'Pollo a la plancha con arroz y frijoles.',4850),
 (N'Cappuccino (demo)',N'Bebidas',N'Café espresso con leche espumada.',1950),
 (N'Limonada natural (demo)',N'Bebidas',N'Limón fresco con agua y azúcar.',1200);
INSERT dbo.Productos(id_categoria_prod,nombre_producto,descripcion,precio_venta,disponible,estado)
SELECT c.id_categoria_prod,p.nombre,p.descripcion,p.precio,1,1 FROM @productos p JOIN dbo.Categorias_Producto c ON c.nombre_categoria=p.categoria
WHERE NOT EXISTS(SELECT 1 FROM dbo.Productos t WHERE t.nombre_producto=p.nombre);
INSERT dbo.Receta(id_producto,estado)
SELECT p.id_producto,1 FROM dbo.Productos p JOIN @productos s ON s.nombre=p.nombre_producto
WHERE NOT EXISTS(SELECT 1 FROM dbo.Receta r WHERE r.id_producto=p.id_producto);
DECLARE @recetas TABLE(producto NVARCHAR(120),insumo NVARCHAR(120),cantidad DECIMAL(10,4));
INSERT @recetas VALUES
 (N'Casado de pollo (demo)',N'Pechuga de pollo (demo)',0.20),
 (N'Casado de pollo (demo)',N'Arroz blanco (demo)',0.15),
 (N'Casado de pollo (demo)',N'Frijoles negros (demo)',0.15),
 (N'Cappuccino (demo)',N'Café molido (demo)',0.02),
 (N'Cappuccino (demo)',N'Leche entera (demo)',0.15),
 (N'Limonada natural (demo)',N'Limón (demo)',0.10),
 (N'Limonada natural (demo)',N'Azúcar (demo)',0.02);
INSERT dbo.RecetaInsumo(id_receta,id_insumo,cantidad_usar)
SELECT r.id_receta,i.id_insumo,s.cantidad FROM @recetas s JOIN dbo.Productos p ON p.nombre_producto=s.producto JOIN dbo.Receta r ON r.id_producto=p.id_producto JOIN dbo.Insumos i ON i.nombre_insumo=s.insumo
WHERE NOT EXISTS(SELECT 1 FROM dbo.RecetaInsumo t WHERE t.id_receta=r.id_receta AND t.id_insumo=i.id_insumo);

-- Solo se descuentan los pedidos realmente insertados en ESTA ejecución.
DECLARE @nuevos TABLE(id_pedido INT PRIMARY KEY);
DECLARE @mesaPagada INT=(SELECT id_mesa FROM dbo.Mesas WHERE numero_mesa=N'AZ-01');
DECLARE @mesaAbierta INT=(SELECT id_mesa FROM dbo.Mesas WHERE numero_mesa=N'AZ-02');
DECLARE @pedido INT;
IF NOT EXISTS(SELECT 1 FROM dbo.Pedidos WHERE observaciones=N'AZ-DEMO-PAGADO-01')
BEGIN
    INSERT dbo.Pedidos(id_mesa,id_empleado,tipo_servicio,cantidad_comensales,estado_pedido,fecha_hora,observaciones,estado,fecha_hora_entrega,fecha_hora_finalizacion)
    VALUES(@mesaPagada,@mesero,N'mesa',2,N'finalizado',DATEADD(MINUTE,-30,@ventaHora),N'AZ-DEMO-PAGADO-01',1,DATEADD(MINUTE,-5,@ventaHora),@ventaHora);
    SET @pedido=SCOPE_IDENTITY(); INSERT @nuevos VALUES(@pedido);
    INSERT dbo.Detalle_Pedido(id_pedido,id_producto,cantidad,precio_unitario,estado_item,estado)
    SELECT @pedido,p.id_producto,2,p.precio_venta,N'entregado',1 FROM dbo.Productos p WHERE p.nombre_producto IN(N'Casado de pollo (demo)',N'Cappuccino (demo)');
END;
DECLARE @pagado INT=(SELECT TOP(1) id_pedido FROM dbo.Pedidos WHERE observaciones=N'AZ-DEMO-PAGADO-01' ORDER BY id_pedido);
IF NOT EXISTS(SELECT 1 FROM dbo.Apertura_Caja WHERE observaciones=N'AZ-DEMO-APERTURA-01')
    INSERT dbo.Apertura_Caja(id_caja,id_cajero,monto_inicial,fecha_apertura,observaciones,estado)
    VALUES(@caja,@cajero,20000,@inicio,N'AZ-DEMO-APERTURA-01',0);
DECLARE @apertura INT=(SELECT TOP(1) id_apertura FROM dbo.Apertura_Caja WHERE observaciones=N'AZ-DEMO-APERTURA-01' ORDER BY id_apertura);
DECLARE @total DECIMAL(12,2)=(SELECT SUM(cantidad*precio_unitario) FROM dbo.Detalle_Pedido WHERE id_pedido=@pagado AND estado=1);
IF NOT EXISTS(SELECT 1 FROM dbo.Ventas WHERE id_pedido=@pagado)
BEGIN
    INSERT dbo.Ventas(id_pedido,id_empleado,id_apertura,tipo_venta,total_cobrado,monto_recibido,vuelto,metodo_pago,estado_venta,fecha_hora,estado)
    VALUES(@pagado,@cajero,@apertura,N'normal',@total,@total+400,400,N'efectivo',N'completada',@ventaHora,1);
    DECLARE @venta INT=SCOPE_IDENTITY();
    INSERT dbo.Detalle_Venta(id_venta,id_producto,cantidad,precio_unitario,subtotal_item,observaciones_item,estado)
    SELECT @venta,id_producto,cantidad,precio_unitario,cantidad*precio_unitario,N'Pedido de demostración Azure',1 FROM dbo.Detalle_Pedido WHERE id_pedido=@pagado AND estado=1;
END;
IF NOT EXISTS(SELECT 1 FROM dbo.Cierres_Caja WHERE id_apertura=@apertura)
    INSERT dbo.Cierres_Caja(id_cajero,id_apertura,fecha_cierre,monto_apertura,total_efectivo,total_sinpe,total_tarjeta,total_egresos,saldo_esperado,saldo_real,descuadre,estado)
    VALUES(@cajero,@apertura,@fin,20000,@total,0,0,0,20000+@total,20000+@total,0,1);

IF NOT EXISTS(SELECT 1 FROM dbo.Pedidos WHERE observaciones=N'AZ-DEMO-ABIERTO-01')
BEGIN
    IF EXISTS(SELECT 1 FROM dbo.Pedidos WHERE id_mesa=@mesaAbierta AND estado=1 AND estado_pedido NOT IN(N'cancelado',N'finalizado'))
        THROW 51103, 'AZ-02 ya tiene un pedido activo. No se creó una segunda orden.', 1;
    IF EXISTS(SELECT 1 FROM dbo.Mesas WHERE id_mesa=@mesaAbierta AND estado_mesa<>N'disponible')
        THROW 51104, 'AZ-02 no está disponible. No se modificó su estado.', 1;
    INSERT dbo.Pedidos(id_mesa,id_empleado,tipo_servicio,cantidad_comensales,estado_pedido,fecha_hora,observaciones,estado)
    VALUES(@mesaAbierta,@mesero,N'mesa',1,N'abierto',@ahora,N'AZ-DEMO-ABIERTO-01',1);
    SET @pedido=SCOPE_IDENTITY(); INSERT @nuevos VALUES(@pedido);
    INSERT dbo.Detalle_Pedido(id_pedido,id_producto,cantidad,precio_unitario,estado_item,estado)
    SELECT @pedido,p.id_producto,1,p.precio_venta,N'pendiente',1 FROM dbo.Productos p WHERE p.nombre_producto IN(N'Casado de pollo (demo)',N'Limonada natural (demo)');
    UPDATE dbo.Mesas SET estado_mesa=N'ocupada' WHERE id_mesa=@mesaAbierta;
END;
DECLARE @consumos TABLE(id_insumo INT PRIMARY KEY,cantidad DECIMAL(10,4));
INSERT @consumos
SELECT ri.id_insumo,SUM(d.cantidad*ri.cantidad_usar) FROM @nuevos n JOIN dbo.Detalle_Pedido d ON d.id_pedido=n.id_pedido JOIN dbo.Receta r ON r.id_producto=d.id_producto AND r.estado=1 JOIN dbo.RecetaInsumo ri ON ri.id_receta=r.id_receta GROUP BY ri.id_insumo;
IF EXISTS(SELECT 1 FROM @consumos c JOIN dbo.Insumos i ON i.id_insumo=c.id_insumo WHERE i.stock_actual<c.cantidad)
    THROW 51105, 'Stock insuficiente para los ejemplos; se revierte toda la carga.', 1;
INSERT dbo.Bitacora_Inventario(id_usuario,accion,valor_anterior,valor_nuevo,detalle,fecha_hora)
SELECT @admin,N'SALIDA',CONVERT(NVARCHAR(50),i.stock_actual),CONVERT(NVARCHAR(50),i.stock_actual-c.cantidad),N'Consumo de recetas en pedidos demo Azure',@ahora
FROM @consumos c JOIN dbo.Insumos i ON i.id_insumo=c.id_insumo;
UPDATE i SET stock_actual=i.stock_actual-c.cantidad FROM dbo.Insumos i JOIN @consumos c ON c.id_insumo=i.id_insumo;
INSERT dbo.Bitacora_Pedidos(id_usuario,id_pedido,accion,estado_nuevo,detalle,fecha_hora)
SELECT @admin,p.id_pedido,N'CREACION',p.estado_pedido,N'Carga inicial de ejemplo Azure',p.fecha_hora FROM @nuevos n JOIN dbo.Pedidos p ON p.id_pedido=n.id_pedido;
INSERT dbo.Historial_Estados_Pedido(id_pedido,estado_nuevo,usuario_responsable,fecha_hora_cambio,detalle,estado)
SELECT p.id_pedido,p.estado_pedido,N'demo.admin',p.fecha_hora,N'Estado inicial del ejemplo Azure',1 FROM @nuevos n JOIN dbo.Pedidos p ON p.id_pedido=n.id_pedido;
IF EXISTS(SELECT 1 FROM dbo.Ventas v WHERE v.id_pedido=@pagado AND v.total_cobrado<>(SELECT SUM(d.subtotal_item) FROM dbo.Detalle_Venta d WHERE d.id_venta=v.id_venta AND d.estado=1))
    THROW 51106, 'Los detalles no cuadran con el total de la venta.', 1;
COMMIT TRANSACTION;
SELECT N'Datos demo cargados sin duplicar ejemplos existentes' AS resultado,
 (SELECT COUNT(*) FROM dbo.Usuarios WHERE nombre_usuario IN(N'demo.admin',N'demo.mesero',N'demo.cocina',N'demo.caja')) AS usuarios_demo,
 (SELECT COUNT(*) FROM dbo.Pedidos WHERE observaciones IN(N'AZ-DEMO-PAGADO-01',N'AZ-DEMO-ABIERTO-01')) AS pedidos_demo;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
