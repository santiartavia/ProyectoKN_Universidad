# Diagnóstico

Revisión reiniciada el 20 de octubre de 2026 sobre `main`, con el árbol de trabajo inicialmente limpio. Se conservaron C#, ASP.NET MVC 5.2.9, Razor, .NET Framework 4.8, Entity Framework 6.4.4 y SQL Server.

La solución tiene cuatro capas: `Abstracciones` (entidades y contratos), `AccesoADatos` (modelo EF y mapeos SQL), `LogicaDeNegocios` (servicios y reportes) y `RestauranteVistas` (controladores, sesiones, filtros y vistas). `BD` genera un DACPAC. Frontend y backend se sirven juntos mediante IIS Express; no hay una aplicación SPA ni una API independiente.

Los tres documentos están disponibles en la raíz del repositorio. `fuentes/` conserva todas las celdas pobladas de las ocho hojas de historias, las 16 hojas del Plan QA y las siete páginas del PDF. `02-trazabilidad.md` registra 380 criterios y requisitos: 50 historias funcionales y 35 requisitos no funcionales. `plan-91-pasos.json` conserva las precondiciones, acciones y resultados originales de los 14 casos.

Las tres copias del repositorio coinciden byte por byte con los archivos de `D:/Usuario/Downloads` indicados por el usuario. Los SHA-256 están en `fuentes/manifest.json`.

El Excel tiene aprobaciones históricas que no coinciden con la evaluación posterior del PDF (66/91 pasos y nueve defectos abiertos). No se usaron esas aprobaciones como evidencia nueva. También hay contradicciones internas: SPS 001 escenario 3 menciona una orden activa y luego una finalizada; C6 paso 4 mezcla «en proceso» con «terminado»; algunos escenarios de auditoría tienen títulos y resultados desplazados. Se preservó el texto original. La preparación se interpreta mediante SPS 002: `abierto` → `en_proceso` → `listo` → `entregado` → `finalizado`, con pago obligatorio antes de finalizar.

La compilación inicial pasó con MSBuild de Visual Studio. No se encontraron proyectos de pruebas existentes. El SDK `dotnet` por sí solo no incluye los targets de aplicaciones web clásicas. Hay SQL Server Express e IIS Express locales; la conexión predeterminada del proyecto apunta a otro equipo. Se agregó `COLIBRI_CONNECTION_STRING` para configurar el entorno sin publicar credenciales. Las pruebas usan exclusivamente `Colibri_QA_20261020`.

Problemas comprobados en el código inicial:

- Credenciales de primer ingreso predecibles y fallback reversible Base64; la autorización solo revisaba valores de sesión.
- El constructor del contexto alteraba tablas, quitaba una restricción e inventaba perfiles de empleado y una caja, ocultando los errores.
- La creación de una orden guardaba primero la cabecera y después sus detalles, inventario y auditoría; podía dejar registros parciales.
- Las asignaciones de meseros se reconstruían desde texto libre de bitácora; no había una relación persistente en la mesa.
- El inventario truncaba las salidas a cero en vez de rechazar existencias insuficientes.
- Caja y PDV podían abrir automáticamente una caja con un monto fijo y usar el primer empleado disponible.
- Los cobros normales no conservaban el detalle de venta que necesita la reportería de productos.
- Existían transiciones que permitían reabrir órdenes y consultas de auditoría con agrupaciones difíciles de traducir por EF.
- Las operaciones POST no tenían protección CSRF consistente y no había una clave persistente para detectar reenvíos.

Las correcciones y la evidencia ejecutada se detallan en los siguientes documentos. La compilación, las pruebas de controladores y la navegación no demuestran por sí solas el cumplimiento individual de los 380 criterios ni disponibilidad durante una jornada completa.
