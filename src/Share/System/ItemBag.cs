using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Share.Serde;
using Share.System.Game;
using Share.System.Resources;

namespace Share.System
{
    public class ItemBag
    {
        public int Count => _bag.Count;

        private readonly ConcurrentDictionary<ushort, Item> _bag;

        public static async Task<ItemBag> BuildItemBagAsync(byte[] rawItemBagData)
        {
            if (rawItemBagData == null)
                return new ItemBag();

            var itemBagData = await CustomSerializer.Instance.DeserializeAsync<ItemBagData>(rawItemBagData);
            return new ItemBag(itemBagData);
        }

        public ItemBag(ItemBagData itemBagData = null)
        {
            _bag = new ConcurrentDictionary<ushort, Item>();

            if (itemBagData != null)
            {
                foreach (var equipment in itemBagData.Items)
                {
                    _ = _bag.TryAdd(equipment.SlotIdx, new Item(equipment.ItemIdx, equipment.Reserved, equipment.ItemOption, equipment.Duration));
                }
            }
        }

        public async Task<byte[]> GetBytes()
        {
            var bagItems = _bag
                .Select(b =>
                {
                    return new ItemBagDataItem()
                    {
                        ItemIdx = b.Value.ItemIdx,
                        Reserved = b.Value.Reserved,
                        ItemOption = b.Value.ItemOption,
                        SlotIdx = b.Key,
                        Duration = b.Value.Duration
                    };
                }).ToList();

            var itemBagData = new ItemBagData()
            {
                Items = bagItems
            };

            using var bagItemsS = await CustomSerializer.Instance.SerializeAsync(itemBagData);
            return bagItemsS.Buffer.ToArray();
        }
    }
}
