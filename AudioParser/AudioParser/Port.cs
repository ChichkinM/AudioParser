using System;
using System.IO.Ports;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодействия с последовательным портом.
    /// </summary>
    class Port
    {
        public bool Use { get; set; }
        public bool Connected { get; private set; }

        private SerialPort serial;


        public Port()
        {
            serial = new SerialPort();
            Use = false;
            Connected = false;
        }

        public void Open()
        {
            try
            {
                serial.Open();
            }
            catch (Exception) { };

            Connected = serial.IsOpen;
        }

        public void Close()
        {
            serial.Close();
            Connected = serial.IsOpen;
        }

        public void Send(byte[] data)
        {
            if (serial.IsOpen)
            serial.Write(data, 0, data.Length);
        }

        public void SetPortParam(string portName)
        {
            bool isUse = Use;

            if (Connected)
            {
                serial.Close();
                Use = false;
            }

            serial.PortName = portName;
            serial.BaudRate = 9600;
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;
            serial.DataBits = 8;

            if (Connected || isUse)
            {
                Open();
                Use = true;
            }
        }

        public static string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}
