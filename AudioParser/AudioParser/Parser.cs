using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.BassWasapi;
using System.Windows.Media;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодейсвия с аудиосигналом.
    /// </summary>
    public static class Parser
    {
        //TODO Регистрацию сделать из настроек.
        private const string licenceUser = @"miffka741@yandex.ru";
        private const string licencePass = "2X22291193438";

        private const int timerInterval = 40;

        public delegate void NewDataHandler(NewDataEventArgs e);
        public static event NewDataHandler OnUpdateData;

        private static DispatcherTimer timer { get; set; }
        private static Port SerialPort { get; set; }
        private static Network Client { get; set; }

        private static byte[] LastData { get; set; }

        /// <summary>
        /// Функция инициализации работы с библиотеками.
        /// </summary>
        public static void Init()
        {
            BassNet.Registration(licenceUser, licencePass);

            SerialPort = new Port();
            Client = new Network();

            LastData = new byte[3];

            //TODO Останавливать таймер при переподключении устройств.
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(CalcSpectrum);
            timer.Interval = new TimeSpan(0, 0, 0, 0, timerInterval);
            timer.Start();
        }

        public static bool PortOpen()
        {
            SerialPort.Use = true;
            SerialPort.Open();
            return SerialPort.Connected;
        }

        public static bool PortSetParam(string portName)
        {
            SerialPort.SetPortParam(portName);
            return SerialPort.Connected;
        }

        public static bool PortClose()
        {
            SerialPort.Use = false;
            SerialPort.Close();
            return SerialPort.Connected;
        }

        public static bool NetworkConnect(string server, int port)
        {
            Client.Use = true;
            Client.Connect(server, port);
            return Client.Connected;
        }
        
        public static bool NetworkDisconnect()
        {
            if (Client.Connected)
            {
                Client.Use = false;
                Client.Disconnect();
            }
            return Client.Connected;
        }

        private static void CalcSpectrum(object sender, EventArgs e)
        {
            float[] buffer = new float[1024];
            BassWasapi.BASS_WASAPI_GetData(buffer, Convert.ToInt32(BASSData.BASS_DATA_FFT2048));
            byte[] newData = BufferToBytes(buffer);

            if (!newData.SequenceEqual(LastData))
            {
                if (SerialPort.Use)
                    SerialPort.Send(newData);
                if (Client.Use && Client.Connected)
                    Client.Send(newData);

                LastData = newData;
            }

            NewDataEventArgs args = new NewDataEventArgs(newData[0], newData[1], newData[2]);
            OnUpdateData(args);
        }

        private static byte[] BufferToBytes(float[] buffer)
        {
            List<byte> result = new List<byte>();

            int x, y;
            int b0 = 0;
            int lines = 16;

            for (x = 0; x < lines; x++)
            {
                float peak = 0;

                int b1 = (int)Math.Pow(2, x * 10 / (lines - 1));

                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;

                for (; b0 < b1; b0++)
                {
                    if (peak < buffer[1 + b0]) peak = buffer[1 + b0];
                }

                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);

                if (y > 255) y = 255;
                if (y < 0) y = 0;

                result.Add((byte)y);
            }

            byte[] res = new byte[3];
            res[0] = (byte)((result[0] + result[1] + result[2] + result[3] + result[4]) / 5);
            res[1] = (byte)((result[5] + result[6] + result[7] + result[8] + result[9]) / 5);
            res[2] = (byte)((result[10] + result[11] + result[12] + result[13] + result[14] + result[15]) / 6);

            return res;
        }
    }

    public class NewDataEventArgs : EventArgs
    {
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public NewDataEventArgs(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
