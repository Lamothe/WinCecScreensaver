using System;
using System.Runtime.InteropServices;

namespace WinCecScreensaver
{
    public static class Win32
    {
        public static readonly Guid GuidConsoleDisplayState = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");
        public const int WmPowerBroadcast = 0x0218;
        public const int PbtPowerSettingsChange = 0x8013;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public int DataLength;
            public int Data;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);
    }
}