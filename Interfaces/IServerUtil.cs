using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace WebRTCServer.Interfaces
{
    public class StreamSubscription
    {
        public StreamSubscription(string streamId, CancellationToken token)
        {
            StreamId = streamId;
            Token = token;
        }

        public string StreamId { get; private set; }
        public CancellationToken Token { get; private set; }
        public async Task<bool> WaitAsync(int millisecondsTimeout = -1)
        {
            RegisteredWaitHandle registeredHandle = null;
            CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);
            try
            {
                var tcs = new TaskCompletionSource<bool>();
                registeredHandle = ThreadPool.RegisterWaitForSingleObject(Token.WaitHandle,
                 (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut),tcs,millisecondsTimeout,true);
                /* tokenRegistration = cancellationToken.Register(
                     state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
                     tcs);*/
                return await tcs.Task;
            }
            finally
            {
                if (registeredHandle != null)
                    registeredHandle.Unregister(null);
                tokenRegistration.Dispose();
            }
            
        }
    }
    public interface IServerUtil
    {
        string GenerateStreamId();
        //StreamSubscription RegisterStream(ServerCallContext context, Object stream);
        Task<GrpcChannel> getGrpcClientChannel(int serverid);
    }
}