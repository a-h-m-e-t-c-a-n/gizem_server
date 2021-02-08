using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCServer.Interfaces;
using WebRTCServer.Models;

namespace WebRTCServer.Mock
{
    public class OnlineListRepositoryMock:IOnlineListRepository
    {
        private ConcurrentDictionary<string, OnlineUser> _users = new ConcurrentDictionary<string, OnlineUser>();

        public Task<List<OnlineUser>> getOnlineUsers()
        {
            return Task.FromResult(_users.Values.ToList<OnlineUser>());
        }

        public Task addToOnlineUser(string userid, string streamid)
        {
            _users.TryAdd(userid, new OnlineUser()
            {
                StreamId = streamid, 
                User = new User()
                {
                    userid = userid,
                    Name = "?????"
                }
            });
            return Task.CompletedTask;
        }

        public  Task removeFromOnlineUser(string userid)
        {
           return Task.FromResult(_users.TryRemove(userid,out OnlineUser old));
        }

    }
}