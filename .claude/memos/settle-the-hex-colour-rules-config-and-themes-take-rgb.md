---
created: 2026-11-25 21:23:18
---

# Settle the hex-colour rules: config and themes take #RGB, HexColor does not

The repo decides what a hex colour is in several places, and they disagree on the #RGB shorthand.

HexColor in Agwinterm.Pty, added with the #rrggbb session tint, requires exactly six digits and refuses #abc rather than expanding it - deliberately, matching agterm, so a caller cannot get a colour it did not name. The session tint and the HUD's 'color' and 'text-color' fields both read it.

These accept #RGB as well as #RRGGBB, and none of them is the fork's:
- HexColor4 in Program.Chrome.cs (yeroo's), which reads the StatusColor* and NotificationColor* config values and the Settings swatches (SwatchColor in Settings.cs).
- Hex in Program.Services.cs, which reads a ghostty theme's palette, background, foreground and cursor-color.
- The '#' branch of the OSC colour parser in TerminalEmulator.cs.

ThemeColors.TryHex in Agwinterm.Core takes six digits only, like HexColor. The tint change left all of these alone: they are upstream files and different input surfaces.

Found by the docs-relevance pass during the 0.20.13.6 review. Nothing is broken: no caller reaches more than one rule. What is uncomfortable is that '#abc' is a colour in a config file or a theme and not a colour on the control API, with no documented reason.

Next step: this is a question for yeroo rather than a local patch - whether the shorthand should be accepted on the control API too, or refused in config and themes, or whether the split is deliberate. Ask on an upstream issue before touching any parser; a fork patch here would conflict on every rebase of a file upstream edits.
