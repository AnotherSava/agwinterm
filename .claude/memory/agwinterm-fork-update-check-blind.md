---
name: agwinterm-fork-update-check-blind
description: The fork's fourth version part makes agwinterm's own update check report "already the latest" forever — deliberate, and it also hides real upstream releases
metadata:
  type: project
---

This clone's builds carry a fourth version component (`0.20.13.1`) to mark them as fork builds, and
that silently disables agwinterm's update check. `ClaudeUpdate.Segments` returns null for any version
that does not split into exactly three numeric parts, and `IsNewer` answers false whenever either side
is null — so the comparison can never be true, and the palette's update entry reports the installed
version is already the latest however far upstream has moved.

**Why:** the fourth part exists so a release installer cannot overwrite a locally built fork, and the
blindness is the price rather than a bug. Nothing in the code says so, which is why it is written here:
the next person to read "0.20.13.1 is already the latest" will otherwise take it as a fact about
upstream.

**How to apply:** never read agwinterm's own update check as evidence about upstream in this clone.
Ask git instead — `git fetch upstream && git log --oneline local..upstream/main` — which is also what
`/pull` here does. If the check is ever wanted back, the fix is to compare only the first three
segments rather than to drop the fourth part, since dropping it reopens the overwrite it prevents.

Found by the tauri-dashboard session's review on 2026-10-02 and verified here against
`ClaudeUpdate.Segments`. The build and version machinery is in [[agwinterm-fork-and-build]].
