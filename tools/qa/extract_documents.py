"""Extract every populated cell and PDF page; preserve original QA results as history."""
import json
import sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / '.qa-tools'))
from openpyxl import load_workbook
from pypdf import PdfReader
out = ROOT / 'docs/qa/fuentes'
out.mkdir(parents=True, exist_ok=True)
for name in ['Historias de Usuario Proyecto (3).xlsx', 'G5_SC803_K_PlanQA_CasodePruebas (2).xlsx']:
    sheets = {}
    lines = []
    for sheet in load_workbook(ROOT / name, data_only=False):
        cells = {c.coordinate: str(c.value) for row in sheet for c in row if c.value is not None}
        sheets[sheet.title] = cells
        lines.append('\n## ' + sheet.title)
        lines.extend(f'{coord}: {value}' for coord, value in cells.items())
    (out / (name + '.json')).write_text(json.dumps(sheets, ensure_ascii=False, indent=2), encoding='utf-8')
    (out / (name + '.txt')).write_text('\n'.join(lines), encoding='utf-8')
    print(name, [(title, len(cells)) for title, cells in sheets.items()])
pages = PdfReader(ROOT / '005-Informe_de_Ejecucion_de_Pruebas_El_Colibri.pdf').pages
(out / 'informe.txt').write_text('\n'.join(f'\n## Página {i+1}\n{p.extract_text()}' for i, p in enumerate(pages)), encoding='utf-8')
print('PDF:', len(pages), 'páginas')
