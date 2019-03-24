using System;
using System.Net.Sockets;

namespace HrothgarSocketServer
{
    public class PlayerConnectRequestEventArgs : EventArgs
    {
        public TcpClient Client;

        public PlayerConnectRequestEventArgs(TcpClient client)
        {
            Client = client;
        }
    }
}
