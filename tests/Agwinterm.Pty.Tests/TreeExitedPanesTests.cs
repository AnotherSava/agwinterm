using System.Text.Json;

namespace Agwinterm.Pty.Tests;

/// <summary>
/// tree's <c>exitedPanes</c>: which panes' processes have exited.
///
/// agwinterm keeps an exited pane on screen, and its <c>foregroundShells</c> entry reads null, which is
/// also what a live program other than a shell reports. A caller that stops a program and waits for it
/// to end, such as a tab whose direct command is a <c>wsl … tmux attach</c> client, would wait forever
/// without this field. It lists pane ids, like the keys of <c>paneCwds</c>, because <c>paneIds</c> is
/// emitted only while a session is split. A session with no exited pane omits the key.
/// </summary>
public class TreeExitedPanesTests
{
    private static (ControlServer server, FakeSessionHost host) New()
    {
        var host = new FakeSessionHost();
        return (new ControlServer(host), host);
    }

    private static JsonElement TreeSession(ControlServer server)
        => JsonDocument.Parse(server.Dispatch("{\"cmd\":\"tree\"}")).RootElement
            .GetProperty("result").GetProperty("workspaces")[0].GetProperty("sessions")[0];

    private static string[]? Exited(ControlServer server)
        => TreeSession(server).TryGetProperty("exitedPanes", out var v) ? v.EnumerateArray().Select(e => e.GetString()!).ToArray() : null;

    private static FakeSessionHost.Sess First(FakeSessionHost host) => host.Workspaces[0].Sessions[0];

    [Fact]
    public void NoPaneExited_OmitsTheKey()
    {
        var (server, _) = New();
        Assert.Null(Exited(server));
    }

    [Fact]
    public void OnePaneSession_ListsItsPaneId_WhichPaneIdsDoesNotOtherwiseGive()
    {
        var (server, host) = New();
        var s = First(host);
        s.Exited.Add(s.PaneIds[0]);

        Assert.False(TreeSession(server).TryGetProperty("paneIds", out _));
        Assert.Equal(new[] { s.PaneIds[0] }, Exited(server));
    }

    [Fact]
    public void Split_ListsOnlyTheExitedPane()
    {
        var (server, host) = New();
        var s = First(host);
        s.AddPane();
        s.Exited.Add(s.PaneIds[1]);

        Assert.Equal(new[] { s.PaneIds[1] }, Exited(server));
    }

    [Fact]
    public void Swap_KeepsTheIdOnTheExitedPane()
    {
        var (server, host) = New();
        var s = First(host);
        s.AddPane();
        string exited = s.PaneIds[1];
        s.Exited.Add(exited);

        host.Swap(null);

        Assert.Equal(exited, s.PaneIds[0]);
        Assert.Equal(new[] { exited }, Exited(server));
    }
}
