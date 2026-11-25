using System.Text.Json;

namespace Agwinterm.Pty.Tests;

/// <summary>
/// <c>session.tint</c>: the caller-chosen sidebar colour, and what the tree reports about it.
///
/// The vocabulary matches agterm's verb of the same name so one caller can drive both terminals.
/// Where the two differ is <c>--on</c>: agwinterm has no free per-session icon lane, so <c>icon</c>
/// is refused rather than accepted and silently ignored — a caller written against agterm's default
/// has to learn that here, not by staring at an untinted row.
/// </summary>
public class SessionTintTests
{
    private static (ControlServer server, FakeSessionHost host) New()
    {
        var host = new FakeSessionHost();
        return (new ControlServer(host), host);
    }

    private static JsonElement TreeSession(ControlServer server)
        => JsonDocument.Parse(server.Dispatch("{\"cmd\":\"tree\"}")).RootElement
            .GetProperty("result").GetProperty("workspaces")[0].GetProperty("sessions")[0];

    private static string Tint(ControlServer server, string color, string? on = null)
        => server.Dispatch("{\"cmd\":\"session.tint\",\"args\":{\"tint\":" + JsonSerializer.Serialize(color)
            + (on is null ? "" : ",\"on\":" + JsonSerializer.Serialize(on)) + "}}");

    private static bool Ok(string reply) => JsonDocument.Parse(reply).RootElement.GetProperty("ok").GetBoolean();

    private static string Error(string reply) => JsonDocument.Parse(reply).RootElement.GetProperty("error").GetString()!;

    [Fact]
    public void Untinted_OmitsBothKeys()
    {
        var (server, _) = New();
        var s = TreeSession(server);

        Assert.False(s.TryGetProperty("tint", out _));
        Assert.False(s.TryGetProperty("tintOn", out _));
    }

    [Fact]
    public void Tinting_ReportsColorAndSurface()
    {
        var (server, _) = New();

        Assert.True(Ok(Tint(server, "purple")));

        var s = TreeSession(server);
        Assert.Equal("purple", s.GetProperty("tint").GetString());
        Assert.Equal("name", s.GetProperty("tintOn").GetString());
    }

    [Fact]
    public void TintOn_DefaultsToName_WithoutTheCallerSayingSo()
    {
        var (server, _) = New();
        Tint(server, "blue");

        Assert.Equal("name", TreeSession(server).GetProperty("tintOn").GetString());
    }

    [Fact]
    public void Clear_RemovesBothKeys_RatherThanStoringTheWord()
    {
        var (server, _) = New();
        Tint(server, "green");

        Assert.True(Ok(Tint(server, "clear")));

        var s = TreeSession(server);
        Assert.False(s.TryGetProperty("tint", out _));
        Assert.False(s.TryGetProperty("tintOn", out _));
    }

    [Fact]
    public void EveryColorInTheVocabularyIsAccepted()
    {
        var (server, _) = New();
        foreach (var color in SessionTints.Colors)
        {
            Assert.True(Ok(Tint(server, color)), color + " was refused");
            if (color != "clear")
                Assert.Equal(color, TreeSession(server).GetProperty("tint").GetString());
        }
    }

    [Fact]
    public void UnknownColor_IsRefused_AndLeavesTheSessionUntinted()
    {
        var (server, _) = New();
        Tint(server, "red");

        string reply = Tint(server, "chartreuse");

        Assert.False(Ok(reply));
        Assert.Equal(SessionTints.UnknownColor, Error(reply));
        Assert.Equal("red", TreeSession(server).GetProperty("tint").GetString());
    }

    [Fact]
    public void Icon_IsRefusedWithItsReason_NotSilentlyIgnored()
    {
        var (server, _) = New();

        string reply = Tint(server, "purple", "icon");

        Assert.False(Ok(reply));
        Assert.Equal(SessionTints.IconUnavailable, Error(reply));
        Assert.False(TreeSession(server).TryGetProperty("tint", out _));
    }

    [Fact]
    public void IconAndName_IsRefusedToo_SinceHalfOfItCannotBeHonoured()
    {
        var (server, _) = New();

        string reply = Tint(server, "purple", "icon,name");

        Assert.False(Ok(reply));
        Assert.Equal(SessionTints.IconUnavailable, Error(reply));
        Assert.False(TreeSession(server).TryGetProperty("tint", out _));
    }

    [Fact]
    public void UnknownSurface_IsRefused()
    {
        var (server, _) = New();

        string reply = Tint(server, "purple", "border");

        Assert.False(Ok(reply));
        Assert.Equal(SessionTints.UnknownSurface, Error(reply));
    }

    [Fact]
    public void MissingColor_IsRefused()
    {
        var (server, _) = New();

        string reply = server.Dispatch("{\"cmd\":\"session.tint\",\"args\":{}}");

        Assert.False(Ok(reply));
        Assert.Equal(SessionTints.UnknownColor, Error(reply));
    }

    [Fact]
    public void UnknownTarget_IsRefused()
    {
        var (server, _) = New();

        // target rides on the ROOT of the request, not inside args — ControlServer reads it there
        // for every verb, so a test that nests it silently addresses the active session instead.
        string reply = server.Dispatch("{\"cmd\":\"session.tint\",\"target\":\"nope\",\"args\":{\"tint\":\"red\"}}");

        Assert.False(Ok(reply));
        Assert.Equal(SessionTints.NoSession, Error(reply));
    }

    [Fact]
    public void ColorIsCaseInsensitive_SinceTheReplyNormalizesIt()
    {
        var (server, _) = New();

        Assert.True(Ok(Tint(server, "PURPLE")));
        Assert.Equal("purple", TreeSession(server).GetProperty("tint").GetString());
    }
}
