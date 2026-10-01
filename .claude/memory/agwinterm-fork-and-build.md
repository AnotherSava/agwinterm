---
name: agwinterm-fork-and-build
description: This clone is a patched fork — origin is the user's fork, upstream is yeroo — built and installed locally by a gitignored deploy wrapper
metadata:
  type: project
---

Since 2026-10-01 this clone carries local patches on a branch rather than in the working tree.
`origin` is the user's own fork, `upstream` is yeroo's repo, and the patches live on `local`.
So `/pull` here means fetching **upstream** and rebasing `local` onto it; a collision shows up
as a rebase conflict instead of a dirty tree. Anything proposed upstream is cherry-picked onto
a fresh topic branch off `upstream/main`, which is also why `.claude/` can be committed on
`local` without ever riding a pull request.

Three patches sit on that branch, each tracked by an upstream issue or memo — see the backlog in
`.claude/memos/`, one file per memo:

- drop `wsl` from the hardcoded restore denylist, so a `wsl`-wrapped launcher can be captured
- gate the left-button forward on `MouseReportsMotion` rather than `MouseReporting`, so an app
  that requested only DECSET 1000 leaves the drag to the terminal
- clear the selection when the wheel is forwarded to the app

**Building and installing is `bash scripts/deploy.sh`, and that wrapper is gitignored.** It is a
per-machine artifact covered by the global excludes, so a fresh clone has none and it has to be
written again. It reads the version from `installer/agwinterm.iss`, runs `installer/build.ps1`
(which needs Inno Setup 6 — ISCC — plus the .NET SDK and cargo), then hands the produced setup
to a **detached hidden PowerShell helper** so the install survives agwinterm being closed. That
matters because the silent setup closes agwinterm through Restart Manager, which would otherwise
take the invoking shell with it; the helper then relaunches the app only if it was already
running. It logs beside the setup exe under the gitignored output folder.

**`AppVersion` in `installer/agwinterm.iss` is the single source of the version** — the installer
filename, the Inno metadata and `-p:Version` for both publishes all come from it, and
`agwintermctl ping` reports what it stamped. A fourth component marks a fork build (upstream
releases carry three). A SemVer `+local` suffix does **not** work: Inno rejects it for
`VersionInfoVersion`, which must be numeric, even though MSBuild tolerates it.
