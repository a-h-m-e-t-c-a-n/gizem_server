using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using gizem_services;
using gizem_models;
using Grpc.Core;
using WebRTCServer.Interfaces;


namespace WebRTCServer
{
    public class NotificationGRPC : Notification.NotificationBase
    {
        private readonly DataBus dataBus;
        private readonly IServerUtil serverUtil;
        private readonly NotificationService notificationManager;

        public NotificationGRPC(DataBus dataBus, IServerUtil serverUtil, NotificationService notificationManager)
        {
            this.dataBus = dataBus;
            this.serverUtil = serverUtil;
            this.notificationManager = notificationManager;
        }

        public override async Task Subscribe(EmptyRequest request, IServerStreamWriter<NotificationData> responseStream, ServerCallContext context)
        {

            var userid = context.GetHttpContext().User.Identity.Name;
            var streamId = serverUtil.GenerateStreamId();

            try
            {
                await dataBus.ReadAsync<NotificationData>(streamId, async (snapShot) =>
                {
                    await responseStream.WriteAsync(snapShot);
                }, context.CancellationToken);
            }
            finally
            {
                notificationManager.Unregister(userid);

            }
        }

    }


}
