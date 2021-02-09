using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using WebRTCServer.Interfaces;
/*

namespace WebRTCServer
{
    public class StreamGatewayService : gizem_server.StreamGateway.StreamGatewayBase
    {
        private readonly StreamContext _streamContext;

        //private readonly ILogger<StreamContextService> _logger;
        public StreamGatewayService(StreamContext streamContext)
        {
            _streamContext = streamContext;

            //_logger = logger;
            Debug.WriteLine($"StreamGatewayService {DateTime.Now}");
        }

        public override async Task<StreamGatewayP> Forward(StreamGatewayQ request, ServerCallContext context)
        {
            return new StreamGatewayP() {Success = await _streamContext.SendToClient(request.Id, request.Data)};
        }
    }

        
}
*/