using HrothgarData;
using HrothgarData.Repositories;
using HrothgarSocketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HrothgarServer
{
    public class GameServer
    {
        public TcpListener TcpListener { get; }
        public IPAddress IpAddr { get; }
        public int Port { get; }
        NetworkSocket socket = new NetworkSocket();
        public List<GameClient> GameClients = new List<GameClient>();
        public AccountRepository AccountRepo;

        public GameServer(IPAddress ipaddr = null, int port = 23000)
        {
            if (ipaddr == null)
            {
                ipaddr = IPAddress.Any;
            }
            IpAddr = ipaddr;
            Port = port;
        }

        public bool LoadData()
        {
            AccountRepo = new AccountRepository();
            return true;
        }

        public void StartSever()
        {
            socket.ListenForIncomingConnectionsAsync(IpAddr, Port);
            socket.OnPlayerConnectRequest += OnClientConnect;
            socket.OnPlayerInputReceived += HandlePlayerInput;
            while (true)
            {
                var sendToAllMessage = Console.ReadLine();
                //socket.SendToAll(sendToAllMessage);
                foreach(var client in GameClients)
                {
                    socket.Write(client.Client, sendToAllMessage);
                }
            }
        }

        public void HandlePlayerInput(object sender, OnPlayerInputReceivedEventArgs args)
        {
            Console.WriteLine(args.Data);
            var splitData = args.Data.Split('|');
            if(int.TryParse(splitData[0], out var requestTypeInt))
            {
                var request = (eClientRequest)requestTypeInt;
                switch (request)
                {
                    case eClientRequest.CharacterSelectScreen:
                        Console.WriteLine("Character Selection Screen");
                        break;
                    default:
                        break;
                }
            }
        }

        public async void OnClientConnect(object sender, PlayerConnectRequestEventArgs args)
        {
            char[] buff = new char[64];
            var stream = args.Client.GetStream();
            var reader = new StreamReader(stream);
            try
            {
                var client = args.Client;
                int nRet = await reader.ReadAsync(buff, 0, buff.Length);
                if (nRet == 0)
                {
                    //Remove the client
                    return;
                }
                string receivedText = new string(buff);
                Console.WriteLine(receivedText);
                var split = receivedText.Split('|');
                var userName = split[0].Trim();
                var password = split[1].Trim();
                var account = AccountRepo.GetAccountByUsernameAndPassword(userName, password);
                if(account != null)
                {
                    socket.AddClient(client);
                    var gameClient = new GameClient();
                    gameClient.Client = client;
                    gameClient.Account = new Account()
                    {
                        AccountId = account.AccountId,
                        AccountName = account.Name,
                        Plvl = account.Plvl,
                        MaxPlvl = account.MaxPlvl
                    };
                    socket.ListenForClientInput(client);
                    var responseCode = ((ushort)eServerConnectResponse.Success).ToString();
                    socket.Write(client, responseCode);
                    Console.WriteLine($"{userName} Successfully Authenticated");
                }
                else
                {
                    var responseCode = ((ushort)eServerConnectResponse.IncorrectUsernameOrPassword).ToString();
                    socket.Write(client, responseCode);
                    socket.RemoveClient(client);
                    Console.WriteLine($"{userName} Failed Authenticated Removing");
                }
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{eServerConnectResponse.Fail} - {ex.ToString()}");
            }
        }
    }
}
