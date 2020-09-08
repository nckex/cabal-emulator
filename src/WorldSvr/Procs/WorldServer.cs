using Share.Serde;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static WorldSvr.Protos.WorldServer;

namespace WorldSvr.Procs
{
    class WorldServer
    {
        private static readonly SemaphoreSlim _s2cPingSemaphore = new SemaphoreSlim(1, 1);
        private static byte[] _s2cPingPacket;

        public static async Task OnC2S_GETSVRTIME(WorldSvrSession session, ArraySegment<byte> _)
        {
            var s2cGetSvrTime = new S2C_GETSVRTIME()
            {
                UnixTimestamp = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                Timezone = (short)DateTimeOffset.Now.Offset.TotalMinutes
            };

            using var s2cGetSvrTimeS = await CabalSerializer.Instance.SerializeAsync(s2cGetSvrTime, Opcode.CSC_GETSVRTIME);
            await session.SendAsync(s2cGetSvrTimeS);
        }

        public static async Task OnC2S_SERVERENV(WorldSvrSession session, ArraySegment<byte> _)
        {
            var s2cServerEnv = new S2C_SERVERENV()
            {
                MaxLevel = Config.Instance.WorldSvr.MaxLevel,
                UseDummy = Config.Instance.WorldSvr.UseDummy,
                AllowCashShop = Config.Instance.WorldSvr.AllowCashShop,
                AllowNetCafePoint = Config.Instance.WorldSvr.AllowNetCafePoint,
                NormalChatMinLevel = Config.Instance.WorldSvr.NormalChatMinLevel,
                LoudChatMinLevel = Config.Instance.WorldSvr.LoudChatMinLevel,
                LoudChatMinMasteryLevel = Config.Instance.WorldSvr.LoudChatMinMasteryLevel,
                MaxInventoryAlz = Config.Instance.WorldSvr.MaxInventoryAlz,
                MaxWarehouseAlz = Config.Instance.WorldSvr.MaxWarehouseAlz,
                MinRank = Config.Instance.WorldSvr.MinRank,
                MaxRank = Config.Instance.WorldSvr.MaxRank,
                DeleteCharMaxLevel = Config.Instance.WorldSvr.DeleteCharMaxLevel,

                U0 = 140000000000,
                U1 = 1,
                U2 = 0,
                U3 = 0,
                U4 = 1,
                U5 = 0,
                U6 = 10,
                U7 = 10,
                U8 = 1,
                U9 = 100,
                U10 = 1000000000,
                U11 = 7,
                U12 = 1,
                U13 = 4000000000,
                U14 = -2000000000,
                U15 = 0,
                U16 = 2,
                U17 = 1,
                U18 = 16,
                U19 = 15,
                U20 = 2,
                U21 = 109,
                U22 = 110,
                U24 = 5,
                U25 = 0
            };

            using var s2cServerEnvS = await CabalSerializer.Instance.SerializeAsync(s2cServerEnv, Opcode.CSC_SERVERENV);
            await session.SendAsync(s2cServerEnvS);
        }

        public static async Task OnC2S_PING(WorldSvrSession session, ArraySegment<byte> _)
        {
            if (_s2cPingPacket == null)
            {
                await _s2cPingSemaphore.WaitAsync();
                try
                {
                    // Double check
                    if (_s2cPingPacket == null)
                    {
                        using var s2cPingS = await CabalSerializer.Instance.SerializeAsync(new Share.Protosdef.S2C_HEADER(), Opcode.CSC_PING);
                        _s2cPingPacket = s2cPingS.Buffer.ToArray();
                    }
                }
                finally
                {
                    _s2cPingSemaphore.Release();
                }
            }

            await session.SendAsync(_s2cPingPacket);
        }
    }
}
