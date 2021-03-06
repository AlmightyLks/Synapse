﻿using LiteNetLib;
using UnityEngine;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class PreAuthenticationEventArgs : EventHandler.ISynapseEventArgs
    {
        public string UserId { get; internal set; }
        public bool Allow { get; set; } = true;
        public ConnectionRequest Request { get; internal set; }
        
        /// <summary>
        /// This field is only required if Allow = false;
        /// </summary>
        public string Reason { get; set; }
    }

    public class RemoteAdminCommandEventArgs : EventHandler.ISynapseEventArgs
    {
        public CommandSender Sender { get; internal set; }
        
        public string Command { get; internal set; }

        public bool Allow { get; set; } = true;
    }

    public class ConsoleCommandEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }
        
        public string Command { get; internal set; }
    }

    public class TransmitPlayerDataEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Player PlayerToShow { get; internal set; }

        public float Rotation { get; set; }

        public Vector3 Position { get; set; }

        public bool Invisible { get; set; }
    }
}