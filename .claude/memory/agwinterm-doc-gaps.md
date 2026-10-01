---
name: agwinterm-doc-gaps
description: "Where agwinterm's docs disagree with its code — verified 2026-08-17 against v0.17.3, re-checked 2026-10-01 against 0.20.13; don't answer user questions from the README alone"
metadata: 
  node_type: memory
  type: project
  modified: 2026-08-18T01:58:41.401Z
---

Verified against v0.17.3 on 2026-08-17. Each of these makes the documentation say something the code
does not do:

1. **The agent skill's config-key list is a stale subset.** `AgentSkill.SkillMarkdown` (~line 173)
   lists roughly 20 keys; `TerminalConfig` defines ~60. Missing from it: `bell`, `ligatures`,
   `unfocused-dim`, `paste-protection`, `clipboard-write`, `word-delimiters`, `toolbar-mode`,
   `session-host`, `emulator-core`, `fresh-env`, `prompt-engine`, `starship-theme`, `update-check`,
   `notification-sound`, and more. Answer key questions from `TerminalConfig.DefaultText`.
2. **The Settings dialog covers about half the config.** Notably the hidden full-bleed toolbar
   advertised in `docs/user-guide.md` has no UI — Settings only offers the "Compact toolbar" toggle
   (`Settings.cs` ~line 148), so `toolbar-mode = hidden` is config-file-only.
3. **"Everything is rebindable in keymap.conf" is overstated.** Ctrl+` (quick terminal) and
   Ctrl+= / Ctrl+- / Ctrl+0 (font zoom) are handled in `Program.Input.cs` (~lines 989-1000), *before*
   the keymap chord lookup (~line 1031), so a `map` line for those never fires.

All three still held when re-checked against 0.20.13 on 2026-10-01. For item 1, matching the keys
`TerminalConfig`'s parse switch accepts against the text of `AgentSkill.SkillMarkdown` found 63 keys
parsed and 22 named, so 41 are absent — `bell` has since been added, the rest of the list above has
not. `Settings.cs` still offers only the "Compact toolbar" toggle, and `Program.Input.cs` still
handles Ctrl+` and the font-zoom chords ahead of the keymap lookup, with a comment saying so.

**Why:** The README is confident and detailed enough to sound authoritative, so answering from it
produces wrong answers about what is configurable and where. The generated in-app docs drift from
`TerminalConfig` independently, so even the skill file can be wrong.

**How to apply:** For any "can agwinterm do X / where do I set X" question, check
`TerminalConfig.cs` and `Settings.cs` before answering, and say explicitly when a setting is
reachable only by editing the conf file. See [[agwinterm-docs-map]] for where each reference lives.
