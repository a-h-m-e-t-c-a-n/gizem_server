using System;

namespace WebRTCServer.Interfaces
{
    public class ServerInfo
    {
        public int Id { get; set; }
        public String Url { get; set; }
        public String Name { get; set; }

    }
    public interface IServerInfoSettings
    {
        public string getServerId();
        public ServerInfo getServerInfo(int serverid);
    }
}
