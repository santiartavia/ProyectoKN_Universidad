const fs = require('fs');
const path = require('path');
const { chromium } = require('../../.qa-tools/browser/node_modules/playwright');
const root = path.resolve(__dirname, '../..');
async function main() {
  const browser = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', headless: true });
  const results = [], errors = [];
  try {
    const page = await browser.newPage({ viewport: { width: 1366, height: 900 } });
    page.setDefaultTimeout(60000);
    page.on('pageerror', error => errors.push(error.message));
    await page.goto('http://localhost:51235/Login');
    await page.locator('[name="usuario"]').fill(process.env.COLIBRI_TEST_USER || 'admin');
    await page.locator('[name="password"]').fill(process.env.COLIBRI_TEST_PASSWORD || 'Password1234.');
    await Promise.all([page.waitForNavigation(), page.locator('button[type="submit"]').click()]);
    if (new URL(page.url()).pathname.toLowerCase().startsWith('/login')) throw new Error('No inició sesión');
    for (const route of ['/Pedidos', '/Mesero', '/Cocina', '/Cajero', '/Mesas', '/Inventario']) {
      const response = await page.goto('http://localhost:51235' + route);
      if (response.status() !== 200 || new URL(page.url()).pathname !== route) throw new Error(route + ': navegación incorrecta');
      await page.locator('.sidebar').waitFor();
      results.push({ route, ok: true });
    }
    await page.goto('http://localhost:51235/Pedidos');
    await page.locator('form[action="/Pedidos/CrearOrden"] [name="cantidad"]').fill('1');
    await page.screenshot({ path: path.join(root, '.qa-runtime', 'pedidos-colibri.png'), fullPage: true });
    await page.setViewportSize({ width: 390, height: 844 });
    await page.locator('.navigation-toggle').click();
    if (!await page.locator('.sidebar').isVisible()) throw new Error('El menú móvil no abre');
    results.push({ route: 'Menú móvil', ok: true });
    if (errors.length) throw new Error('JavaScript: ' + errors.join('; '));
    await page.goto('http://localhost:51235/Login/Logout');
  } finally {
    fs.writeFileSync(path.join(root, '.qa-runtime', 'resultados-colibri-ui.json'), JSON.stringify({ results, errors }, null, 2));
    await browser.close();
  }
  console.log(results.length + ' comprobaciones de interfaz aprobadas; 0 errores JavaScript.');
}
main().catch(error => { console.error(error); process.exitCode = 1; });
