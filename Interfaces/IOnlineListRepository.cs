using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using gizem_models;

namespace WebRTCServer.Interfaces
{
    public interface IOnlineListRepository
    {
        public Task<List<OnlineUser>> getOnlineUsers();
        public Task addToOnlineUser(string userid,string streamid);
        public Task removeFromOnlineUser(string userid);

    }
}