using BinarySerialization;
using System.Collections.Generic;
using static Share.Protodef;
using static Share.Protosdef;

namespace WorldSvr.Protos
{
    static class CharLobby
    {
        public class S2C_GETMYCHARTR : S2C_HEADER
        {
            [FieldOrder(0)] public bool UseACSUB { get; set; }
            [FieldOrder(1)] [FieldLength(12)] public byte U0 { get; set; }
            [FieldOrder(2)] public byte U1 { get; set; }
            [FieldOrder(3)] public uint LastCharacter { get; set; }
            [FieldOrder(4)] public uint SlotOrder { get; set; }
            [FieldOrder(5)] public uint ExtendedCharCreation { get; set; }
            [FieldOrder(6)] public int U2 { get; set; }
            [FieldOrder(7)] [FieldLength(8)] public List<S2C_GETMYCHARTR_CHARACTER> Characters { get; set; }
        }

        public class S2C_GETMYCHARTR_CHARACTER
        {
            [FieldOrder(0)] public uint CharacterIdx { get; set; }
            [FieldOrder(1)] public ulong CreationDate { get; set; }
            [FieldOrder(2)] public uint Style { get; set; }
            [FieldOrder(3)] public uint LEV { get; set; }
            [FieldOrder(4)] public byte U0 { get; set; }

            [FieldOrder(5)] public uint Rank { get; set; }
            [FieldOrder(6)] public ushort U1 { get; set; }

            [FieldOrder(7)] [FieldLength(17)] public string Name { get; set; }

            [FieldOrder(8)] public ulong Reputation { get; set; }
            [FieldOrder(9)] public ulong Alz { get; set; }
            [FieldOrder(10)] public byte WorldIdx { get; set; }
            [FieldOrder(11)] public ushort X { get; set; }
            [FieldOrder(12)] public ushort Y { get; set; }

            [FieldOrder(13)] public ushort EquippedItemCount { get; set; }

            //[FieldOrder(14)] [FieldLength(1760)] public List<S2C_GETMYCHARTR_CHARACTER_EQUIPMENT> Equipments { get; set; }
        }

        public class S2C_GETMYCHARTR_CHARACTER_EQUIPMENT
        {

        }

        public class S2C_SPECIALCHAREVT : S2C_HEADER
        {
            [FieldOrder(0)] public bool IsActive { get; set; }
        }

        public class C2S_NEWMYCHARTR : C2S_HEADER
        {
            [FieldOrder(0)] public int Style { get; set; }
            [FieldOrder(1)] public bool BeginnerGuildJoinCheck { get; set; }
            [FieldOrder(2)] public byte U0 { get; set; }
            [FieldOrder(3)] public byte CharSlotIdx { get; set; }
            [FieldOrder(4)] public byte CharNameLen { get; set; }
            [FieldOrder(5)] [FieldLength(nameof(CharNameLen))] public string CharName { get; set; }
        }

        public class S2C_NEWMYCHARTR : S2C_HEADER
        {
            [FieldOrder(0)] public int CharacterIdx { get; set; }
            [FieldOrder(1)] public CharResult CharResult { get; set; }
        }

        public class C2S_CHARACTER_SLOTORDER : C2S_HEADER
        {
            [FieldOrder(0)] public int SlotOrder { get; set; }
        }

        public class S2C_CHARACTER_SLOTORDER : S2C_HEADER
        {
            [FieldOrder(0)] public bool Updated { get; set; }
        }

        public class C2S_CHECKPASSWD : C2S_HEADER
        {
            [FieldOrder(0)] public int U0 { get; set; }
            [FieldOrder(1)] [FieldLength(33)] public string Password { get; set; }
        }

        public class S2C_CHECKPASSWD : S2C_HEADER
        {
            [FieldOrder(0)] public bool Check { get; set; }
        }

        public class C2S_DELMYCHARTR : C2S_HEADER
        {
            [FieldOrder(0)] public int CharacterIdx { get; set; }
        }

        public class S2C_DELMYCHARTR : S2C_HEADER
        {
            [FieldOrder(0)] public DelResult DelResult { get; set; }
        }
    }
}
