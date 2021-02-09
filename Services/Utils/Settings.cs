using System;
using System.Collections.Generic;
using WebRTCServer.Interfaces;
using gizem_models;

namespace WebRTCServer.Utils
{
    public class Settings:IAuthenticationSettings,IServerInfoSettings
    {
        
        public string getSecretKey()
        {
            return "SECRET1FORAUTHENTICATION";
        }

        public TimeSpan getTokenExpiration()
        {
            return TimeSpan.FromDays(30);
        }

        

        public string getServerId()
        {

            return "0";
        }

        public ServerInfo getServerInfo(int serverid)
        {
            return new ServerInfo(){Id = 0,Url = "http://127.0.0.1:5001",Name = "DOCKER SERVER"};
        }
    }
}