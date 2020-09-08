using System;
using System.Net;

namespace HPT.Network
{
    public static class IPAddressExtensions
    {
        public static uint ToUInt32(this IPAddress ipAddress)
        {
            return BitConverter.ToUInt32(ipAddress.GetAddressBytes(), 0);
        }
    }
}
