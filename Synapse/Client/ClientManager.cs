﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JWT.Algorithms;
using JWT.Builder;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Swan;
using Swan.Formatters;
using Synapse.Api;
using Synapse.Api.Events;
using Synapse.Client.Packets;
using Synapse.Network;
using UnityEngine;
using Logger = Synapse.Api.Logger;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Synapse.Client
{
    public class ClientManager
    {
        public static bool isSynapseClientEnabled = true;

        public static ClientManager Singleton = new ClientManager();

        public static GameObject debugPrefab;
        public static GameObject debugPrefab2;

        public ClientConnectionData DecodeJWT(string jwt)
        {
            var webClient = new WebClient();
            var pem = webClient.DownloadString("https://central.synapsesl.xyz/session/verificationKey");
            var pr = new PemReader(new StringReader(pem));
            var publicKey = (AsymmetricKeyParameter)pr.ReadObject();
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);
            var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportParameters(rsaParams);
            var payload = JwtBuilder.Create()
                .WithAlgorithm(new RS256Algorithm(rsa))
                .MustVerifySignature()
                .Decode<ClientConnectionData>(jwt);
            return payload;
        }

        public static void Initialise()
        {
            new SynapseServerListThread().Run();
            
            Logger.Get.Info("Loading Bundle");
            var stream = File.OpenRead("firstaid.bundle");
            var stream2 = File.OpenRead("environment.bundle");
            var bundle = AssetBundle.LoadFromStream(stream);
            var bundle2 = AssetBundle.LoadFromStream(stream2);
            debugPrefab = bundle.LoadAsset<GameObject>("FirstAidKit_Green.prefab");
            debugPrefab2 = bundle2.LoadAsset<GameObject>("ExampleEnvironment.prefab");
            
            Server.Get.Events.Round.RoundStartEvent += delegate
            {
                Logger.Get.Info("Spawning Networked Prefab");
                
                var pos = new Vector3(30, 10, 40);
                var rot = Quaternion.identity;
                var obj = Object.Instantiate(debugPrefab, pos, rot);
                obj.name = "FirstAidTest(1)";
                
                var pos1 = new Vector3(15, 1000, 60);
                var rot1 = Quaternion.identity;
                var obj1 = Object.Instantiate(debugPrefab2, pos1, rot1);
                obj1.name = "Environment(1)";
                
                foreach (var player in Server.Get.Players)
                {
                    ClientPipeline.invoke(player, SpawnPacket.Encode(pos, rot, "FirstAidTest(1)", "FirstAid"));
                    ClientPipeline.invoke(player, SpawnPacket.Encode(pos1, rot1, "Environment(1)", "Environment"));
                }
                
                Logger.Get.Info("Spawned Networked Prefab");
            };
            
            Logger.Get.Info("Loading Complete");
            
            ClientPipeline.DataReceivedEvent += delegate(Player player, PipelinePacket ev)
            {
                switch (ev.PacketId)
                {
                    case 1:
                        break;
                    
                    case 10:
                        break;
                    case 11:
                        break;
                    case 12:
                        break;
                }
            };
        }

        public class SynapseServerListThread : JavaLikeThread
        {
            private WebClient _webClient = new WebClient();
            
            public override async void Run()
            {
                for (;;)
                {
                    try
                    {
                        if (File.Exists("serverlist.token"))
                        {
                            var token = File.ReadAllText("serverlist.token");
                            _webClient.Headers.Clear();
                            _webClient.Headers.Add("User-Agent", "SynapseServerClient");
                            _webClient.Headers.Add("Content-Type", "application/json");
                            _webClient.Headers.Add("Api-Key", token);
                            _webClient.UploadString("https://servers.synapsesl.xyz/serverlist", Json.Serialize(new SynapseServerListMark
                            {
                                onlinePlayers = new Random().Next(0, 31),
                                maxPlayers = 30,
                                info = Base64.ToBase64String("=====> Nordholz.Games <=====\nSynapse Modded Client Server\nFriendlyFire: Active".ToBytes())
                            }));
                            Logger.Get.Info("Sent mark to serverlist");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Get.Error("Error when trying to mark server to serverlist: " + e.ToString());
                    }
                    await Task.Delay(1000 * 10);
                    
                }
            }
        }

        public class SynapseServerListMark
        {
            public int onlinePlayers { get; set; }
            public int maxPlayers { get; set; }
            public string info { get; set; }
        }
        
        public Dictionary<String, ClientConnectionData> Clients { get; set; } =
            new Dictionary<string, ClientConnectionData>();
    }
    
    
    public class ClientConnectionData
    {
        //JWT Subject == Name
        public string sub { get; set; }
        //JWT Audience == Connected Server
        public string aud { get; set; }
        //JWT Issuer == Most likely Synapse
        public string iss { get; set; }
        public string uuid { get; set; }
        public string session { get; set; }
    }
}