using HrothgarSocketServer;
using System;
using System.Threading;

namespace HrothgarServer
{
    class Program
    {
        static SocketServer server;

        static void Main(string[] args)
        {
            server = new SocketServer();
            server.StartListeningForIncomingConnection();
            while(true)
            {
                //var input = Console.ReadLine();
                //server.SendToAllPlayers(input);
                Thread.Sleep(1000);
            }
        }
    }
}
