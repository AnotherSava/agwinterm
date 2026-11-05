---
created: 2026-11-04 15:06:23
---

# Let the sidebar rename box clear a session's custom name on empty commit

Reported by the tauri-dashboard session (agwinterm 0.20.13.1 as installed), seen by the user on a live session.

Repro: rename a session to `1`, open rename again, delete the text, press Enter. The name stays `1`. Only `agwintermctl session rename --clear` resets it.

Cause: `CommitRename` in src/Agwinterm.Win32/Program.Chrome.cs trims the text and writes only inside `if (name.Length > 0)`, so an empty commit is a no-op for every kind of item. The control host's `SessionRename` (Program.ControlHost.cs) already has the clear path in its `name is null` branch: it nulls `CustomName` and sets `Name` to `session N`, with N recomputed from the session's index in `ses.Ws.Sessions`. The UI never reaches that branch.

Fix: on an empty commit for a `Ses`, run the same clear, extracted into one helper that both `CommitRename` and `SessionRename` call so they cannot drift. An empty commit for a workspace or window rename can keep the current no-op.
