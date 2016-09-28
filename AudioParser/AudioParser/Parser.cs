using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.BassWasapi;
using AudioParser.Properties;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодейсвия с аудиосигналом.
    /// </summary>
    public static class Parser
    {
        private const int timerInterval = 40;

        public delegate void NewDataHandler(NewDataEventArgs e);
        public static event NewDataHandler OnUpdateData;

        private static DispatcherTimer Timer;
        private static Port SerialPort;
        private static Network Client;

        private static byte[] LastData;
        private static int[] LastChanges;
        private static int[] Indexes;

        /// <summary>
        /// Функция инициализации работы с библиотеками.
        /// </summary>
        public static void Init()
        {
            BassNet.Registration(Settings.Default.LicenseUser, Settings.Default.LicensePass);

            SerialPort = new Port();
            Client = new Network();

            LastData = new byte[3];
            LastChanges = new int[3];
            Indexes = new int[3] { 0, 1, 2 };

            Timer = new DispatcherTimer();
            Timer.Tick += new EventHandler(CalcSpectrum);
            Timer.Interval = new TimeSpan(0, 0, 0, 0, timerInterval);
        }

        public static void Start()
        {
            if (!Timer.IsEnabled)
                Timer.Start();
        }

        public static void Close()
        {
            if (Timer.IsEnabled)
                Timer.Stop();
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

            int x, y, b0 = 0, lines = 16;

            for (x = 0; x < lines; x++)
            {
                float peak = 0;

                int b1 = (int)Math.Pow(2, x * 10 / (lines - 1));
                
                if (b1 > 1023)
                    b1 = 1023;
                if (b1 <= b0)
                    b1 = b0 + 1;

                for (; b0 < b1; b0++)
                    if (peak < buffer[1 + b0]) peak = buffer[1 + b0];

                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);

                if (y > 255) y = 255;
                if (y < 0) y = 0;

                result.Add((byte)y);
            }

            byte[] res = new byte[3];
            res[Indexes[0]] = (byte)((result[0] + result[1] + result[2] + result[3] + result[4]) / 5);
            res[Indexes[1]] = (byte)((result[5] + result[6] + result[7] + result[8] + result[9]) / 5);
            res[Indexes[2]] = (byte)((result[10] + result[11] + result[12] + result[13] + result[14] + result[15]) / 6);

            List<int> changingIndexes = new List<int>();
            for (int i = 0; i < res.Length; i++)
                for (int j = 0; j < res.Length; j++)
                    if (res[i] == res[j] && i != j &&
                        (!changingIndexes.Contains(i) || !changingIndexes.Contains(j)))
                    {
                        changingIndexes.Add(i);
                        changingIndexes.Add(j);

                        byte valueByte = res[j];
                        res[j] = res[i];
                        res[i] = valueByte;

                        int valueInt = Indexes[j];
                        Indexes[j] = Indexes[i];
                        Indexes[i] = valueInt;
                    }

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
