using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using gizem_services;
using gizem_models;
using Microsoft.AspNetCore.Authorization;
using WebRTCServer.Interceptors;
using WebRTCServer.Interfaces;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using gizem_models;

namespace WebRTCServer
{
    public class OnlineListService
    {
        ConcurrentDictionary<String, OnlineUser> users = new ConcurrentDictionary<string, OnlineUser>();
        private readonly DataBus dataBus;
        private readonly IUserRepository userRepository;
        private readonly ILogger<OnlineListService> logger;

        public OnlineListService(DataBus dataBus,
                                 IUserRepository userRepository,
                                 ILogger<OnlineListService> logger)
        {
            this.dataBus = dataBus;
            this.userRepository = userRepository;
            this.logger = logger;
        }

        public async Task NotifyUserList()
        {
            var onlineUsers = users.Values.ToArray();
            foreach (var onlineUser1 in onlineUsers)
            {
                var response = new UserListData();
                foreach (var onlineUser2 in onlineUsers)
                {
                    var userInfo = new UserInfo();
                    userInfo.Userid = onlineUser2.User.UserId;
                    userInfo.Username = onlineUser2.User.Name;
                    response.User.Add(userInfo);
                }
                await dataBus.WriteAsync(onlineUser1.StreamId, response);
            }
            logger.Info(() => $"user list notified");
        }
        public async Task<bool> Add(String userid, String streamid)
        {
            var user = await userRepository.getUserById(userid);
            if(users.TryAdd(userid, new OnlineUser() { User = user, StreamId = streamid })){
                await NotifyUserList();
                return true;
            }
            else{
                return false;
            }
 
        }
        public async Task<bool> AddByName(String username, String streamid)
        {
            var user = await userRepository.getUserByName(username);

            return await Add(user.UserId,streamid);

        }
        public async Task Remove(String userid)
        {
            users.TryRemove(userid, out OnlineUser old);
            await NotifyUserList();
            logger.Info(() => $"{userid} removed is offline");

        }
        public String GetUserStream(String userid)
        {
            if (users.TryGetValue(userid, out OnlineUser onlineUser))
            {
                return onlineUser.StreamId;
            }
            return null;
        }
    }
}
