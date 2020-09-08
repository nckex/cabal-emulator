namespace Share.System.Game
{
    public class Position
    {
        public short X { get; set; }
        public short Y { get; set; }

        public Position(int position)
        {
            X = (short)((position >> 16) & 0xFFFF);
            Y = (short)(position & 0xFFFF);
        }

        public int GetPosition()
        {
            return (X << 16) + Y;
        }

        public static implicit operator int(Position position)
        {
            return position.GetPosition();
        }
    }
}
