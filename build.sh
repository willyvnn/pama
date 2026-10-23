#!/usr/bin/env bash
# Builds the Windows release (full + slim) and writes SHA256SUMS next to the exes
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
PROJECT="$ROOT/src/Pama.Desktop"
OUT="$ROOT/artifacts"

dotnet test "$ROOT" -c Release

# Self-contained: runs without any .NET runtime installed
dotnet publish "$PROJECT" -p:PublishProfile=win-x64

# Framework-dependent: needs .NET 10 Desktop Runtime on the target machine
dotnet publish "$PROJECT" -c Release -r win-x64 -p:SelfContained=false \
  -p:PublishSingleFile=true -p:DebugType=none -o "$OUT/win-x64-slim"

mv -f "$OUT/win-x64-slim/Pama.exe" "$OUT/win-x64-slim/Pama-slim.exe"

cd "$OUT"
sha256sum win-x64/Pama.exe win-x64-slim/Pama-slim.exe > SHA256SUMS
cat SHA256SUMS
