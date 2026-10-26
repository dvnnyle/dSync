namespace TimeResync;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(18, 22, 30);
    private static readonly Color PanelColor = Color.FromArgb(28, 34, 45);
    private static readonly Color MutedColor = Color.FromArgb(157, 169, 188);
    private static readonly Color AccentColor = Color.FromArgb(92, 137, 255);

    private readonly Button _resyncButton = new();
    private readonly Button _restartButton = new();
    private readonly Button _serverButton = new();
    private readonly TextBox _serverBox = new();
    private readonly Label _status = new();
    private readonly TextBox _output = new();
    private readonly List<Button> _actionButtons = [];

    public MainForm()
    {
        Text = "Dsync";
        Icon = LoadAppIcon();
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(620, 610);
        MinimumSize = new Size(640, 570);
        BackColor = BackgroundColor;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        var header = new Panel { BackColor = BackgroundColor, Dock = DockStyle.Top, Height = 100, Padding = new Padding(28, 20, 28, 10) };
        var title = new Label { Text = "Dsync", Font = new Font("Segoe UI", 23F, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(26, 14) };
        var subtitle = new Label { Text = "Windows time synchronization", Font = new Font("Segoe UI", 10F), ForeColor = MutedColor, AutoSize = true, Location = new Point(29, 60) };
        header.Controls.AddRange([title, subtitle]);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 330,
            Padding = new Padding(24, 6, 24, 0),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = BackgroundColor,
            AutoScroll = true
        };

        var syncCard = MakeCard("Quick sync", "Run w32tm /resync using your current Windows time settings.", 82);
        _resyncButton.Text = "Resync now";
        StyleButton(_resyncButton, primary: true, 130);
        _resyncButton.Click += async (_, _) => await RunActionAsync("Synchronizing time", [new("w32tm.exe", "/resync")]);
        syncCard.Controls.Add(_resyncButton);

        var restartCard = MakeCard("Restart the time service", "Stop and start Windows Time, then request a sync.", 82);
        _restartButton.Text = "Restart + sync";
        StyleButton(_restartButton, primary: false, 140);
        _restartButton.Click += async (_, _) => await RunActionAsync("Restarting Windows Time", [
            new("net.exe", "stop w32time"), new("net.exe", "start w32time"), new("w32tm.exe", "/resync")
        ]);
        restartCard.Controls.Add(_restartButton);

        var serverCard = MakeCard("Use a specific time server", "Set the manual peer, restart the service, and sync.", 130);
        _serverBox.Text = "time.windows.com,0x8";
        _serverBox.Location = new Point(18, 78);
        _serverBox.Size = new Size(280, 34);
        _serverBox.BackColor = Color.FromArgb(18, 22, 30);
        _serverBox.ForeColor = Color.White;
        _serverBox.BorderStyle = BorderStyle.FixedSingle;
        _serverBox.Font = new Font("Segoe UI", 10F);
        serverCard.Controls.Add(_serverBox);
        _serverButton.Text = "Set server + sync";
        StyleButton(_serverButton, primary: true, 155);
        _serverButton.Location = new Point(318, 74);
        _serverButton.Click += async (_, _) => await SyncWithServerAsync();
        serverCard.Controls.Add(_serverButton);

        actions.Controls.Add(syncCard);
        actions.Controls.Add(restartCard);
        actions.Controls.Add(serverCard);

        var resultPanel = new Panel { Dock = DockStyle.Fill, BackColor = PanelColor, Padding = new Padding(18), Margin = new Padding(24, 0, 24, 20) };
        var resultHeader = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = PanelColor };
        var resultTitle = new Label { Text = "Activity", ForeColor = Color.White, Font = new Font("Segoe UI", 11F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
        _status.Text = "Ready";
        _status.ForeColor = MutedColor;
        _status.AutoSize = true;
        _status.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _status.Location = new Point(430, 3);
        resultHeader.Controls.AddRange([resultTitle, _status]);

        _output.Multiline = true;
        _output.ReadOnly = true;
        _output.ScrollBars = ScrollBars.Vertical;
        _output.Dock = DockStyle.Fill;
        _output.BackColor = Color.FromArgb(18, 22, 30);
        _output.ForeColor = Color.FromArgb(204, 214, 230);
        _output.BorderStyle = BorderStyle.None;
        _output.Font = new Font("Consolas", 9.5F);
        _output.Text = "Choose an action above to synchronize your clock.";
        resultPanel.Controls.Add(_output);
        resultPanel.Controls.Add(resultHeader);

        var footer = new Label
        {
            Text = "danny | dvnny.no",
            ForeColor = MutedColor,
            Font = new Font("Segoe UI", 8.5F),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Bottom,
            Height = 28,
            BackColor = BackgroundColor
        };

        Controls.Add(resultPanel);
        Controls.Add(actions);
        Controls.Add(header);
        Controls.Add(footer);
        _actionButtons.AddRange([_resyncButton, _restartButton, _serverButton]);
        Resize += (_, _) => PositionStatus();
        PositionStatus();
    }

    private static Panel MakeCard(string heading, string description, int height)
    {
        var card = new Panel
        {
            BackColor = PanelColor,
            Width = 540,
            Height = height,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(18)
        };
        var title = new Label { Text = heading, AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(16, 12) };
        var detail = new Label { Text = description, AutoSize = true, MaximumSize = new Size(330, 0), ForeColor = MutedColor, Font = new Font("Segoe UI", 9F), Location = new Point(17, 40) };
        card.Controls.AddRange([title, detail]);
        return card;
    }

    private void StyleButton(Button button, bool primary, int width)
    {
        button.Size = new Size(width, 36);
        button.Location = new Point(370, 23);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(76, 88, 110);
        button.BackColor = primary ? AccentColor : Color.FromArgb(39, 47, 62);
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }

    private void PositionStatus()
    {
        _status.Left = Math.Max(160, ClientSize.Width - 225);
    }

    private async Task SyncWithServerAsync()
    {
        string server = _serverBox.Text.Trim();
        if (server.Length == 0 || server.Contains('"') || server.Contains('\r') || server.Contains('\n'))
        {
            SetStatus("Enter a valid server", Color.FromArgb(255, 112, 112));
            return;
        }

        await RunActionAsync($"Configuring {server}", [
            new("w32tm.exe", $"/config /manualpeerlist:\"{server}\" /syncfromflags:manual /update"),
            new("net.exe", "stop w32time"), new("net.exe", "start w32time"), new("w32tm.exe", "/resync")
        ]);
    }

    private async Task RunActionAsync(string activity, IReadOnlyList<(string File, string Arguments)> commands)
    {
        SetBusy(true);
        SetStatus(activity + "…", AccentColor);
        _output.Clear();
        var log = new System.Text.StringBuilder();
        bool succeeded = true;

        try
        {
            foreach (var command in commands)
            {
                log.AppendLine($"> {command.File} {command.Arguments}");
                var result = await RunCommandAsync(command.File, command.Arguments);
                if (result.Output.Length > 0) log.AppendLine(result.Output.TrimEnd());
                if (result.Error.Length > 0) log.AppendLine(result.Error.TrimEnd());
                log.AppendLine($"Exit code: {result.ExitCode}");
                log.AppendLine();
                succeeded &= result.ExitCode == 0;
                _output.Text = log.ToString();
                _output.SelectionStart = _output.TextLength;
                _output.ScrollToCaret();
            }
            SetStatus(succeeded ? "Completed" : "Finished with an error",
                succeeded ? Color.FromArgb(105, 220, 150) : Color.FromArgb(255, 112, 112));
        }
        catch (Exception ex)
        {
            log.AppendLine(ex.Message);
            _output.Text = log.ToString();
            SetStatus("Could not complete", Color.FromArgb(255, 112, 112));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private static async Task<(int ExitCode, string Output, string Error)> RunCommandAsync(string file, string arguments)
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = file,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await outputTask, await errorTask);
    }

    private void SetBusy(bool busy)
    {
        foreach (Button button in _actionButtons) button.Enabled = !busy;
        _serverBox.Enabled = !busy;
    }

    private void SetStatus(string text, Color color)
    {
        _status.Text = text;
        _status.ForeColor = color;
    }

    private static Icon LoadAppIcon()
    {
        using Stream stream = typeof(MainForm).Assembly.GetManifestResourceStream("Dsync.ico")
            ?? throw new InvalidOperationException("The Dsync icon resource is missing.");
        using var source = new Icon(stream);
        return (Icon)source.Clone();
    }
}
