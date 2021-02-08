using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace WebRTCServer.Models
{
    public class WebRTCSessionStatus
    {
        public enum EventTypes { None, PeersChanged }
        public EventTypes Event { get; set; }
        public string SessionId { get; set; }
        public List<string> Peers { get; private set; } = new List<string>();


    }
}