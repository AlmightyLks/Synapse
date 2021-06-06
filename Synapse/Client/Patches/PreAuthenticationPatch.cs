﻿using System;
using System.Text;
using HarmonyLib;
using LiteNetLib;
using Mirror.LiteNetLib4Mirror;
using Swan;
using Synapse.Api;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),
        nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    internal class PreAuthenticationPatch
    {
         private static bool Prefix(CustomLiteNetLib4MirrorTransport __instance, ConnectionRequest request)
        {
            try
            {
                if (ClientManager.isSynapseClientEnabled)
                {
                    var exists = request.Data.TryGetByte(out var packetId);
                    if (!exists)
                    {
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put(2);
                        request.RejectForce();
                        return false;
                    }
                    else
                    {
                        if (packetId == 5)
                        {
                            request.Data.GetByte();
                            Logger.Get.Info("Prefix!!");
                            byte[] uidBytes;
                            byte[] jwtBytes;
                            byte[] nonceBytes;
                            Logger.Get.Info(request.Data._dataSize);
                            Logger.Get.Info("Next Int:" + request.Data.PeekInt());
                            if (!request.Data.TryGetBytesWithLength(out uidBytes) ||
                                !request.Data.TryGetBytesWithLength(out jwtBytes) ||
                                !request.Data.TryGetBytesWithLength(out nonceBytes))
                            {
                                Logger.Get.Info("Rejecting!");
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Put(2);
                                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                return false;
                            }

                            var uid = Encoding.UTF8.GetString(uidBytes);
                            var jwt = Encoding.UTF8.GetString(jwtBytes);
                            var nonce = Encoding.UTF8.GetString(nonceBytes);
                            Logger.Get.Info(uid);
                            Logger.Get.Info(jwt);
                            Logger.Get.Info(nonce);

                            Logger.Get.Info("Decoding JWT Token");
                            var clientConnectionData = ClientManager.Singleton.DecodeJWT(jwt);

                            Logger.Get.Warn("ClientConnectionData: " + clientConnectionData.Humanize());

                            int num = CustomNetworkManager.slots;
                            if (LiteNetLib4MirrorCore.Host.ConnectedPeersCount < num)
                            {
                                if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
                                    CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].SetUserId(uid);
                                else
                                    CustomLiteNetLib4MirrorTransport.UserIds.Add(request.RemoteEndPoint,
                                        new PreauthItem(uid));

                                ClientManager.Singleton.Clients[clientConnectionData.uuid] = clientConnectionData;

                                request.Accept();
                                ServerConsole.AddLog(
                                    string.Format("Player {0} preauthenticated from endpoint {1}.", (object) uid,
                                        (object) request.RemoteEndPoint), ConsoleColor.Gray);
                                ServerLogs.AddLog(ServerLogs.Modules.Networking,
                                    string.Format("{0} preauthenticated from endpoint {1}.", (object) uid,
                                        (object) request.RemoteEndPoint), ServerLogs.ServerLogType.ConnectionUpdate,
                                    false);
                                CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
                            }
                            else
                            {
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 1);
                                request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                            }

                            return false;
                        }
                        else
                        {
                            request.Data._position = 0;
                            request.Data._offset = 0;
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Get.Error(e);
            }

            return true;
        }
    }
}