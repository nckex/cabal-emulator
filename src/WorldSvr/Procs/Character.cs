using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Share.Serde;
using Share.System;
using Share.System.Game;
using HPT.Logging;
using static Share.Protosdef;
using static Share.Protos.Character;
using static WorldSvr.Protos.Character;
using WorldSvr.System.Game;

namespace WorldSvr.Procs
{
    class Character
    {
        public static async Task OnC2S_INITIALIZED(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sInitialized = await CabalSerializer.Instance.DeserializeAsync<C2S_INITIALIZED>(buffer);

            if ((c2sInitialized.CharacterIdx / 8) != session.CharUserContext.UserNum)
            {
                CustomLogger<Character>.Instance
                    .LogError("OnC2S_INITIALIZED(): Invalid CharacterIdx({0}) UserNum({1})",
                    c2sInitialized.CharacterIdx,
                    session.CharUserContext.UserNum);

                session.Disconnect();

                return;
            }

            var ipsGetMyCharacter = new IPS_GETMYCHARACTER()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                CharacterIdx = c2sInitialized.CharacterIdx
            };

            using var ipsGetMyCharacterS = await IPCSerializer.Instance.SerializeAsync(ipsGetMyCharacter, Share.Opcode.IPC_GETMYCHARACTER);
            await DBAgentClient.Instance.SendAsync(ipsGetMyCharacterS);
        }

        public static async Task OnIPC_RESULT_GETMYCHARACTER(ArraySegment<byte> buffer)
        {
            var ipsResultGetMyCharacter = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_GETMYCHARACTER>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultGetMyCharacter.ToSession.SessionId, ipsResultGetMyCharacter.ToSession.SessionTime, out var session))
                return;

            session.CharUserContext.IsConnected = true;

            session.CharUserContext.CharacterIdx = ipsResultGetMyCharacter.CharacterIdx;
            session.CharUserContext.Name = ipsResultGetMyCharacter.Name;

            session.CharUserContext.CharacterStyle = new CharacterStyle(ipsResultGetMyCharacter.Style);

            session.CharUserContext.LEV = ipsResultGetMyCharacter.LEV;
            session.CharUserContext.EXP = ipsResultGetMyCharacter.EXP;
            session.CharUserContext.STR = ipsResultGetMyCharacter.STR;
            session.CharUserContext.DEX = ipsResultGetMyCharacter.DEX;
            session.CharUserContext.INT = ipsResultGetMyCharacter.INT;
            session.CharUserContext.PNT = ipsResultGetMyCharacter.PNT;

            if (!WorldManager.Instance.TryGetWorld(ipsResultGetMyCharacter.WorldIdx, out var world))
            {
                // TODO: Log
                session.Disconnect();
                return;
            }

            if (!world.TryEnterWorld(session.CharUserContext))
            {
                // TODO: Log
                session.Disconnect();
                return;
            }

            session.CharUserContext.WorldIdx = ipsResultGetMyCharacter.WorldIdx;
            session.CharUserContext.Position = new Position(ipsResultGetMyCharacter.Position);

            session.CharUserContext.Equipment = await ItemBag.BuildItemBagAsync(ipsResultGetMyCharacter.EquipmentData);
            session.CharUserContext.Inventory = await ItemBag.BuildItemBagAsync(ipsResultGetMyCharacter.InventoryData);

            // TODO: Init CharacterContext and MUCH MORE!!

            var s2cInitialized = new S2C_INITIALIZED()
            {
                GroupIdx = Config.Instance.WorldSvr.GroupIdx,
                ServerIdx = Config.Instance.WorldSvr.ServerIdx,
                MaxPlayers = Config.Instance.WorldSvr.MaxPlayers,
                IP = IPAddress.Parse(Config.Instance.Listen.IP).GetAddressBytes(), // TODO: Cache
                Port = Config.Instance.Listen.Port,

                WorldIdx = session.CharUserContext.WorldIdx,
                PosX = session.CharUserContext.Position.X,
                PosY = session.CharUserContext.Position.Y,

                EXP = session.CharUserContext.EXP,
                Alz = 0,
                Wexp = 0,

                LEV = session.CharUserContext.LEV,
                STR = session.CharUserContext.STR,
                DEX = session.CharUserContext.DEX,
                INT = session.CharUserContext.INT,
                PNT = session.CharUserContext.PNT,

                SwordRank = 1,
                MagicRank = 0,

                MaxHP = 35,
                CurHP = 35,
                MaxMP = 5,
                CurMP = 5,
                MaxSP = 0,
                CurSP = 0,
                MaxRage = 0,

                DP = 0,

                SkillExpBars = 0,
                SkillExp = 0,
                SkillPoints = 100,

                HonorPoints = 50,

                Style = session.CharUserContext.CharacterStyle,

                EquipCount = (ushort)session.CharUserContext.Equipment.Count,
                InventoryCount = (ushort)session.CharUserContext.Inventory.Count,
                SkillCount = 0,
                QuickSlotCount = 0,

                NameLength = session.CharUserContext.Name.Length,
                NameLengthNullByteTerminated = (byte)(session.CharUserContext.Name.Length + 1),
                Name = session.CharUserContext.Name,

                EquipmentDataLen = ipsResultGetMyCharacter.EquipmentDataLen,
                EquipmentData = ipsResultGetMyCharacter.EquipmentData,

                InventoryDataLen = ipsResultGetMyCharacter.InventoryDataLen,
                InventoryData = ipsResultGetMyCharacter.InventoryData,
            };

            using var s2cInitializedS = await CabalSerializer.Instance.SerializeUncompressedAsync(s2cInitialized, Opcode.CSC_INITIALIZED);
            await session.SendAsync(s2cInitializedS);
        }
    }
}
