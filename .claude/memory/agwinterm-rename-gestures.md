---
name: agwinterm-rename-gestures
description: F2 renames only the ACTIVE session, never the row under the pointer — a workspace row is renamed by double-click or right-click → Rename
metadata:
  type: project
---

**F2 cannot rename a workspace row.** The default binding is `("f2", "rename_session")` in
`Keymap.cs`, and `Program.Input.cs` dispatches it to `StartRename(_active)` — the active
*session*, which is never a workspace. The gesture that renames whatever row the pointer is over
is a **double-click**: `WM_LBUTTONDBLCLK` in `Program.WndProc.cs` calls
`StartRename(RowAt(my))`. **Right-click → Rename** on the row does the same thing.

The app is already honest about this: `BuildContextItems` puts the `"F2"` hint on a *session's*
Rename item and leaves the workspace's hint empty. No document claims otherwise either — the
mistake is purely in assuming the two gestures are interchangeable.

**Why:** F2 was handed to the user twice in one session as the way to rename a workspace row, and
they had to correct it both times. A wrong gesture is worse than no instruction, because it sends
them to a key that silently renames something else — the active session — if one happens to be
selected.

**How to apply:** When telling the user how to reach an inline rename, say double-click (or
right-click → Rename) unless the target is specifically the active session. See
[[agwinterm-session-title-route]] for how a name set this way reaches the row, and the global
`win32-native-control-over-direct2d` learning for the rename box's own painting and font traps.
