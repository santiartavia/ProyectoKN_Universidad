$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$runtime = Join-Path $root '.qa-runtime'
New-Item -ItemType Directory -Force $runtime | Out-Null
$env:COLIBRI_CONNECTION_STRING = 'Server=lpc:.\SQLEXPRESS;Initial Catalog=Colibri_QA_20261020;Integrated Security=True;Encrypt=False;MultipleActiveResultSets=True;Connect Timeout=5'
$process = Start-Process -FilePath 'C:\Program Files\IIS Express\iisexpress.exe' -ArgumentList @('/path:"' + (Join-Path $root 'RestauranteVistas') + '"', '/port:51235', '/systray:false') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtime 'iis.log') -RedirectStandardError (Join-Path $runtime 'iis-error.log')
$process.Id | Set-Content (Join-Path $runtime 'iis.pid')
Write-Output ('IIS Express QA: http://localhost:51235, PID ' + $process.Id)
