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
    /// Логика обработки аудиосигнала.
    /// </summary>
    public static class Parser
    {
        public delegate void NewDataHandler(NewDataEventArgs e);
        public static event NewDataHandler OnUpdateData;

        private static DispatcherTimer timer;
        private static Port serialPort;
        private static Network client;

        private static byte[] lastData;
        private static int[] indexes;
        private static int[] lastChanges;

        private const int minTimeWithoutChange = 3;
        private const int componentsGlobal = 3;
        private const int componentsForCalc = 15;

        private const int timerInterval = 40;


        public static void Init()
        {
            BassNet.Registration(Settings.Default.LicenseUser, Settings.Default.LicensePass);

            serialPort = new Port();
            client = new Network();

            lastData = new byte[componentsGlobal];
            lastChanges = new int[componentsGlobal];
            indexes = new int[componentsGlobal];

            Random rand = new Random();
            for (int i = 0; i < componentsGlobal; i++)
            {
                bool isContains;
                int value;

                do
                {
                    isContains = false;
                    value = rand.Next(componentsGlobal);
                    for (int j = 0; j < i; j++)
                        if (value == indexes[j])
                            isContains = true;
                }
                while (isContains);

                indexes[i] = value;
            }


            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(CalcSpectrum);
            timer.Interval = new TimeSpan(0, 0, 0, 0, timerInterval);
        }

        public static void Start()
        {
            if (!timer.IsEnabled)
                timer.Start();
        }

        public static void Close()
        {
            if (timer.IsEnabled)
                timer.Stop();
        }

        public static bool PortOpen()
        {
            serialPort.Use = true;
            serialPort.Open();
            return serialPort.Connected;
        }

        public static bool PortSetParam(string portName)
        {
            serialPort.SetPortParam(portName);
            return serialPort.Connected;
        }

        public static bool PortClose()
        {
            serialPort.Use = false;
            serialPort.Close();
            return serialPort.Connected;
        }

        public static bool NetworkConnect(string server, int port)
        {
            client.Use = true;
            client.Connect(server, port);
            return client.Connected;
        }
        
        public static bool NetworkDisconnect()
        {
            if (client.Connected)
            {
                client.Use = false;
                client.Disconnect();
            }
            return client.Connected;
        }

        private static void CalcSpectrum(object sender, EventArgs e)
        {
            float[] buffer = new float[1024];
            BassWasapi.BASS_WASAPI_GetData(buffer, Convert.ToInt32(BASSData.BASS_DATA_FFT2048));
            byte[] newData = BufferToBytes(buffer);

            if (!newData.SequenceEqual(lastData))
            {
                if (serialPort.Use)
                    serialPort.Send(newData);
                if (client.Use && client.Connected)
                    client.Send(newData);

                lastData = newData;
            }

            NewDataEventArgs args = new NewDataEventArgs(newData[0], newData[1], newData[2]);
            OnUpdateData(args);
        }

        private static byte[] BufferToBytes(float[] buffer)
        {
            byte[] result = new byte[componentsGlobal];
            int spectrumVal, b0 = 0;

            for (int i = 0; i < componentsGlobal; i++)
            {
                int sum = 0;
                for (int x = 0; x < componentsForCalc / componentsGlobal; x++)
                {
                    float peak = 0;
                    int b1 = (int)Math.Pow(2, x * 10 / (componentsForCalc - 1));

                    if (b1 > 1023)
                        b1 = 1023;
                    if (b1 <= b0)
                        b1 = b0 + 1;

                    for (; b0 < b1; b0++)
                        if (peak < buffer[1 + b0])
                            peak = buffer[1 + b0];

                    spectrumVal = (int)(Math.Sqrt(peak) * 3 * 255 - 4);

                    if (spectrumVal > 255)
                        spectrumVal = 255;
                    if (spectrumVal < 0)
                        spectrumVal = 0;

                    sum += spectrumVal;
                }
                result[indexes[i]] = (byte)(sum / (componentsForCalc / componentsGlobal));
                lastChanges[indexes[i]]++;
            }

            List<int> changingIndexes = new List<int>();
            for (int i = 0; i < result.Length; i++)
                for (int j = 0; j < result.Length; j++)
                    if (result[i] == result[j] && 
                        i != j &&
                        (!changingIndexes.Contains(i) || !changingIndexes.Contains(j)) &&
                        (lastChanges[i] >= minTimeWithoutChange * timerInterval && 
                        lastChanges[j] >= minTimeWithoutChange * timerInterval))
                    {
                        changingIndexes.Add(i);
                        changingIndexes.Add(j);

                        lastChanges[i] = 0;
                        lastChanges[j] = 0;

                        byte valueByte = result[j];
                        result[j] = result[i];
                        result[i] = valueByte;

                        int valueInt = indexes[j];
                        indexes[j] = indexes[i];
                        indexes[i] = valueInt;
                    }
            return result;
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
