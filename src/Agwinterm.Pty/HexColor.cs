namespace Agwinterm.Pty;

/// <summary>
/// What counts as a <c>#rrggbb</c> colour on the control API, and how one is stored — decided here
/// and nowhere else. The session tint (<see cref="SessionTints"/>) and the HUD's two colour fields
/// (<see cref="SessionHuds"/>) both read this, so a colour one verb accepts the other accepts, both
/// store it with one spelling, and widening the rule (an alpha byte, a CSS shorthand) is one edit
/// rather than two sites to keep level.
/// </summary>
public static class HexColor
{
    /// <summary>How the form is spelled wherever a vocabulary or a refusal advertises it.</summary>
    public const string Word = "#rrggbb";

    /// <summary>The digit count, exactly. <c>#abc</c> is refused rather than expanded, matching
    /// agterm, so a caller cannot get a colour it did not name.</summary>
    public const int Digits = 6;

    /// <summary>The components the text names, or null when it is not a hex colour. The <c>#</c> is
    /// optional, the case is free, and surrounding whitespace is ignored.</summary>
    public static (byte R, byte G, byte B)? Parse(string text)
    {
        ReadOnlySpan<char> d = text.AsSpan().Trim();
        if (d.Length > 0 && d[0] == '#') d = d[1..];
        if (d.Length != Digits) return null;
        int v = 0;
        foreach (char ch in d)
        {
            int n = ch is >= '0' and <= '9' ? ch - '0'
                  : ch is >= 'a' and <= 'f' ? ch - 'a' + 10
                  : ch is >= 'A' and <= 'F' ? ch - 'A' + 10
                  : -1;
            if (n < 0) return null;
            v = (v << 4) | n;
        }
        return ((byte)(v >> 16), (byte)(v >> 8), (byte)v);
    }

    /// <summary>The stored spelling — lowercase, with the <c>#</c> — or null when the text is not a
    /// hex colour, so one colour reads back one way whatever the caller typed. Spelled out from the
    /// components <see cref="Parse"/> returned, so what a <c>#</c> and surrounding whitespace mean
    /// is decided in that one method.</summary>
    public static string? Normalize(string text) =>
        Parse(text) is { } c ? $"#{c.R:x2}{c.G:x2}{c.B:x2}" : null;
}
