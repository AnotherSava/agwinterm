---
created: 2026-11-25 21:23:18
---

# Settle the two hex-colour rules: ParseColor takes #RGB, HexColor does not

The repo now decides what a hex colour is in two places, and they disagree.

HexColor in Agwinterm.Pty, added with the #rrggbb session tint, requires exactly six digits and refuses #abc rather than expanding it - deliberately, matching agterm, so a caller cannot get a colour it did not name. The session tint and the HUD's 'color' and 'text-color' fields both read it.

ParseColor in Program.Chrome.cs accepts #RGB as well as #RRGGBB, and serves theme and config values. It is yeroo's file and a different input surface, so the tint change left it alone.

Found by the docs-relevance pass during the 0.20.13.6 review. Nothing is broken: no caller reaches both rules. What is uncomfortable is that '#abc' is a colour in a config file and not a colour on the control API, with no documented reason.

Next step: this is a question for yeroo rather than a local patch - whether the shorthand should be accepted on the control API too, or refused in config, or whether the split is deliberate. Ask on an upstream issue before touching either parser; a fork patch here would conflict on every rebase of a file upstream edits.
