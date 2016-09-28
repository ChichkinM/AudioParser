using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.BassWasapi;
using Un4seen.Bass;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодействия с устройствами воспроизведения.
    /// </summary>
    public static class Devices
    {
        private static List<Tuple<int, string>> DeviceList { get; set; }
        private static WASAPIPROC process;

        /// <summary>
        /// Функция получения списка аудио устройств.
        /// </summary>
        public static List<string> GetDevices()
        {
            DeviceList = new List<Tuple<int, string>>();

            for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                //TODO Поймать устройство по-умолчанию.
                //TODO Сделать устройства с русским названием читаемыми.
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                    if (device.IsDefault)
                    {
                        DeviceList.Add(new Tuple<int, string>(DeviceList[0].Item1, DeviceList[0].Item2));
                        DeviceList[0] = new Tuple<int, string>(i, device.name);
                    }
                    else
                        DeviceList.Add(new Tuple<int, string>(i, device.name));
            }

            return DeviceList.Select(obj => obj.Item2).ToList<string>();
        }

        /// <summary>
        /// Функция подключения к аудио устройству.
        /// </summary>
        public static bool Connect(string deviceName)
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();

            process = new WASAPIPROC(Process);
            int deviceId = DeviceList.Find(x => x.Item2 == deviceName).Item1;

            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            bool isBassInit = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            bool isWasApiInit = BassWasapi.BASS_WASAPI_Init(deviceId,
                0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1, 0, process, IntPtr.Zero);
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
