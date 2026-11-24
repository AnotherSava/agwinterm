---
created: 2026-11-22 06:27:08
platform: windows
---

# Enable Windows Sandbox so the UI suites can run as a pre-push check

Upstream #366 (d80bc30) added tests/integration/sandbox-ui.ps1, which stages the Release binaries, pwsh and the .NET runtime into a Windows Sandbox guest and runs one hud-ui.ps1 suite there. Suites: Hud, Quick, Navigation, Picker, Accessibility, Command, Menu — all of them skipped by .claude/commit-checks.sh because on the desktop they seize focus and the pointer.

Two things make it worth trying: sandbox-ui-guest.ps1 sets CI=true itself, so the suites need no maintainer suite-token helper, and the wrapper never sends input to the host, so the desktop stays usable during a run. Cost is 4 GB RAM and up to 900 s per suite, one suite per launch.

Blocker: Windows Sandbox is not installed on this machine (no WindowsSandbox.exe). Enabling it needs an admin prompt and probably a restart:

    Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All

Then open Windows Sandbox from Start once — on Windows 11 24H2 and later the client comes from the Microsoft Store, so enabling the Windows component alone may not produce a launcher.

After that, run one suite and judge whether it is fast and reliable enough to become a local pre-push check for the fork. docs/sandbox-ui-tests.md is upstream's write-up.
