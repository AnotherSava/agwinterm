## What's missing

`Ctrl+Insert` (copy) and `Shift+Insert` (paste) do nothing in agwinterm. Both are long-standing
Windows conventions: Windows Terminal binds them to `copy`/`paste` in its shipped defaults, standard
Win32 edit controls handle them natively, and the same pair has meant copy/paste in X11 terminals for
decades. agwinterm implements `Ctrl+C` / `Ctrl+Shift+C` / `Ctrl+V` / `Ctrl+Shift+V` but not these.

On its own that would be a missing default. What makes it a dead end is the second half:

## There is no user-side workaround

The keymap grammar cannot express these chords, so a user who notices the gap cannot close it.

- `Keymap.IsKey` (`src/Agwinterm.Win32/Keymap.cs`, ~line 218) accepts only a single letter or digit,
  `f1`–`f12`, `tab enter escape space up down left right`, and OEM punctuation (`comma period slash
  semicolon quote backtick minus equals lbracket rbracket backslash`). `insert` is not a valid token,
  so `map ctrl+insert = copy_selection` is rejected by `Canonicalize` with a "bad chord" diagnostic.
- `Keymap.KeyToken` (~line 236) has the mirror-image hole: it maps letters, digits, F-keys, the four
  arrows, Tab/Enter/Escape/Space and the OEM punctuation VKs, and returns null for everything else.
  So `ChordFor` yields null on a real `VK_INSERT` (0x2D) keypress, and the chord lookup in
  `Program.Input.cs` (~line 1031) can never match a binding for it even if one could be written.

The actions themselves already exist — `copy_selection` and `paste` are both in
`Keymap.ValidActions`. Only the key tokens are missing.

## The whole navigation-key group is affected

`Insert`, `Delete`, `Home`, `End`, `PageUp` and `PageDown` are all absent from `IsKey` and
`KeyToken`, for the same reason. Where behaviour on those keys was needed it was hardcoded ahead of
the chord lookup instead — `Shift+PageUp`/`PageDown`/`Home`/`End` scroll the scrollback
(`Program.Input.cs` ~lines 976-987). So the group is reachable ad hoc but never through keymap.conf.

## Expected

`Ctrl+Insert` copies the selection and `Shift+Insert` pastes, out of the box, with no configuration —
matching Windows Terminal, conhost, and the platform convention.

## Suggested fix

Two independent parts, either useful alone:

1. **Make the navigation keys bindable.** Add `insert delete home end pageup pagedown` to `IsKey`,
   and the matching VKs to `KeyToken` (`VK_INSERT` 0x2D, `VK_DELETE` 0x2E, `VK_HOME` 0x24,
   `VK_END` 0x23, `VK_PRIOR` 0x21, `VK_NEXT` 0x22 — the last four are already defined in `Win32.cs`).
   The existing hardcoded scrollback behaviour would stay as the default; making the tokens
   expressible just means a user can override it.
2. **Ship the two defaults**, in `Keymap.DefaultBindings`:
   `("ctrl+insert", "copy_selection")` and `("shift+insert", "paste")`.

## Note

`docs/windows-terminal-gap-analysis.md` inventories WT features and sorts them into "already at
parity" / backlog tiers / "probably not, by design". This pair appears in none of them, so it looks
like a gap in the audit rather than a considered decision to skip it.

## Environment

- agwinterm **0.17.3**, win-x64
- Windows 11 Pro 10.0.26200
