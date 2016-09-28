using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace AudioParser
{
    class Network
    {
        private TcpClient Client { get; set; }
        private NetworkStream ClientStream { get; set; }

        public bool Use { get; set; }
        public bool Connected { get; private set; }

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        public Network()
        {
            Use = false;
            Connected = false;
        }

        /// <summary>
        /// Функция подключения к серверу.
        /// </summary>
        /// <param name="server">Ip адрес сервера.</param>
        /// <param name="port">Порт сервера.</param>
        public void Connect(string server, int port)
        {
            Client = new TcpClient();

            //TODO Проверять доступность сервера.
            try
            {
                Client.Connect(server, port);
                ClientStream = Client.GetStream();
            }
            catch (Exception) { };

            Connected = Client.Connected;
        }

        /// <summary>
        /// Функция отключения от сервера.
        /// </summary>
        public void Disconnect()
        {
            if(Client.Connected)
                Client.Close();

            Connected = Client.Connected;
        }

        /// <summary>
        /// Функция отправки данных.
        /// </summary>
        /// <param name="data">Байтовый массив данных.</param>
        public void Send(byte[] data)
        {
            if(Client.Connected)
                ClientStream.Write(data, 0, data.Length);
        }
    }
}
