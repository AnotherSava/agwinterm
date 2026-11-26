using System.Text.Json;

namespace Agwinterm.Pty;

/// <summary>
/// Rules for a session's sidebar tint — a colour chosen by the caller, persisted, exposed on
/// <c>tree</c> as <c>tint</c> and <c>tintOn</c>, set via <c>session.tint</c>.
///
/// The vocabulary is a subset of agterm's <c>session tint</c>, so one caller can drive both
/// terminals as long as it stays inside this list — agterm's five 0.41.0 names (teal, pink,
/// brown, indigo, mint) are deliberately not taken here, while its <c>#rrggbb</c> hex is.
/// The user settled that split on 2026-11-24: hex yes, the five extra names no.
/// <see cref="DefaultOn"/> is where the two differ: agwinterm has no free per-session icon lane —
/// the left gutter belongs to the flag marker and the status dot is rewritten by the agent status
/// hooks on every tool call — so only <c>name</c> is accepted here and <c>icon</c> is refused
/// rather than silently ignored.
/// </summary>
public static class SessionTints
{
    public const string Key = "tint";
    public const string OnKey = "tintOn";

    /// <summary>The word that removes a tint. It is never stored: a cleared session holds null.</summary>
    public const string Clear = "clear";

    /// <summary>
    /// The vocabulary, and the sidebar RGB (0xRRGGBB) each colour paints with. Two sets per colour,
    /// picked off the sidebar's own luminance: the hues that read on a dark background wash out on a
    /// light one, and a single set would make half the vocabulary unreadable under a light theme.
    /// This table is the only place a colour name lives in code — <see cref="Colors"/> (what the
    /// verb accepts), <see cref="Paint"/> (what the sidebar draws) and <see cref="ColorWords"/>
    /// (what the CLI, the agent skill and the refusals spell out) are all read off it, so a name
    /// cannot be accepted by the control API and then drawn in the row's ordinary colour.
    /// The one copy outside this file is prose: the vocabulary in the `session tint` section of
    /// docs/control-api.md, which an edit here has to carry.
    /// </summary>
    private static readonly (string Name, uint OnDark, uint OnLight)[] Palette =
    [
        ("red",    0xFF706B, 0xB82121),
        ("orange", 0xFFA84D, 0xB35E08),
        ("yellow", 0xF2D959, 0x8F7000),
        ("green",  0x73DB87, 0x1C7A33),
        ("blue",   0x70B5FF, 0x1C61C2),
        ("purple", 0xC299FF, 0x783DBF),
        ("gray",   0x9EA6B0, 0x6B737D),
    ];

    /// <summary>The accepted colours. <c>clear</c> removes the tint and is not stored. A hex colour
    /// (<see cref="HexColor"/>) is accepted too and is not in this list, since it is a shape rather
    /// than a word — <see cref="NormalizeColor"/> is the one place both are decided.</summary>
    public static readonly string[] Colors = [.. Palette.Select(p => p.Name), Clear];

    /// <summary>The vocabulary as a caller reads it in usage text: <c>red|…|clear|#rrggbb</c>.</summary>
    public static readonly string ColorWords = string.Join('|', Colors) + "|" + HexColor.Word;

    /// <summary>A sidebar colour, as 8-bit components.</summary>
    public readonly record struct Rgb(byte R, byte G, byte B);

    /// <summary>
    /// The colour a stored tint paints with, or null when the session has no tint. The argument is
    /// the stored value, so <c>clear</c> and a name this build does not know both answer null and
    /// leave the row in its ordinary colour.
    /// A hex tint ignores <paramref name="onLight"/>: it is the literal colour the caller asked
    /// for, so unlike a palette name it does not re-pick itself for the theme, and a hex chosen
    /// against one sidebar can read poorly against the other. agterm is read from its source as
    /// doing the same; that one is unmeasured, since <c>tree</c> cannot show what a row painted.
    /// </summary>
    public static Rgb? Paint(string? tint, bool onLight)
    {
        if (string.IsNullOrEmpty(tint)) return null;
        if (HexColor.Parse(tint) is { } hex) return new Rgb(hex.R, hex.G, hex.B);
        foreach (var p in Palette)
            if (string.Equals(p.Name, tint, StringComparison.Ordinal))
            {
                uint v = onLight ? p.OnLight : p.OnDark;
                return new Rgb((byte)(v >> 16), (byte)(v >> 8), (byte)v);
            }
        return null;
    }

    /// <summary>
    /// The one surface a tint can paint here, which is therefore also what <c>--on</c> defaults to.
    /// Every site that names a surface reads this: <see cref="ValidateOn"/> accepts it,
    /// <see cref="UnknownSurface"/> advertises it and <see cref="OnFor"/> reports it, so the
    /// accepted set and the advertised set cannot drift.
    /// </summary>
    public const string DefaultOn = "name";

    /// <summary>
    /// The surface a stored tint paints. Derived from the colour wherever it is needed and never
    /// stored beside it — <c>name</c> is the only surface this build can paint, so a second copy
    /// could only go out of step with the colour it describes.
    /// </summary>
    public static string? OnFor(string? tint) => string.IsNullOrEmpty(tint) ? null : DefaultOn;

    public const string NoSession = "session not found; nothing changed";

    public static readonly string UnknownColor =
        "session tint: unknown colour. Pass one of " + string.Join(", ", Colors) + ", or a " +
        HexColor.Word + " (exactly " + HexColor.Digits + " hex digits, the # optional). Nothing changed.";

    public const string IconUnavailable =
        "session tint: agwinterm has no free per-session icon lane — the left gutter is the flag marker's " +
        "and the status dot is written by the agent status hooks. Pass --on name. Nothing changed.";

    public static readonly string UnknownSurface =
        "session tint: --on accepts only " + DefaultOn + " here. Nothing changed.";

    /// <summary>
    /// Normalize a colour; null when it is neither one of <see cref="Colors"/> nor a hex. A hex
    /// comes back from <see cref="HexColor.Normalize"/> lowercase and with its <c>#</c>, whatever
    /// the caller passed, so the stored value and the <c>tree</c> readback have one spelling per
    /// colour.
    /// </summary>
    public static string? NormalizeColor(string raw)
    {
        string c = raw.Trim().ToLowerInvariant();
        return Array.IndexOf(Colors, c) >= 0 ? c : HexColor.Normalize(c);
    }

    /// <summary>
    /// Validate <c>--on</c>. Returns the refusal, or null when the call may proceed. Nothing is
    /// handed back: every surface that survives this is <see cref="DefaultOn"/>, which
    /// <see cref="OnFor"/> derives from the colour instead.
    /// An <c>icon</c> anywhere in the list refuses with <see cref="IconUnavailable"/>, so a caller
    /// written against agterm learns why instead of seeing a tint that never appears — agterm
    /// defaults an untinted session to the icon, which is the one surface unavailable here.
    /// </summary>
    public static string? ValidateOn(string? raw)
    {
        if (raw is null || raw.Trim().Length == 0) return null;
        var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        bool name = false;
        foreach (var p in parts)
        {
            string s = p.ToLowerInvariant();
            if (s == "icon") return IconUnavailable;
            if (s != DefaultOn) return UnknownSurface;
            name = true;
        }
        if (!name) return UnknownSurface;
        return null;
    }

    /// <summary>The verb's reply: the colour as stored, with the surface read off <see cref="OnFor"/>
    /// rather than echoed from the caller's <c>--on</c>, so the reply cannot name a surface the row is
    /// not painted on.</summary>
    public static string Reply(string sessionId, string? tint) =>
        "{\"session\":" + JsonSerializer.Serialize(sessionId) + ",\"" + Key + "\":" +
        (tint is null ? "null" : JsonSerializer.Serialize(tint)) + ",\"" + OnKey + "\":" +
        (OnFor(tint) is { } on ? JsonSerializer.Serialize(on) : "null") + "}";
}
