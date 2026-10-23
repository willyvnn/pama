# Security policy

Pama generates and handles secrets, so security reports are taken seriously.

## Supported versions

Only the latest release receives fixes.

## Reporting a vulnerability

Please do not open a public issue. Report privately through
[GitHub private vulnerability reporting](https://github.com/willyvnn/pama/security/advisories/new).

Include the Pama version, Windows version, steps to reproduce and the impact you expect.
You will get an acknowledgement within 7 days; a fix and advisory follow once the issue is confirmed.

## Scope

In scope: secret generation quality, clipboard handling (history, auto-clear), settings storage,
screen-capture protection, process hardening, and the release pipeline (tampered or unattested exes).

Out of scope: attacks that already require administrator rights or code running as the same user,
and SmartScreen warnings on unsigned builds.
