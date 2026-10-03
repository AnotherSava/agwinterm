using System.Text.Json;

namespace Agwinterm.Pty.Tests;

/// <summary>
/// tree's <c>paneCwds</c> — WHICH PANE SITS IN WHICH PROJECT.
///
/// Every other per-pane fact the tree reports describes what a pane is <em>doing</em>; this one says
/// where it <em>is</em>, which is the only thing an outside tool can match its own records against.
/// Nothing inside a pane can answer it: <c>AGWINTERM_SESSION_ID</c> reaches the process agwinterm
/// starts, so a pane running <c>wsl.exe -d Ubuntu -- launcher.sh</c> or <c>ssh host cmd</c> launches
/// the real program in an environment that never received it, and that program cannot name its own
/// pane. The directory survives the wrapper because it is read off the pane rather than off the
/// process, which is what makes it the usable key.
///
/// It is an object keyed by PANE id, like <c>restoreCommands</c> and <c>capturedCommands</c> and not
/// an array in pane order like <c>foregroundShells</c>, and the one-pane case below is why: the tree
/// emits <c>paneIds</c> only while a session is split, so an array would hand a one-pane session's
/// directory back attached to no addressable id — and the id is the whole point. A pane whose
/// directory is unknown is absent, and a session with none omits the key, the spelling the maps
/// beside it already use for "no".
/// </summary>
public class TreePaneCwdTests
{
    private static (ControlServer server, FakeSessionHost host) New()
    {
        var host = new FakeSessionHost();
        return (new ControlServer(host), host);
    }

    private static JsonElement TreeSession(ControlServer server, int index = 0)
        => JsonDocument.Parse(server.Dispatch("{\"cmd\":\"tree\"}")).RootElement
            .GetProperty("result").GetProperty("workspaces")[0].GetProperty("sessions")[index];

    private static JsonElement? Cwds(ControlServer server, int index = 0)
        => TreeSession(server, index).TryGetProperty("paneCwds", out var v) ? v : null;

    private static FakeSessionHost.Sess First(FakeSessionHost host) => host.Workspaces[0].Sessions[0];

    // ---- the one-pane case the map exists for ----

    [Fact]
    public void OnePaneSession_ReportsItsDirectory_UnderAnIdTheTreeDoesNotOtherwiseEmit()
    {
        var (server, host) = New();
        var s = First(host);
        s.Cwds[s.PaneIds[0]] = @"D:\work\agwinterm";

        var node = TreeSession(server);
        Assert.False(node.TryGetProperty("paneIds", out _));   // absent while unsplit — an array would have nothing to key against
        Assert.Equal(@"D:\work\agwinterm", node.GetProperty("paneCwds").GetProperty(s.PaneIds[0]).GetString());
    }

    // ---- a split is two projects, not one ----

    [Fact]
    public void SplitSession_ReportsEachPanesOwnDirectory()
    {
        var (server, host) = New();
        var s = First(host);
        s.AddPane();
        s.Cwds[s.PaneIds[0]] = @"D:\work\alpha";
        s.Cwds[s.PaneIds[1]] = @"D:\work\beta";

        var cwds = Cwds(server)!.Value;
        Assert.Equal(@"D:\work\alpha", cwds.GetProperty(s.PaneIds[0]).GetString());
        Assert.Equal(@"D:\work\beta", cwds.GetProperty(s.PaneIds[1]).GetString());
    }

    [Fact]
    public void UnknownDirectory_IsAbsent_WhileItsNeighbourStillReports()
    {
        var (server, host) = New();
        var s = First(host);
        s.AddPane();
        s.Cwds[s.PaneIds[1]] = @"D:\work\alpha";

        var cwds = Cwds(server)!.Value;
        Assert.False(cwds.TryGetProperty(s.PaneIds[0], out _));
        Assert.Equal(@"D:\work\alpha", cwds.GetProperty(s.PaneIds[1]).GetString());
    }

    [Fact]
    public void NoPaneHasOne_OmitsTheKey()
    {
        var (server, _) = New();
        Assert.Null(Cwds(server));
    }

    // ---- ids are durable, so the directories ride on them ----

    [Fact]
    public void Swap_MovesThePanes_AndEachDirectoryGoesWithItsId()
    {
        var (server, host) = New();
        var s = First(host);
        s.AddPane();
        string first = s.PaneIds[0], second = s.PaneIds[1];
        s.Cwds[first] = @"D:\work\alpha";
        s.Cwds[second] = @"D:\work\beta";

        host.Swap(null);

        var cwds = Cwds(server)!.Value;
        Assert.Equal(new[] { second, first }, s.PaneIds);                                  // the slots exchanged
        Assert.Equal(@"D:\work\alpha", cwds.GetProperty(first).GetString());          // the directories did not
        Assert.Equal(@"D:\work\beta", cwds.GetProperty(second).GetString());
    }

    [Fact]
    public void ClosingAPane_TakesItsDirectoryWithIt()
    {
        var (server, host) = New();
        var s = First(host);
        s.AddPane();
        string gone = s.PaneIds[1];
        s.Cwds[s.PaneIds[0]] = @"D:\work\alpha";
        s.Cwds[gone] = @"D:\work\beta";

        s.RemovePane(1);

        var cwds = Cwds(server)!.Value;
        Assert.False(cwds.TryGetProperty(gone, out _));
        Assert.Equal(@"D:\work\alpha", cwds.GetProperty(s.PaneIds[0]).GetString());
    }
}
