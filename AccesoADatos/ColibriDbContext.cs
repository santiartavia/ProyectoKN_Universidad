using Abstracciones.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace AccesoADatos
{
    public class ColibriDbContext : DbContext
    {
        public ColibriDbContext() : base("name=ColibriDbContext") { }
        private static readonly Type _ = typeof(SqlProviderServices);

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<TurnoTrabajo> TurnosTrabajo { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Vacacion> Vacaciones { get; set; }
        public DbSet<HoraExtra> HorasExtra { get; set; }
        public DbSet<BitacoraRRHH> BitacoraRRHH { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }
        public DbSet<BitacoraPedido> BitacoraPedidos { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<AperturaCaja> AperturasCaja { get; set; }
        public DbSet<CierreCaja> CierresCaja { get; set; }
        public DbSet<EgresoCaja> EgresosCaja { get; set; }
        public DbSet<NotaCredito> NotasCredito { get; set; }
        public DbSet<BitacoraFinanciera> BitacoraFinanciera { get; set; }
        public DbSet<ReporteGenerado> ReportesGenerados { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<SubcuentaPedido> SubcuentasPedido { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<EnvioPdv> EnviosPdv { get; set; }
        public DbSet<BitacoraPdv> BitacoraPdv { get; set; }
        public DbSet<SubcuentaVenta> SubcuentasVenta { get; set; }
        public DbSet<SubcuentaDetalleVenta> SubcuentaDetallesVenta { get; set; }
        public DbSet<HistorialEstadoPedido> HistorialEstadosPedido { get; set; }
        public DbSet<NominaMensual> NominasMensuales { get; set; }
        public DbSet<PasswordHistorial> PasswordHistorial { get; set; }
        public DbSet<Sesion> Sesiones { get; set; }
        public DbSet<BitacoraAcceso> BitacoraAcceso { get; set; }
        public DbSet<CategoriaInsumo> CategoriasInsumo { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<InsumoProveedor> InsumosProveedores { get; set; }
        public DbSet<BitacoraInventario> BitacoraInventario { get; set; }
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<RecetaInsumo> RecetaInsumos { get; set; }
        public DbSet<CategoriaProducto> CategoriasProducto { get; set; }
        public DbSet<CierrePeriodo> CierresPeriodo { get; set; }
        public DbSet<BitacoraReporte> BitacoraReportes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<Rol>().ToTable("Roles").HasKey(r => r.IdRol);
            modelBuilder.Entity<Rol>().Property(r => r.IdRol).HasColumnName("id_rol");
            modelBuilder.Entity<Rol>().Property(r => r.NombreRol).HasColumnName("nombre_rol");
            modelBuilder.Entity<Rol>().Property(r => r.Descripcion).HasColumnName("descripcion");
            modelBuilder.Entity<Rol>().Property(r => r.Estado).HasColumnName("estado");

            modelBuilder.Entity<Usuario>().ToTable("Usuarios").HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Usuario>().Property(u => u.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<Usuario>().Property(u => u.IdRol).HasColumnName("id_rol");
            modelBuilder.Entity<Usuario>().Property(u => u.NombreUsuario).HasColumnName("nombre_usuario");
            modelBuilder.Entity<Usuario>().Property(u => u.Correo).HasColumnName("correo");
            modelBuilder.Entity<Usuario>().Property(u => u.PasswordHash).HasColumnName("password_hash");
            modelBuilder.Entity<Usuario>().Property(u => u.CambioPasswordRequerido).HasColumnName("cambio_password_requerido");
            modelBuilder.Entity<Usuario>().Property(u => u.IntentosFallidos).HasColumnName("intentos_fallidos");
            modelBuilder.Entity<Usuario>().Property(u => u.Bloqueado).HasColumnName("bloqueado");
            modelBuilder.Entity<Usuario>().Property(u => u.FechaUltimoAcceso).HasColumnName("fecha_ultimo_acceso");
            modelBuilder.Entity<Usuario>().Property(u => u.Estado).HasColumnName("estado");
            modelBuilder.Entity<Usuario>().Property(u => u.FechaCreacion).HasColumnName("fecha_creacion");
            modelBuilder.Entity<Usuario>().Property(u => u.FechaPassword).HasColumnName("fecha_password");
            modelBuilder.Entity<Usuario>().Property(u => u.FechaAvisoPassword).HasColumnName("fecha_aviso_password");
            modelBuilder.Entity<Usuario>().Property(u => u.UltimoCambioPasswordIp).HasColumnName("ultimo_cambio_password_ip");
            modelBuilder.Entity<Usuario>().Property(u => u.UltimoCambioPasswordDispositivo).HasColumnName("ultimo_cambio_password_dispositivo");
            modelBuilder.Entity<Usuario>().Property(u => u.Direccion).HasColumnName("direccion");
            modelBuilder.Entity<Usuario>().HasRequired(u => u.Rol).WithMany(r => r.Usuarios).HasForeignKey(u => u.IdRol);

            modelBuilder.Entity<Empleado>().ToTable("Empleados").HasKey(e => e.IdEmpleado);
            modelBuilder.Entity<Empleado>().Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            modelBuilder.Entity<Empleado>().Property(e => e.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<Empleado>().Property(e => e.Cedula).HasColumnName("cedula");
            modelBuilder.Entity<Empleado>().Property(e => e.Nombre).HasColumnName("nombre");
            modelBuilder.Entity<Empleado>().Property(e => e.Apellidos).HasColumnName("apellidos");
            modelBuilder.Entity<Empleado>().Property(e => e.Telefono).HasColumnName("telefono");
            modelBuilder.Entity<Empleado>().Property(e => e.CorreoPersonal).HasColumnName("correo_personal");
            modelBuilder.Entity<Empleado>().Property(e => e.Direccion).HasColumnName("direccion");
            modelBuilder.Entity<Empleado>().Property(e => e.SalarioHora).HasColumnName("salario_hora");
            modelBuilder.Entity<Empleado>().Property(e => e.DiasVacacionesDisponibles).HasColumnName("dias_vacaciones_disponibles");
            modelBuilder.Entity<Empleado>().Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
            modelBuilder.Entity<Empleado>().Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            modelBuilder.Entity<Empleado>().Property(e => e.FechaReactivacion).HasColumnName("fecha_reactivacion");
            modelBuilder.Entity<Empleado>().Property(e => e.MotivoInactivacion).HasColumnName("motivo_inactivacion");
            modelBuilder.Entity<Empleado>().Property(e => e.Estado).HasColumnName("estado");
            modelBuilder.Entity<Empleado>().HasRequired(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario);

            modelBuilder.Entity<TurnoTrabajo>().ToTable("Turnos_Trabajo").HasKey(t => t.IdTurno);
            modelBuilder.Entity<TurnoTrabajo>().Property(t => t.IdTurno).HasColumnName("id_turno");
            modelBuilder.Entity<TurnoTrabajo>().Property(t => t.IdEmpleado).HasColumnName("id_empleado");
            modelBuilder.Entity<TurnoTrabajo>().Property(t => t.FechaTurno).HasColumnName("fecha_turno");
            modelBuilder.Entity<TurnoTrabajo>().Property(t => t.HoraInicio).HasColumnName("hora_inicio");
            modelBuilder.Entity<TurnoTrabajo>().Property(t => t.HoraFin).HasColumnName("hora_fin");
            modelBuilder.Entity<TurnoTrabajo>().Property(t => t.Descripcion).HasColumnName("descripcion");
            modelBuilder.Entity<TurnoTrabajo>().Property(t => t.Estado).HasColumnName("estado");
            modelBuilder.Entity<TurnoTrabajo>().HasRequired(t => t.Empleado).WithMany().HasForeignKey(t => t.IdEmpleado);

            modelBuilder.Entity<Asistencia>().ToTable("Asistencia").HasKey(a => a.IdAsistencia);
            modelBuilder.Entity<Asistencia>().Property(a => a.IdAsistencia).HasColumnName("id_asistencia");
            modelBuilder.Entity<Asistencia>().Property(a => a.IdEmpleado).HasColumnName("id_empleado");
            modelBuilder.Entity<Asistencia>().Property(a => a.IdTurno).HasColumnName("id_turno");
            modelBuilder.Entity<Asistencia>().Property(a => a.FechaHoraEntrada).HasColumnName("fecha_hora_entrada");
            modelBuilder.Entity<Asistencia>().Property(a => a.FechaHoraSalida).HasColumnName("fecha_hora_salida");
            modelBuilder.Entity<Asistencia>().Property(a => a.Observaciones).HasColumnName("observaciones");
            modelBuilder.Entity<Asistencia>().Property(a => a.Estado).HasColumnName("estado");
            modelBuilder.Entity<Asistencia>().HasRequired(a => a.Empleado).WithMany().HasForeignKey(a => a.IdEmpleado);
            modelBuilder.Entity<Asistencia>().HasOptional(a => a.Turno).WithMany().HasForeignKey(a => a.IdTurno);

            modelBuilder.Entity<Vacacion>().ToTable("Vacaciones").HasKey(v => v.IdVacacion);
            modelBuilder.Entity<Vacacion>().Property(v => v.IdVacacion).HasColumnName("id_vacacion");
            modelBuilder.Entity<Vacacion>().Property(v => v.IdEmpleado).HasColumnName("id_empleado");
            modelBuilder.Entity<Vacacion>().Property(v => v.IdAprobador).HasColumnName("id_aprobador");
            modelBuilder.Entity<Vacacion>().Property(v => v.FechaInicio).HasColumnName("fecha_inicio");
            modelBuilder.Entity<Vacacion>().Property(v => v.FechaFin).HasColumnName("fecha_fin");
            modelBuilder.Entity<Vacacion>().Property(v => v.DiasSolicitados).HasColumnName("dias_solicitados");
            modelBuilder.Entity<Vacacion>().Property(v => v.EstadoSolicitud).HasColumnName("estado_solicitud");
            modelBuilder.Entity<Vacacion>().Property(v => v.MotivoRechazo).HasColumnName("motivo_rechazo");
            modelBuilder.Entity<Vacacion>().Property(v => v.FechaSolicitud).HasColumnName("fecha_solicitud");
            modelBuilder.Entity<Vacacion>().Property(v => v.Estado).HasColumnName("estado");
            modelBuilder.Entity<Vacacion>().HasRequired(v => v.Empleado).WithMany().HasForeignKey(v => v.IdEmpleado);
            modelBuilder.Entity<Vacacion>().HasOptional(v => v.Aprobador).WithMany().HasForeignKey(v => v.IdAprobador);

            modelBuilder.Entity<HoraExtra>().ToTable("Horas_Extra").HasKey(h => h.IdHoraExtra);
            modelBuilder.Entity<HoraExtra>().Property(h => h.IdHoraExtra).HasColumnName("id_hora_extra");
            modelBuilder.Entity<HoraExtra>().Property(h => h.IdAsistencia).HasColumnName("id_asistencia");
            modelBuilder.Entity<HoraExtra>().Property(h => h.CantidadHoras).HasColumnName("cantidad_horas");
            modelBuilder.Entity<HoraExtra>().Property(h => h.FactorPago).HasColumnName("factor_pago");
            modelBuilder.Entity<HoraExtra>().Property(h => h.MontoCalculado).HasColumnName("monto_calculado");
            modelBuilder.Entity<HoraExtra>().Property(h => h.FechaRegistro).HasColumnName("fecha_registro");
            modelBuilder.Entity<HoraExtra>().Property(h => h.MotivoAjuste).HasColumnName("motivo_ajuste");
            modelBuilder.Entity<HoraExtra>().Property(h => h.Estado).HasColumnName("estado");
            modelBuilder.Entity<HoraExtra>().HasRequired(h => h.Asistencia).WithMany().HasForeignKey(h => h.IdAsistencia);

            modelBuilder.Entity<BitacoraRRHH>().ToTable("Bitacora_Usuarios_RRHH").HasKey(b => b.IdRegistro);
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.IdRegistro).HasColumnName("id_registro");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.TablaAfectada).HasColumnName("tabla_afectada");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.IdRegistroAfectado).HasColumnName("id_registro_afectado");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.Accion).HasColumnName("accion");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.ValorAnterior).HasColumnName("valor_anterior");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.ValorNuevo).HasColumnName("valor_nuevo");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.Detalle).HasColumnName("detalle");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.IpOrigen).HasColumnName("ip_origen");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.Dispositivo).HasColumnName("dispositivo");
            modelBuilder.Entity<BitacoraRRHH>().Property(b => b.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<BitacoraRRHH>().HasRequired(b => b.Usuario).WithMany().HasForeignKey(b => b.IdUsuario);

            modelBuilder.Entity<Mesa>().ToTable("Mesas").HasKey(m => m.IdMesa);
            modelBuilder.Entity<Mesa>().Property(m => m.IdMesa).HasColumnName("id_mesa");
            modelBuilder.Entity<Mesa>().Property(m => m.NumeroMesa).HasColumnName("numero_mesa");
            modelBuilder.Entity<Mesa>().Property(m => m.Capacidad).HasColumnName("capacidad");
            modelBuilder.Entity<Mesa>().Property(m => m.EstadoMesa).HasColumnName("estado_mesa");
            modelBuilder.Entity<Mesa>().Property(m => m.Estado).HasColumnName("estado");

            modelBuilder.Entity<Pedido>().ToTable("Pedidos").HasKey(p => p.IdPedido);
            modelBuilder.Entity<Pedido>().Property(p => p.IdPedido).HasColumnName("id_pedido");
            modelBuilder.Entity<Pedido>().Property(p => p.IdMesa).HasColumnName("id_mesa");
            modelBuilder.Entity<Pedido>().Property(p => p.IdEmpleado).HasColumnName("id_empleado");
            modelBuilder.Entity<Pedido>().Property(p => p.TipoServicio).HasColumnName("tipo_servicio");
            modelBuilder.Entity<Pedido>().Property(p => p.CantidadComensales).HasColumnName("cantidad_comensales");
            modelBuilder.Entity<Pedido>().Property(p => p.EstadoPedido).HasColumnName("estado_pedido");
            modelBuilder.Entity<Pedido>().Property(p => p.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<Pedido>().Property(p => p.Observaciones).HasColumnName("observaciones");
            modelBuilder.Entity<Pedido>().Property(p => p.Estado).HasColumnName("estado");
            modelBuilder.Entity<Pedido>().Property(p => p.FechaHoraEntrega).HasColumnName("fecha_hora_entrega");
            modelBuilder.Entity<Pedido>().Property(p => p.FechaHoraFinalizacion).HasColumnName("fecha_hora_finalizacion");

            modelBuilder.Entity<Producto>().ToTable("Productos").HasKey(p => p.IdProducto);
            modelBuilder.Entity<Producto>().Property(p => p.IdProducto).HasColumnName("id_producto");
            modelBuilder.Entity<Producto>().Property(p => p.IdCategoriaProd).HasColumnName("id_categoria_prod");
            modelBuilder.Entity<Producto>().Property(p => p.NombreProducto).HasColumnName("nombre_producto");
            modelBuilder.Entity<Producto>().Property(p => p.Descripcion).HasColumnName("descripcion");
            modelBuilder.Entity<Producto>().Property(p => p.PrecioVenta).HasColumnName("precio_venta");
            modelBuilder.Entity<Producto>().Property(p => p.Disponible).HasColumnName("disponible");
            modelBuilder.Entity<Producto>().Property(p => p.Estado).HasColumnName("estado");

            modelBuilder.Entity<DetallePedido>().ToTable("Detalle_Pedido").HasKey(d => d.IdDetalle);
            modelBuilder.Entity<DetallePedido>().Property(d => d.IdDetalle).HasColumnName("id_detalle");
            modelBuilder.Entity<DetallePedido>().Property(d => d.IdPedido).HasColumnName("id_pedido");
            modelBuilder.Entity<DetallePedido>().Property(d => d.IdProducto).HasColumnName("id_producto");
            modelBuilder.Entity<DetallePedido>().Property(d => d.Cantidad).HasColumnName("cantidad");
            modelBuilder.Entity<DetallePedido>().Property(d => d.PrecioUnitario).HasColumnName("precio_unitario");
            modelBuilder.Entity<DetallePedido>().Property(d => d.ObservacionesItem).HasColumnName("observaciones_item");
            modelBuilder.Entity<DetallePedido>().Property(d => d.EstadoItem).HasColumnName("estado_item");
            modelBuilder.Entity<DetallePedido>().Property(d => d.Estado).HasColumnName("estado");
            modelBuilder.Entity<DetallePedido>().HasRequired(d => d.Pedido).WithMany().HasForeignKey(d => d.IdPedido);
            modelBuilder.Entity<DetallePedido>().HasRequired(d => d.Producto).WithMany().HasForeignKey(d => d.IdProducto);

            modelBuilder.Entity<BitacoraPedido>().ToTable("Bitacora_Pedidos").HasKey(b => b.IdRegistro);
            modelBuilder.Entity<BitacoraPedido>().Property(b => b.IdRegistro).HasColumnName("id_registro");
            modelBuilder.Entity<BitacoraPedido>().Property(b => b.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<BitacoraPedido>().Property(b => b.IdPedido).HasColumnName("id_pedido");
            modelBuilder.Entity<BitacoraPedido>().Property(b => b.Accion).HasColumnName("accion");
            modelBuilder.Entity<BitacoraPedido>().Property(b => b.EstadoAnterior).HasColumnName("estado_anterior");
            modelBuilder.Entity<BitacoraPedido>().Property(b => b.EstadoNuevo).HasColumnName("estado_nuevo");
            modelBuilder.Entity<BitacoraPedido>().Property(b => b.Detalle).HasColumnName("detalle");
            modelBuilder.Entity<BitacoraPedido>().Property(b => b.FechaHora).HasColumnName("fecha_hora");

            modelBuilder.Entity<Venta>().ToTable("Ventas").HasKey(v => v.IdVenta);
            modelBuilder.Entity<Venta>().Property(v => v.IdVenta).HasColumnName("id_venta");
            modelBuilder.Entity<Venta>().Property(v => v.IdPedido).HasColumnName("id_pedido");
            modelBuilder.Entity<Venta>().Property(v => v.IdSubcuenta).HasColumnName("id_subcuenta");
            modelBuilder.Entity<Venta>().Property(v => v.IdEmpleado).HasColumnName("id_empleado");
            modelBuilder.Entity<Venta>().Property(v => v.IdApertura).HasColumnName("id_apertura");
            modelBuilder.Entity<Venta>().Property(v => v.TipoVenta).HasColumnName("tipo_venta");
            modelBuilder.Entity<Venta>().Property(v => v.TotalCobrado).HasColumnName("total_cobrado");
            modelBuilder.Entity<Venta>().Property(v => v.MontoRecibido).HasColumnName("monto_recibido");
            modelBuilder.Entity<Venta>().Property(v => v.Vuelto).HasColumnName("vuelto");
            modelBuilder.Entity<Venta>().Property(v => v.MetodoPago).HasColumnName("metodo_pago");
            modelBuilder.Entity<Venta>().Property(v => v.EstadoVenta).HasColumnName("estado_venta");
            modelBuilder.Entity<Venta>().Property(v => v.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<Venta>().Property(v => v.Estado).HasColumnName("estado");
            modelBuilder.Entity<Venta>().HasOptional(v => v.Apertura).WithMany().HasForeignKey(v => v.IdApertura);

            modelBuilder.Entity<SubcuentaPedido>().ToTable("Subcuentas_Pedido").HasKey(s => s.IdSubcuenta);
            modelBuilder.Entity<SubcuentaPedido>().Property(s => s.IdSubcuenta).HasColumnName("id_subcuenta");
            modelBuilder.Entity<SubcuentaPedido>().Property(s => s.IdPedido).HasColumnName("id_pedido");
            modelBuilder.Entity<SubcuentaPedido>().Property(s => s.NombreSubcuenta).HasColumnName("nombre_subcuenta");
            modelBuilder.Entity<SubcuentaPedido>().Property(s => s.Estado).HasColumnName("estado");

            modelBuilder.Entity<DetalleVenta>().ToTable("Detalle_Venta").HasKey(d => d.IdDetalleVenta);
            modelBuilder.Entity<DetalleVenta>().Property(d => d.IdDetalleVenta).HasColumnName("id_detalle_venta");
            modelBuilder.Entity<DetalleVenta>().Property(d => d.IdVenta).HasColumnName("id_venta");
            modelBuilder.Entity<DetalleVenta>().Property(d => d.IdProducto).HasColumnName("id_producto");
            modelBuilder.Entity<DetalleVenta>().Property(d => d.Cantidad).HasColumnName("cantidad");
            modelBuilder.Entity<DetalleVenta>().Property(d => d.PrecioUnitario).HasColumnName("precio_unitario");
            modelBuilder.Entity<DetalleVenta>().Property(d => d.SubtotalItem).HasColumnName("subtotal_item");
            modelBuilder.Entity<DetalleVenta>().Property(d => d.ObservacionesItem).HasColumnName("observaciones_item");
            modelBuilder.Entity<DetalleVenta>().Property(d => d.Estado).HasColumnName("estado");
            modelBuilder.Entity<DetalleVenta>().HasRequired(d => d.Venta).WithMany().HasForeignKey(d => d.IdVenta);
            modelBuilder.Entity<DetalleVenta>().HasRequired(d => d.Producto).WithMany().HasForeignKey(d => d.IdProducto);

            modelBuilder.Entity<EnvioPdv>().ToTable("Envios_PDV").HasKey(e => e.IdEnvio);
            modelBuilder.Entity<EnvioPdv>().Property(e => e.IdEnvio).HasColumnName("id_envio");
            modelBuilder.Entity<EnvioPdv>().Property(e => e.IdVenta).HasColumnName("id_venta");
            modelBuilder.Entity<EnvioPdv>().Property(e => e.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<EnvioPdv>().Property(e => e.Destino).HasColumnName("destino");
            modelBuilder.Entity<EnvioPdv>().Property(e => e.Prioridad).HasColumnName("prioridad");
            modelBuilder.Entity<EnvioPdv>().Property(e => e.EstadoEnvio).HasColumnName("estado_envio");
            modelBuilder.Entity<EnvioPdv>().Property(e => e.CodigoConfirmacion).HasColumnName("codigo_confirmacion");
            modelBuilder.Entity<EnvioPdv>().Property(e => e.Observaciones).HasColumnName("observaciones");
            modelBuilder.Entity<EnvioPdv>().Property(e => e.MotivoReenvio).HasColumnName("motivo_reenvio");
            modelBuilder.Entity<EnvioPdv>().Property(e => e.FechaHoraEnvio).HasColumnName("fecha_hora_envio");

            modelBuilder.Entity<BitacoraPdv>().ToTable("Bitacora_PDV").HasKey(b => b.IdRegistro);
            modelBuilder.Entity<BitacoraPdv>().Property(b => b.IdRegistro).HasColumnName("id_registro");
            modelBuilder.Entity<BitacoraPdv>().Property(b => b.IdCaja).HasColumnName("id_caja");
            modelBuilder.Entity<BitacoraPdv>().Property(b => b.IdUsuarioCajero).HasColumnName("id_usuario_cajero");
            modelBuilder.Entity<BitacoraPdv>().Property(b => b.AccionOperativa).HasColumnName("accion_operativa");
            modelBuilder.Entity<BitacoraPdv>().Property(b => b.IdVenta).HasColumnName("id_venta");
            modelBuilder.Entity<BitacoraPdv>().Property(b => b.Detalle).HasColumnName("detalle");
            modelBuilder.Entity<BitacoraPdv>().Property(b => b.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<BitacoraPdv>().Property(b => b.Estado).HasColumnName("estado");

            modelBuilder.Entity<SubcuentaVenta>().ToTable("Subcuentas_Venta").HasKey(s => s.IdSubcuenta);
            modelBuilder.Entity<SubcuentaVenta>().Property(s => s.IdSubcuenta).HasColumnName("id_subcuenta");
            modelBuilder.Entity<SubcuentaVenta>().Property(s => s.IdVenta).HasColumnName("id_venta");
            modelBuilder.Entity<SubcuentaVenta>().Property(s => s.NombreSubcuenta).HasColumnName("nombre_subcuenta");
            modelBuilder.Entity<SubcuentaVenta>().Property(s => s.Subtotal).HasColumnName("subtotal");
            modelBuilder.Entity<SubcuentaVenta>().Property(s => s.Pagada).HasColumnName("pagada");
            modelBuilder.Entity<SubcuentaVenta>().Property(s => s.FechaOperacion).HasColumnName("fecha_operacion");
            modelBuilder.Entity<SubcuentaVenta>().Property(s => s.Estado).HasColumnName("estado");

            modelBuilder.Entity<SubcuentaDetalleVenta>().ToTable("Subcuenta_Detalle_Venta").HasKey(sd => sd.IdSubcuentaDetalle);
            modelBuilder.Entity<SubcuentaDetalleVenta>().Property(sd => sd.IdSubcuentaDetalle).HasColumnName("id_subcuenta_detalle");
            modelBuilder.Entity<SubcuentaDetalleVenta>().Property(sd => sd.IdSubcuenta).HasColumnName("id_subcuenta");
            modelBuilder.Entity<SubcuentaDetalleVenta>().Property(sd => sd.IdDetalleVenta).HasColumnName("id_detalle_venta");
            modelBuilder.Entity<SubcuentaDetalleVenta>().Property(sd => sd.Cantidad).HasColumnName("cantidad");
            modelBuilder.Entity<SubcuentaDetalleVenta>().Property(sd => sd.Subtotal).HasColumnName("subtotal");
            modelBuilder.Entity<SubcuentaDetalleVenta>().Property(sd => sd.FechaOperacion).HasColumnName("fecha_operacion");
            modelBuilder.Entity<SubcuentaDetalleVenta>().Property(sd => sd.Estado).HasColumnName("estado");

            modelBuilder.Entity<HistorialEstadoPedido>().ToTable("Historial_Estados_Pedido").HasKey(h => h.IdHistorial);
            modelBuilder.Entity<HistorialEstadoPedido>().Property(h => h.IdHistorial).HasColumnName("id_historial");
            modelBuilder.Entity<HistorialEstadoPedido>().Property(h => h.IdPedido).HasColumnName("id_pedido");
            modelBuilder.Entity<HistorialEstadoPedido>().Property(h => h.EstadoAnterior).HasColumnName("estado_anterior");
            modelBuilder.Entity<HistorialEstadoPedido>().Property(h => h.EstadoNuevo).HasColumnName("estado_nuevo");
            modelBuilder.Entity<HistorialEstadoPedido>().Property(h => h.UsuarioResponsable).HasColumnName("usuario_responsable");
            modelBuilder.Entity<HistorialEstadoPedido>().Property(h => h.FechaHoraCambio).HasColumnName("fecha_hora_cambio");
            modelBuilder.Entity<HistorialEstadoPedido>().Property(h => h.Detalle).HasColumnName("detalle");
            modelBuilder.Entity<HistorialEstadoPedido>().Property(h => h.Estado).HasColumnName("estado");

            modelBuilder.Entity<NominaMensual>().ToTable("nomina_mensual").HasKey(n => n.IdNominaMensual);
            modelBuilder.Entity<NominaMensual>().Property(n => n.IdNominaMensual).HasColumnName("id");
            modelBuilder.Entity<NominaMensual>().Property(n => n.IdEmpleado).HasColumnName("empleado_id");
            modelBuilder.Entity<NominaMensual>().Property(n => n.Mes).HasColumnName("mes");
            modelBuilder.Entity<NominaMensual>().Property(n => n.Anio).HasColumnName("anio");
            modelBuilder.Entity<NominaMensual>().Property(n => n.HorasTrabajadas).HasColumnName("horas_trabajadas");
            modelBuilder.Entity<NominaMensual>().Property(n => n.HorasExtra).HasColumnName("horas_extra");
            modelBuilder.Entity<NominaMensual>().Property(n => n.VacacionesPagadas).HasColumnName("vacaciones_pagadas");
            modelBuilder.Entity<NominaMensual>().Property(n => n.HorasPorDiaVacacion).HasColumnName("horas_por_dia_vacacion");
            modelBuilder.Entity<NominaMensual>().Property(n => n.MontoVacaciones).HasColumnName("monto_vacaciones");
            modelBuilder.Entity<NominaMensual>().Property(n => n.DiasTrabajados).HasColumnName("dias_trabajados");
            modelBuilder.Entity<NominaMensual>().Property(n => n.DiasAusentes).HasColumnName("dias_ausentes");
            modelBuilder.Entity<NominaMensual>().Property(n => n.ValorHora).HasColumnName("valor_hora");
            modelBuilder.Entity<NominaMensual>().Property(n => n.SalarioBase).HasColumnName("salario_base");
            modelBuilder.Entity<NominaMensual>().Property(n => n.MontoHorasExtra).HasColumnName("monto_horas_extra");
            modelBuilder.Entity<NominaMensual>().Property(n => n.Bonificaciones).HasColumnName("bonificaciones");
            modelBuilder.Entity<NominaMensual>().Property(n => n.SalarioBruto).HasColumnName("salario_bruto");
            modelBuilder.Entity<NominaMensual>().Property(n => n.Estado).HasColumnName("estado");
            modelBuilder.Entity<NominaMensual>().Property(n => n.FechaCierre).HasColumnName("fecha_cierre");
            modelBuilder.Entity<NominaMensual>().Property(n => n.Observaciones).HasColumnName("observaciones");
            modelBuilder.Entity<NominaMensual>().Property(n => n.CreadoPor).HasColumnName("creado_por");
            modelBuilder.Entity<NominaMensual>().Property(n => n.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<NominaMensual>().Property(n => n.UpdatedAt).HasColumnName("updated_at");
            modelBuilder.Entity<NominaMensual>().HasRequired(n => n.Empleado).WithMany().HasForeignKey(n => n.IdEmpleado);

            // New entities
            modelBuilder.Entity<PasswordHistorial>().ToTable("PasswordHistorial").HasKey(p => p.IdHistorial);
            modelBuilder.Entity<PasswordHistorial>().Property(p => p.IdHistorial).HasColumnName("id_historial");
            modelBuilder.Entity<PasswordHistorial>().Property(p => p.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<PasswordHistorial>().Property(p => p.PasswordHash).HasColumnName("password_hash");
            modelBuilder.Entity<PasswordHistorial>().Property(p => p.FechaCambio).HasColumnName("fecha_cambio");
            modelBuilder.Entity<PasswordHistorial>().Property(p => p.MetodoCambio).HasColumnName("metodo_cambio");
            modelBuilder.Entity<PasswordHistorial>().Property(p => p.Dispositivo).HasColumnName("dispositivo");
            modelBuilder.Entity<PasswordHistorial>().Property(p => p.DireccionIp).HasColumnName("direccion_ip");
            modelBuilder.Entity<PasswordHistorial>().HasRequired(p => p.Usuario).WithMany().HasForeignKey(p => p.IdUsuario);

            modelBuilder.Entity<Sesion>().ToTable("Sesiones").HasKey(s => s.IdSesion);
            modelBuilder.Entity<Sesion>().Property(s => s.IdSesion).HasColumnName("id_sesion");
            modelBuilder.Entity<Sesion>().Property(s => s.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<Sesion>().Property(s => s.EstadoSesion).HasColumnName("estado_sesion");
            modelBuilder.Entity<Sesion>().Property(s => s.FechaHoraInicio).HasColumnName("fecha_hora_inicio");
            modelBuilder.Entity<Sesion>().Property(s => s.FechaHoraUltimaAct).HasColumnName("fecha_hora_ultima_act");
            modelBuilder.Entity<Sesion>().Property(s => s.FechaHoraCierre).HasColumnName("fecha_hora_cierre");
            modelBuilder.Entity<Sesion>().Property(s => s.DispositivoAcceso).HasColumnName("dispositivo_acceso");
            modelBuilder.Entity<Sesion>().Property(s => s.DireccionIp).HasColumnName("direccion_ip");
            modelBuilder.Entity<Sesion>().Property(s => s.MotivoCierre).HasColumnName("motivo_cierre");
            modelBuilder.Entity<Sesion>().HasRequired(s => s.Usuario).WithMany().HasForeignKey(s => s.IdUsuario);

            modelBuilder.Entity<BitacoraAcceso>().ToTable("Bitacora_Acceso").HasKey(b => b.IdRegistro);
            modelBuilder.Entity<BitacoraAcceso>().Property(b => b.IdRegistro).HasColumnName("id_registro");
            modelBuilder.Entity<BitacoraAcceso>().Property(b => b.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<BitacoraAcceso>().Property(b => b.Accion).HasColumnName("accion");
            modelBuilder.Entity<BitacoraAcceso>().Property(b => b.ValorAnterior).HasColumnName("valor_anterior");
            modelBuilder.Entity<BitacoraAcceso>().Property(b => b.ValorNuevo).HasColumnName("valor_nuevo");
            modelBuilder.Entity<BitacoraAcceso>().Property(b => b.Detalle).HasColumnName("detalle");
            modelBuilder.Entity<BitacoraAcceso>().Property(b => b.IpOrigen).HasColumnName("ip_origen");
            modelBuilder.Entity<BitacoraAcceso>().Property(b => b.Dispositivo).HasColumnName("dispositivo");
            modelBuilder.Entity<BitacoraAcceso>().Property(b => b.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<BitacoraAcceso>().HasRequired(b => b.Usuario).WithMany().HasForeignKey(b => b.IdUsuario);

            modelBuilder.Entity<CategoriaInsumo>().ToTable("Categorias_Insumo").HasKey(c => c.IdCategoria);
            modelBuilder.Entity<CategoriaInsumo>().Property(c => c.IdCategoria).HasColumnName("id_categoria");
            modelBuilder.Entity<CategoriaInsumo>().Property(c => c.NombreCategoria).HasColumnName("nombre_categoria");
            modelBuilder.Entity<CategoriaInsumo>().Property(c => c.Estado).HasColumnName("estado");

            modelBuilder.Entity<Insumo>().ToTable("Insumos").HasKey(i => i.IdInsumo);
            modelBuilder.Entity<Insumo>().Property(i => i.IdInsumo).HasColumnName("id_insumo");
            modelBuilder.Entity<Insumo>().Property(i => i.IdCategoria).HasColumnName("id_categoria");
            modelBuilder.Entity<Insumo>().Property(i => i.NombreInsumo).HasColumnName("nombre_insumo");
            modelBuilder.Entity<Insumo>().Property(i => i.UnidadMedida).HasColumnName("unidad_medida");
            modelBuilder.Entity<Insumo>().Property(i => i.StockMinimo).HasColumnName("stock_minimo");
            modelBuilder.Entity<Insumo>().Property(i => i.StockActual).HasColumnName("stock_actual");
            modelBuilder.Entity<Insumo>().Property(i => i.CostoUnitario).HasColumnName("costo_unitario");
            modelBuilder.Entity<Insumo>().Property(i => i.Estado).HasColumnName("estado");
            modelBuilder.Entity<Insumo>().HasRequired(i => i.Categoria).WithMany().HasForeignKey(i => i.IdCategoria);

            modelBuilder.Entity<Proveedor>().ToTable("Proveedores").HasKey(p => p.IdProveedor);
            modelBuilder.Entity<Proveedor>().Property(p => p.IdProveedor).HasColumnName("id_proveedor");
            modelBuilder.Entity<Proveedor>().Property(p => p.CedulaJuridica).HasColumnName("cedula_juridica");
            modelBuilder.Entity<Proveedor>().Property(p => p.NombreEmpresa).HasColumnName("nombre_empresa");
            modelBuilder.Entity<Proveedor>().Property(p => p.ContactoNombre).HasColumnName("contacto_nombre");
            modelBuilder.Entity<Proveedor>().Property(p => p.Telefono).HasColumnName("telefono");
            modelBuilder.Entity<Proveedor>().Property(p => p.Correo).HasColumnName("correo");
            modelBuilder.Entity<Proveedor>().Property(p => p.Estado).HasColumnName("estado");

            modelBuilder.Entity<InsumoProveedor>().ToTable("Insumos_Proveedores").HasKey(ip => new { ip.IdInsumo, ip.IdProveedor });
            modelBuilder.Entity<InsumoProveedor>().Property(ip => ip.IdInsumo).HasColumnName("id_insumo");
            modelBuilder.Entity<InsumoProveedor>().Property(ip => ip.IdProveedor).HasColumnName("id_proveedor");
            modelBuilder.Entity<InsumoProveedor>().Property(ip => ip.FechaAsoc).HasColumnName("fecha_asoc");
            modelBuilder.Entity<InsumoProveedor>().HasRequired(ip => ip.Insumo).WithMany().HasForeignKey(ip => ip.IdInsumo);
            modelBuilder.Entity<InsumoProveedor>().HasRequired(ip => ip.Proveedor).WithMany().HasForeignKey(ip => ip.IdProveedor);

            modelBuilder.Entity<BitacoraInventario>().ToTable("Bitacora_Inventario").HasKey(b => b.IdRegistro);
            modelBuilder.Entity<BitacoraInventario>().Property(b => b.IdRegistro).HasColumnName("id_registro");
            modelBuilder.Entity<BitacoraInventario>().Property(b => b.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<BitacoraInventario>().Property(b => b.Accion).HasColumnName("accion");
            modelBuilder.Entity<BitacoraInventario>().Property(b => b.ValorAnterior).HasColumnName("valor_anterior");
            modelBuilder.Entity<BitacoraInventario>().Property(b => b.ValorNuevo).HasColumnName("valor_nuevo");
            modelBuilder.Entity<BitacoraInventario>().Property(b => b.Detalle).HasColumnName("detalle");
            modelBuilder.Entity<BitacoraInventario>().Property(b => b.IpOrigen).HasColumnName("ip_origen");
            modelBuilder.Entity<BitacoraInventario>().Property(b => b.Dispositivo).HasColumnName("dispositivo");
            modelBuilder.Entity<BitacoraInventario>().Property(b => b.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<BitacoraInventario>().HasRequired(b => b.Usuario).WithMany().HasForeignKey(b => b.IdUsuario);

            modelBuilder.Entity<CategoriaProducto>().ToTable("Categorias_Producto").HasKey(c => c.IdCategoriaProd);
            modelBuilder.Entity<CategoriaProducto>().Property(c => c.IdCategoriaProd).HasColumnName("id_categoria_prod");
            modelBuilder.Entity<CategoriaProducto>().Property(c => c.NombreCategoria).HasColumnName("nombre_categoria");
            modelBuilder.Entity<CategoriaProducto>().Property(c => c.Estado).HasColumnName("estado");

            modelBuilder.Entity<Receta>().ToTable("Receta").HasKey(r => r.IdReceta);
            modelBuilder.Entity<Receta>().Property(r => r.IdReceta).HasColumnName("id_receta");
            modelBuilder.Entity<Receta>().Property(r => r.IdProducto).HasColumnName("id_producto");
            modelBuilder.Entity<Receta>().Property(r => r.Estado).HasColumnName("estado");
            modelBuilder.Entity<Receta>().HasRequired(r => r.Producto).WithMany().HasForeignKey(r => r.IdProducto);

            modelBuilder.Entity<RecetaInsumo>().ToTable("RecetaInsumo").HasKey(ri => new { ri.IdReceta, ri.IdInsumo });
            modelBuilder.Entity<RecetaInsumo>().Property(ri => ri.IdReceta).HasColumnName("id_receta");
            modelBuilder.Entity<RecetaInsumo>().Property(ri => ri.IdInsumo).HasColumnName("id_insumo");
            modelBuilder.Entity<RecetaInsumo>().Property(ri => ri.CantidadUsar).HasColumnName("cantidad_usar");
            modelBuilder.Entity<RecetaInsumo>().HasRequired(ri => ri.Receta).WithMany().HasForeignKey(ri => ri.IdReceta);
            modelBuilder.Entity<RecetaInsumo>().HasRequired(ri => ri.Insumo).WithMany().HasForeignKey(ri => ri.IdInsumo);

            modelBuilder.Entity<Caja>().ToTable("Cajas").HasKey(c => c.IdCaja);
            modelBuilder.Entity<Caja>().Property(c => c.IdCaja).HasColumnName("id_caja");
            modelBuilder.Entity<Caja>().Property(c => c.NombreCaja).HasColumnName("nombre_caja");
            modelBuilder.Entity<Caja>().Property(c => c.EstadoCaja).HasColumnName("estado_caja");
            modelBuilder.Entity<Caja>().Property(c => c.Estado).HasColumnName("estado");

            modelBuilder.Entity<AperturaCaja>().ToTable("Apertura_Caja").HasKey(a => a.IdApertura);
            modelBuilder.Entity<AperturaCaja>().Property(a => a.IdApertura).HasColumnName("id_apertura");
            modelBuilder.Entity<AperturaCaja>().Property(a => a.IdCaja).HasColumnName("id_caja");
            modelBuilder.Entity<AperturaCaja>().Property(a => a.IdCajero).HasColumnName("id_cajero");
            modelBuilder.Entity<AperturaCaja>().Property(a => a.MontoInicial).HasColumnName("monto_inicial");
            modelBuilder.Entity<AperturaCaja>().Property(a => a.FechaApertura).HasColumnName("fecha_apertura");
            modelBuilder.Entity<AperturaCaja>().Property(a => a.Observaciones).HasColumnName("observaciones");
            modelBuilder.Entity<AperturaCaja>().Property(a => a.Estado).HasColumnName("estado");
            modelBuilder.Entity<AperturaCaja>().HasRequired(a => a.Caja).WithMany(c => c.Aperturas).HasForeignKey(a => a.IdCaja);
            modelBuilder.Entity<AperturaCaja>().HasRequired(a => a.Cajero).WithMany().HasForeignKey(a => a.IdCajero);

            modelBuilder.Entity<CierreCaja>().ToTable("Cierres_Caja").HasKey(c => c.IdCierre);
            modelBuilder.Entity<CierreCaja>().Property(c => c.IdCierre).HasColumnName("id_cierre");
            modelBuilder.Entity<CierreCaja>().Property(c => c.IdCajero).HasColumnName("id_cajero");
            modelBuilder.Entity<CierreCaja>().Property(c => c.IdApertura).HasColumnName("id_apertura");
            modelBuilder.Entity<CierreCaja>().Property(c => c.FechaCierre).HasColumnName("fecha_cierre");
            modelBuilder.Entity<CierreCaja>().Property(c => c.MontoApertura).HasColumnName("monto_apertura");
            modelBuilder.Entity<CierreCaja>().Property(c => c.TotalEfectivo).HasColumnName("total_efectivo");
            modelBuilder.Entity<CierreCaja>().Property(c => c.TotalSinpe).HasColumnName("total_sinpe");
            modelBuilder.Entity<CierreCaja>().Property(c => c.TotalTarjeta).HasColumnName("total_tarjeta");
            modelBuilder.Entity<CierreCaja>().Property(c => c.TotalEgresos).HasColumnName("total_egresos");
            modelBuilder.Entity<CierreCaja>().Property(c => c.SaldoEsperado).HasColumnName("saldo_esperado");
            modelBuilder.Entity<CierreCaja>().Property(c => c.SaldoReal).HasColumnName("saldo_real");
            modelBuilder.Entity<CierreCaja>().Property(c => c.Descuadre).HasColumnName("descuadre");
            modelBuilder.Entity<CierreCaja>().Property(c => c.Estado).HasColumnName("estado");
            modelBuilder.Entity<CierreCaja>().HasRequired(c => c.Apertura).WithMany().HasForeignKey(c => c.IdApertura);
            modelBuilder.Entity<CierreCaja>().HasRequired(c => c.Cajero).WithMany().HasForeignKey(c => c.IdCajero);

            modelBuilder.Entity<EgresoCaja>().ToTable("Egresos_Caja").HasKey(e => e.IdEgreso);
            modelBuilder.Entity<EgresoCaja>().Property(e => e.IdEgreso).HasColumnName("id_egreso");
            modelBuilder.Entity<EgresoCaja>().Property(e => e.IdApertura).HasColumnName("id_apertura");
            modelBuilder.Entity<EgresoCaja>().Property(e => e.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<EgresoCaja>().Property(e => e.CategoriaGasto).HasColumnName("categoria_gasto");
            modelBuilder.Entity<EgresoCaja>().Property(e => e.Descripcion).HasColumnName("descripcion");
            modelBuilder.Entity<EgresoCaja>().Property(e => e.Monto).HasColumnName("monto");
            modelBuilder.Entity<EgresoCaja>().Property(e => e.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<EgresoCaja>().Property(e => e.Estado).HasColumnName("estado");
            modelBuilder.Entity<EgresoCaja>().HasRequired(e => e.Apertura).WithMany().HasForeignKey(e => e.IdApertura);
            modelBuilder.Entity<EgresoCaja>().HasRequired(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario);

            modelBuilder.Entity<NotaCredito>().ToTable("Notas_Credito").HasKey(n => n.IdNotaCredito);
            modelBuilder.Entity<NotaCredito>().Property(n => n.IdNotaCredito).HasColumnName("id_nota_credito");
            modelBuilder.Entity<NotaCredito>().Property(n => n.IdVenta).HasColumnName("id_venta");
            modelBuilder.Entity<NotaCredito>().Property(n => n.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<NotaCredito>().Property(n => n.Motivo).HasColumnName("motivo");
            modelBuilder.Entity<NotaCredito>().Property(n => n.Monto).HasColumnName("monto");
            modelBuilder.Entity<NotaCredito>().Property(n => n.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<NotaCredito>().Property(n => n.Estado).HasColumnName("estado");
            modelBuilder.Entity<NotaCredito>().HasRequired(n => n.Venta).WithMany().HasForeignKey(n => n.IdVenta);
            modelBuilder.Entity<NotaCredito>().HasRequired(n => n.Usuario).WithMany().HasForeignKey(n => n.IdUsuario);

            modelBuilder.Entity<BitacoraFinanciera>().ToTable("Bitacora_Financiera").HasKey(b => b.IdRegistro);
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.IdRegistro).HasColumnName("id_registro");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.TablaAfectada).HasColumnName("tabla_afectada");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.IdRegistroAfectado).HasColumnName("id_registro_afectado");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.Accion).HasColumnName("accion");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.ValorAnterior).HasColumnName("valor_anterior");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.ValorNuevo).HasColumnName("valor_nuevo");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.Detalle).HasColumnName("detalle");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.IpOrigen).HasColumnName("ip_origen");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.Dispositivo).HasColumnName("dispositivo");
            modelBuilder.Entity<BitacoraFinanciera>().Property(b => b.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<BitacoraFinanciera>().HasRequired(b => b.Usuario).WithMany().HasForeignKey(b => b.IdUsuario);

            modelBuilder.Entity<ReporteGenerado>().ToTable("Reportes_Generados").HasKey(r => r.IdReporte);
            modelBuilder.Entity<ReporteGenerado>().Property(r => r.IdReporte).HasColumnName("id_reporte");
            modelBuilder.Entity<ReporteGenerado>().Property(r => r.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<ReporteGenerado>().Property(r => r.TipoReporte).HasColumnName("tipo_reporte");
            modelBuilder.Entity<ReporteGenerado>().Property(r => r.Parametros).HasColumnName("parametros");
            modelBuilder.Entity<ReporteGenerado>().Property(r => r.FormatoSalida).HasColumnName("formato_salida");
            modelBuilder.Entity<ReporteGenerado>().Property(r => r.FechaGeneracion).HasColumnName("fecha_generacion");
            modelBuilder.Entity<ReporteGenerado>().Property(r => r.Estado).HasColumnName("estado");
            modelBuilder.Entity<ReporteGenerado>().HasRequired(r => r.Usuario).WithMany().HasForeignKey(r => r.IdUsuario);

            modelBuilder.Entity<CierrePeriodo>().ToTable("Cierres_Periodo").HasKey(c => c.IdCierrePeriodo);
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.IdCierrePeriodo).HasColumnName("id_cierre_periodo");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.TipoPeriodo).HasColumnName("tipo_periodo");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.Mes).HasColumnName("mes");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.Anio).HasColumnName("anio");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.FechaCierre).HasColumnName("fecha_cierre");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.TotalIngresos).HasColumnName("total_ingresos");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.TotalEgresos).HasColumnName("total_egresos");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.TotalNotasCredito).HasColumnName("total_notas_credito");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.SaldoFinal).HasColumnName("saldo_final");
            modelBuilder.Entity<CierrePeriodo>().Property(c => c.Estado).HasColumnName("estado");
            modelBuilder.Entity<CierrePeriodo>().HasRequired(c => c.Usuario).WithMany().HasForeignKey(c => c.IdUsuario);

            modelBuilder.Entity<BitacoraReporte>().ToTable("Bitacora_Reportes").HasKey(b => b.IdRegistro);
            modelBuilder.Entity<BitacoraReporte>().Property(b => b.IdRegistro).HasColumnName("id_registro");
            modelBuilder.Entity<BitacoraReporte>().Property(b => b.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<BitacoraReporte>().Property(b => b.Accion).HasColumnName("accion");
            modelBuilder.Entity<BitacoraReporte>().Property(b => b.ValorAnterior).HasColumnName("valor_anterior");
            modelBuilder.Entity<BitacoraReporte>().Property(b => b.ValorNuevo).HasColumnName("valor_nuevo");
            modelBuilder.Entity<BitacoraReporte>().Property(b => b.Detalle).HasColumnName("detalle");
            modelBuilder.Entity<BitacoraReporte>().Property(b => b.IpOrigen).HasColumnName("ip_origen");
            modelBuilder.Entity<BitacoraReporte>().Property(b => b.Dispositivo).HasColumnName("dispositivo");
            modelBuilder.Entity<BitacoraReporte>().Property(b => b.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<BitacoraReporte>().HasRequired(b => b.Usuario).WithMany().HasForeignKey(b => b.IdUsuario);
        }
    }
}
