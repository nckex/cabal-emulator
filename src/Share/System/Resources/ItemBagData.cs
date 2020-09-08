using System.Collections.Generic;
using BinarySerialization;

namespace Share.System.Resources
{
    public class ItemBagData
    {
        [FieldOrder(0)] [FieldCount(Protodef.MAX_ITEMBAG_NUM)] public List<ItemBagDataItem> Items { get; set; }
    }

    public class ItemBagDataItem
    {
        [FieldOrder(0)] public long ItemIdx { get; set; }
        [FieldOrder(1)] public uint Reserved { get; set; }
        [FieldOrder(2)] public long ItemOption { get; set; }
        [FieldOrder(3)] public ushort SlotIdx { get; set; }
        [FieldOrder(4)] public uint Duration { get; set; }
    }
}
