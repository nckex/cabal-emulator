using System;
using System.Threading.Tasks;
using Share.Serde;
using static WorldSvr.Protos.Charge;

namespace WorldSvr.Procs
{
    class Charge
    {
        public static async Task OnC2S_CHARGEINFO(WorldSvrSession session, ArraySegment<byte> _)
        {
            var s2cChargeInfo = new S2C_CHARGEINFO()
            {
                ServiceType = session.CharUserContext.ServiceType,
                ServiceKind = session.CharUserContext.ServiceKind,
                ExpireDate = session.CharUserContext.ExpirationDate
            };

            using var s2cChargeInfoS = await CabalSerializer.Instance.SerializeAsync(s2cChargeInfo, Opcode.CSC_CHARGEINFO);
            await session.SendAsync(s2cChargeInfoS);
        }
    }
}
