# Ejecución

## Aplicación con la base existente COLIBRI

```powershell
.\tools\Start-Colibri.ps1
```

Abra http://localhost:51235 e ingrese con una cuenta existente en COLIBRI y su contraseña habitual. Web.config apunta a la instancia local SQLEXPRESS y a COLIBRI. El lanzador limpia la variable de conexión QA para el proceso del servidor y reinicia únicamente el IIS Express de este proyecto. No instala, migra, borra ni carga registros.

Las asignaciones de mesa se guardan en Bitacora_Usuarios_RRHH y el control de solicitudes repetidas en Bitacora_Acceso. No requieren id_empleado_asignado ni Solicitudes_Operacion. La división de pedidos usa Subcuentas_Pedido, Ventas y Detalle_Venta existentes. La división de ventas rápidas se oculta y rechaza si no existen sus dos tablas; no se crean automáticamente.

COLIBRI tampoco contiene Envios_PDV. Los envíos de ventas rápidas se ocultan y rechazan cuando falta esa tabla; los pedidos se registran desde Pedidos. Compruebe la compatibilidad en solo lectura con `.\tools\qa\Check-Colibri.ps1`.

## Pruebas aisladas

Requisitos: Windows, Visual Studio con desarrollo web ASP.NET y targeting pack .NET Framework 4.8; SQL Server; Python para extraer documentos; Node.js y Edge para las pruebas de navegador.

Desde PowerShell, en la raíz del repositorio:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' RestauranteVistas\RestauranteVistas.sln /restore /t:Build /p:Configuration=Debug /v:minimal /nologo
```

No ejecute scripts de instalación, migraciones ni semillas para arrancar COLIBRI. El script histórico BD/qa_integridad.sql no es necesario para esta configuración.

Para las pruebas aisladas se genera el esquema desde el modelo EF; no se borra ni reemplaza una base existente:

```powershell
.\tools\qa\Run-Tests.ps1 -Provision  # Una sola vez; solo Colibri_QA_20261020
.\tools\qa\Run-Tests.ps1
.\tools\qa\Run-Tests.ps1 -Integration
.\tools\qa\Start-Qa.ps1
```

`Start-Qa.ps1` sirve frontend y backend en `http://localhost:51235` mediante IIS Express y apunta únicamente a la base QA. El PID y los logs quedan en `.qa-runtime`. Los ensayos de integración crean usuarios y operaciones claramente identificados como QA; su fixture queda en esa carpeta ignorada por Git.

```powershell
npm install --prefix .qa-tools/browser playwright
node tools/qa/browser-smoke.cjs
node tools/qa/browser-smoke.cjs --plan
python tools/qa/build_traceability.py
python tools/qa/publish_results.py
```

El navegador usa Edge instalado, sin descargar otro navegador. El plan combina interacción visible y envío HTTP de los formularios reales con sus tokens CSRF y claves de solicitud. Verifica los resultados consultando páginas posteriores; no ejecuta SQL para simular esas operaciones.

Para volver a extraer la documentación y generar la matriz:

```powershell
python -m pip install --target .qa-tools openpyxl pypdf
python tools/qa/extract_documents.py
python tools/qa/build_traceability.py
```

Las pruebas deben ejecutarse de manera secuencial: el ejecutor copia ensamblados a `.qa-runtime` y los procesos en ejecución bloquean esos archivos. Ejecute el plan con un fixture recién generado por la integración para disponer de las precondiciones iniciales.

Para repetir únicamente C14 conservando los resultados previos, con la orden de inventario del plan todavía abierta: `node tools/qa/browser-smoke.cjs --plan --cases=C14`. El informe identifica el resultado como consolidado, no como una pasada única nueva.
