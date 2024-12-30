using CecSharp;
using System;
using System.IO;
using System.Windows.Forms;
using WinCecScreeensaver.Resources;

namespace WinCecScreensaver
{
    internal static class Program
    {
        private const string _logFileName = "WinCecScreensaver.log";

        static void Log(string message)
        {
            File.AppendAllText(_logFileName, $"{DateTime.Now.ToLongTimeString()}: {message}" + Environment.NewLine);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Log(e.Exception.ToString());
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Log(e.ExceptionObject.ToString());

            if (File.Exists(_logFileName))
            {
                File.Delete(_logFileName);
            }

            var contextMenu = new ContextMenu();
            var mainForm = new MainForm();
            var cec = new Cec();
            var on = true;

            contextMenu.MenuItems.Add(new MenuItem(Resource.ApplicationName, (s, e) => mainForm.Show(true)));
            contextMenu.MenuItems.Add(new MenuItem("-"));
            contextMenu.MenuItems.Add(new MenuItem("Exit", (s, e) => Application.Exit()));

            var trayIcon = new NotifyIcon()
            {
                Icon = Resource.screensaver_icon,
                BalloonTipTitle = Resource.ApplicationName,
                ContextMenu = contextMenu,
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

            cec.SetDisplayState(true);

            Application.ApplicationExit += (s,e) => cec.SetDisplayState(false);

            Application.Run(mainForm);
        }
    }
}
