---
created: 2026-08-17 20:30
---

# Uncomment ctrl+insert and shift+insert in keymap.conf for copy/paste

Ctrl+Insert / Shift+Insert do not copy/paste out of the box. **Corrected 2026-10-01: there is no upstream issue to file.** The original premise — "the keymap grammar can't express them" — was answered by commit #300: `Keymap.cs` now accepts `insert` and `delete` (aliases `ins`/`del`) as nameable keys, and ships `# map ctrl+insert = copy_selection` and `# map shift+insert = paste` commented out as examples. So what is left is a local keymap choice, not a defect: uncomment both in `%LOCALAPPDATA%\agwinterm\keymap.conf` and reload, pairing the paste line with `copy-on-ctrl-c = false` if `^C` should keep interrupting. The draft body at `.claude/issue-drafts/ctrl-insert-copy-paste.md` is superseded and can go with this memo.
