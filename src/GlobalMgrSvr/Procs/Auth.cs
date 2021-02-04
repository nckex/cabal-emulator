using System;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using Share.Serde;
using GlobalMgrSvr.System;
using GlobalMgrSvr.Resources;
using static Share.Protodef;
using static Share.Protos.Auth;
using static Share.Protos.Context;
using static Share.Protosdef;
using static Share.Protos.Connect;
using HPT.Logging;
using Microsoft.Extensions.Logging;

namespace GlobalMgrSvr.Procs
{
    class Auth
    {
        public static async Task OnIPC_AUTH(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsAuth = await IPCSerializer.Instance.DeserializeAsync<IPS_AUTH>(buffer);

            var username = ipsAuth.Username.Trim(char.MinValue);
            var password = ipsAuth.Password.Trim(char.MinValue);
            var ipAddress = ipsAuth.IpAddress.Trim(char.MinValue);

            using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
            var spCabalAuthAccount = await conn.QueryFirstOrDefaultAsync<SP_CABAL_AUTH_ACCOUNT>("dbo.sp_cabal_auth_account", new
            {
                pUsername = username,
                pPassword = password,
                pIpAddress = ipAddress
            }, commandType: CommandType.StoredProcedure);

            if (spCabalAuthAccount == null)
            {
                CustomLogger<Auth>.Instance
                    .LogError("OnIPC_AUTH(): Failed to authenticate account! UserName({0}) IPAddress({1})",
                    username,
                    ipAddress);

                return;
            }

            var ipsReturnAuth = new IPS_RESULT_AUTH()
            {
                ToService = ipsAuth.FromService,
                ToSession = ipsAuth.FromSession,
                AuthResult = spCabalAuthAccount.AuthResult
            };

            if (spCabalAuthAccount.AuthResult == AuthResult.Success || spCabalAuthAccount.AuthResult == AuthResult.Already)
            {
                ipsReturnAuth.UserContextData = new IPS_USERCONTEXT_DATA()
                {
                    UserNum = spCabalAuthAccount.UserNum
                };

                if (spCabalAuthAccount.AuthResult == AuthResult.Success)
                {
                    if (
                        AccountLoginStateHandler.Instance
                            .TryAddState(
                                spCabalAuthAccount.UserNum,
                                ipsAuth.FromService.GroupIdx,
                                ipsAuth.FromService.ServerIdx,
                                ipsAuth.FromSession.SessionId,
                                ipsAuth.FromSession.SessionTime)
                        )
                    {
                        ipsReturnAuth.UserContextData.AuthKey = spCabalAuthAccount.AuthKey;
                        ipsReturnAuth.UserContextData.UseACSUB = spCabalAuthAccount.UseACSUB;
                        ipsReturnAuth.UserContextData.UseWHSUB = spCabalAuthAccount.UseWHSUB;
                        ipsReturnAuth.UserContextData.UseEQSUB = spCabalAuthAccount.UseEQSUB;
                        ipsReturnAuth.UserContextData.IsWHLOCK = spCabalAuthAccount.IsWHLOCK;
                        ipsReturnAuth.UserContextData.IsEQLOCK = spCabalAuthAccount.IsEQLOCK;
                        ipsReturnAuth.UserContextData.ServiceType = spCabalAuthAccount.ServiceType;
                        ipsReturnAuth.UserContextData.ServiceKind = spCabalAuthAccount.ServiceKind;
                        ipsReturnAuth.UserContextData.ExpirationDate = spCabalAuthAccount.ExpirationDate;
                        ipsReturnAuth.UserContextData.ExtendedCharCreation = spCabalAuthAccount.ExtendedCharCreation;
                    }
                    else
                    {
                        ipsReturnAuth.AuthResult = AuthResult.Already;
                    }
                }
            }

            using var ipsReturnAuthS = await IPCSerializer.Instance.SerializeAsync(ipsReturnAuth, Share.Opcode.IPC_RESULT_AUTH);
            await session.SendAsync(ipsReturnAuthS);
        }

        public static async Task OnIPC_LOGINSTATE(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            // TODO: A logica de setar Login = 0 na conta deve levar em conta que somente deve ser atualizado APOS salvar os dados do personagem!

            var ipsLoginState = await IPCSerializer.Instance.DeserializeAsync<IPS_LOGINSTATE>(buffer);

            if (ipsLoginState.Mode == LoginStateMode.Connect || ipsLoginState.Mode == LoginStateMode.Disconnect)
            {
                using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
                await conn.ExecuteAsync("sp_cabal_loginstate", new
                {
                    pUserNum = ipsLoginState.UserNum,
                    pMode = ipsLoginState.Mode
                }, commandType: CommandType.StoredProcedure);

                if (ipsLoginState.Mode == LoginStateMode.Connect)
                {
                    var ipsResultLoginState = new IPS_RESULT_LOGINSTATE()
                    {
                        ToService = ipsLoginState.FromService,
                        ToSession = ipsLoginState.FromSession
                    };

                    using var ipsResultLoginStateS = await IPCSerializer.Instance.SerializeAsync(ipsResultLoginState, Share.Opcode.IPC_RESULT_LOGINSTATE);
                    await session.SendAsync(ipsResultLoginStateS);

                    return;
                } else
                {
                    AccountLoginStateHandler.Instance
                        .TryRemoveState(ipsLoginState.UserNum);

                    return;
                }
            }
            else if (ipsLoginState.Mode == LoginStateMode.SoftDisconnect)
            {
                AccountLoginStateHandler.Instance
                    .TryRemoveState(ipsLoginState.UserNum, ipsLoginState.FromService.GroupIdx, ipsLoginState.FromService.ServerIdx);

                return;
            }
        }

        public static async Task OnIPC_REQ_FDISCONNECT(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsReqFDisconnect = await IPCSerializer.Instance.DeserializeAsync<IPS_REQ_FDISCONNECT>(buffer);

            var isStateFound = AccountLoginStateHandler.Instance
                .TryGetCurrentState(ipsReqFDisconnect.UserNum, out var currentAccountLoginState);

            if (isStateFound && currentAccountLoginState.Length > 0)
            {
                foreach (var dupSession in currentAccountLoginState)
                {
                    if (ServiceStateHandler.Instance.TryGet(dupSession.Key.groupIdx, dupSession.Key.serverIdx, out var service))
                    {
                        var ipsFDisconnect = new IPS_FDISCONNECT()
                        {
                            ToSession = new IPS_SESSION()
                            {
                                SessionId = dupSession.Value.sessionId,
                                SessionTime = dupSession.Value.sessionTime
                            },
                            UserNum = ipsReqFDisconnect.UserNum
                        };

                        using var ipsFDisconnectS = await IPCSerializer.Instance.SerializeAsync(ipsFDisconnect, Share.Opcode.IPC_FDISCONNECT);
                        await service.Session.SendAsync(ipsFDisconnectS);
                    }
                    else
                    {
                        AccountLoginStateHandler.Instance
                            .TryRemoveState(ipsReqFDisconnect.UserNum, dupSession.Key.groupIdx, dupSession.Key.serverIdx);
                    }
                }
            }
            else
            {
                using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
                await conn.ExecuteAsync("sp_cabal_loginstate", new
                {
                    pUserNum = ipsReqFDisconnect.UserNum,
                    pMode = LoginStateMode.SoftDisconnect
                }, commandType: CommandType.StoredProcedure);
            }

            var ipsResultReqFDisconnect = new IPS_RESULT_REQ_FDISCONNECT()
            {
                ToSession = ipsReqFDisconnect.FromSession,
                Success = true
            };

            using var ipsResultReqFDisconnectS = await IPCSerializer.Instance.SerializeAsync(ipsResultReqFDisconnect, Share.Opcode.IPC_RESULT_REQ_FDISCONNECT);
            await session.SendAsync(ipsResultReqFDisconnectS);
        }
    }
}
