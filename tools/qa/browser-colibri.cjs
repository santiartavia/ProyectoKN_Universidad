const fs = require('fs');
const path = require('path');
const { request } = require('../../.qa-tools/browser/node_modules/playwright');
const root = path.resolve(__dirname, '../..');
const base = process.env.COLIBRI_TEST_URL || 'http://localhost:51235';
const role = process.env.COLIBRI_TEST_ROLE || 'admin';
const results = [];
async function main() {
  const context = await request.newContext({ baseURL: base, timeout: 60000 });
  async function visit(route) {
    let url = route;
    const chain = [];
    for (let i = 0; i < 8; i++) {
      const response = await context.get(url, { maxRedirects: 0 });
      chain.push({ url, status: response.status(), location: response.headers().location });
      if (response.status() >= 300 && response.status() < 400 && response.headers().location) {
        url = new URL(response.headers().location, new URL(url, base)).href;
        continue;
      }
      const html = await response.text();
      fs.writeFileSync(path.join(root, '.qa-runtime', 'pagina-' + route.replace(/\W/g, '_') + '.html'), html);
      const finalPath = new URL(response.url()).pathname.toLowerCase();
      const result = { route, status: response.status(), chain, ok: response.status() === 200 && !/Server Error|Error de servidor|Error de compilaci[oó]n|No se pudo mostrar esta página/.test(html) && !['/login', '/login/index'].includes(finalPath) };
      results.push(result); console.log(JSON.stringify(result));
      return;
    }
    results.push({ route, ok: false, chain, error: 'Bucle de redirecciones' });
    console.log(JSON.stringify(results[results.length - 1]));
  }
  try {
    const login = await context.get('/Login');
    const html = await login.text();
    const token = html.match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/);
    if (!token) throw new Error('No se encontró el formulario de acceso.');
    const response = await context.post('/Login/Entrar', { form: { __RequestVerificationToken: token[1], usuario: process.env.COLIBRI_TEST_USER || (role === 'admin' ? 'admin' : role + '_demo'), password: process.env.COLIBRI_TEST_PASSWORD || 'Password1234.' }, maxRedirects: 0 });
    console.log('Login HTTP ' + response.status() + ' ' + response.headers().location);
    const defaults = {
      mesero: ['/Mesero', '/Mesas', '/Pedidos', '/MiAsistencia', '/MisVacaciones', '/Login/CambiarPassword'],
      cajero: ['/Cajero', '/PuntoVenta', '/OperacionPdv', '/Contabilidad', '/MiAsistencia', '/MisVacaciones', '/Login/CambiarPassword'],
      cocinero: ['/Cocina', '/MiAsistencia', '/MisVacaciones', '/Login/CambiarPassword']
    };
    const routes = process.argv.length > 2 ? process.argv.slice(2) : (defaults[role] || []);
    for (const route of routes.length ? routes : ['/Home', '/Pedidos', '/Mesas', '/Mesero', '/Cocina', '/Cajero', '/PuntoVenta', '/Inventario', '/Productos', '/Empleados', '/Empleados/Asistencia', '/Empleados/MetricasMesas', '/Usuarios', '/Usuarios/BitacoraAcceso', '/MiAsistencia', '/MisVacaciones', '/Contabilidad', '/Contabilidad/Bitacora', '/Reporteria', '/Reporteria/BitacoraReportes', '/NominaMensual', '/OperacionPdv', '/BitacoraPedidos', '/BitacoraPdv', '/Inventario/Bitacora', '/Empleados/Bitacora', '/Login/CambiarPassword']) await visit(route);
    if (role !== 'admin') {
      const denied = await context.get('/Usuarios', { maxRedirects: 0 });
      results.push({ route: '/Usuarios acceso restringido', status: denied.status(), ok: denied.status() === 403 && !denied.headers().location });
    }
    await context.get('/Login/Logout');
    const signedOut = await context.get(role === 'admin' ? '/Home' : '/' + (role === 'cocinero' ? 'Cocina' : role === 'cajero' ? 'Cajero' : 'Mesero'), { maxRedirects: 0 });
    results.push({ route: 'Acceso tras cerrar sesión', status: signedOut.status(), ok: signedOut.status() === 302 && /Login/.test(signedOut.headers().location || '') });
  } finally {
    fs.writeFileSync(path.join(root, '.qa-runtime', 'resultados-colibri-web-' + role + '.json'), JSON.stringify(results, null, 2));
    await context.get('/Login/Logout').catch(() => {});
    await context.dispose();
  }
  if (results.some(r => !r.ok)) process.exitCode = 1;
}
main().catch(error => { console.error(error); process.exitCode = 1; });
