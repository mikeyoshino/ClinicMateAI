param(
    [string] $PgsqlDir = $(if ($env:PGSQL_DIR) { $env:PGSQL_DIR } else { 'C:\tools\pgsql' })
)

$ErrorActionPreference = 'Stop'

$pgCtl  = Join-Path $PgsqlDir 'bin\pg_ctl.exe'
$pgData = Join-Path $PgsqlDir 'data'

if (-not (Test-Path $pgCtl)) {
    Write-Error ("pg_ctl not found at '{0}'. Is PGSQL_DIR set correctly?" -f $pgCtl)
    exit 1
}

$status = & $pgCtl status -D $pgData 2>&1
if ($status -match 'no server running') {
    Write-Host 'PostgreSQL is not running -- nothing to do.'
    exit 0
}

Write-Host 'Stopping PostgreSQL...'
& $pgCtl stop -D $pgData -m fast

if ($LASTEXITCODE -ne 0) { Write-Error 'pg_ctl stop failed.'; exit 1 }

Write-Host 'PostgreSQL stopped.'
