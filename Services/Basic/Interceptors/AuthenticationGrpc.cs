using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using WebRTCServer.Interceptors;
using WebRTCServer.Interfaces;

namespace WebRTCServer.Interceptors
{
    public class GrpcFrontRequirement : IAuthorizationRequirement
    { 

    }
    
    public class GrpcFrontAuthHandler : AuthorizationHandler<GrpcFrontRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GrpcFrontAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GrpcFrontRequirement requirement)
        {

            var headers = _httpContextAccessor.HttpContext.Request.Headers;
            StringValues token;
            if (!headers.TryGetValue("token", out token))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            context.Succeed(requirement);
            return Task.CompletedTask;

        }
    }
    public class GrpcBackRequirement : IAuthorizationRequirement
    { 

    }
    public class GrpcBackAuthHandler : AuthorizationHandler<GrpcBackRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GrpcBackAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GrpcBackRequirement requirement)
        {

            /*var headers = _httpContextAccessor.HttpContext.Request.Headers;
            if (!headers.TryGetValue("token", out StringValues token))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            if (token != "1")
            {
                context.Fail();
                return Task.CompletedTask;
            }*/

            context.Succeed(requirement);
            return Task.CompletedTask;

        }
    }

    public class FrontAuth : AuthorizeAttribute
    {
        public FrontAuth() : base("front")
        {
        }
    }
    public class BackAuth : AuthorizeAttribute
    {
        public BackAuth() : base("back")
        {
        }
    }
    public static class AuthenticationExtensions
    {
        public static void 
            AddGrpcAuthentiction(
                this IServiceCollection services,
                IAuthenticationSettings settings
            )
        {
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.getSecretKey())),
                    ValidateIssuer = false,
                    //ValidIssuer = "jwtSettings.Issuer",

                    ValidateAudience = false,
                    //ValidAudience = "jwtSettings.Audience",

                    ValidateLifetime = true,
                    ClockSkew = settings.getTokenExpiration()
                };
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("front", policy =>
                {                   
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                   // policy.AddRequirements(new GrpcFrontRequirement());
                });
                options.AddPolicy("back", policy =>
                {                   
                    policy.AddRequirements(new GrpcBackRequirement());
                });
                
                
            });
            services.AddHttpContextAccessor();
            services.AddSingleton<IAuthorizationHandler, GrpcFrontAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, GrpcBackAuthHandler>();
        }

    }
        
}