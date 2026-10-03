---
name: agwinterm-session-title-route
description: the dashboard labels a session through the pane's OSC 0/2 title, and the tmux options that forward it exist only in the running server
metadata:
  type: project
---

**A label reaches the sidebar as the pane's OSC 0/2 title, and nothing else can carry it.** No
environment variable can: every `claude.exe` is a child of the tmux holder that Task Scheduler
starts, so there is no point after launch at which one could be injected — the same reason a
`wsl`- or `ssh`-wrapped process cannot name its own pane ([[agwinterm-control-api-gotchas]]).
`DisplayName` reads that title as its second step, after a custom name, so a session carrying a
custom name shows the custom name and hides the title entirely; `agwintermctl tree` reports the
title in its own `title` field for exactly that case.

**The tmux options that forward it are runtime-only, so a tmux server restart silently breaks
the labelling.** Measured 2026-10-03: the running server has `set-titles on` and a custom
`set-titles-string` of `#{?#{m:*active-pane*,#{client_flags}},⇄ #T,#T}`, while none of
`~/.tmux.conf`, `~/.config/tmux/tmux.conf` or `/etc/tmux.conf` exists in the Ubuntu WSL distro
and nothing under that home mentions `set-titles`. Both options therefore live only in the
running server's state, set over the wire by something outside WSL. When sidebar rows go back to
reading as directory basenames, check `tmux show-options -g | grep set-titles` before looking at
anything in agwinterm.
