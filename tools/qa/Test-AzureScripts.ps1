$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$databaseName = 'Colibri_AzureValidacion_' + [Guid]::NewGuid().ToString('N')
$master = New-Object System.Data.SqlClient.SqlConnection 'Server=lpc:.\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Encrypt=False;Connect Timeout=5'
$connection = $null
$previousConnection = $env:COLIBRI_CONNECTION_STRING
try {
    $master.Open()
    $command = $master.CreateCommand()
    $command.CommandText = 'CREATE DATABASE [' + $databaseName + ']'
    [void]$command.ExecuteNonQuery()
    $connection = New-Object System.Data.SqlClient.SqlConnection ('Server=lpc:.\SQLEXPRESS;Initial Catalog=' + $databaseName + ';Integrated Security=True;Encrypt=False;MultipleActiveResultSets=True;Connect Timeout=5')
    $connection.Open()
    $snapshots = @()
    foreach ($run in 1..2) {
        foreach ($file in @('01_esquema_azure.sql', '02_datos_demo_pequenos.sql')) {
            $command = $connection.CreateCommand()
            $command.CommandTimeout = 120
            $command.CommandText = Get-Content -LiteralPath (Join-Path $root ('BD\Azure\' + $file)) -Raw
            [void]$command.ExecuteNonQuery()
        }
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT (SELECT COUNT(*) FROM sys.tables WHERE schema_id=SCHEMA_ID('dbo')) tablas, (SELECT COUNT(*) FROM Usuarios) usuarios, (SELECT COUNT(*) FROM Empleados) empleados, (SELECT COUNT(*) FROM Mesas) mesas, (SELECT COUNT(*) FROM Productos) productos, (SELECT COUNT(*) FROM Insumos) insumos, (SELECT COUNT(*) FROM Pedidos) pedidos, (SELECT COUNT(*) FROM Ventas) ventas, (SELECT COUNT(*) FROM Cierres_Caja) cierres, (SELECT SUM(stock_actual) FROM Insumos) stock;"
        $table = New-Object System.Data.DataTable
        $table.Load($command.ExecuteReader())
        $snapshot = ($table.Rows[0].ItemArray -join '|')
        $snapshots += $snapshot
        Write-Output "Ejecución ${run}: $snapshot"
        if ($table.Rows[0]['tablas'] -ne 42 -or $table.Rows[0]['pedidos'] -ne 2 -or $table.Rows[0]['usuarios'] -ne 4 -or $table.Rows[0]['ventas'] -ne 1) { throw 'Cantidades inesperadas en los ejemplos.' }
    }
    if ($snapshots[0] -ne $snapshots[1]) { throw 'La repetición cambió los datos o el stock.' }
    # Usa exactamente el verificador de los modelos EF de la aplicación.
    $runtime = Join-Path $root '.qa-runtime\azure-validacion'
    New-Item -ItemType Directory -Force $runtime | Out-Null
    Copy-Item -Path (Join-Path $root 'RestauranteVistas\bin\*.dll') -Destination $runtime -Force
    $compiler = Join-Path $root 'packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1\tools\RoslynLatest\csc.exe'
    $references = @('System.Data.dll', 'System.Core.dll', 'System.Web.dll', 'System.ComponentModel.DataAnnotations.dll')
    $references += Get-ChildItem $runtime -Filter '*.dll' | ForEach-Object { $_.FullName }
    $arguments = @('/nologo', '/target:exe', ('/out:' + (Join-Path $runtime 'CheckAzure.exe')))
    $arguments += $references | ForEach-Object { '/reference:' + $_ }
    $arguments += Join-Path $PSScriptRoot 'Check-Colibri.cs'
    & $compiler @arguments
    if ($LASTEXITCODE -ne 0) { throw 'No compiló el verificador EF.' }
    $env:COLIBRI_CONNECTION_STRING = $connection.ConnectionString
    & (Join-Path $runtime 'CheckAzure.exe') '--azure-validacion'
    if ($LASTEXITCODE -ne 0) { throw 'El esquema no coincide con los modelos EF.' }
    Write-Output 'APROBADO: instalación limpia, repetición sin duplicados y compatibilidad EF. No se conectó a Azure ni se modificó COLIBRI.'
} finally {
    $env:COLIBRI_CONNECTION_STRING = $previousConnection
    if ($connection) { $connection.Dispose() }
    [System.Data.SqlClient.SqlConnection]::ClearAllPools()
    # Solo se elimina la base única que este proceso acaba de crear.
    if ($master.State -eq 'Open' -and $databaseName -match '^Colibri_AzureValidacion_[a-f0-9]{32}$') {
        $cleanup = $master.CreateCommand()
        $cleanup.CommandText = 'IF DB_ID(N''' + $databaseName + ''') IS NOT NULL BEGIN ALTER DATABASE [' + $databaseName + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [' + $databaseName + ']; END'
        [void]$cleanup.ExecuteNonQuery()
    }
    $master.Dispose()
}
