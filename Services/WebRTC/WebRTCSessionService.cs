using System;
using System.Threading.Tasks;
using Grpc.Core;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;

namespace WebRTCServer
{
    public class WebRTCSessionData
    {
        public string SessionId { get; set; }
        public List<string> Peers { get; set; } = new List<string>();
    }
    public class WebRTCSessionService
    {
        private readonly ConcurrentDictionary<String,WebRTCSessionData> list = new ConcurrentDictionary<string, WebRTCSessionData>();

        private string newId()
        {
            var id=Guid.NewGuid().ToString();
            return id;
        }

        /// <summary>
        /// Register Stream connection as WEBRTC Peer
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="peerId"></param>
        /// <param name="streamId"></param>
        /// <returns></returns>
        public void RegisterPeer(string sessionId,string peerId)
        {
            var sessionData = list[sessionId];
            sessionData.Peers.Add(peerId);
            //TODO:save to db
        }
        public void UnregisterPeer(string sessionId,string peerId)
        {
            var sessionData = list[sessionId];
            sessionData.Peers.Remove(peerId);
            
            //TODO:save to db
        }
        public WebRTCSessionData OpenSession()
        {
            var id = newId();
            var sessionData = new WebRTCSessionData() {SessionId = id};
            if (!list.TryAdd(id,sessionData))
            {
                Debug.WriteLine("SessionContext:OpenSession-> TryAdd is failure");
            }
            
            //TODO:save to db
            return sessionData;
        }
        public WebRTCSessionData CloseSession(string sessionId)
        {
            if (!list.TryRemove(sessionId,out WebRTCSessionData sessionData))
            {
                Debug.WriteLine("SessionContext:CloseSession-> TryRemove is failure");
            }
            
            //TODO:save to db
            return sessionData;
        }
        public WebRTCSessionData GetSession(string sessionId)
        {
            if (!list.TryGetValue(sessionId,out WebRTCSessionData sessionData))
            {
                Debug.WriteLine("SessionContext:GetSession-> TryGetValue is failure");
                return sessionData;

            }

            return null;
        }
    }
}
