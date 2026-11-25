---
created: 2026-11-24 22:34:26
---

# Make a split open in the live cwd, and say where a split pane starts in the global agwinterm skill

Two items parked from the 0.20.13 upstream-feature triage on 2026-11-24 (they were items #1 and item 0 on tmp/agwinterm-v0.20.13-d80bc30-features.html, which lives under the gitignored tmp/ and will not survive a clean-up).

1. **A split opens in the session's starting cwd, not the live one.** Upstream 0.20.13 added the behaviour where a new session inherits the invoking pane's directory; the split path was looked at and left undecided. Walking into a subdirectory and splitting gives a pane back at the original cwd, which is not what the gesture reads as. Decide whether the fork wants the live cwd here, and whether that is a patch for local or a question for yeroo first.

2. **The global agwinterm skill says nothing about where a split pane starts.** An agent driving `session split` has no way to know which directory the new pane comes up in, so it cannot tell a caller where a command it is about to type will run. One line in the skill text (SkillMarkdown in AgentSkill.cs, the split section) closes it. Item 1 settles what that line should say, so do them in that order.
