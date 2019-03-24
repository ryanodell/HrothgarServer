using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace HrothgarSocketServer
{
    internal class PacketWriter
    {
        public async void WriteAsync(TcpClient client, string data)
        {
            byte[] buffMessage = Encoding.ASCII.GetBytes(data);
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(buffMessage, 0, buffMessage.Length);
        }
    }
}
