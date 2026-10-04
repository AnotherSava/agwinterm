---
created: 2026-10-03 23:00:35
platform: windows
---

# PR the rename-box font fix upstream — EnsureEditGdi should take the covered format's em size, not a hardcoded 13 DIP

Verified 2026-10-03 against a throwaway git worktree at upstream/main (6f3d8d9, agwinterm 0.20.13), since removed.

The bug is stock upstream, not a side effect of the local patches. `EnsureEditGdi` hardcodes `int px = ToDevice(13)`, while the sidebar row text is drawn with `_sidebarFont`, whose em size comes from the `sidebar-font-size` key (clamped 9..20). At any value but 13 the inline rename box's text is a different size from the name it covers; at 20 it is visibly smaller. Reported while renaming a workspace row by double-click.

The fix, already applied on `local`: give `EnsureEditGdi` a `float dip` parameter and have each caller pass the em size of the DirectWrite format whose text the box covers — `_sidebarFont.FontSize` from `StartRename`, `_uiFont.FontSize` from `StartWindowRename`. No literal left to drift, and the sidebar box follows a live `sidebar-font-size` change for free.

Measured: 3 files, 9 insertions, 5 deletions. It applies to upstream/main with no dependency on any of the 14 local commits — the only nearby divergence, `DisplayName(s)` against upstream's `s.Name` in `StartRename`, is a context line rather than a changed one. `dotnet build Agwinterm.slnx -c Release` succeeds on the patched upstream tree.

Two things to settle before opening it:

- `StartRename` sets `ey = ToDevice(ry0 + 4)` and `leftMargin = ToDevice(isWs ? 23 : 25)`, both pixel-tuned against the 13 DIP font per their own comment. They are arguably the same defect and were left alone, so the PR should either cover them or say why not.
- On a fresh clone `RustPtyHostTests.CreationTickets_StartupSweepProtectsUnpublishedPane` fails rather than skips: it asserts `NotNull(ExePath)` where its eleven siblings guard with `if (ExePath is null) return;`, and `native/target/` is gitignored, so a tree without the Rust build reports a failure. Unrelated to this patch, but it is what the repo's own gate shows to a first-time contributor, and worth its own upstream issue.

CONTRIBUTING requires an issue stating the problem and a maintainer reply before any code, and the branch cut from upstream/main rather than `local`, which carries installer and `.claude/` commits that must not go in a feature PR.
