---
created: 2026-11-24 16:14:12
---

# Accept any #rrggbb in `session tint`, and match agterm's named colours

Requested by the agterm session on the user's behalf, so one tauri-dashboard `tab_tints` value means
the same colour on both terminals.

`session tint` now exists on `local` (`feat(control): tint a session's sidebar name`), but
`SessionTints.NormalizeColor` accepts only the seven names in `SessionTints.Palette` (red, orange,
yellow, green, blue, purple, gray) plus `clear`. agterm v0.41.0 accepts `#rrggbb` in
`session tint` / `workspace tint` / `--tint`, and its named set is wider: it adds teal, pink, brown,
indigo and mint. A hex value has no on-dark/on-light pair the way `Palette` entries do, so `Paint`
needs a rule for it (paint as given, or adjust for the sidebar's luminance).

tauri-dashboard owns tab tints through its `tab_tints` config (a model tint, plus a remote tint for
⇄ tabs) and applies them only to agterm today; its agwinterm adapter builds every target with
`tint: None`. It has a matching memo on its side. This also bears on the open memo about offering
`session tint` upstream: the vocabulary should settle before that conversation starts.
