using CecSharp;
using System;

namespace WinCecScreensaver
{
    public class Cec : CecCallbackMethods
    {
        private static readonly LibCECConfiguration _libCecConfiguration = new LibCECConfiguration();
        private readonly LibCecSharp _libCecSharp;
        private readonly CecAdapter _adapter = null;

        public delegate void LogMessageHandler(string message);
        public event LogMessageHandler OnLogMessage;

        public delegate void CecAlertHandler(CecAlert type);
        public event CecAlertHandler OnAlert;

        public Cec()
        {
            _libCecConfiguration.DeviceTypes.Types[0] = CecDeviceType.RecordingDevice;
            _libCecConfiguration.DeviceName = Environment.MachineName;
            _libCecConfiguration.ClientVersion = LibCECConfiguration.CurrentVersion;

            _libCecSharp = new LibCecSharp(this, _libCecConfiguration);
            _libCecSharp.InitVideoStandalone();
            var adapters = _libCecSharp.FindAdapters(string.Empty);

            if (adapters.Length == 0)
            {
                throw new Exception("No CEC adapters");
            }

            _adapter = adapters[0];
            if (!_libCecSharp.Open(_adapter.ComPort, 1000))
            {
                throw new Exception($"Failed to open device at {_adapter.ComPort}");
            }
        }

        public void SetDisplayState(bool on)
        {
            if (on)
            {
                _libCecSharp.PowerOnDevices(CecLogicalAddress.Broadcast);
            }
            else
            {
                _libCecSharp.StandbyDevices(CecLogicalAddress.Broadcast);
            }
        }

        public override int ReceiveAlert(CecAlert alert, CecParameter data)
        {
            OnAlert?.Invoke(alert);
            return base.ReceiveAlert(alert, data);
        }

        public override int ReceiveLogMessage(CecLogMessage message)
        {
            if ((message.Level & (CecLogLevel.Error | CecLogLevel.Warning | CecLogLevel.Notice)) != 0)
            {
                OnLogMessage?.Invoke($"{Enum.GetName(typeof(CecLogLevel), message.Level).ToUpper()}: {message.Message}");
            }

            return base.ReceiveLogMessage(message);
        }

        public CecPowerStatus GetDevicePowerStatus(CecLogicalAddress cecLogicalAddress)
        {
            return _libCecSharp.GetDevicePowerStatus(cecLogicalAddress);
        }
    }
}