const fs = require('fs');
const path = require('path');
const { chromium } = require('../../.qa-tools/browser/node_modules/playwright');
const root = path.resolve(__dirname, '../..');
const fixture = JSON.parse(fs.readFileSync(path.join(root, '.qa-runtime/fixture.json'), 'utf8'));
const results = [];
const base = 'http://localhost:51235';
function assert(condition, text) { if (!condition) throw new Error(text); }
async function main() {
  const browser = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', headless: true });
  if (process.argv.includes('--plan')) {
    try { await require('./browser-plan.cjs')(browser, fixture, root); }
    finally { await browser.close(); }
    return;
  }
  try {
    for (const role of ['admin', 'mesero', 'cocinero', 'cajero']) {
      const context = await browser.newContext({ viewport: { width: 1440, height: 1000 } });
      const page = await context.newPage();
      page.setDefaultTimeout(120000);
      const response = await page.goto(base + '/Login');
      assert(response.status() === 200, 'Pantalla login no disponible');
      await page.locator('[name="usuario"]').fill(fixture[role]);
      await page.locator('[name="password"]').fill(fixture.password);
      assert(await page.locator('[name="password"]').getAttribute('type') === 'password', 'Contraseña visible');
      await Promise.all([page.waitForNavigation(), page.locator('button[type="submit"]').click()]);
      assert(!page.url().includes('/Login'), 'No inició sesión como ' + role);
      results.push({ prueba: 'Inicio de sesión ' + role, estado: 'APROBADA', url: page.url() });
      if (process.argv.includes('--inspect')) {
        const forms = {};
        for (const route of ['Empleados', 'Contabilidad', 'Reporteria', 'NominaMensual', 'Inventario', 'Productos', 'Cajero']) {
          await page.goto(base + '/' + route);
          forms[route] = await page.locator('form').evaluateAll(nodes => nodes.map(form => ({
            action: form.action, fields: Array.from(form.elements).filter(e => e.name && !/password|token|requestKey/i.test(e.name)).map(e => ({ name: e.name, type: e.type, value: e.value, options: e.options ? Array.from(e.options).slice(0, 8).map(o => ({ value: o.value, text: o.text })) : undefined }))
          })));
        }
        fs.writeFileSync(path.join(root, '.qa-runtime/forms.json'), JSON.stringify(forms, null, 2));
        break;
      }
      const routes = role === 'admin' ? [
        '/Home', '/Usuarios', '/Empleados', '/Empleados/Asistencia', '/Mesas', '/Pedidos', '/Mesero', '/Cocina', '/Cajero', '/Productos', '/Inventario', '/Contabilidad', '/Reporteria', '/NominaMensual', '/PuntoVenta', '/OperacionPdv', '/BitacoraPedidos', '/BitacoraPdv', '/Usuarios/BitacoraAcceso', '/Inventario/Bitacora', '/Contabilidad/Bitacora', '/Reporteria/BitacoraReportes'
      ] : role === 'mesero' ? ['/Mesas', '/Pedidos', '/Mesero', '/MiAsistencia', '/MisVacaciones'] : role === 'cocinero' ? ['/Cocina'] : ['/Cajero', '/PuntoVenta', '/OperacionPdv', '/Contabilidad'];
      for (const route of routes) {
        try {
          const result = await page.goto(base + route);
          const text = await page.locator('body').innerText();
          assert(result.status() === 200, 'HTTP ' + result.status());
          assert(!/Server Error|Error de servidor|Error de compilaci[oó]n|No fue posible consultar la bitácora|Aviso al consultar/i.test(text), 'La vista contiene un error');
          results.push({ prueba: role + ' ' + route, estado: 'APROBADA', url: page.url() });
        } catch (error) { results.push({ prueba: role + ' ' + route, estado: 'FALLIDA', obtenido: error.message }); }
      }
      if (role === 'mesero') {
        const denied = await page.goto(base + '/Usuarios');
        assert(denied.status() === 403, 'Mesero accede a usuarios');
        results.push({ prueba: 'C2 acceso directo restringido', estado: 'APROBADA', obtenido: 'HTTP 403' });
      }
      if (role === 'admin') {
        await page.goto(base + '/Home');
        await page.screenshot({ path: path.join(root, 'docs/qa/dashboard-desktop.png'), fullPage: true });
        await page.setViewportSize({ width: 390, height: 844 });
        await page.locator('.navigation-toggle').click();
        assert(await page.locator('.sidebar').isVisible(), 'Navegación móvil no abre');
        await page.screenshot({ path: path.join(root, 'docs/qa/navegacion-mobile.png'), fullPage: true });
        results.push({ prueba: 'Navegación móvil', estado: 'APROBADA' });
      }
      await page.goto(base + '/Login/Logout');
      const privatePage = await page.goto(base + routes[0]);
      assert(page.url().includes('/Login'), 'Sigue teniendo acceso tras cerrar sesión');
      results.push({ prueba: 'Cierre de sesión ' + role, estado: 'APROBADA' });
      await context.close();
    }
  } finally {
    await browser.close();
    fs.writeFileSync(path.join(root, process.argv.includes('--inspect') ? '.qa-runtime/resultado-inspeccion.json' : 'docs/qa/resultados-browser.json'), JSON.stringify(results, null, 2));
  }
  console.log(JSON.stringify(results, null, 2));
  if (results.some(r => r.estado === 'FALLIDA')) process.exitCode = 1;
}
main().catch(error => { console.error(error); fs.writeFileSync(path.join(root, 'docs/qa/resultados-browser.json'), JSON.stringify([...results, { prueba: 'Ejecución navegador', estado: 'BLOQUEADA', obtenido: error.message }], null, 2)); process.exitCode = 1; });
