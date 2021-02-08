using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using gizem_server;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using WebRTCServer.Interfaces;
using WebRTCServer.Models;

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


        public override async Task SubscribeSession(SubscribeSessionQ request, IServerStreamWriter<SubscribeSessionP> responseStream, ServerCallContext context)
        {
            logger.Info(() => $"connected {context.Peer}");

            var streamId = serverUtil.GenerateStreamId();
            await dataBus.ReadAsync<WebRTCSessionStatus>(streamId, async (update) =>
            {
               switch (update.Event)
                {
                    case WebRTCSessionStatus.EventTypes.PeersChanged:
                        var data = new SubscribeSessionP();
                        data.Peers.AddRange(update.Peers);
                        await responseStream.WriteAsync(data);
                        break;
                }
            }, context.CancellationToken);
            logger.Debug(() => $"End of Call {context.Peer}");
        }


    }
}
