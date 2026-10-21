"""Build a complete, source-addressable acceptance inventory and the exact QA steps."""
import json
import re
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/qa'
sources = OUT / 'fuentes'
stories = json.loads((sources / 'Historias de Usuario Proyecto (3).xlsx.json').read_text(encoding='utf-8'))
qa = json.loads((sources / 'G5_SC803_K_PlanQA_CasodePruebas (2).xlsx.json').read_text(encoding='utf-8'))
def cell(value):
    return str(value or '').replace('|', '\\|').replace('\r', '').replace('\n', '<br>')
modules = {
    'GES': ('Empleados', 'EmpleadoService, TurnoService, AsistenciaService, VacacionService, HoraExtraService', 'C3, C4, C13, C14'),
    'GUS': ('Usuarios, Login', 'UsuarioService, PasswordHelper, AutorizacionFilter', 'C1, C2, C14'),
    'SPS': ('Mesas, Pedidos, Cocina, Mesero, Cajero', 'OperacionController, InventarioService, PedidoService', 'C5, C6, C7, C14'),
    'INV': ('Inventario, Productos', 'InventarioService, ProductoService', 'C9, C10, C14'),
    'CNT': ('Contabilidad, Cajero', 'ContabilidadService, OperacionController', 'C7, C8, C11, C14'),
    'RPT': ('Reporteria', 'Reportes/ReporteService, Reportes/ExportadorReportes', 'C12, C14'),
    'PDV': ('PuntoVenta, OperacionPdv, BitacoraPdv', 'OperacionController, InventarioService', 'C7, C8, C14'),
}
lines = ['# Trazabilidad completa', '',
    'Fuente: las ocho hojas del Excel original, sin sustituir sus códigos por HU-01…HU-14. '
    'Los archivos indicados son los puntos de implementación que deben verificarse; su existencia no certifica el cumplimiento. '
    'Los ensayos automatizados específicos y sus resultados están en `04-resultados-pruebas.md` y `resultado--integration.txt`.', '',
    'Estado de esta matriz: inventario completo de criterios; validación individual pendiente salvo la evidencia específica del ejecutor QA. '
    'Cada contexto, evento y resultado se conserva textualmente. Las contradicciones documentales no se corrigen alterando el original.', '']
inventory = []
for title, cells in stories.items():
    lines += ['## ' + title, '']
    current = None
    for row in sorted({int(re.search(r'\d+', k)[0]) for k in cells}):
        code = cells.get(f'A{row}', '').strip()
        if re.fullmatch(r'(?:GES|GUS|SPS|INV|CNT|RPT|PDV)\s*\d+|RQNF-\d+', code):
            current = code
            if code.startswith('RQNF'):
                item = {'codigo': code, 'hoja': title, 'fila': row, 'tipo': cells.get(f'B{row}'), 'requerimiento': cells.get(f'C{row}'), 'solucion_documentada': cells.get(f'D{row}'), 'estado': 'PENDIENTE DE VERIFICACION'}
                inventory.append(item)
                lines += [f'### {code}', '', cell(item['requerimiento']), '', 'Solución documentada: ' + cell(item['solucion_documentada']), '', 'Verificación: pruebas de seguridad, integridad, rendimiento, disponibilidad o revisión de despliegue según corresponda; pendiente de certificación individual.', '']
                continue
            prefix = code.split()[0]
            controllers, services, cases = modules[prefix]
            lines += [f'### {code} — {cell(cells.get(f"B{row}"))}', '',
                f'Implementación: `RestauranteVistas/Controllers/` ({controllers}), `RestauranteVistas/Views/` y `LogicaDeNegocios/Services/` ({services}). Casos relacionados: {cases}.', '',
                '| Escenario / fila | Criterio | Contexto | Evento y validaciones / permisos | Resultado esperado | Estado |',
                '|---|---|---|---|---|---|']
        if current and not current.startswith('RQNF') and cells.get(f'C{row}') and cells.get(f'D{row}'):
            item = dict(codigo=current, hoja=title, fila=row, escenario=cells[f'C{row}'], criterio=cells[f'D{row}'], contexto=cells.get(f'E{row}', ''), evento=cells.get(f'F{row}', ''), resultado=cells.get(f'G{row}', ''), estado='PENDIENTE DE VERIFICACION')
            inventory.append(item)
            lines.append('| ' + ' | '.join(cell(x) for x in [str(item['escenario']) + ' / ' + str(row), item['criterio'], item['contexto'], item['evento'], item['resultado'], item['estado']]) + ' |')
    lines.append('')
(OUT / '02-trazabilidad.md').write_text('\n'.join(lines), encoding='utf-8')
(OUT / 'trazabilidad.json').write_text(json.dumps(inventory, ensure_ascii=False, indent=2), encoding='utf-8')
steps = []
actual_path = OUT / 'resultados-91.json'
actual = {(r['caso'], r['paso']): r for r in json.loads(actual_path.read_text(encoding='utf-8'))} if actual_path.exists() else {}
lines = ['# Plan QA: nueva ejecución', '',
    'Las columnas Estado y Resultados Obtenidos del Excel son antecedentes históricos. No se reutilizan como evidencia de esta ejecución. '
    'El PDF posterior registra 66/91 pasos aprobados y nueve defectos abiertos, aunque el Excel contiene aprobaciones anteriores.', '',
    'Los resultados de navegador y formularios HTTP reales se incorporan desde `resultados-91.json`. '
    'Las pruebas de controladores se presentan por separado; no se usan para aprobar automáticamente estos pasos.', '']
for name, cells in qa.items():
    if not re.fullmatch(r'C\d+', name):
        continue
    lines += ['## ' + name + ' — ' + cell(cells.get('C3')), '', 'Precondiciones: ' + cell(cells.get('C6')), '',
        '| Paso / fila | Acción original | Esperado original | Obtenido nuevo | Estado nuevo |', '|---|---|---|---|---|']
    expected_step = 1
    for row in sorted({int(re.search(r'\d+', k)[0]) for k in cells}):
        if cells.get(f'C{row}') == str(expected_step) and cells.get(f'D{row}') and cells.get(f'F{row}'):
            step = dict(caso=name, paso=int(cells[f'C{row}']), fila=row, precondiciones=cells.get('C6'), accion=cells[f'D{row}'], esperado=cells[f'F{row}'], obtenido='Sin ejecución del recorrido exacto de interfaz', estado='PENDIENTE E2E')
            if (name, step['paso']) in actual:
                step.update({k: actual[(name, step['paso'])][k] for k in ['obtenido', 'estado']})
            steps.append(step)
            expected_step += 1
            lines.append('| ' + ' | '.join(cell(x) for x in [str(step['paso']) + ' / ' + str(row), step['accion'], step['esperado'], step['obtenido'], step['estado']]) + ' |')
    lines.append('')
assert len(steps) == 91, f'El plan contiene {len(steps)} pasos, se esperaban 91'
(OUT / 'plan-91-pasos.md').write_text('\n'.join(lines), encoding='utf-8')
(OUT / 'plan-91-pasos.json').write_text(json.dumps(steps, ensure_ascii=False, indent=2), encoding='utf-8')
print(len(inventory), 'criterios y requisitos; historias:', len({i['codigo'] for i in inventory}), '; pasos QA:', len(steps))
