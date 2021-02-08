using System;

namespace WebRTCServer.Interfaces
{
    public interface IAuthenticationSettings
    {
        public string getSecretKey();
        public TimeSpan getTokenExpiration();
    }
}
