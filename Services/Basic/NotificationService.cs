using System;
using Microsoft.Extensions.Logging;
using gizem_server;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebRTCServer
{
    public class NotificationService
    {
        ConcurrentDictionary<string, string> users = new ConcurrentDictionary<string, string>();
        private readonly DataBus dataBus;
        private readonly ILogger<OnlineListService> logger;

        public NotificationService(
                                 DataBus dataBus,
                                 ILogger<OnlineListService> logger)
        {
            this.dataBus = dataBus;
            this.logger = logger;
        }


        public void Register(String userid, String streamid)
        {
            users.TryAdd(userid, streamid);

        }
        public void Unregister(String userid)
        {
            users.TryRemove(userid, out string old);
        }
        public async Task<bool> SendAsync(string userid, string name,string context, string detail)
        {

            return await dataBus.WriteAsync<NotificationP>(userid, new NotificationP() { Name = name, Context = context, Detail = detail});

        }
    }
}
