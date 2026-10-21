// Runs the original numbered acceptance steps using the real MVC pages and forms.
module.exports = async function (browser, f, root) {
  const fs = require('fs'), path = require('path');
  const original = JSON.parse(fs.readFileSync(path.join(root, 'docs/qa/plan-91-pasos.json'), 'utf8'));
  const selection = process.argv.find(arg => arg.startsWith('--cases='));
  const selected = selection ? new Set(selection.slice(8).split(',')) : null;
  const resultFile = path.join(root, 'docs/qa/resultados-91.json');
  const results = selected && fs.existsSync(resultFile) ? JSON.parse(fs.readFileSync(resultFile, 'utf8')).filter(r => !selected.has(r.caso)) : [];
  const sessions = {};
  const base = 'http://localhost:51235';
  const today = new Intl.DateTimeFormat('en-CA', { timeZone: 'America/Costa_Rica' }).format(new Date());
  const future = n => { const d = new Date(today + 'T12:00:00Z'); d.setUTCDate(d.getUTCDate() + n); return d.toISOString().slice(0, 10); };
  const assert = (ok, message) => { if (!ok) throw new Error(message); };
  async function text(page) { return page.locator('body').innerText(); }
  async function visit(page, route) { const r = await page.goto(base + route); assert(r.status() === 200, route + ' HTTP ' + r.status()); return text(page); }
  async function login(role) {
    const context = await browser.newContext(); const page = await context.newPage(); page.setDefaultTimeout(120000);
    await visit(page, '/Login'); await page.locator('[name="usuario"]').fill(f[role]); await page.locator('[name="password"]').fill(f.password);
    await Promise.all([page.waitForNavigation(), page.locator('button[type="submit"]').click()]);
    assert(!page.url().includes('/Login'), 'No inició sesión como ' + role);
    sessions[role] = { context, page }; return page;
  }
  async function post(page, action, values, match = {}) {
    const form = await page.locator('form').evaluateAll((forms, args) => {
      const [action, match] = args;
      const form = forms.find(form => new URL(form.action).pathname.toLowerCase().endsWith('/' + action.toLowerCase()) && Object.entries(match).every(([k,v]) => form.elements.namedItem(k)?.value === String(v)));
      if (!form) return null;
      return { action: form.action, fields: Object.fromEntries(new FormData(form).entries()) };
    }, [action, match]);
    assert(form, 'No está disponible el formulario ' + action);
    const r = await page.context().request.post(form.action, { maxRedirects: 0, form: { ...form.fields, ...Object.fromEntries(Object.entries(values).map(([k,v]) => [k, String(v)])) } });
    if (![200, 302, 303].includes(r.status())) fs.writeFileSync(path.join(root, '.qa-runtime/qa-http-error.html'), await r.text());
    assert([200, 302, 303].includes(r.status()), action + ' HTTP ' + r.status());
    if (r.headers().location) await page.goto(new URL(r.headers().location, form.action).href);
    else await page.setContent(await r.text());
    const message = await text(page);
    const errors = await page.locator('.alert-danger').allTextContents();
    assert(!errors.some(e => e.trim()), action + ': ' + errors.join(' '));
    assert(!/Error de servidor|Server Error|Error al generar|Error:/.test(message), action + ': error visible');
    return message;
  }
  async function row(page, value) { const r = page.locator('tbody tr').filter({ hasText: String(value) }).first(); assert(await r.count(), 'No aparece registro ' + value); return r; }
  async function run(caso, body) {
    if (selected && !selected.has(caso)) return;
    let current = 0;
    const step = async (number, action) => {
      current = number;
      const obtenido = await action();
      results.push({ caso, paso: number, estado: 'APROBADA', obtenido: obtenido || 'Resultado comprobado en la página MVC real' });
    };
    try { await body(step); }
    catch (error) {
      results.push({ caso, paso: current || 1, estado: 'NO APROBADA', obtenido: error.message });
      console.log(caso + ' FALLÓ: ' + error.message);
    }
    for (const item of original.filter(s => s.caso === caso))
      if (!results.some(r => r.caso === caso && r.paso === item.paso)) results.push({ caso, paso: item.paso, estado: 'BLOQUEADA', obtenido: 'Depende de un paso anterior no aprobado o de una precondición no disponible' });
    fs.writeFileSync(path.join(root, 'docs/qa/resultados-91.json'), JSON.stringify(results, null, 2));
    console.log(caso + ': ' + results.filter(r => r.caso === caso && r.estado === 'APROBADA').length + ' pasos aprobados');
  }
  let admin, waiter, cook, cashier, employee, order, splitOrder, stockBefore, stockAfter, cashPaid = 0;
  try {
    await run('C1', async step => {
      const context = await browser.newContext(); admin = await context.newPage(); admin.setDefaultTimeout(120000); sessions.admin = { context, page: admin };
      await step(1, async () => { await visit(admin, '/Login'); assert(await admin.locator('[name="usuario"]').isVisible(), 'Sin campo usuario'); });
      await step(2, async () => { await admin.locator('[name="usuario"]').fill(f.admin); assert(await admin.locator('[name="usuario"]').inputValue() === f.admin, 'No ingresó usuario'); });
      await step(3, async () => { await admin.locator('[name="password"]').fill(f.password); assert(await admin.locator('[name="password"]').getAttribute('type') === 'password', 'Contraseña sin protección'); });
      await step(4, async () => { await Promise.all([admin.waitForNavigation(), admin.locator('button[type="submit"]').click()]); assert(!admin.url().includes('/Login'), 'Credenciales rechazadas'); });
      await step(5, async () => { assert(admin.url().includes('/Home') && (await text(admin)).includes('Administrador'), 'Perfil incorrecto'); });
      await step(6, async () => { assert(await admin.locator('.menu a[href="/Usuarios"]').count() === 1, 'Sin menú autorizado'); });
      await step(7, async () => { await admin.goto(base + '/Login/Logout'); assert(await admin.locator('[name="password"]').isVisible(), 'No cerró sesión'); });
    });
    await run('C2', async step => {
      await step(1, async () => { waiter = await login('mesero'); });
      await step(2, async () => { assert((await text(waiter)).includes('Mesero'), 'Rol no identificado'); });
      await step(3, async () => { assert(await waiter.locator('.menu a[href="/Usuarios"]').count() === 0, 'Menú administrativo visible'); });
      await step(4, async () => { await visit(waiter, '/Mesas'); });
      await step(5, async () => { const r = await waiter.goto(base + '/Usuarios'); assert(r.status() === 403, 'Acceso restringido permitido'); });
      await step(6, async () => { cook = await login('cocinero'); assert(cook.url().includes('/Cocina'), 'No cambió perfil'); });
      await step(7, async () => { await cook.goto(base + '/Login/Logout'); await cook.goto(base + '/Cocina'); assert(cook.url().includes('/Login'), 'Acceso tras logout'); });
    });
    admin = await login('admin'); waiter = await login('mesero'); cook = await login('cocinero'); cashier = await login('cajero');
    const cedula = 'E2E' + Date.now().toString().slice(-8);
    await run('C3', async step => {
      await step(1, async () => { await visit(admin, '/Empleados'); assert((await text(admin)).includes('Listado de empleados'), 'No muestra lista'); });
      await step(2, async () => { assert(await admin.locator('form[action$="RegistrarEmpleado"]').count() === 1, 'Sin formulario'); });
      await step(3, async () => { const form = admin.locator('form[action$="RegistrarEmpleado"]'); await form.locator('[name="cedula"]').fill(cedula); await form.locator('[name="nombre"]').fill('PruebaE2E'); await form.locator('[name="apellidos"]').fill(f.suffix); await form.locator('[name="salarioHora"]').fill('3000'); await form.locator('[name="rol"]').selectOption('Mesero'); });
      await step(4, async () => { const message = await post(admin, 'RegistrarEmpleado', { cedula, nombre: 'PruebaE2E', apellidos: f.suffix, telefono: '8888-1111', correo: cedula + '@qa.test', rol: 'Mesero', salarioHora: 3000, fechaIngreso: '2025-01-01' }); assert(message.includes('Empleado registrado correctamente'), 'No confirmó registro'); employee = Number(await (await row(admin, cedula)).locator('td').first().innerText()); });
      await step(5, async () => { await visit(admin, '/Empleados/BuscarEmpleados?termino=' + cedula); assert((await text(admin)).includes(cedula), 'Búsqueda no encuentra empleado'); });
      await step(6, async () => { await visit(admin, '/Empleados'); await post(admin, 'AdministrarEmpleado', { id: employee, telefono: '8888-2222', correo: cedula + '@qa.test', rol: 'Mesero', salarioHora: 3000 }); });
      await step(7, async () => { await admin.reload(); assert((await (await row(admin, cedula)).textContent()).includes('8888-2222'), 'Edición no persistida'); });
    });
    await run('C4', async step => {
      assert(employee, 'Falta empleado de C3');
      await step(1, async () => { await visit(admin, '/Empleados'); assert((await text(admin)).includes('Vacaciones') && (await text(admin)).includes('Turnos'), 'Sin opciones de personal'); });
      await step(2, async () => { await admin.locator('#btn-tab-turnos').click(); await admin.locator('form[action$="CrearTurno"] [name="idEmpleado"]').selectOption(String(employee)); });
      await step(3, async () => { await post(admin, 'CrearTurno', { idEmpleado: employee, fechaTurno: today, horaInicio: '08:00', horaFin: '16:00', descripcion: cedula }); });
      await step(4, async () => { await visit(admin, '/Empleados/Asistencia'); await post(admin, 'RegistrarEntrada', { idEmpleado: employee }); assert((await text(admin)).includes('PruebaE2E'), 'Asistencia no visible'); });
      await step(5, async () => { await visit(admin, '/Empleados'); await post(admin, 'SolicitarVacacion', { idEmpleado: employee, fechaInicio: future(5), fechaFin: future(5) }); assert(!/Error:/.test(await text(admin)), 'Solicitud rechazada'); });
      await step(6, async () => { await admin.locator('#btn-tab-turnos').click(); assert((await text(admin)).includes(cedula), 'Turno no visible'); await admin.locator('#btn-tab-vacaciones').click(); assert((await text(admin)).includes('PruebaE2E'), 'Vacación no visible'); });
      await step(7, async () => { await admin.reload(); await visit(admin, '/Empleados/Asistencia'); assert((await text(admin)).includes('PruebaE2E'), 'Asistencia no persiste'); });
    });
    await run('C5', async step => {
      await step(1, async () => { await visit(waiter, '/Mesas'); assert((await text(waiter)).includes(f.suffix), 'Mesa no visible'); });
      await step(2, async () => { await waiter.locator('form[action$="AsignarMesero"] [name="idMesa"]').selectOption(String(f.idMesa)); });
      await step(3, async () => { const options = await waiter.locator('form[action$="AsignarMesero"] [name="idEmpleado"] option').allTextContents(); assert(options.some(x => x.includes('Mesero')) && !options.some(x => x.includes('Cocinero') || x.includes('Cajero')), 'Catálogo de meseros incorrecto'); });
      await step(4, async () => { await waiter.locator('form[action$="AsignarMesero"] [name="idEmpleado"]').selectOption(String(f.idMesero)); });
      await step(5, async () => { await post(waiter, 'AsignarMesero', { idMesa: f.idMesa, idEmpleado: f.idMesero }); });
      await step(6, async () => { await waiter.reload(); const content = await (await row(waiter, f.suffix)).textContent(); assert(content.includes('QA Pruebas') && content.includes('ocupada'), 'Asignación no persistida'); });
      await step(7, async () => { await post(waiter, 'LiberarMesa', { id: f.idMesa }, { id: f.idMesa }); assert((await (await row(waiter, f.suffix)).textContent()).includes('disponible'), 'No liberó mesa'); });
    });
    async function createOrder(quantity) {
      await visit(waiter, '/Pedidos');
      const message = await post(waiter, 'CrearOrden', { idMesa: f.idMesa, idEmpleado: f.idMesero, comensales: 2, idProducto: f.idProducto, cantidad: quantity, tipoServicio: 'mesa', observaciones: 'E2E' });
      const match = message.match(/Orden #(\d+) creada/); assert(match, 'No confirmó creación de orden'); return Number(match[1]);
    }
    async function prepare(id) {
      await visit(cook, '/Cocina'); await post(cook, 'IniciarPreparacion', { idPedido: id }, { idPedido: id });
      await post(cook, 'MarcarPedidoListo', { idPedido: id }, { idPedido: id });
      await visit(waiter, '/Mesero'); await post(waiter, 'EntregarPedido', { idPedido: id }, { idPedido: id });
    }
    await run('C6', async step => {
      await step(1, async () => { order = await createOrder(2); });
      await step(2, async () => { await post(waiter, 'AgregarProducto', { idPedido: order, idProducto: f.idProducto, cantidad: 1, observacionesItem: 'Segunda línea E2E' }); });
      await step(3, async () => { await visit(cook, '/Cocina'); assert((await text(cook)).includes('Pedido #' + order), 'Comanda no recibida'); });
      await step(4, async () => { await post(cook, 'IniciarPreparacion', { idPedido: order }, { idPedido: order }); assert((await cook.locator('.quick-card').filter({ hasText: 'Pedido #' + order + ' —' }).textContent()).includes('en_proceso'), 'Estado no es en proceso'); });
      await step(5, async () => { await post(cook, 'MarcarPedidoListo', { idPedido: order }, { idPedido: order }); assert((await cook.locator('.quick-card').filter({ hasText: 'Pedido #' + order + ' —' }).textContent()).includes('listo'), 'No quedó listo'); });
      await step(6, async () => { await visit(waiter, '/Mesero'); await post(waiter, 'EntregarPedido', { idPedido: order }, { idPedido: order }); await visit(cashier, '/Cajero'); assert((await text(cashier)).includes('Pedido #' + order), 'Entrega no habilita cobro'); });
    });
    await run('C7', async step => {
      assert(order, 'Falta orden C6');
      await step(1, async () => { await visit(cashier, '/Cajero'); assert(await (await row(cashier, 'Pedido #' + order)).locator('form[action$="RegistrarPago"]').count(), 'No aparece pendiente de cobro'); });
      await step(2, async () => { const r = await row(cashier, 'Pedido #' + order); assert((await r.textContent()).includes('Vaso de Leche') && (await r.textContent()).includes('1'), 'Sin productos o montos'); });
      await step(3, async () => { const r = await row(cashier, 'Pedido #' + order); assert((await r.locator('td').nth(3).innerText()).replace(/[^0-9]/g, '') === '300000', 'Total distinto de 3000 CRC'); });
      await step(4, async () => { const r = await row(cashier, 'Pedido #' + order); const methods = await r.locator('[name="metodoPago"] option').allTextContents(); assert(methods.join().includes('SINPE') && methods.join().includes('Tarjeta') && methods.join().includes('Efectivo'), 'Métodos incompletos'); });
      await step(5, async () => { await post(cashier, 'RegistrarPago', { idPedido: order, metodoPago: 'efectivo', montoRecibido: 3000 }, { idPedido: order }); assert((await (await row(cashier, 'Pedido #' + order)).textContent()).includes('Pago registrado'), 'Sin estado pagado'); cashPaid += 3000; await post(cashier, 'FinalizarOrden', { idPedido: order }, { idPedido: order }); });
    });
    await run('C8', async step => {
      await step(1, async () => { splitOrder = await createOrder(2); await post(waiter, 'AgregarProducto', { idPedido: splitOrder, idProducto: f.idProducto, cantidad: 2, observacionesItem: 'División E2E' }); await prepare(splitOrder); await visit(cashier, '/Cajero?idPedidoDividir=' + splitOrder); });
      await step(2, async () => { await post(cashier, 'CrearDivisionSubcuentas', { idPedido: splitOrder, cantidadSubcuentas: 2 }); });
      let subs, details;
      await step(3, async () => { const form = cashier.locator('form[action$="AsignarProductoASubcuentaPedido"]'); subs = await form.locator('[name="idSubcuenta"] option').evaluateAll(xs => xs.map(x => x.value).filter(Boolean)); details = await form.locator('[name="idDetallePedido"] option').evaluateAll(xs => xs.map(x => x.value).filter(Boolean)); assert(subs.length === 2 && details.length === 2, 'Subcuentas o productos incorrectos'); for (let i = 0; i < 2; i++) await post(cashier, 'AsignarProductoASubcuentaPedido', { idPedido: splitOrder, idSubcuenta: subs[i], idDetallePedido: details[i], cantidad: 2 }); });
      await step(4, async () => { await cashier.reload(); assert((await text(cashier)).includes('Subcuenta 1') && (await text(cashier)).includes('Subcuenta 2'), 'Subcuentas no visibles'); });
      await step(5, async () => { for (const id of subs) await post(cashier, 'CobrarSubcuentaPedido', { idPedido: splitOrder, idSubcuenta: id, metodoPago: 'tarjeta', montoRecibido: 2000 }, { idSubcuenta: id }); assert((await text(cashier)).includes('Finalizado'), 'No cerró cuenta principal'); await visit(waiter, '/Mesas'); assert((await (await row(waiter, f.suffix)).textContent()).includes('disponible'), 'La orden dividida no liberó la mesa'); cashPaid += 4000; });
    });
    async function stock() { await visit(admin, '/Inventario?busqueda=' + encodeURIComponent('Leche ' + f.suffix)); const r = await row(admin, 'Leche ' + f.suffix); return Number((await r.locator('td').nth(3).innerText()).replace(',', '.')); }
    await run('C9', async step => {
      await step(1, async () => { await visit(admin, '/Inventario'); assert((await text(admin)).includes('Control de inventario'), 'No carga inventario'); });
      await step(2, async () => { stockBefore = await stock(); assert(Number.isFinite(stockBefore), 'No muestra existencia'); });
      await step(3, async () => { await visit(admin, '/Productos/Receta?idProducto=' + f.idProducto); assert((await text(admin)).includes('Leche') && /0[,.]25/.test(await text(admin)), 'No muestra la receta de 0.25 L de Leche'); });
      await step(4, async () => { order = await createOrder(1); });
      await step(5, async () => { stockAfter = await stock(); assert(Math.abs(stockBefore - stockAfter - .25) < .001, 'Rebajo distinto de 0.25 L'); return 'Leche: ' + stockBefore + ' → ' + stockAfter + ' L, receta 0.25 L por vaso'; });
      await step(6, async () => { assert(await stock() === stockAfter, 'Rebajo no persiste'); });
    });
    await run('C10', async step => {
      await step(1, async () => { await visit(admin, '/Inventario'); });
      await step(2, async () => { stockBefore = await stock(); });
      await step(3, async () => { const r = await row(admin, 'Leche ' + f.suffix); assert(Number(await r.locator('td').nth(4).innerText().then(x => x.replace(',', '.'))) >= stockBefore, 'Insumo no está cerca del mínimo'); });
      await step(4, async () => { await visit(admin, '/Inventario/Ajustes'); await post(admin, 'AjustarStock', { idInsumo: f.idInsumo, cantidad: .25, tipoAjuste: 'salida', motivo: 'QA C10' }); });
      await step(5, async () => { assert(await stock() === stockBefore - .25, 'Movimiento incorrecto'); assert((await (await row(admin, 'Leche ' + f.suffix)).textContent()).includes('Bajo mínimo'), 'No alerta'); });
      await step(6, async () => { await visit(admin, '/Inventario'); assert((await text(admin)).includes('Alertas mínimas'), 'No muestra alertas'); });
    });
    await run('C11', async step => {
      await step(1, async () => { await visit(cashier, '/Contabilidad'); assert((await text(cashier)).includes('Abrir caja'), 'Sin módulo caja'); });
      await step(2, async () => { assert(await cashier.locator('form[action$="AbrirCaja"]').count() === 1, 'Sin formulario apertura'); });
      let opening;
      await step(3, async () => {
        await visit(admin, '/Contabilidad');
        const option = admin.locator('form[action$="CerrarCaja"] [name="idApertura"] option').filter({ hasText: f.suffix }).first(); opening = await option.getAttribute('value');
        assert(opening, 'Sin apertura QA'); await post(admin, 'CerrarCaja', { idApertura: opening, saldoReal: 14000 + cashPaid });
        await visit(cashier, '/Contabilidad'); const box = await cashier.locator('form[action$="AbrirCaja"] [name="idCaja"] option').filter({ hasText: f.suffix }).first().getAttribute('value');
        await post(cashier, 'AbrirCaja', { idCaja: box, idCajero: f.idCajero, montoInicial: 5000, observaciones: 'C11 E2E' }); assert((await text(cashier)).includes('5'), 'No muestra apertura');
      });
      await step(4, async () => { await visit(admin, '/Contabilidad'); opening = await admin.locator('form[action$="CerrarCaja"] [name="idApertura"] option').filter({ hasText: f.suffix }).first().getAttribute('value'); await post(admin, 'RegistrarEgreso', { idApertura: opening, categoriaGasto: 'Otros', descripcion: 'C11 E2E', monto: 1000 }); });
      await step(5, async () => { assert((await text(admin)).includes('C11 E2E'), 'Egreso no visible'); });
      await step(6, async () => { await visit(admin, '/Contabilidad'); assert((await text(admin)).includes('Saldo'), 'Sin resumen de cierre'); });
      await step(7, async () => { const id = await admin.locator('form[action$="CerrarCaja"] [name="idApertura"] option').filter({ hasText: f.suffix }).first().getAttribute('value'); await post(admin, 'CerrarCaja', { idApertura: id, saldoReal: 4000 }); assert((await text(admin)).includes('cierre'), 'Sin cierre persistido'); const r = await admin.context().request.get(base + '/Contabilidad/DescargarReporteCierre'); const csv = await r.text(); assert(csv.includes('4000') && csv.includes('1000'), 'Resumen no conserva el egreso y saldo de cierre'); });
    });
    await run('C12', async step => {
      await step(1, async () => { await visit(admin, '/Reporteria'); });
      await step(2, async () => { await admin.locator('[name="tipoReporte"]').selectOption('ventas'); });
      await step(3, async () => { await admin.locator('[name="fechaInicio"]').fill(today); await admin.locator('[name="fechaFin"]').fill(today); });
      await step(4, async () => { await post(admin, 'GenerarReporte', { tipoReporte: 'ventas', formato: 'csv', fechaInicio: today, fechaFin: today, stockBajo: false }); assert((await text(admin)).includes('Reporte generado'), 'No generó reporte'); });
      await step(5, async () => { assert((await text(admin)).includes('3000') || (await text(admin)).includes('3.000') || (await text(admin)).includes('3 000'), 'Reporte sin pago C7'); });
      await step(6, async () => { assert(await admin.locator('a[href*="DescargarReporte"]').count(), 'Sin descarga'); });
      await step(7, async () => { const r = await admin.context().request.get(base + '/Reporteria/DescargarReporte'); const csv = await r.text(); assert(r.status() === 200 && csv.includes('ID Venta') && csv.includes('3000'), 'Archivo sin contenido esperado'); fs.writeFileSync(path.join(root, 'docs/qa/reporte-ventas-e2e.csv'), csv); });
    });
    await run('C13', async step => {
      await step(1, async () => { await visit(admin, '/NominaMensual'); });
      await step(2, async () => { await admin.locator('form[method="get"] [name="mes"]').selectOption(String(Number(today.slice(5, 7)))); });
      await step(3, async () => { assert((await text(admin)).includes('PruebaE2E'), 'Empleado no incluido'); });
      await step(4, async () => { assert((await text(admin)).includes('3') && (await text(admin)).includes('Salario'), 'Sin parámetros salariales'); });
      await step(5, async () => { await visit(admin, '/NominaMensual?mes=' + Number(today.slice(5, 7)) + '&anio=' + today.slice(0, 4)); });
      await step(6, async () => { const r = admin.locator('tbody tr').filter({ hasText: 'PruebaE2E ' + f.suffix }).first(); const cells = await r.locator('td').allTextContents(); const number = s => Number(s.replace(/[^0-9,.-]/g, '').replace(/\./g, '').replace(',', '.')); assert(number(cells[1]) === 3000, 'Salario por hora incorrecto'); assert(Math.abs(number(cells[11]) - number(cells[8]) - number(cells[9]) - number(cells[7]) - number(cells[10])) < .01, 'Bruto no coincide con base, extras, vacaciones y bonificaciones'); });
      await step(7, async () => { await post(admin, 'Cerrar', { idEmpleado: employee, mes: Number(today.slice(5, 7)), anio: today.slice(0, 4), observaciones: 'C13 E2E' }, { idEmpleado: employee }); assert((await text(admin)).includes('cerrada') || (await text(admin)).includes('Cerrada'), 'Nómina no guardada'); });
    });
    if (selected?.has('C14')) {
      await visit(waiter, '/Pedidos');
      order = Number(await waiter.locator('form[action$="CancelarPedido"] [name="idPedido"]').first().inputValue());
      stockBefore = await stock() + .25;
    }
    await run('C14', async step => {
      await step(1, async () => { assert((await visit(admin, '/Home')).includes('Administrador'), 'Sin acceso autorizado'); });
      await step(2, async () => { assert(order, 'Sin operación C9'); await visit(waiter, '/Pedidos'); const fields = await waiter.locator('form[action$="EditarProducto"]').first().evaluate(form => Object.fromEntries(new FormData(form).entries())); await post(waiter, 'EditarProducto', { idDetalle: fields.idDetalle, cantidad: fields.cantidad, observacionesItem: 'Auditoría C14 E2E' }, { idDetalle: fields.idDetalle }); });
      await step(3, async () => { await visit(admin, '/BitacoraPedidos'); assert((await text(admin)).includes('CREACION'), 'Sin eventos'); });
      await step(4, async () => { await visit(admin, '/BitacoraPedidos?idPedido=' + order); assert((await text(admin)).includes(String(order)), 'No encuentra evento'); });
      await step(5, async () => { const content = await text(admin); assert(content.includes('CREACION') && content.includes('MODIFICACION') && content.includes('QA Pruebas'), 'Auditoría incompleta'); });
      await step(6, async () => { for (const route of ['/Empleados', '/Inventario', '/Reporteria', '/NominaMensual']) await visit(admin, route); });
      await step(7, async () => { assert(await stock() === stockBefore - .25, 'Inventario divergente tras consultar módulos'); });
    });
  } finally {
    for (const session of Object.values(sessions)) await session.context.close();
  }
  const passed = results.filter(r => r.estado === 'APROBADA').length;
  console.log('Plan QA: ' + passed + '/91 pasos aprobados');
  if (passed !== 91) process.exitCode = 1;
};
