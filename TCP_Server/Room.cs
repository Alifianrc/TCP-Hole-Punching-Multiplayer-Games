using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Server
{
    class Room
    {
        private List<ClientHandler> m_clients;


        public List<ClientHandler> Clients
        {
            get         => m_clients;
            private set => m_clients = value;
        }
    }
}
