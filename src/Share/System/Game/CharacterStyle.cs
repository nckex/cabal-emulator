namespace Share.System.Game
{
    public class CharacterStyle
    {
        public byte ClassType { get; set; }
        public byte ClassRank { get; set; }
        public byte FaceID { get; set; }
        public byte HairColor { get; set; }
        public byte HairStyle { get; set; }
        public bool ShowHelmet { get; set; }
        public byte AuraCode { get; set; }
        public byte Sex { get; set; }

        public CharacterStyle(int style)
        {
            ClassType = (byte)((style & 7) ^ ((style >> 20) & 8));
            ClassRank = (byte)((style >> 3) & 0x1F);
            FaceID = (byte)((style >> 8) & 0x1F);
            HairColor = (byte)((style >> 13) & 0x0F);
            HairStyle = (byte)((style >> 17) & 0x1F);
            ShowHelmet = ((style >> 22) & 0x01) == 0x01;
            AuraCode = (byte)((style >> 24) & 0x03);
            Sex = (byte)((style >> 26) & 0x01);
        }

        public int GetStyle()
        {
            var style = (ClassType < 8) ? ClassType : (ClassType << 20);
            style += ClassRank << 3;
            style += FaceID << 8;
            style += HairColor << 13;
            style += HairStyle << 17;
            style += (ShowHelmet ? 1 : 0) << 22;
            style += AuraCode << 24;
            style += Sex << 26;
            return style;
        }

        public static implicit operator int(CharacterStyle characterStyle)
        {
            return characterStyle.GetStyle();
        }
    }
}
