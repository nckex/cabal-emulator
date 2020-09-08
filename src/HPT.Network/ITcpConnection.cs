using System;
using System.Threading.Tasks;

namespace HPT.Network
{
    public interface ITcpConnection
    {
        ValueTask<int> SendAsync(ArraySegment<byte> buffer);

        void Disconnect();
    }
}
