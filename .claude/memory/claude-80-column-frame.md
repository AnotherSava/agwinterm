---
name: claude-80-column-frame
description: Open since 2026-10-09 — Claude panes sometimes draw at 80 columns in a 241-wide tmux pane after a boot; not agwinterm's fault; tmux logging hooks are installed in WSL to catch it
metadata:
  type: project
---

Claude Code in a `cc-*` tmux session sometimes keeps drawing at 80 columns while its tmux pane
is the full width. One `tmux resize-window` (or Ctrl+D split/close in agwinterm) fixes it. 80x24
is tmux's `default-size` for a session created with no client attached, which is what
`start-here.sh` in the dotfiles repo does before attaching.

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

Likely remedy if it recurs: create the session at the attaching terminal's size in
`start-here.sh` (`tmux new-session -d -x "$(tput cols)" -y "$(tput lines)"`), a dotfiles change,
not an agwinterm PR.
