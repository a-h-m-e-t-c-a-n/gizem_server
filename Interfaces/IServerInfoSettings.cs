using System;
using WebRTCServer.Models;

namespace WebRTCServer.Interfaces
{
    public interface IServerInfoSettings
    {
        public string getServerId();
        public ServerInfo getServerInfo(int serverid);
    }
}
