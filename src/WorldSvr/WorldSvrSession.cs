using HPT.Logging;
using HPT.Network;
using Microsoft.Extensions.Logging;
using Share.System;
using Share.Encryption;
using Share.Serde;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Share.Protodef;
using static Share.Protos.Auth;
using static Share.Protosdef;
using WorldSvr.System.Game;

namespace WorldSvr
{
    class WorldSvrSession : TcpSession
    {
        public CabalEncryptable Encryptable { get; }
        public CharUserContext CharUserContext { get; private set; }

        public WorldSvrSession(Socket socket, ushort sessionId) : base(socket, sessionId)
        {
            Encryptable = new CabalEncryptable();
        }

        public void InitAccountContext()
        {
            if (CharUserContext == null)
                CharUserContext = new CharUserContext();
        }

        protected override int GetMessageSize(in ReadOnlySequence<byte> headerBytes)
        {
            return CabalEncryption.GetPacketSize(Encryptable, BitConverter.ToUInt32(headerBytes.FirstSpan));
        }

        protected override async Task OnDisconnect()
        {
            // TODO: Pensar numa forma de enviar LOGINSTATE somente apos as informacoes do personagem serem salvas!
            // Talvez seja uma boa ideia criar um RESULT do save de CharacterContext, e somente apos receber o result
            // enviar o LOGINSTATE para a conta ser setada como Login = 0
            // A regra abaixo deve mudar para que NAO seja enviado o IPS_LOGINSTATE caso essa sessao contenha um CharacterContext
            // em vez disso, enviar um pacote para salvar CharacterContext, e na result, finalmente enviar a IPS_LOGINSTATE

            if (CharUserContext != null)
            {
                if (WorldManager.Instance.TryGetWorld(CharUserContext.WorldIdx, out var world))
                {
                    world.TryLeaveWorld(CharUserContext);
                }

                var ipsLoginState = new IPS_LOGINSTATE()
                {
                    FromService = new IPS_SERVICE()
                    {
                        GroupIdx = Config.Instance.WorldSvr.GroupIdx,
                        ServerIdx = Config.Instance.WorldSvr.ServerIdx
                    },
                    FromSession = new IPS_SESSION()
                    {
                        SessionId = SessionId,
                        SessionTime = SessionTime
                    },
                    UserNum = CharUserContext.UserNum,
                    Mode = CharUserContext.ShouldSoftDisconnect ? LoginStateMode.SoftDisconnect : LoginStateMode.Disconnect
                };

                using var ipsLoginStateS = await IPCSerializer.Instance.SerializeAsync(ipsLoginState, Share.Opcode.IPC_LOGINSTATE);
                await GlobalMgrClient.Instance.SendAsync(ipsLoginStateS);
            }
        }

        protected override async Task ProcessMessageReceivedAsync(ReadOnlySequence<byte> messageBytes)
        {
            var cpMessageBytes = messageBytes.ToArray(); // TODO: Maybe pool array

            CabalEncryption.Instance.Decrypt(Encryptable, ref cpMessageBytes, cpMessageBytes.Length);

            var c2sHeader = await CabalSerializer.Instance.DeserializeAsync<C2S_HEADER>(cpMessageBytes);
            if (Procdef.WorldSvrProcMethodHandler.TryGet(c2sHeader.Opcode, out var worldSvrProc))
            {
                await worldSvrProc(this, cpMessageBytes);
            } 
            else
            {
                CustomLogger<WorldSvrSession>.Instance
                    .LogWarning($"Session({Signature}) sent an unknown packet opcode ({c2sHeader.Opcode})");
            }
        }
    }
}
