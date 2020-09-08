using System.Collections.Generic;
using BinarySerialization;
using static Share.Protodef;
using static Share.Protosdef;

namespace Share.Protos
{
    public static class CharLobby
    {
        public class IPS_GETCHARS : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
        }

        public class IPS_RESULT_GETCHARS : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public int LastSelectedCharacterIdx { get; set; }
            [FieldOrder(2)] public int CharSlotOrder { get; set; }
            [FieldOrder(3)] public byte CharCount { get; set; }
            [FieldOrder(4)] [FieldCount(nameof(CharCount))] public List<IPS_RESULT_GETCHARS_CHARACTER> Characters { get; set; }
        }

        public class IPS_RESULT_GETCHARS_CHARACTER
        {
            [FieldOrder(0)] public int CharacterIdx { get; set; }
            [FieldOrder(1)] [FieldLength(MAX_USERNAME_LENGHT)] public string Name { get; set; }
            [FieldOrder(2)] public int LEV { get; set; }
            [FieldOrder(3)] public int Style { get; set; }
            [FieldOrder(4)] public byte WorldIdx { get; set; }
            [FieldOrder(5)] public int Position { get; set; }
            [FieldOrder(6)] public int Rank { get; set; }
            [FieldOrder(7)] public int CreationDate { get; set; }
            [FieldOrder(8)] public int EquipmentDataLen { get; set; }
            [FieldOrder(9)] [FieldLength(nameof(EquipmentDataLen))] public byte[] EquipmentData { get; set; }
        }

        public class IPS_NEWCHAR : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public int Style { get; set; }
            [FieldOrder(3)] public byte CharSlotIdx { get; set; }
            [FieldOrder(4)] public byte CharNameLen { get; set; }
            [FieldOrder(5)] [FieldLength(nameof(CharNameLen))] public string CharName { get; set; }
        }

        public class IPS_RESULT_NEWCHAR : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public int CharachterIdx { get; set; }
            [FieldOrder(2)] public CharResult CharResult { get; set; }
        }

        public class IPS_CHAR_SLOTORDER : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public int SlotOrder { get; set; }
        }

        public class IPS_RESULT_CHAR_SLOTORDER : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public bool Updated { get; set; }
        }

        public class IPS_CHECKPASSWD : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public byte PasswordLen { get; set; }
            [FieldOrder(3)] [FieldLength(nameof(PasswordLen))] public string Password { get; set; }
        }

        public class IPS_RESULT_CHECKPASSWD : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public bool Checked { get; set; }
        }

        public class IPS_DELCHAR : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int CharacterIdx { get; set; }
        }

        public class IPS_RESULT_DELCHAR : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public DelResult DelResult { get; set; }
        }
    }
}
