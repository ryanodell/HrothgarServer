using Hrothgar.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;

namespace HrothgarSocketServer
{
    public class SocketServer
    {
        //Another comment for testing
        IPAddress mIp;
        int mPort;
        TcpListener mTCPListener;
        List<TcpClient> mClients;
        int currClient = 0;

        public List<GamePlayer> Players = new List<GamePlayer>();

        public bool KeepRunning { get; set; }

        public SocketServer()
        {
            mClients = new List<TcpClient>();
        }
        
        public async void StartListeningForIncomingConnection(IPAddress ipaddr = null, int port = 23000)
        {
            if (ipaddr == null)
            {
                ipaddr = IPAddress.Any;
            }
            mIp = ipaddr;
            mPort = port;
            Console.WriteLine($"IP Address: {ipaddr.ToString()} - Port: {mPort.ToString()}");
            mTCPListener = new TcpListener(mIp, mPort);
            try
            {
                mTCPListener.Start();
                KeepRunning = true;
                while (KeepRunning)
                {
                    var returnedByAccept = await mTCPListener.AcceptTcpClientAsync();
                    //returnedByAccept.NoDelay = true;
                    currClient++;
                    var newPlayer = new GamePlayer()
                    {
                        Id = currClient,
                        Client = returnedByAccept,
                        Position = Vector2.Zero
                    };
                    Players.Add(newPlayer);
                    Console.WriteLine($"Client connected successfully: {returnedByAccept.ToString()} - Count: {mClients.Count.ToString()}");
                    TakeCareOfNewPlayer(newPlayer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void StopServer()
        {
            try
            {
                if (mTCPListener != null)
                {
                    mTCPListener.Stop();
                }
                foreach (TcpClient c in mClients)
                {
                    c.Close();
                }
                mClients.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async void TakeCareOfNewPlayer(GamePlayer player)
        {
            NetworkStream stream = null;
            StreamReader reader = null;
            try
            {
                stream = player.Client.GetStream();
                reader = new StreamReader(stream);
                char[] buff = new char[64];
                Console.WriteLine("** Ready to read");

                string response = $"0|{player.Id.ToString()}|";
                //Asset ID:
                if (player.Id == 1)
                {
                    player.ModelId = 0;
                    response += "0";
                }
                else
                {
                    player.ModelId = 1;
                    response += "1";
                }
                byte[] buffMessage = Encoding.ASCII.GetBytes(response);
                await player.Client.GetStream().WriteAsync(buffMessage, 0, buffMessage.Length);
                SendPlayerAddToAllClients(player);
                Thread.Sleep(1000);
                AddExistingPlayersToClient(player);
                while (KeepRunning)
                {
                    int nRet = await reader.ReadAsync(buff, 0, buff.Length);
                    //Console.WriteLine($"Returned: {nRet.ToString()}");
                    if (nRet == 0)
                    {
                        RemoveClient(player.Client);
                        Console.WriteLine($"Socket Disconnect");
                        break;
                    }
                    string receivedText = new string(buff);
                    HandleDataFromClient(player, receivedText);
                    //Console.WriteLine($"** Received: {receivedText}");
                    Array.Clear(buff, 0, buff.Length);
                }
            }
            catch (Exception ex)
            {
                RemoveClient(player.Client);
                Console.WriteLine(ex.ToString());
            }
        }

        private void AddExistingPlayersToClient(GamePlayer gamePlayer)
        {
            foreach (var player in Players)
            {
                if (player.Id != gamePlayer.Id)
                {
                    var data = $"1|{player.Id}|{player.ModelId}";
                    byte[] buffMessage = Encoding.ASCII.GetBytes(data);
                    gamePlayer.Client.GetStream().WriteAsync(buffMessage, 0, buffMessage.Length);
                }
            }
        }

        private void HandleDataFromClient(GamePlayer player, string data)
        {
            var split = data.Split(' ');
            var x = split[0].Replace("{X:", string.Empty).Trim();
            var y = split[1].Replace("Y:", string.Empty).Replace("}", string.Empty).Trim();
            var dataToSend = $"2|{player.Id}|{x}|{y}";
            BroadcastToAllOtherClients(player, dataToSend);
        }

        private void SendPlayerAddToAllClients(GamePlayer gamePlayer)
        {
            var data = $"1|{gamePlayer.Id}|{gamePlayer.ModelId}";
            byte[] buffMessage = Encoding.ASCII.GetBytes(data);
            foreach (var player in Players)
            {
                if (player.Id != gamePlayer.Id)
                {
                    player.Client.GetStream().WriteAsync(buffMessage, 0, buffMessage.Length);
                }
            }
        }

        private void BroadcastToAllOtherClients(GamePlayer player, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            try
            {
                byte[] buffMessage = Encoding.ASCII.GetBytes(message);
                foreach (var gamePlayer in Players)
                {
                    if (gamePlayer.Id != player.Id)
                    {
                        gamePlayer.Client.GetStream().WriteAsync(buffMessage, 0, buffMessage.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void SendToAllPlayers(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            try
            {
                byte[] buffMessage = Encoding.ASCII.GetBytes(message);
                foreach (var gamePlayer in Players)
                {
                    gamePlayer.Client.GetStream().WriteAsync(buffMessage, 0, buffMessage.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void RemoveClient(TcpClient paramClient)
        {
            if (mClients.Contains(paramClient))
            {
                mClients.Remove(paramClient);
                Console.WriteLine($"Client Removed, Count {mClients.Count.ToString()}");
            }
        }

    }
}
