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

        private SerialPort Serial;

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        public Port()
        {
            Serial = new SerialPort();
            Use = false;
            Connected = false;
        }

        /// <summary>
        /// Функция отерытия последовательного порта.
        /// </summary>
        public void Open()
        {
            //TODO Проверять доступность порта.
            try
            {
                Serial.Open();
            }
            catch (Exception) { };

            Connected = Serial.IsOpen;
        }

        /// <summary>
        /// Функция закрытия последовательного порта.
        /// </summary>
        public void Close()
        {
            Serial.Close();
            Connected = Serial.IsOpen;
        }

        /// <summary>
        /// Функция отправки данных.
        /// </summary>
        /// <param name="data">Байтовый массив данных.</param>
        public void Send(byte[] data)
        {
            if (Serial.IsOpen)
            Serial.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Функция установки параметров порта.
        /// </summary>
        /// <param name="portName">Имя порта.</param>
        public void SetPortParam(string portName)
        {
            bool isUse = Use;

            if (Connected)
            {
                Serial.Close();
                Use = false;
            }

            Serial.PortName = portName;
            Serial.BaudRate = 9600;
            Serial.Parity = Parity.None;
            Serial.StopBits = StopBits.One;
            Serial.DataBits = 8;

            if (Connected || isUse)
            {
                Open();
                Use = true;
            }
        }

        /// <summary>
        /// Функция получения имен портов.
        /// </summary>
        public static string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}
