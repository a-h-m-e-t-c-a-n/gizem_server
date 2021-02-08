using Grpc.Core;

namespace WebRTCServer
{

    public static class ServerCallContextExtension
    {
        public static string UserId(this ServerCallContext context)
        {
            var userid = context.GetHttpContext().User.Identity.Name;
            return userid;
        }
    }
}