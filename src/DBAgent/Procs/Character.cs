using Share.Serde;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using static Share.Protos.Character;
using DBAgent.Resources;
using System.Data;
using HPT.Logging;
using Microsoft.Extensions.Logging;

namespace DBAgent.Procs
{
    class Character
    {
        public static async Task OnIPC_GETMYCHARACTER(DBAgentSession session, ArraySegment<byte> buffer)
        {
            var ipsGetMyCharacter = await IPCSerializer.Instance.DeserializeAsync<IPS_GETMYCHARACTER>(buffer);

            using var conn = new SqlConnection(Config.Instance.DBAgent.ConnectionString);
            var spCabalGetCharacter = await conn.QueryFirstOrDefaultAsync<SP_CABAL_GET_CHARACTER>("sp_cabal_get_character", new
            {
                pCharacterIdx = ipsGetMyCharacter.CharacterIdx
            }, commandType: CommandType.StoredProcedure);

            if (spCabalGetCharacter == null)
            {
                CustomLogger<Character>.Instance
                    .LogError("OnIPC_GETMYCHARACTER(): CharacterIdx({0}) not found",
                    ipsGetMyCharacter.CharacterIdx);

                return;
            }

            var ipsResultGetMyCharacter = new IPS_RESULT_GETMYCHARACTER()
            {
                ToSession = ipsGetMyCharacter.FromSession,

                CharacterIdx = ipsGetMyCharacter.CharacterIdx,

                NameLen = spCabalGetCharacter.Name.Length,
                Name = spCabalGetCharacter.Name,

                Style = spCabalGetCharacter.Style,

                LEV = spCabalGetCharacter.LEV,
                EXP = spCabalGetCharacter.EXP,
                STR = spCabalGetCharacter.STR,
                DEX = spCabalGetCharacter.DEX,
                INT = spCabalGetCharacter.INT,
                PNT = spCabalGetCharacter.PNT,

                WorldIdx = spCabalGetCharacter.WorldIdx,
                Position = spCabalGetCharacter.Position,

                EquipmentDataLen = spCabalGetCharacter.EquipmentData.Length,
                EquipmentData = spCabalGetCharacter.EquipmentData,

                InventoryDataLen = spCabalGetCharacter.InventoryData.Length,
                InventoryData = spCabalGetCharacter.InventoryData
            };

            using var ipsResultGetMyCharacterS = await IPCSerializer.Instance.SerializeAsync(ipsResultGetMyCharacter, Share.Opcode.IPC_RESULT_GETMYCHARACTER);
            await session.SendAsync(ipsResultGetMyCharacterS);
        }
    }
}
