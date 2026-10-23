# Builds the Windows release (full + slim) and writes SHA256SUMS next to the exes
$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$project = Join-Path $root 'src/Pama.Desktop'
$out = Join-Path $root 'artifacts'

dotnet test $root -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Self-contained: runs without any .NET runtime installed
dotnet publish $project -p:PublishProfile=win-x64
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Framework-dependent: needs .NET 10 Desktop Runtime on the target machine
dotnet publish $project -c Release -r win-x64 -p:SelfContained=false `
  -p:PublishSingleFile=true -p:DebugType=none -o (Join-Path $out 'win-x64-slim')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Move-Item -Force (Join-Path $out 'win-x64-slim/Pama.exe') (Join-Path $out 'win-x64-slim/Pama-slim.exe')

# Same format as sha256sum so either side can verify with the other's tool
$lines = 'win-x64/Pama.exe', 'win-x64-slim/Pama-slim.exe' | ForEach-Object {
    "$((Get-FileHash (Join-Path $out $_) -Algorithm SHA256).Hash.ToLower())  $_"
}
$lines | Set-Content -Encoding ascii (Join-Path $out 'SHA256SUMS')
$lines
