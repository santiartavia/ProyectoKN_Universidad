"""Publish only observed results; do not infer acceptance from missing exceptions."""
import json
import re
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/qa'
plan = json.loads((OUT / 'plan-91-pasos.json').read_text(encoding='utf-8'))
raw = json.loads((OUT / 'resultados-91.json').read_text(encoding='utf-8'))
actual = {(r['caso'], r['paso']): r for r in raw}
assert len(actual) == len(raw) == 91, 'Deben existir 91 resultados únicos'
assert set(actual) == {(s['caso'], s['paso']) for s in plan}, 'Los resultados no corresponden al plan original'
browser = json.loads((OUT / 'resultados-browser.json').read_text(encoding='utf-8')) if (OUT / 'resultados-browser.json').exists() else []
def summary(name):
    file = OUT / name
    if not file.exists(): return 'BLOQUEADA: sin evidencia de ejecución'
    match = re.search(r'RESULTADO: (\d+) aprobadas, (\d+) fallidas', file.read_text(encoding='utf-8-sig'))
    return f'{match[1]} aprobadas, {match[2]} fallidas' if match else 'Consulte el registro: ' + name
passed = sum(r['estado'] == 'APROBADA' for r in raw)
accepted = sum(all(actual[(s['caso'], s['paso'])]['estado'] == 'APROBADA' for s in plan if s['caso'] == case) for case in {s['caso'] for s in plan})
def cell(s): return str(s).replace('|', '\\|').replace('\n', '<br>')
lines = ['# Resultados reales de pruebas', '',
    f'Plan QA: **{accepted}/14 casos aceptados; {passed}/91 pasos aprobados** en el resultado consolidado de la ejecución automatizada local.', '',
    'Se realizó una reprueba específica de C14 después de corregir la representación de cantidades en los campos HTML numéricos. '
    'Los demás resultados proceden de la pasada completa sobre el mismo fixture. No se modificaron los resultados esperados del Excel.', '',
    'La aceptación corresponde al fixture y al esquema de la base aislada `Colibri_QA_20261020`. '
    'No se certifica el cumplimiento de los 380 criterios individuales, el despliegue operativo ni los requisitos de disponibilidad, carga o respaldo.', '',
    '| Verificación | Resultado | Evidencia |', '|---|---|---|',
    '| Compilación | Solución compilada con MSBuild de Visual Studio | `compilacion.txt` |',
    f'| Unitarias y seguridad sin BD | {summary("resultado--unit.txt")} | `resultado--unit.txt` |',
    f'| Integración, seguridad y regresión SQL | {summary("resultado--integration.txt")} | `resultado--integration.txt` |',
    f'| Navegación y permisos en Edge | {sum(r["estado"] == "APROBADA" for r in browser)} aprobadas de {len(browser)} | `resultados-browser.json` |',
    f'| Plan original | {accepted}/14 casos, {passed}/91 pasos | `resultados-91.json` |', '',
    'Método: inicio de sesión mediante interacción del navegador; lectura de pantallas, selección de opciones, envío HTTP de los formularios reales con sus tokens CSRF y claves de idempotencia, y consultas posteriores para verificar persistencia. '
    'No todos los envíos se realizan haciendo clic en el botón: el ejecutor envía los campos del formulario mediante la sesión HTTP del navegador. '
    'Los ensayos de controladores verifican adicionalmente la base SQL, los totales, la reversión y los efectos entre módulos.', '',
    'Los datos «QA» y «E2E» son fixtures exclusivos de la base de pruebas; no se agregaron valores ficticios al dashboard ni a la base operativa.', '',
    '## Resumen por caso', '', '| Caso | Pasos aprobados | Resultado |', '|---|---|---|']
for case in sorted({s['caso'] for s in plan}, key=lambda x: int(x[1:])):
    selected = [s for s in plan if s['caso'] == case]
    total = sum(actual[(case, s['paso'])]['estado'] == 'APROBADA' for s in selected)
    lines.append(f'| {case} | {total}/{len(selected)} | {"ACEPTADO" if total == len(selected) else "NO ACEPTADO"} |')
lines += ['', '## Evidencia por paso', '']
for case in sorted({s['caso'] for s in plan}, key=lambda x: int(x[1:])):
    selected = [s for s in plan if s['caso'] == case]
    lines += ['### ' + case, '', 'Precondiciones originales: ' + cell(selected[0]['precondiciones']), '',
        '| Paso | Acción original | Esperado original | Obtenido | Estado |', '|---|---|---|---|---|']
    for step in selected:
        r = actual[(case, step['paso'])]
        lines.append('| ' + ' | '.join(cell(x) for x in [step['paso'], step['accion'], step['esperado'], r['obtenido'], r['estado']]) + ' |')
    lines.append('')
(OUT / '04-resultados-pruebas.md').write_text('\n'.join(lines), encoding='utf-8')
print(f'{accepted}/14 casos; {passed}/91 pasos; unitarias: {summary("resultado--unit.txt")}; integración: {summary("resultado--integration.txt")}')
