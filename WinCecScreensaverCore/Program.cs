using CecSharp;
using WinCecScreensaverCore;

const string _logFileName = "WinCecScreensaver.log";

static void Log(string message)
{
    File.AppendAllText(_logFileName, $"{DateTime.Now.ToLongTimeString()}: {message}" + Environment.NewLine);
}

// To customize application configuration such as set high DPI settings or default font,
// see https://aka.ms/applicationconfiguration.
ApplicationConfiguration.Initialize();

Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
Application.ThreadException += (s, e) => Log(e.Exception.ToString());
AppDomain.CurrentDomain.UnhandledException += (s, e) => Log(e.ExceptionObject.ToString()!);

if (File.Exists(_logFileName))
{
    File.Delete(_logFileName);
}

var contextMenuStrip = new ContextMenuStrip();
var mainForm = new MainForm();
var cec = new Cec();
var on = true;

contextMenuStrip.Items.Add(new ToolStripMenuItem(Resource.ApplicationName, null, (s, e) => mainForm.Show(true)));
contextMenuStrip.Items.Add(new ToolStripMenuItem("-"));
contextMenuStrip.Items.Add(new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit()));

var trayIcon = new NotifyIcon()
{
    Icon = Resource.screensaver_icon,
    BalloonTipTitle = Resource.ApplicationName,
    ContextMenuStrip = contextMenuStrip,
    Visible = true
};

cec.OnLogMessage += Log;
cec.OnAlert += (alert) =>
{
    Log($"ALERT: {Enum.GetName(typeof(CecAlert), alert)}");

    cec.Dispose();
    cec = new Cec();
};
mainForm.Text = Resource.ApplicationName;
mainForm.OnDisplayStateChanged += (state) =>
{
    Log($"Setting display state to {state}");
    cec.SetDisplayState(on = state);
};

Application.Run(mainForm);
