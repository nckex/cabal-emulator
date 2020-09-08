using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HPT.Network;
using HPT.Logging;
using Share.Serde;
using Share.System;
using Share.Encryption;
using static Share.Protosdef;
using static Share.Protos.Auth;
using static Share.Protodef;
using LoginSvr.System;

namespace LoginSvr
{
    class LoginSvrSession : TcpSession
    {
        public RSA RSA { get; private set; }
        public int DupUserNum { get; private set; }
        public CabalEncryptable Encryptable { get; }
        public UserContext UserContext { get; private set; }

        public LoginSvrSession(Socket socket, ushort sessionId) : base(socket, sessionId)
        {
            Encryptable = new CabalEncryptable();
        }

        public void InitRSA()
        {
            RSA = new RSA();
        }

        public void InitAccountContext()
        {
            if (UserContext == null)
                UserContext = new UserContext();
        }

        public void UpdateDupUserNum(int userNum)
        {
            DupUserNum = userNum;
        }

        protected override async Task OnDisconnect()
        {
            if (UserContext != null)
            {
                ServiceStateHandler.Instance.Unsubscribe(SessionId); // Unsubscribe from serverstate pool

                var ipsLoginState = new IPS_LOGINSTATE()
                {
                    FromService = new IPS_SERVICE()
                    {
                        ServerIdx = Config.Instance.LoginSvr.ServerIdx,
                        GroupIdx = Protodef.LOGINSVR_GROUPIDX
                    },
                    FromSession = new IPS_SESSION()
                    {
                        SessionId = SessionId,
                        SessionTime = SessionTime
                    },
                    UserNum = UserContext.UserNum,
                    Mode = UserContext.ShouldSoftDisconnect ? LoginStateMode.SoftDisconnect : LoginStateMode.Disconnect
                };

                using var ipsLoginStateS = await IPCSerializer.Instance.SerializeAsync(ipsLoginState, Share.Opcode.IPC_LOGINSTATE);
                await GlobalMgrClient.Instance.SendAsync(ipsLoginStateS);
            }
        }

        protected override int GetMessageSize(in ReadOnlySequence<byte> headerBytes)
        {
            return CabalEncryption.GetPacketSize(Encryptable, BitConverter.ToUInt32(headerBytes.FirstSpan));
        }

        protected override async Task ProcessMessageReceivedAsync(ReadOnlySequence<byte> messageBytes)
        {
            var cpMessageBytes = messageBytes.ToArray(); // TODO: Maybe pool array

            CabalEncryption.Instance.Decrypt(Encryptable, ref cpMessageBytes, cpMessageBytes.Length);

            var c2sHeader = await CabalSerializer.Instance.DeserializeAsync<C2S_HEADER>(cpMessageBytes);
            if (c2sHeader != null && Procdef.LoginServerMethodHandler.TryGet(c2sHeader.Opcode, out var loginProcAction))
            {
                await loginProcAction(this, cpMessageBytes);
            } 
            else
            {
                CustomLogger<LoginSvrSession>.Instance
                    .LogWarning($"Session({Signature}) sent an unknown packet opcode ({c2sHeader.Opcode})");
            }
        }
    }
}
