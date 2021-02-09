using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using gizem_services;
using gizem_models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebRTCServer.Interceptors;
using WebRTCServer.Interfaces;
using WebRTCServer.Mock;
using WebRTCServer.Utils;

namespace WebRTCServer
{

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddGrpc();
            services.AddGrpcAuthentiction(new Settings());
            services.AddSingleton<DataBus>();
            services.AddSingleton<IServerInfoSettings, Settings>();
            services.AddSingleton<IAuthenticationSettings, Settings>();
            services.AddSingleton<IServerUtil, ServerUtil>();
            services.AddSingleton<WebRTCSessionService>();
            services.AddSingleton<IUserRepository, UserRepositoryMock>();
            services.AddSingleton<OnlineListService>();
            services.AddSingleton<NotificationService>();


            // services.AddHostedService<TimedHostedService>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<AuthenticationGRPC>();
                endpoints.MapGrpcService<WebRTCSignalGRPC>();
                endpoints.MapGrpcService<UserListGRPC>();


               // endpoints.MapGrpcService<StreamGatewayService>();



                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("<html>"
                                                      + "<head></head>"
                                                        + "<body>"
                                                            + "<h1></h1>"
                                                        + "</body>"
                                                      + "</html>");
                });
            });
        }
    }
}
