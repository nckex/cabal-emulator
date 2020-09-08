using System;
using System.Net;
using System.Net.Sockets;

namespace HPT.Network
{
    public abstract class TcpSession : TcpConnection, ITcpSession
    {
        public string Signature { get; }
        public ushort SessionId { get; }
        public uint SessionTime { get; }
        public IPAddress IPAddress { get; }
        public uint IPAddress32 { get; }

        protected TcpSession(Socket socket, ushort sessionId) : base(socket)
        {
            var sessionTime = (uint)DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var ipv4Address = ((IPEndPoint)socket.RemoteEndPoint).Address.MapToIPv4();

            Signature = $"{sessionId}@{sessionTime}:{ipv4Address.ToString()}";
            SessionId = sessionId;
            SessionTime = sessionTime;
            IPAddress = ipv4Address;
            IPAddress32 = ipv4Address.ToUInt32();
        }
    }
}
