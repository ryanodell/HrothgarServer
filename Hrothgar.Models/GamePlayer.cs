using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace Hrothgar.Models
{
    public class GamePlayer
    {
        public int Id { get; set; }
        public int ModelId { get; set; }
        public TcpClient Client { get; set; }
        public Vector2 Position { get; set; }
    }
}
