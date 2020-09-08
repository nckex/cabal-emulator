using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using HPT.Logging;
using Share.Serde;
using Share.System.Game;
using static Share.Protosdef;
using static Share.Protos.CharLobby;
using static WorldSvr.Protos.CharLobby;

namespace WorldSvr.Procs
{
    class CharLobby
    {
        public static async Task OnC2S_GETMYCHARTR(WorldSvrSession session, ArraySegment<byte> _)
        {
            var ipsGetChars = new IPS_GETCHARS()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum
            };

            using var ipsGetCharsS = await IPCSerializer.Instance.SerializeAsync(ipsGetChars, Share.Opcode.IPC_GETCHARS);
            await DBAgentClient.Instance.SendAsync(ipsGetCharsS);
        }

        public static async Task OnIPC_RESULT_GETCHARS(ArraySegment<byte> buffer)
        {
            var ipsResultGetChars = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_GETCHARS>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultGetChars.ToSession.SessionId, ipsResultGetChars.ToSession.SessionTime, out var session))
                return;

            var s2cGetMyChartrChars = new List<S2C_GETMYCHARTR_CHARACTER>(ipsResultGetChars.Characters.Count());
            foreach (var ipsResultCharacter in ipsResultGetChars.Characters)
            {
                var name = ipsResultCharacter.Name.Trim(char.MinValue);
                var position = new Position(ipsResultCharacter.Position);
                // var equipment = await ItemBag.BuildItemBag(ipsResultCharacter.EquipmentData);

                var s2cCharacter = new S2C_GETMYCHARTR_CHARACTER()
                {
                    CharacterIdx = ipsResultCharacter.CharacterIdx,
                    CreationDate = ipsResultCharacter.CreationDate,
                    LEV = ipsResultCharacter.LEV,
                    Style = ipsResultCharacter.Style,
                    WorldIdx = ipsResultCharacter.WorldIdx,
                    PosX = position.X,
                    PosY = position.Y,
                    Rank = ipsResultCharacter.Rank,
                    NameLen = (byte)(name.Length + 1), // name len + nullbyte
                    Name = name + char.MinValue // name + nullbyte
                };

                s2cGetMyChartrChars.Add(s2cCharacter);
            }

            var s2cGetMyChartr = new S2C_GETMYCHARTR()
            {
                UseACSUB = session.CharUserContext.UseACSUB,
                LastCharacter = ipsResultGetChars.LastSelectedCharacterIdx,
                SlotOrder = ipsResultGetChars.CharSlotOrder,
                ExtendedCharCreation = session.CharUserContext.ExtendedCharCreation,
                Characters = s2cGetMyChartrChars
            };

            using var s2cGetMyChartrS = await CabalSerializer.Instance.SerializeAsync(s2cGetMyChartr, Opcode.CSC_GETMYCHARTR);
            await session.SendAsync(s2cGetMyChartrS);
        }

        public static async Task OnC2S_SPECIALCHAREVT(WorldSvrSession session, ArraySegment<byte> _)
        {
            using var s2cPacketS = await CabalSerializer.Instance.SerializeAsync(new S2C_SPECIALCHAREVT(), Opcode.CSC_SPECIALCHAREVT);
            await session.SendAsync(s2cPacketS);
        }

        public static async Task OnC2S_NEWMYCHARTR(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sNewMyCharTr = await CabalSerializer.Instance.DeserializeAsync<C2S_NEWMYCHARTR>(buffer);

            var characterStyle = new CharacterStyle(c2sNewMyCharTr.Style);
            if ((characterStyle.ClassType < 1 || characterStyle.ClassType > 8)
                || (characterStyle.ClassRank != 1)
                || (characterStyle.FaceID > 3)
                || (characterStyle.HairColor > 7)
                || (characterStyle.HairStyle > 6)
                || (characterStyle.AuraCode != 0)
                )
            {
                CustomLogger<CharLobby>.Instance
                    .LogError("OnC2S_NEWMYCHARTR(): Invalid Style({0}) Session({1}) UserNum({2})",
                    c2sNewMyCharTr.Style,
                    session.Signature,
                    session.CharUserContext.UserNum);

                session.Disconnect();
                return;
            }

            var ipsNewChar = new IPS_NEWCHAR()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum,
                Style = c2sNewMyCharTr.Style,
                CharSlotIdx = c2sNewMyCharTr.CharSlotIdx,
                CharNameLen = c2sNewMyCharTr.CharNameLen,
                CharName = c2sNewMyCharTr.CharName
            };

            using var ipsNewCharS = await IPCSerializer.Instance.SerializeAsync(ipsNewChar, Share.Opcode.IPC_NEWCHAR);
            await DBAgentClient.Instance.SendAsync(ipsNewCharS);
        }

        public static async Task OnIPC_RESULT_NEWCHAR(ArraySegment<byte> buffer)
        {
            var ipsResultNewChar = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_NEWCHAR>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultNewChar.ToSession.SessionId, ipsResultNewChar.ToSession.SessionTime, out var session))
                return;

            var s2cNewMyChartr = new S2C_NEWMYCHARTR()
            {
                CharResult = ipsResultNewChar.CharResult,
                CharacterIdx = ipsResultNewChar.CharachterIdx
            };

            using var s2cNewMyChartrS = await CabalSerializer.Instance.SerializeAsync(s2cNewMyChartr, Opcode.CSC_NEWMYCHARTR);
            await session.SendAsync(s2cNewMyChartrS);
        }

        public static async Task OnC2S_CHARACTER_SLOTORDER(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sCharacterSlotOrder = await CabalSerializer.Instance.DeserializeAsync<C2S_CHARACTER_SLOTORDER>(buffer);

            var ipsCharSlotOrder = new IPS_CHAR_SLOTORDER()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum,
                SlotOrder = c2sCharacterSlotOrder.SlotOrder
            };

            using var ipsCharSlotOrderS = await IPCSerializer.Instance.SerializeAsync(ipsCharSlotOrder, Share.Opcode.IPC_CHAR_SLOTORDER);
            await DBAgentClient.Instance.SendAsync(ipsCharSlotOrderS);
        }

        public static async Task OnIPC_RESULT_CHAR_SLOTORDER(ArraySegment<byte> buffer)
        {
            var ipsResultCharSlotOrder = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_CHAR_SLOTORDER>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultCharSlotOrder.ToSession.SessionId, ipsResultCharSlotOrder.ToSession.SessionTime, out var session))
                return;

            var s2cCharacterSlotorder = new S2C_CHARACTER_SLOTORDER()
            {
                Updated = ipsResultCharSlotOrder.Updated
            };

            using var s2cCharacterSlotorderS = await CabalSerializer.Instance.SerializeAsync(s2cCharacterSlotorder, Opcode.CSC_CHARACTER_SLOTORDER);
            await session.SendAsync(s2cCharacterSlotorderS);
        }

        public static async Task OnC2S_CHECKPASSWD(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sCheckPasswd = await CabalSerializer.Instance.DeserializeAsync<C2S_CHECKPASSWD>(buffer);

            var pPassword = c2sCheckPasswd.Password.Trim(char.MinValue);
            var ipsCheckPasswd = new IPS_CHECKPASSWD()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum,
                PasswordLen = (byte)pPassword.Length,
                Password = pPassword
            };

            using var ipsCheckPasswdS = await IPCSerializer.Instance.SerializeAsync(ipsCheckPasswd, Share.Opcode.IPC_CHECKPASSWD);
            await GlobalMgrClient.Instance.SendAsync(ipsCheckPasswdS);
        }

        public static async Task OnIPC_RESULT_CHECKPASSWD(ArraySegment<byte> buffer)
        {
            var ipsResultCheckPasswd = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_CHECKPASSWD>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultCheckPasswd.ToSession.SessionId, ipsResultCheckPasswd.ToSession.SessionTime, out var session))
                return;

            var s2cCheckPasswd = new S2C_CHECKPASSWD()
            {
                Check = ipsResultCheckPasswd.Checked
            };

            using var s2cCheckPasswdS = await CabalSerializer.Instance.SerializeAsync(s2cCheckPasswd, Opcode.CSC_CHECKPASSWD);
            await session.SendAsync(s2cCheckPasswdS);
        }

        public static async Task OnC2S_DELMYCHARTR(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sDelMyChartr = await CabalSerializer.Instance.DeserializeAsync<C2S_DELMYCHARTR>(buffer);

            if ((c2sDelMyChartr.CharacterIdx / 8) != session.CharUserContext.UserNum)
            {
                CustomLogger<CharLobby>.Instance
                    .LogError($"OnC2S_DELMYCHARTR(): Invalid CharacterIdx({c2sDelMyChartr.CharacterIdx}). UserNum({session.CharUserContext.UserNum})");

                return;
            }

            var ipsDelChar = new IPS_DELCHAR()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                CharacterIdx = c2sDelMyChartr.CharacterIdx
            };

            using var ipsDelCharS = await IPCSerializer.Instance.SerializeAsync(ipsDelChar, Share.Opcode.IPC_DELCHAR);
            await DBAgentClient.Instance.SendAsync(ipsDelCharS);
        }

        public static async Task OnIPC_RESULT_DELCHAR(ArraySegment<byte> buffer)
        {
            var ipsResultDelChar = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_DELCHAR>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultDelChar.ToSession.SessionId, ipsResultDelChar.ToSession.SessionTime, out var session))
                return;

            var s2cDelMyChartr = new S2C_DELMYCHARTR()
            {
                DelResult = ipsResultDelChar.DelResult
            };

            using var s2cDelMyChartrS = await CabalSerializer.Instance.SerializeAsync(s2cDelMyChartr, Opcode.CSC_DELMYCHARTR);
            await session.SendAsync(s2cDelMyChartrS);
        }
    }
}
