# -----------------------------------------------------------------------
# LOCAL DEVELOPMENT ONLY — do not use these credentials in any deployed
# environment. The hardcoded passwords are intentional for local
# developer convenience only.
# -----------------------------------------------------------------------
param(
    [string] $PgsqlDir = $(if ($env:PGSQL_DIR) { $env:PGSQL_DIR } else { 'C:\tools\pgsql' })
)

$ErrorActionPreference = 'Stop'

$pgBin    = Join-Path $PgsqlDir 'bin'
$pgData   = Join-Path $PgsqlDir 'data'
$pgLog    = Join-Path $PgsqlDir 'postgres.log'
$pgCtl    = Join-Path $pgBin 'pg_ctl.exe'
$initDb   = Join-Path $pgBin 'initdb.exe'
$psql     = Join-Path $pgBin 'psql.exe'
$createDb = Join-Path $pgBin 'createdb.exe'

if (-not (Test-Path $pgCtl)) {
    Write-Error ("PostgreSQL binaries not found at '{0}'.`nDownload from https://www.enterprisedb.com/download-postgresql-binaries (Windows x86-64, v16.x) and extract to {0}" -f $PgsqlDir)
    exit 1
}

if (-not (Test-Path $pgData)) {
    Write-Host 'Initialising PostgreSQL data directory...'
    $pwFile = Join-Path $env:TEMP 'pg_pwfile_clinicmate.txt'
    'postgres' | Set-Content -Encoding ASCII $pwFile
    & $initDb -D $pgData -U postgres --auth=md5 --pwfile=$pwFile -E UTF8 | Out-Null
    Remove-Item $pwFile -ErrorAction SilentlyContinue
    if ($LASTEXITCODE -ne 0) { Write-Error 'initdb failed.'; exit 1 }
    Write-Host 'Data directory initialised.'
}

$status = & $pgCtl status -D $pgData 2>&1
if ($status -match 'server is running') {
    Write-Host 'PostgreSQL is already running -- nothing to do.'
} else {
    Write-Host 'Starting PostgreSQL...'
    & $pgCtl start -D $pgData -l $pgLog
    if ($LASTEXITCODE -ne 0) { Write-Error ('pg_ctl start failed. Check log: {0}' -f $pgLog); exit 1 }
    Start-Sleep -Seconds 2
    Write-Host ('PostgreSQL started. Log: {0}' -f $pgLog)
}

$env:PGPASSWORD = 'postgres'

# Create or update the clinicmate role (always ensures correct password)
Write-Host "Ensuring role 'clinicmate' with correct password..."
$sqlFile = Join-Path $env:TEMP 'clinicmate_role.sql'
@'
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'clinicmate') THEN
    ALTER ROLE clinicmate WITH LOGIN PASSWORD 'clinicmate_dev' CREATEDB;
  ELSE
    CREATE ROLE clinicmate WITH LOGIN PASSWORD 'clinicmate_dev' CREATEDB;
  END IF;
END
$$;
'@ | Set-Content -Encoding UTF8 $sqlFile
& $psql -U postgres -h localhost -p 5432 -f $sqlFile | Out-Null
Remove-Item $sqlFile -ErrorAction SilentlyContinue
if ($LASTEXITCODE -ne 0) { Write-Error 'Failed to create/update role clinicmate.'; exit 1 }
Write-Host "Role 'clinicmate' ready."

# Create the database if it does not exist
$dbExists = & $psql -U postgres -h localhost -p 5432 -tAc "SELECT 1 FROM pg_database WHERE datname='clinicmateai_dev';" 2>&1
if ($dbExists -notmatch '1') {
    Write-Host "Creating database 'clinicmateai_dev'..."
    & $createDb -U postgres -h localhost -p 5432 -O clinicmate clinicmateai_dev
    if ($LASTEXITCODE -ne 0) { Write-Error 'createdb failed.'; exit 1 }
    Write-Host "Database 'clinicmateai_dev' created."
} else {
    Write-Host "Database 'clinicmateai_dev' already exists."
}

Write-Host ''
Write-Host 'Ready. Connection string:'
Write-Host '  Host=localhost;Port=5432;Database=clinicmateai_dev;Username=clinicmate;Password=clinicmate_dev'
Write-Host ''
Write-Host 'Apply migrations:'
Write-Host '  dotnet ef database update --project src\ClinicMateAI.Infrastructure --startup-project src\ClinicMateAI.Web'
