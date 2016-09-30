using System;
using System.Collections.Generic;
using System.Linq;
using Un4seen.BassWasapi;
using Un4seen.Bass;
using AudioDeviceUtil;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодействия с устройствами воспроизведения.
    /// </summary>
    public static class Devices
    {
        private static List<Tuple<int, string, int>> deviceList;
        private static WASAPIPROC process;
        private static AudioDeviceManager deviceManager;
        
        public static void Init()
        {
            deviceManager = new AudioDeviceManager();
        }

        public static string[] GetDevices()
        {
            deviceList = new List<Tuple<int, string, int>>();

            for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                    deviceList.Add(new Tuple<int, string, int>(i, "", device.mixfreq));
            }

            List<AudioDevice> devices = deviceManager.PlaybackDevices;
            for (int i = 0; i < deviceList.Count; i++)
                if (devices[i].DeviceState == AudioDeviceStateType.Active)
                    if (devices[i].IsDefaultDevice && i != 0)
                    {
                        var deviceFirst = deviceList[0];
                        deviceList[0] = new Tuple<int, string, int>
                            (deviceList[i].Item1, devices[i].FriendlyName, deviceList[i].Item3);
                        deviceList[i] = deviceFirst;
                    }
                    else
                        deviceList[i] = new Tuple<int, string, int>
                                                    (deviceList[i].Item1, devices[i].FriendlyName, deviceList[i].Item3);

            return deviceList.Select(obj => obj.Item2).ToArray();
        }

        public static bool Connect(string deviceName)
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();

            process = new WASAPIPROC(Process);
            var dev = deviceList.Find(x => x.Item2 == deviceName);

            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            bool isBassInit = Bass.BASS_Init(0, dev.Item3, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            bool isWasApiInit = BassWasapi.BASS_WASAPI_Init(dev.Item1, 0, 0, 
                BASSWASAPIInit.BASS_WASAPI_BUFFER, 1, 0, process, IntPtr.Zero);
            BassWasapi.BASS_WASAPI_Start();

            bool init = false;
            if (isBassInit && isWasApiInit)
                init = true;

            return init;
        }

        private static int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        public static void SetDefaultDevice(string deviceName)
        {
            deviceManager.DefaultPlaybackDeviceName = deviceName;
        }

        public static bool IsDefaultDevice(string deviceName)
        {
            bool isDefualt = false;
            if (deviceManager.DefaultPlaybackDeviceName == deviceName)
                isDefualt = true;

            return isDefualt;
        }
    }
}
