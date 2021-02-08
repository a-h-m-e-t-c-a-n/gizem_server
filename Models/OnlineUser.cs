using System;

namespace WebRTCServer.Models
{
    public class OnlineUser
    {
        public string StreamId { get; set; }
        public User User { get; set; }
    }
}