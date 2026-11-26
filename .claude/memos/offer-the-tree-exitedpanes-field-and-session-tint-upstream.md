---
created: 2026-11-24 00:00
---
# Offer the `exitedPanes` tree field and `session tint` upstream

Two control-API patches now sit on `local` with no upstream issue, PR or prior memo. Both were
written for an outside caller rather than for this machine, so both are candidates upstream; neither
has been offered.

- **`feat(control): report exited panes in tree`** (9ff0293) — each session node carries
  `exitedPanes`, the ids of panes whose process has ended while the pane stays on screen. Asked for
  by the tauri-dashboard session: `foregroundShells` reads null for an exited pane and for a live
  non-shell alike, so nothing else in the node says whether a program finished. A `tmux has-session`
  probe was considered and rejected — it answers for the tmux server, not for the pane agwinterm
  shows.
- **`feat(control): session tint`** — colours a session's name in the sidebar, persisted through
  `RestoreState`. The verb, its vocabulary and its `--on` flag mirror agterm's verb of the same name
  so one caller can drive both terminals; agwinterm has no free per-session icon lane, so `--on icon`
  is refused with its reason instead of accepted and ignored.

## What has to happen before either is offered

1. **CONTRIBUTING asks for an issue and a maintainer reply before a first PR.** Neither patch has
   one, and `session tint` is a new verb rather than a field on an existing one, so it needs the
   conversation more than `exitedPanes` does.
2. **`tests/conformance/control-api.json` is read by agliteterm's CI.** The two `session.tint` rows
   this fork added carry a note saying they belong in that file only once a lite PR implements the
   verb; a PR that moves the verb upstream has to settle that file's shape with yeroo rather than
   land the rows alone.
3. **`session tint` has no `tests/integration/win32-control.ps1` case** — the suite says so outright.
   CONTRIBUTING wants one for a control-API change, and the pwsh suite has never been run on this
   machine.

## Also unoffered: the minimized-creation fix, which is item 4 of #332

`fix(win32): size a new pane from the content box` is not a fork feature — it answers item 4 of
yeroo's own open issue #332, whose acceptance criteria match what was measured here (a session
created while the window is minimized came up 1x1 and a TUI drew nothing into the one cell). So this
one needs no issue of its own and no "is this wanted" conversation; the issue is the conversation.
Items 1-3 of #332 — window-state reporting on `tree`, a `surface size` verb, and
`window restore --no-activate` — are untouched here, so a PR would close part of the issue only, and
should say which part.
