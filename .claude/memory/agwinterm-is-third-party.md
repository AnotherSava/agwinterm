---
name: agwinterm-is-third-party
description: "The agwinterm project belongs to yeroo, not the user — the upstream remote names his repo; report upstream, and changes to his files stay on the local branch"
metadata: 
  node_type: memory
  type: project
  modified: 2026-08-18T01:58:17.541Z
---

This clone's `upstream` remote is **github.com/yeroo/agwinterm** (MIT © Boris Kudriashov), and the
user is a *user* of that project, not its author — their GitHub handle is `AnotherSava`
(see [[user-github-account]]). The user's own fork is `origin`, and the patches they carry live on
the `local` branch — [[agwinterm-fork-and-build]] has the details, including that `.claude/` is
committed there, so the project memory directory is symlinked from the machine-local cache by
`link-project-memory.sh` like any of the user's own repos.

**Why:** A foreign owner is easy to miss — the clone sits among the user's own projects and they
administer `origin`, so only the `upstream` remote says the source belongs to someone else. Treating
it like the user's own repo leads to offering someone else's code as this project's own work, or to a
fix made here that the upstream maintainer never hears about.

**How to apply:** Diagnose and explain freely. A change to a file yeroo owns goes on `local` and
nowhere else — it reaches upstream only as a cherry-pick onto a topic branch off `upstream/main`,
and only when the user asks for a PR. When a real defect turns up, file it upstream with
`gh issue create --repo yeroo/agwinterm` after checking for duplicates —
that is how #189, #352, #353 and #361 were filed. The user's first upstream PR is #362
(2026-10-09), so CONTRIBUTING's first-PR rule no longer applies to them. Fix the *user's machine* (config, fonts,
installs) rather than the *source*, unless they explicitly ask for a patch or a PR.
