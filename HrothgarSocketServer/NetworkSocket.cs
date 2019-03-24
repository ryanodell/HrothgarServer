using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace HrothgarSocketServer
{
    public class NetworkSocket
    {
        public TcpListener Listener { get; set; }
        public List<TcpClient> Clients { get; }
        private PacketReader packetReader { get; set; }
        private PacketWriter packetWriter { get; set; }
        public event EventHandler<PlayerConnectRequestEventArgs> OnPlayerConnectRequest;
        public event EventHandler<OnPlayerInputReceivedEventArgs> OnPlayerInputReceived;

        public NetworkSocket()
        {
            packetReader = new PacketReader();
            packetWriter = new PacketWriter();
            Clients = new List<TcpClient>();
        }

        public async void ListenForIncomingConnectionsAsync(IPAddress ipAddress, int port)
        {
            Listener = new TcpListener(ipAddress, port);
            try
            {
                Listener.Start();
                Console.WriteLine($"Server is open for connections...");
                while (true)
                {
                    var client = await Listener.AcceptTcpClientAsync();
                    Console.WriteLine($"Client Attempting To Connect...");
                    OnPlayerConnectRequest(this, new PlayerConnectRequestEventArgs(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }
        }

        public void SendToAll(string data)
        {
            foreach(var client in Clients)
            {
                Write(client, data);
            }
        }


        //public async void ListenForClientInput(TcpClient client)
        //{
        //    byte[] buff = new byte[64];
        //    NetworkStream stream = null;
        //    StreamReader reader = null;
        //    try
        //    {
        //        stream = client.GetStream();
        //        var readOffset = 0;
        //        var sizeArray = new byte[2];
        //        while (true)
        //        {
        //            var nRet = await stream.ReadAsync(buff, readOffset, buff.Length);
        //            if(nRet == 0)
        //            {
        //                //Remove Client
        //                break;
        //            }                    
        //            if (readOffset == 0)
        //            {
        //                sizeArray[0] = buff[0];
        //                sizeArray[1] = buff[1];
        //            }
        //            var size = BitConverter.ToInt16(sizeArray, 0);
        //            Console.WriteLine(size);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

        public async void ListenForClientInput(TcpClient client)
        {
            char[] buff = new char[64];
            NetworkStream stream = null;
            StreamReader reader = null;
            try
            {
                stream = client.GetStream();
                reader = new StreamReader(stream);
                while (true)
                {
                    int nRet = await reader.ReadAsync(buff, 0, buff.Length);
                    if (nRet == 0)
                    {
                        RemoveClient(client);
                        Console.WriteLine($"Socket Disconnect");
                        break;
                    }
                    string receivedText = new string(buff);
                    OnPlayerInputReceived(this, new OnPlayerInputReceivedEventArgs(client, receivedText));
                    Array.Clear(buff, 0, buff.Length);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public void Write(TcpClient client, string data)
        {
            packetWriter.WriteAsync(client, data);
        }

        public void AddClient(TcpClient client)
        {
            Clients.Add(client);
        }

        public void RemoveClient(TcpClient client)
        {
            Clients.Remove(client);
            client.Dispose();
        }
    }
}
