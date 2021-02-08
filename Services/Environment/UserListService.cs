using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using gizem_server;
using WebRTCServer.Interceptors;
using WebRTCServer.Interfaces;
using System.Threading;
using WebRTCServer.Models;

namespace WebRTCServer
{
    public class UserListGRPC : gizem_server.UserList.UserListBase
    {
        private readonly ILogger<UserListGRPC> logger;
        private readonly DataBus dataBus;
        private readonly OnlineListService onlineListManager;
        private readonly IServerUtil serverUtil;
        private readonly NotificationService notificationManager;
        private readonly WebRTCSessionService webRTCSessionContext;

        public UserListGRPC(DataBus dataBus,
                               IUserRepository test,
                               OnlineListService onlineListManager,
                               IServerUtil serverUtil,
                               NotificationService notificationManager,
                               WebRTCSessionService webRTCSessionContext,
                               ILogger<UserListGRPC> logger)
        {

            Debug.WriteLine($"WebRTCSignalService {DateTime.Now}");
            this.dataBus = dataBus;
            this.onlineListManager = onlineListManager;
            this.serverUtil = serverUtil;
            this.notificationManager = notificationManager;
            this.webRTCSessionContext = webRTCSessionContext;
            this.logger = logger;
        }

        /* [FrontAuth]
         public override async Task SubscribeListUpdate(SubscribeListUpdateQ request,
                                                               IServerStreamWriter<SubscribeListUpdateP> responseStream,
                                                               ServerCallContext context)
         {
             var userid = context.GetHttpContext().User.Identity.Name;
             var streamId = serverUtil.GenerateStreamId();

             var readTask = dataBus.ReadAsync<SubscribeListUpdateP>(streamId context.CancellationToken);

             await onlineListManager.Add(userid, streamId);
             readTask.
             await onlineListManager.Remove(userid);

             /* var subscription = serverUtil.RegisterStream(context, responseStream);
              await onlineListManager.AddToList(username, subscription.StreamId);
              await subscription.WaitAsync();
              await onlineListManager.RemoveFromList(username);
             logger.Debug(() => $"End of SubscribeListUpdate {context.Peer}");
         }*/

        [FrontAuth]
        public override async Task SubscribeListUpdate(SubscribeListUpdateQ request,
                                                       IServerStreamWriter<SubscribeListUpdateP> responseStream,
                                                       ServerCallContext context)
        {
            var userid = context.GetHttpContext().User.Identity.Name;
            var streamId = serverUtil.GenerateStreamId();
            try
            {
                var readTask = dataBus.ReadAsync<SubscribeListUpdateP>(streamId, async (update) =>
                {
                    await responseStream.WriteAsync(update);
                }, context.CancellationToken);

                if (await onlineListManager.AddByName(userid, streamId))
                {
                    await readTask;
                }
                else
                {
                    throw new Exception("User Exist");
                }

            }
            finally
            {
                dataBus.Complete(streamId);
                await onlineListManager.Remove(userid);
                logger.Debug(() => $"End of SubscribeListUpdate {context.Peer}");
            }

        }

        [FrontAuth]
        public override async Task Ring(RingQ request, IServerStreamWriter<RingP> responseStream, ServerCallContext context)
        {

            CancellationTokenSource s_cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            s_cts.CancelAfter(TimeSpan.FromSeconds(3000));
            var streamId = serverUtil.GenerateStreamId();
            try
            {
                var readTask = dataBus.ReadAsync<CallStatus>(streamId, async (update) =>
                          {
                              if (update.State == CallStatus.Status.Answered)
                              {
                                  await responseStream.WriteAsync(new RingP() { Response = RingP.Types.Status.Accepted, WebRTCSessionId = update.WebRTCSessionId });
                                  dataBus.Complete(streamId);
                              }

                          }, context.CancellationToken);

                if (await notificationManager.SendAsync(request.Userid, "ring", streamId, $"{context.UserId()} is ringing"))
                {
                    await readTask;
                }
                else
                {
                    throw new Exception("peer not found");
                }


            }
            finally
            {
                dataBus.Complete(streamId);
            }


            logger.Debug(() => $"End of Ring {context.Peer}");
        }
        public override async Task Answer(AnswerQ request, IServerStreamWriter<AnswerP> responseStream, ServerCallContext context)
        {
            WebRTCSessionData webRTCSessionData = webRTCSessionContext.OpenSession();

            if (!await dataBus.WriteAsync<CallStatus>(
                request.Context,
                new CallStatus() { Context = request.Context, State = CallStatus.Status.Answered, WebRTCSessionId = webRTCSessionData.SessionId }))
            {
                webRTCSessionContext.CloseSession(webRTCSessionData.SessionId);
                await responseStream.WriteAsync(new AnswerP() { Response = AnswerP.Types.Status.Missed });
            }
            else
            {
                await responseStream.WriteAsync(new AnswerP() { Response = AnswerP.Types.Status.Connected, WebRTCSessionId = webRTCSessionData.SessionId });
            }


        }
    }
}
