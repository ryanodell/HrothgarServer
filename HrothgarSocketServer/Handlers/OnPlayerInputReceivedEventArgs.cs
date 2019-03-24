using System;
using System.Net.Sockets;

namespace HrothgarSocketServer
{
    public class OnPlayerInputReceivedEventArgs : EventArgs
    {
        public TcpClient Client;
        public string Data;

        public OnPlayerInputReceivedEventArgs(TcpClient client, string data)
        {
            Client = client;
            Data = data;
        }
    }
}
