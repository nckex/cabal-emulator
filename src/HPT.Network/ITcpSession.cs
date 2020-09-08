namespace HPT.Network
{
    public interface ITcpSession : ITcpConnection
    {
        string Signature { get; }
        ushort SessionId { get; }
        uint SessionTime { get; }
    }
}
