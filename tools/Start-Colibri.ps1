$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$runtime = Join-Path $root '.qa-runtime'
New-Item -ItemType Directory -Force $runtime | Out-Null
[xml]$config = Get-Content -LiteralPath (Join-Path $root 'RestauranteVistas\Web.config')
$connectionString = ($config.configuration.connectionStrings.add | Where-Object name -eq 'ColibriDbContext').connectionString
$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = 'SELECT DB_NAME()'
    if ($command.ExecuteScalar() -ne 'COLIBRI') { throw 'La conexión debe apuntar a COLIBRI.' }
} finally { $connection.Dispose() }

# Reinicia solamente el IIS Express de este proyecto en el puerto de la aplicación.
foreach ($server in (Get-CimInstance Win32_Process -Filter "Name = 'iisexpress.exe'" -ErrorAction Stop)) {
    if ($server.CommandLine -and $server.CommandLine.Contains($root) -and $server.CommandLine.Contains('/port:51235')) {
        Stop-Process -Id $server.ProcessId -ErrorAction Stop
        Wait-Process -Id $server.ProcessId -Timeout 10 -ErrorAction SilentlyContinue
    }
}
$previousConnection = $env:COLIBRI_CONNECTION_STRING
try {
    Remove-Item Env:COLIBRI_CONNECTION_STRING -ErrorAction SilentlyContinue
    $process = Start-Process -FilePath 'C:\Program Files\IIS Express\iisexpress.exe' -ArgumentList @('/path:"' + (Join-Path $root 'RestauranteVistas') + '"', '/port:51235', '/systray:false') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtime 'colibri.log') -RedirectStandardError (Join-Path $runtime 'colibri-error.log')
    Start-Sleep -Milliseconds 700
    $process.Refresh()
    if ($process.HasExited) { throw ('IIS Express no pudo iniciar: ' + (Get-Content -LiteralPath (Join-Path $runtime 'colibri-error.log') -Raw)) }
    $process.Id | Set-Content -LiteralPath (Join-Path $runtime 'colibri.pid')
} finally { $env:COLIBRI_CONNECTION_STRING = $previousConnection }
Write-Output "COLIBRI: http://localhost:51235 (PID $($process.Id)). Usa las cuentas existentes en COLIBRI."
