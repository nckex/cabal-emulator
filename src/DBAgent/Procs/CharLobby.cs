using Dapper;
using DBAgent.Resources;
using HPT.Logging;
using Microsoft.Extensions.Logging;
using Share.Serde;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static DBAgent.Protodef;
using static Share.Protos.CharLobby;

namespace DBAgent.Procs
{
    class CharLobby
    {
        public static async Task OnIPC_GETCHARS(DBAgentSession session, ArraySegment<byte> buffer)
        {
            var ipsGetChars = await IPCSerializer.Instance.DeserializeAsync<IPS_GETCHARS>(buffer);

            using var conn = new SqlConnection(Config.Instance.DBAgent.ConnectionString);

            var spCabalGetCharacters = await conn.QueryAsync<SP_CABAL_GET_CHARACTERS>("sp_cabal_get_characters", new
            {
                pUserNum = ipsGetChars.UserNum
            }, commandType: CommandType.StoredProcedure);

            var spLastCharacter = await conn.QueryFirstOrDefaultAsync<SP_CABAL_LAST_CHARACTER>("sp_cabal_last_character", new
            {
                pMode = LastCharacterMode.SelectLastCharacter,
                pIndex = ipsGetChars.UserNum
            }, commandType: CommandType.StoredProcedure);

            if (spLastCharacter == null)
            {
                CustomLogger<CharLobby>.Instance
                    .LogError("OnIPC_GETCHARS(): LastCharacter not found! UserNum({0})",
                    ipsGetChars.UserNum);

                return;
            }

            var ipsResultGetCharsCharacters = new List<IPS_RESULT_GETCHARS_CHARACTER>(spCabalGetCharacters.Count());
            foreach (var spChar in spCabalGetCharacters)
            {
                var ipsResultChar = new IPS_RESULT_GETCHARS_CHARACTER()
                {
                    CharacterIdx = spChar.CharacterIdx,
                    Name = spChar.Name,
                    LEV = spChar.LEV,
                    Style = spChar.Style,
                    WorldIdx = spChar.WorldIdx,
                    Position = spChar.Position,
                    Rank = spChar.Rank,
                    CreationDate = spChar.CreationDate,
                    EquipmentDataLen = spChar.EquipmentData.Length,
                    EquipmentData = spChar.EquipmentData
                };

                ipsResultGetCharsCharacters.Add(ipsResultChar);
            }

            var ipsResultGetChars = new IPS_RESULT_GETCHARS()
            {
                ToSession = ipsGetChars.FromSession,
                LastSelectedCharacterIdx = spLastCharacter.CharacterIdx,
                CharSlotOrder = spLastCharacter.CharOrder,
                CharCount = (byte)ipsResultGetCharsCharacters.Count,
                Characters = ipsResultGetCharsCharacters
            };

            using var ipsResultGetCharsS = await IPCSerializer.Instance.SerializeAsync(ipsResultGetChars, Share.Opcode.IPC_RESULT_GETCHARS);
            await session.SendAsync(ipsResultGetCharsS);
        }

        public static async Task OnIPC_NEWCHAR(DBAgentSession session, ArraySegment<byte> buffer)
        {
            var ipsNewChar = await IPCSerializer.Instance.DeserializeAsync<IPS_NEWCHAR>(buffer);

            using var conn = new SqlConnection(Config.Instance.DBAgent.ConnectionString);
            var spCabalNewCharacter = await conn.QueryFirstOrDefaultAsync<SP_CABAL_NEW_CHARACTER>("sp_cabal_new_character", new
            {
                pUserNum = ipsNewChar.UserNum,
                pSlotIdx = ipsNewChar.CharSlotIdx,
                pStyle = ipsNewChar.Style,
                pName = ipsNewChar.CharName
            }, commandType: CommandType.StoredProcedure);

            if (spCabalNewCharacter == null)
            {
                CustomLogger<CharLobby>.Instance
                    .LogError("OnIPC_NEWCHAR(): Failed to create new character! UserNum({0}) SlotIdx({1}) Style({2}) Name({3})",
                    ipsNewChar.UserNum,
                    ipsNewChar.CharSlotIdx,
                    ipsNewChar.Style,
                    ipsNewChar.CharName);

                return;
            }

            var ipsResultNewChar = new IPS_RESULT_NEWCHAR()
            {
                ToSession = ipsNewChar.FromSession,
                CharResult = spCabalNewCharacter.CharResult,
                CharachterIdx = spCabalNewCharacter.CharacterIdx
            };

            using var ipsResultNewCharS = await IPCSerializer.Instance.SerializeAsync(ipsResultNewChar, Share.Opcode.IPC_RESULT_NEWCHAR);
            await session.SendAsync(ipsResultNewCharS);
        }

        public static async Task OnIPC_CHAR_SLOTORDER(DBAgentSession session, ArraySegment<byte> buffer)
        {
            var ipsCharSlotOrder = await IPCSerializer.Instance.DeserializeAsync<IPS_CHAR_SLOTORDER>(buffer);

            using var conn = new SqlConnection(Config.Instance.DBAgent.ConnectionString);
            var spLastCharacter = await conn.ExecuteAsync("sp_cabal_last_character", new
            {
                pMode = LastCharacterMode.UpdateCharSlotOrder,
                pIndex = ipsCharSlotOrder.UserNum,
                pValue = ipsCharSlotOrder.SlotOrder
            }, commandType: CommandType.StoredProcedure);

            var ipsResultCharSlotOrder = new IPS_RESULT_CHAR_SLOTORDER()
            {
                ToSession = ipsCharSlotOrder.FromSession,
                Updated = spLastCharacter > 0
            };

            using var ipsResultCharSlotOrderS = await IPCSerializer.Instance.SerializeAsync(ipsResultCharSlotOrder, Share.Opcode.IPC_RESULT_CHAR_SLOTORDER);
            await session.SendAsync(ipsResultCharSlotOrderS);
        }

        public static async Task OnIPC_DELCHAR(DBAgentSession session, ArraySegment<byte> buffer)
        {
            var ipsDelChar = await IPCSerializer.Instance.DeserializeAsync<IPS_DELCHAR>(buffer);

            using var conn = new SqlConnection(Config.Instance.DBAgent.ConnectionString);
            var spCabalDelCharacter = await conn.QueryFirstOrDefaultAsync<SP_CABAL_DEL_CHARACTER>("sp_cabal_del_character", new
            {
                pCharacterIdx = ipsDelChar.CharacterIdx
            }, commandType: CommandType.StoredProcedure);

            if (spCabalDelCharacter == null)
            {
                CustomLogger<CharLobby>.Instance
                    .LogError("OnIPC_DELCHAR(): Failed to delete character! CharacterIdx({0})",
                    ipsDelChar.CharacterIdx);

                return;
            }

            var ipsResultDelChar = new IPS_RESULT_DELCHAR()
            {
                ToSession = ipsDelChar.FromSession,
                DelResult = spCabalDelCharacter.DelResult
            };

            using var ipsResultDelCharS = await IPCSerializer.Instance.SerializeAsync(ipsResultDelChar, Share.Opcode.IPC_RESULT_DELCHAR);
            await session.SendAsync(ipsResultDelCharS);
        }
    }
}
