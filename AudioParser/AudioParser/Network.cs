using System;
using System.Net.Sockets;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодействия с сетевым подключением.
    /// </summary>
    class Network
    {
        public bool Use { get; set; }
        public bool Connected { get; private set; }

        private TcpClient client;
        private NetworkStream clientStream;


        public Network()
        {
            Use = false;
            Connected = false;
        }

        public void Connect(string server, int port)
        {
            client = new TcpClient();

            try
            {
                client.Connect(server, port);
                clientStream = client.GetStream();
            }
            catch (Exception) { };

            Connected = client.Connected;
        }

        public void Disconnect()
        {
            if(client.Connected)
                client.Close();

            Connected = client.Connected;
        }

        public void Send(byte[] data)
        {
            if(client.Connected)
                clientStream.Write(data, 0, data.Length);
        }
    }
}
