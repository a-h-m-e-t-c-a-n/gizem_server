using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace WebRTCServer.Interceptors
{
        public class AuthenticationInterceptor : Interceptor
        {

            public AuthenticationInterceptor()
            {
            }

            public override async Task<TResponse> 
                UnaryServerHandler<TRequest, TResponse>(
                    TRequest request,
                    ServerCallContext context,
                    UnaryServerMethod<TRequest, TResponse> continuation
                    )
            {

                
                return await continuation(request, context);
                
            }

            public override async  Task<TResponse> 
                ClientStreamingServerHandler<TRequest, TResponse>(
                    IAsyncStreamReader<TRequest> requestStream, 
                    ServerCallContext context,
                    ClientStreamingServerMethod<TRequest, TResponse> continuation
                    )
            {
                return await continuation(requestStream, context);

            }

            public override async Task 
                ServerStreamingServerHandler<TRequest, TResponse>(
                    TRequest request,
                    IServerStreamWriter<TResponse> responseStream,
                    ServerCallContext context, 
                    ServerStreamingServerMethod<TRequest, TResponse> continuation
                    )
            {
                await continuation(request,responseStream, context);
            }

            public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
                IAsyncStreamReader<TRequest> requestStream,
                IServerStreamWriter<TResponse> responseStream, 
                ServerCallContext context, 
                DuplexStreamingServerMethod<TRequest, TResponse> continuation
                )
            {
               await continuation(requestStream,responseStream, context);
            }
        }
    
}