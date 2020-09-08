namespace Share.System.Game
{
    public class Item
    {
        public long ItemIdx { get; set; }
        public uint Reserved { get; set; }
        public long ItemOption { get; set; }
        public uint Duration { get; set; }

        public Item(long itemIdx, uint reserved, long itemOption, uint duration)
        {
            ItemIdx = itemIdx;
            Reserved = reserved;
            ItemOption = itemOption;
            Duration = duration;
        }
    }
}
