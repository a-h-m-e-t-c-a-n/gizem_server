using System;
using System.Security.Principal;

namespace WebRTCServer.Models
{
    public class CallStatus
    {
        public enum Status {None,Ringing,Answered,Missed}
        public String Context { get; set; }
        public Status State  { get; set; }
        public string WebRTCSessionId { get; set; }
        
    }
}