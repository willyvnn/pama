# Changelog

All notable changes to this project are documented here. The first `## [x.y.z]` heading sets the app version.

## [Unreleased]

### Fixed
- The What's new window shows released versions only, without the file header or unreleased notes.

## [0.1.0] - 2026-10-23

### Added
- Secret generator for Windows 10 and later: password, API key (hex, like `openssl rand -hex 32`), token (base64url) and PIN.
- Password options: length, lowercase, uppercase, digits, symbols, and excluding look-alike characters.
- Strength indicator showing the estimated entropy of each value.
- Eye button (Ctrl+H) masks or reveals the value; values are shown by default.
- Copy (Ctrl+C) with a 30-second countdown, after which the clipboard is cleared; auto-clear can be turned off.
- Preferences are saved to `%AppData%\Pama\settings.json`.
- Notification area icon: right-click to copy a new password, API key, token or PIN without opening the window, or to change settings.
- The close button keeps Pama running in the notification area (can be turned off); exit from the tray menu or with Ctrl+Q.
- Optional Start with Windows, starting straight to the notification area.
- Startup card with a progress bar showing each loading step.
- Compact layout with a light/dark theme that follows Windows, and an app icon.
- Version shown in the window; clicking it opens this changelog.
- Only one instance runs per user session; launching again brings the open window to the front.

### Security
- Copied values are kept out of Win+V clipboard history and cloud clipboard.
- The clipboard is only cleared if it still holds the value Pama copied, so newer content is never wiped.
- The window is hidden from screenshots, screen recording and screen sharing.
- DLLs are never loaded from the current directory.
