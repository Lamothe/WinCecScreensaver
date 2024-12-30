using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WinCecScreeensaver.Resources;

namespace WinCecScreensaver
{
    public partial class MainForm : Form
    {
        public readonly Guid _guidConsoleDisplayState = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");
        public const int _wmPowerBroadcast = 0x0218;
        public const int _pbtPowerSettingsChange = 0x8013;
        public bool _hidden = true;

        public delegate void DisplayStateChangedHandler(bool on);
        public DisplayStateChangedHandler OnDisplayStateChanged { get; set; }

        public MainForm()
        {
            InitializeComponent();

            Text = Resource.ApplicationName;
            Win32.RegisterPowerSettingNotification(Handle, ref _guidConsoleDisplayState, 0);

            var standByButton = new Button { Text = "&Stand By" };
            standByButton.Click += (s, e) => OnDisplayStateChanged(false);
            Controls.Add(standByButton);

            var powerOnButton = new Button { Text = "&Power On", Top = standByButton.Height };
            powerOnButton.Click += (s, e) => OnDisplayStateChanged(true);
            Controls.Add(powerOnButton);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
            
            base.OnClosing(e);
        }

        public void Show(bool force)
        {
            if (force)
            {
                _hidden = false;
            }

            Show();
        }

        protected override void OnShown(EventArgs e)
        {
            if (_hidden)
            {
                Hide();
            }

            base.OnShown(e);
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == _wmPowerBroadcast && message.WParam.ToInt32() == _pbtPowerSettingsChange)
            {
                var result = Marshal.PtrToStructure(message.LParam, typeof(Win32.PowerBroadcastSetting))
                    ?? throw new Exception("Failed to marshall PowerBroadcastSetting");

                var settings = (Win32.PowerBroadcastSetting)result;
                if (settings.PowerSetting == _guidConsoleDisplayState)
                {
                    switch (settings.Data)
                    {
                        case 0: OnDisplayStateChanged(false); break;
                        case 1: OnDisplayStateChanged(true); break;
                    }
                }
            }

            base.WndProc(ref message);
        }
    }
}
