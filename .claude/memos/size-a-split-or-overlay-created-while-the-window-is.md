---
created: 2026-11-25 21:23:08
---

# Size a split or overlay created while the window is minimized to its own box

A pane added to an existing session while agwinterm is minimized is handed the WHOLE content box instead of its share of the split, and a percent-sized overlay the same way.

Read from the code during the 0.20.13.6 review, not run: GridSizeFor answers the whole content box (or the last on-screen one, or the restore rect) and has no notion of a pane's share; CreatePane passes that grid to the backend verbatim; SplitPane halves Ratio and then calls RegridSession, which returns at its IsIconic guard. OverlayOpen takes the same path. The first WM_SIZE on restore corrects every pane, so the wrong size lasts only as long as the window stays minimized.

This is the IsIconic guard's pre-existing behaviour, not something the 1x1 creation fix introduced - that fix stopped a new SESSION being created at 1x1, and routes through the same GridSizeFor. The review rated it rare and dropped it.

Next step: decide whether creation should ask for the pane's own box rather than the session's. That means giving GridSizeFor, or a sibling of it, the target pane's rectangle, which SplitPane and OverlayOpen both know before they call CreatePane. Measure it first: create a split and a percent overlay through the control API while the window is minimized, and read cols/rows back with 'agwintermctl session metrics --json'.
