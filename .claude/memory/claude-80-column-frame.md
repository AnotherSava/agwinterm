---
name: claude-80-column-frame
description: "Open since 2026-10-09 — Claude panes sometimes draw at 80 columns in a 241-wide tmux pane after a boot; not agwinterm's fault; tmux logging hooks are installed in WSL to catch it"
metadata:
  node_type: memory
  type: project
  modified: 2026-10-11T18:11:40.167Z
---

Claude Code in a `cc-*` tmux session sometimes keeps drawing at 80 columns while its tmux pane
is the full width. One `tmux resize-window` (or Ctrl+D split/close in agwinterm) fixes it. 80x24
is tmux's `default-size` for a session created with no client attached, which is what
`start-here.sh` in the dotfiles repo did before attaching until the remedy below.

Measured, not assumed:
- agwinterm delivers the right size: a probe pane and every tmux client came up at full width.
- The stuck Claude was drawing live at 80 (its lines rely on autowrap at column 80), so one
  80→wide resize never reached `claude.exe` through WSL interop. Later resizes arrive fine.
- Neither single nor six concurrent Claude starts reproduced it at any resize delay. On two clean
  boots (2026-10-10) tmux resized each new session within ~15 ms of creating it, before Claude started.
- The bad instance's six `claude.exe --continue` processes all started at 5:25 PM on 2026-10-09,
  through a path not identified.

**Logging hooks are installed in WSL `~/.tmux.conf`, writing `~/tmux-size.log`** (CREATED, ATTACH,
DETACH, CLIENT, WINDOW lines with sizes and timestamps). Keep them until the next occurrence;
removing them early was a mistake once already. When it recurs, read the log for that session
before resizing anything, then delete both files and unset the hooks.

**Recurred 2026-10-11** in `cc-claude` and `cc-whats-next-agent` (Claude processes started
2026-10-10 ~12:28 after a boot; the VM clock was ~30 min behind at boot, so tmux's
`session_created` reads 11:53). The log did not help: it has no CREATED or first ATTACH lines for
any `cc-*` session, and the 2026-10-11 reattach was 241→241 with no resize. The hooks were kept for
another occurrence. A check that needs no screen: the width of the last `─{20,}` run in
`tmux capture-pane -p` is 80 in a stuck pane. Fixed by `resize-window -x 240` then `-A`.

**Remedy in place since 2026-10-11** (dotfiles e721f9e): `remote_session_create` in the dotfiles
repo's `claude/remote-session/lib.sh` creates each session at the terminal's size from `stty size`,
and both `start-here.sh` and `cc-session.sh` call it. Only tested against a private tmux server, not
yet on a real Claude session after a reboot. A recurrence after that means this was not the cause;
the dotfiles memory `project_claude_80_column_frame.md` tracks the same bug.
