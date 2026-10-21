param([switch]$Pedido)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$runtime = Join-Path $root '.qa-runtime\check-colibri'
New-Item -ItemType Directory -Force $runtime | Out-Null
Copy-Item -Path (Join-Path $root 'RestauranteVistas\bin\*.dll') -Destination $runtime -Force
$compiler = Join-Path $root 'packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1\tools\RoslynLatest\csc.exe'
$references = @('System.Data.dll', 'System.Core.dll', 'System.Web.dll', 'System.ComponentModel.DataAnnotations.dll')
$references += Get-ChildItem $runtime -Filter '*.dll' | ForEach-Object { $_.FullName }
$arguments = @('/nologo', '/target:exe', ('/out:' + (Join-Path $runtime 'CheckColibri.exe')))
$arguments += $references | ForEach-Object { '/reference:' + $_ }
$arguments += Join-Path $PSScriptRoot 'Check-Colibri.cs'
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar la comprobación.' }
[xml]$config = Get-Content -LiteralPath (Join-Path $root 'RestauranteVistas\Web.config')
$previousConnection = $env:COLIBRI_CONNECTION_STRING
try {
    $env:COLIBRI_CONNECTION_STRING = ($config.configuration.connectionStrings.add | Where-Object name -eq 'ColibriDbContext').connectionString
    if ($Pedido.IsPresent) {
        & (Join-Path $runtime 'CheckColibri.exe') '--pedido' | Tee-Object -FilePath (Join-Path $runtime 'resultado.txt')
    } else {
        & (Join-Path $runtime 'CheckColibri.exe') | Tee-Object -FilePath (Join-Path $runtime 'resultado.txt')
    }
    if ($LASTEXITCODE -ne 0) { throw 'Hay mapeos incompatibles con COLIBRI.' }
} finally { $env:COLIBRI_CONNECTION_STRING = $previousConnection }
