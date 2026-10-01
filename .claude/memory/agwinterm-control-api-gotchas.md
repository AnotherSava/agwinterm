---
name: agwinterm-control-api-gotchas
description: agwintermctl traps measured 2026-10-01 — session type submits only on CR, and a wrapped process can never be targeted
metadata:
  type: project
---

**`session type` submits a line only on CR.** A trailing `\n` is delivered and then sits there
unexecuted, with `typed` reported and exit 0 — indistinguishable from success. Two attempts were
lost to this before `\r` worked; agwinterm's own restore replay appends `"\r"` for the same
reason. Use `printf '…\r' | agwintermctl session type --stdin --target <pane>`.

**Check what is in a pane before typing into it.** Typing into a pane that is running Claude Code
puts the text in *Claude's prompt box*, not a shell, where it waits to be submitted as a prompt.
Clearing it needs `--allow-control` plus backspaces, since `session type` refuses control bytes
by default. Read the pane with `session text` first.

**A wrapped process cannot be targeted.** `--target` defaults to `$AGWINTERM_SESSION_ID`, which
*is* the pane id and is stable across restarts — but it only reaches a process agwinterm started
directly. A pane whose foreground is `wsl.exe -d … -- launcher.sh`, or `ssh host cmd`, launches
the real program in an environment that never received the variable, so nothing running there can
name its own pane. `tree` carries no cwd either, so an outside tool cannot match a pane by
project directory; that is filed upstream as #353.

Two smaller ones from the same session: `session text` can come back empty for a pane that is not
the active one, so an empty buffer is not evidence the pane is dead — check for a live shell
process instead. And `tree` is already JSON, so passing `--json` makes it fail.
