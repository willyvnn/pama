# Pama

A password manager for Windows. The first release is a random secret generator: passwords, API keys (hex, like `openssl rand -hex 32`), base64url tokens and PINs.
Secrets are copied without entering Win+V history, the clipboard is cleared after 30 seconds, and the window is hidden from screen capture.

## Build

Requires the .NET 10 SDK. Builds on Linux or Windows; the output always targets Windows x64.

```bash
./build.sh        # Linux / macOS
.\build.ps1       # Windows
```

Both scripts run the tests, publish into `artifacts/win-x64/` and `artifacts/win-x64-slim/`, and write `artifacts/SHA256SUMS`.

Run the tests only:

```bash
dotnet test
```

## Run on Windows

Download from [Releases](https://github.com/willyvnn/pama/releases), or build the exes (above) and copy one to the Windows machine.

| File | Size | Needs |
|---|---|---|
| `Pama.exe` | ~62 MB | Windows 10 or later, nothing else |
| `Pama-slim.exe` | ~0.6 MB | Windows 10 or later + [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) |

Check the file is unchanged; the hash must match its line in `SHA256SUMS` (attached to each release, or in `artifacts/`):

```powershell
Get-FileHash .\Pama.exe -Algorithm SHA256
```

No installer: run the exe from any folder. Preferences live in `%AppData%\Pama\settings.json`; "Start with Windows" adds the value `Pama` under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.

## Release

Promote `[Unreleased]` in `CHANGELOG.md` to `## [x.y.z] - yyyy-mm-dd`, commit, then push a matching tag:

```bash
git tag v0.1.0 && git push origin v0.1.0
```

`.github/workflows/release.yml` checks the tag matches the CHANGELOG version, builds, attests the exes and publishes a GitHub release with the CHANGELOG section as notes. Verify an attestation with `gh attestation verify Pama.exe --repo willyvnn/pama`.

## Layout

| Path | Contents |
|---|---|
| `src/Pama.Core` | Generation and settings logic, no UI, cross-platform |
| `src/Pama.Desktop` | WPF app: window, tray icon, clipboard and Windows integration |
| `tests/Pama.Core.Tests` | xUnit tests for `Pama.Core` |
| `tools/make_icon.py` | Regenerates `Assets/pama.ico` |

## Versioning

The app version comes from the first `## [x.y.z]` heading in [CHANGELOG.md](CHANGELOG.md). Add changes under `[Unreleased]`; a release promotes them to a new version heading.
