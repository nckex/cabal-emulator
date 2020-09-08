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
            [FieldOrder(3)] public int LastCharacter { get; set; }
            [FieldOrder(4)] public int SlotOrder { get; set; }
            [FieldOrder(5)] public int ExtendedCharCreation { get; set; }
            [FieldOrder(6)] public List<S2C_GETMYCHARTR_CHARACTER> Characters { get; set; }
        }

        public class S2C_GETMYCHARTR_CHARACTER
        {
            [FieldOrder(0)] public int CharacterIdx { get; set; }
            [FieldOrder(1)] public long CreationDate { get; set; }
            [FieldOrder(2)] public int Style { get; set; }
            [FieldOrder(3)] public int LEV { get; set; }
            [FieldOrder(4)] public int Rank { get; set; }
            [FieldOrder(5)] [FieldLength(9)] public byte U1 { get; set; }
            [FieldOrder(6)] public byte WorldIdx { get; set; }
            [FieldOrder(7)] public short PosX { get; set; }
            [FieldOrder(8)] public short PosY { get; set; }
            [FieldOrder(9)] [FieldLength(688 + 112)] public byte U2 { get; set; }
            [FieldOrder(10)] public byte NameLen { get; set; }
            [FieldOrder(11)] [FieldLength(nameof(NameLen))] public string Name { get; set; }
            [FieldOrder(12)] [FieldLength(8)] public byte U3 { get; set; }
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
