using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using WebRTCServer.Interfaces;
using Google.Protobuf.Collections;
using gizem_services;
using gizem_models;


namespace WebRTCServer
{
    public class WebRTCSignalGRPC : WebRTCSignal.WebRTCSignalBase
    {
        private readonly ILogger<WebRTCSignalGRPC> logger;
        private readonly DataBus dataBus;
        private readonly WebRTCSessionService webRtcSessionContext;
        private readonly IServerUtil serverUtil;

        public WebRTCSignalGRPC(DataBus dataBus,
                                WebRTCSessionService webRtcSessionContext,
                                IServerUtil serverUtil,
                                ILogger<WebRTCSignalGRPC> logger)
        {
            this.dataBus = dataBus;
            this.webRtcSessionContext = webRtcSessionContext;
            this.serverUtil = serverUtil;
            this.logger = logger;

        }
        public override  async Task SubscribeToRoom(WebRTCSubscribeReqest request,
                                                    IServerStreamWriter<WebRTCEvent> responseStream,
                                                    ServerCallContext context)
        {

            logger.Info(() => $"connected {context.Peer}");

            var streamId = serverUtil.GenerateStreamId();
            var readTask = dataBus.ReadAsync<WebRTCEvent>(streamId, async (snapshot) =>
             {
                 await responseStream.WriteAsync(snapshot);

             }, context.CancellationToken);

            await webRtcSessionContext.RegisterPeer(request.SessionId, streamId);

            await readTask;

            logger.Debug(() => $"End of Call {context.Peer}");
        }
        public override async Task SendEvent(WebRTCEvent request,
                                             IServerStreamWriter<EmptyResponse> responseStream,
                                             ServerCallContext context)
        {
             await webRtcSessionContext.SendToOthers(request.SessionId,request.From,request);
        }

    }
}
