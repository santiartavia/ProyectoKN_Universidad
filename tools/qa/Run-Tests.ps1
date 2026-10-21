param([switch]$Integration, [switch]$Provision)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$runtime = Join-Path $root '.qa-runtime'
New-Item -ItemType Directory -Force $runtime | Out-Null
Copy-Item -Path (Join-Path $root 'RestauranteVistas\bin\*.dll') -Destination $runtime -Force
$compiler = Join-Path $root 'packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1\tools\RoslynLatest\csc.exe'
$references = @('System.Web.dll', 'System.Data.dll', 'System.Core.dll', 'System.Configuration.dll', 'System.ComponentModel.DataAnnotations.dll')
$references += Get-ChildItem $runtime -Filter '*.dll' | ForEach-Object { $_.FullName }
$arguments = @('/nologo', '/target:exe', ('/out:' + (Join-Path $runtime 'ColibriQA.exe')))
$arguments += $references | ForEach-Object { '/reference:' + $_ }
$arguments += Join-Path $PSScriptRoot 'Tests.cs'
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar el ejecutor QA.' }
$env:COLIBRI_CONNECTION_STRING = 'Server=lpc:.\SQLEXPRESS;Initial Catalog=Colibri_QA_20261020;Integrated Security=True;Encrypt=False;MultipleActiveResultSets=True;Connect Timeout=5'
$mode = if ($Provision) { '--provision' } elseif ($Integration) { '--integration' } else { '--unit' }
& (Join-Path $runtime 'ColibriQA.exe') $mode | Tee-Object -FilePath (Join-Path $root ('docs\qa\resultado' + $mode + '.txt'))
if ($LASTEXITCODE -ne 0) { throw 'Hay pruebas fallidas; consulte docs/qa/resultado*.txt.' }
