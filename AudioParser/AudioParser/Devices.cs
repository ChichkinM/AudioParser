using System;
using System.Collections.Generic;
using System.Linq;
using Un4seen.BassWasapi;
using Un4seen.Bass;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодействия с устройствами воспроизведения.
    /// </summary>
    public static class Devices
    {
        private static List<Tuple<int, string, int>> DeviceList { get; set; }
        private static WASAPIPROC process;

        /// <summary>
        /// Функция получения списка аудио устройств.
        /// </summary>
        public static string[] GetDevices()
        {
            DeviceList = new List<Tuple<int, string, int>>();

            for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                //TODO Сделать устройства с русским названием читаемыми.
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                    DeviceList.Add(new Tuple<int, string, int>(i, device.name, device.mixfreq));
            }

            return DeviceList.Select(obj => obj.Item2).ToArray();
        }

        /// <summary>
        /// Функция подключения к аудио устройству.
        /// </summary>
        public static bool Connect(string deviceName)
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();

            process = new WASAPIPROC(Process);
            var dev = DeviceList.Find(x => x.Item2 == deviceName);

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
    }
}
