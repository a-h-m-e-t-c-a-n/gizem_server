using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using gizem_services;
using gizem_models;
using WebRTCServer.Interceptors;
using WebRTCServer.Interfaces;
using System.Threading;
using gizem_models;

namespace WebRTCServer
{
    public class UserListGRPC : UserList.UserListBase
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
        public override async Task Subscribe(EmptyRequest request, IServerStreamWriter<UserListData> responseStream, ServerCallContext context)
        {
            var userid = context.GetHttpContext().User.Identity.Name;
            var streamId = serverUtil.GenerateStreamId();
            try
            {
                var readTask = dataBus.ReadAsync<UserListData>(streamId, async (update) =>
                {
                    await responseStream.WriteAsync(update);
                }, context.CancellationToken);

                if (await onlineListManager.Add(userid, streamId))
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
        public override async Task Ring(RingRequest request, IServerStreamWriter<RingResponse> responseStream, ServerCallContext context)
        {

            CancellationTokenSource s_cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            s_cts.CancelAfter(TimeSpan.FromSeconds(3000));
            var streamId = serverUtil.GenerateStreamId();
            try
            {
                var readTask = dataBus.ReadAsync<CallStatus>(streamId, async (snapshot) =>
                          {
                              if (snapshot.State == CallStatus.Types.Status.Answered)
                              {
                                  await responseStream.WriteAsync(new RingResponse() { Response = RingResponse.Types.Status.Accepted, WebRTCSessionId = snapshot.WebRTCSessionId });
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
        public override async Task Answer(AnswerRequest request, IServerStreamWriter<AnswerResponse> responseStream, ServerCallContext context)
        {
            WebRTCSessionData webRTCSessionData = webRTCSessionContext.OpenSession();

            if (!await dataBus.WriteAsync<CallStatus>(
                request.Context,
                new CallStatus() { Context = request.Context, State = CallStatus.Types.Status.Answered, WebRTCSessionId = webRTCSessionData.SessionId }))
            {
                webRTCSessionContext.CloseSession(webRTCSessionData.SessionId);
                await responseStream.WriteAsync(new AnswerResponse() { Response = AnswerResponse.Types.Status.Missed });
            }
            else
            {
                await responseStream.WriteAsync(new AnswerResponse() { Response = AnswerResponse.Types.Status.Connected, WebRTCSessionId = webRTCSessionData.SessionId });
            }


        }
    }
}

