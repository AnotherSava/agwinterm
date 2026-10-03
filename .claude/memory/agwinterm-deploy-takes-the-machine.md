---
name: agwinterm-deploy-takes-the-machine
description: Running scripts/deploy.sh closes the terminal the user works inside, so it needs a machine-takeover ask with a duration, every time
metadata:
  type: feedback
---

Treat `bash scripts/deploy.sh` as taking over the machine, not as ordinary work. Ask immediately
before each run, say that agwinterm will close and relaunch, and give the duration — measured across
five runs on 2026-10-02, the window is gone for **ten to twenty-five seconds** (apply to relaunch:
8.5 s, 9.8 s, 10.6 s, 13.5 s, 23.3 s) inside a whole run of roughly a minute. Quote the range rather
than the average, and let it widen as runs are measured: a figure stated tighter than the
measurements makes the next run look like a hang, and this one has been restated twice for exactly
that reason. Then wait for a yes. Approval covers that one run.

**Why:** the user's own sessions live inside agwinterm, and the relaunch moves focus away from
whatever they were doing. Two deploys on 2026-10-02 (03:48 and 04:11 local) each had a yes beforehand
and each said the app would close and come back, which was not enough — neither said how long, and
both were framed as a routine deploy rather than as seizing the screen. The user raised it through
the tauri-dashboard session afterwards. This is the `CLAUDE.md` "Taking Over the Machine" rule
applying to a verb that does not look like it: nothing here moves the pointer or synthesises a
keystroke, and it still costs the user the foreground.

**How to apply:** put the restart in the closing ask with its duration — "agwinterm closes and comes
back, about ten seconds" — and never fold it into an approval given earlier in the task. Batch
changes so one deploy covers several where you can, since each one is its own interruption. Panes do
survive: each is a `wsl.exe` attach to a tmux session in the holder, so they come back re-attached
rather than restarted, and that is worth saying so the cost reads as ten seconds rather than as lost
work. For checking something rather than shipping it, prefer a route that needs no restart at all —
`agwintermctl tree` reads live state, and `PrintWindow` captures the window without raising it.

The deploy's mechanics, and why a detached helper performs the install, are in
[[agwinterm-fork-and-build]].
