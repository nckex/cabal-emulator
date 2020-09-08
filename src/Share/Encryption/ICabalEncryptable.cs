namespace Share.Encryption
{
    public interface ICabalEncryptable
    {
        uint HeaderXor { get; set; }
        uint Step { get; set; }
        uint Mul { get; set; }
    }
}
