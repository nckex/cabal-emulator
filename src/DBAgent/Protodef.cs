using System;
using System.Collections.Generic;
using System.Text;

namespace DBAgent
{
    class Protodef
    {
        public enum LastCharacterMode : byte
        {
            UpdateLastCharacter,
            UpdateCharSlotOrder,
            SelectLastCharacter
        }
    }
}
