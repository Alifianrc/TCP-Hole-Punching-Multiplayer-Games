using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TCP_Server
{
    class Server
    {
        private List<ClientHandler>     m_clients;
        private List<Room>              m_rooms;

        private int                     m_port;
        private TcpListener             m_listener;
        private bool                    m_isOnline;

        public Server()
        {
            m_clients   = new List<ClientHandler>();
            m_rooms     = new List<Room>();

            m_port      = 1001;
        }

        public void StartListening()
        {
            try
            {
                m_listener = new TcpListener(IPAddress.Any, m_port);
                m_listener.Start();
                m_isOnline = true;

                Console.WriteLine("------- Server Port " + m_port + " Created -------\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Server Start Error : " + e.Message);
            }

            Thread beginListenThread = new Thread(BeginAcceptClient);
            beginListenThread.Start();
        }
        private void BeginAcceptClient()
        {
            while (m_isOnline)
            {
                TcpClient       tcpClient = m_listener.AcceptTcpClient();
                ClientHandler   client = new ClientHandler(tcpClient, this);
            }
        }

        public List<ClientHandler> Clients
        {
            get         => m_clients;
            private set => m_clients = value;
        }
        public List<Room> Rooms
        {
            get         => m_rooms;
            private set => m_rooms = value;
        }
        public bool IsOnline
        {
            get         => m_isOnline;
            private set => m_isOnline = value;
        }
    }
}
