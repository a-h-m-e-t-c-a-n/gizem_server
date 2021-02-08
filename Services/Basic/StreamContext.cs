using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using WebRTCServer.Interfaces;

namespace WebRTCServer
{
    public class StreamContext
    {
        private readonly IServerUtil _serverUtil;
        private readonly ILogger<StreamContext> _logger;
        private readonly ConcurrentDictionary<string, Tuple<SemaphoreSlim, object,CancellationTokenSource>> _peerList = new ConcurrentDictionary<string, Tuple<SemaphoreSlim, object, CancellationTokenSource>>();


        public StreamContext(IServerUtil serverUtil,ILogger<StreamContext> logger)
        {
            _serverUtil = serverUtil;
            _logger = logger;
        }

        public CancellationToken RegisterStream(string id,Object stream)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            if (_peerList.TryAdd(id,Tuple.Create(new SemaphoreSlim(1,1),stream,cancellationTokenSource)))
            {
                return cancellationTokenSource.Token;
            }
            else
            {
                Debug.WriteLine("PeerGateway:Register-> TryAdd is failure");
            }
            return CancellationToken.None;

        }

        public void UnregisterStream(string id)
        {
            if (_peerList.TryRemove(id, out Tuple<SemaphoreSlim, object, CancellationTokenSource> old))
            {
                old.Item3.Cancel();
            }
            else
            {
                Debug.WriteLine("PeerGateway:Unregister-> TryRemove is failure");
                
            }
        }
      
        private async Task<bool> Forward<T>(int serverid,string id,T response)
            where T:IMessage,new()
        {
            using var channel = await _serverUtil.getGrpcClientChannel(serverid);
            var client = new backend_client.StreamGateway.StreamGatewayClient(channel);
            client.Forward(new backend_client.StreamGatewayQ() {Id = id,Data = Any.Pack(response)});
            return false;
        }

        public async Task<bool> SendToClient(string id, Any response)
        {
          
                if (_peerList.TryGetValue(id, out Tuple<SemaphoreSlim, object, CancellationTokenSource> stream))
                {
                    try
                    {
                        await stream.Item1.WaitAsync();

                        var outType =  stream.Item2.GetType().GetGenericArguments().First();
                        var unPackMethod = response.GetType().GetMethod("Unpack").MakeGenericMethod(outType);
                        var outputData = unPackMethod.Invoke(response,null);

                        var writeMethod = stream.Item2.GetType().GetMethod("WriteAsync");
                        await (Task) writeMethod.Invoke(stream.Item2, new[] {outputData});
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(() => ex.Message);
                    }
                    finally
                    {
                        stream.Item1.Release();

                    }
                }

                return false;

        }

        public async Task<bool> Send<T>(string streamId,T response)
            where T:IMessage,new() 
        {
            
            
            var parts=streamId.Split("#");
            var peerid = parts[0];
            int serverid = 0;
            if (parts.Length > 1)
            {
                serverid = int.Parse(parts[1]);
            }
            if (serverid == 0)
            {
                if (_peerList.TryGetValue(streamId, out Tuple<SemaphoreSlim, object, CancellationTokenSource> stream))
                {
                    
                    IAsyncStreamWriter<T> writer = (IAsyncStreamWriter<T>) stream.Item2;
                    
                    await stream.Item1.WaitAsync();
                    try
                    {
                        
                        await writer.WriteAsync(response);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    finally
                    {
                        stream.Item1.Release();
                    }

                    return true;


                }
                else
                {
                    return false;
                }
            }
            else
            {
                return await Forward(serverid,streamId,response);
            }
        }

       
    }
}
