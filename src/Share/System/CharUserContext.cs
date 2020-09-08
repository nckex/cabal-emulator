using Share.System.Game;

namespace Share.System
{
    public class CharUserContext : UserContext
    {
        public int CharacterIdx { get; set; }
        
        public string Name { get; set; }

        public CharacterStyle CharacterStyle { get; set; }

        public int LEV { get; set; }
        public long EXP { get; set; }
        public int STR { get; set; }
        public int DEX { get; set; }
        public int INT { get; set; }
        public int PNT { get; set; }

        public short WorldIdx { get; set; }
        public Position Position { get; set; }

        public ItemBag Equipment { get; set; }
        public ItemBag Inventory { get; set; }

        public bool IsConnected { get; set; }
        public bool IsDead { get; set; }
    }
}
