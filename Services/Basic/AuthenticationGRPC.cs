using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using gizem_server;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using WebRTCServer.Interfaces;

namespace WebRTCServer
{
    public class AuthenticationGRPC: gizem_server.Authentication.AuthenticationBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationSettings _authenticationSettings;

        public AuthenticationGRPC(IUserRepository userRepository,IAuthenticationSettings authenticationSettings)
        {
            _userRepository = userRepository;
            _authenticationSettings = authenticationSettings;
        }
        

        public override async Task<AuthenticationP> Login(AuthenticationQ request, ServerCallContext context)
        {
            //TODO check request.Username and request.Password

            var  user=await _userRepository.getUserByName(request.Username);
            
            var sessionId = Guid.NewGuid().ToString();
            
            

            // Creates the signed JWT
            var symmetricSecurityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.getSecretKey()));
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("session",sessionId),
                }),
                Expires =DateTime.UtcNow.Add(_authenticationSettings.getTokenExpiration()),// DateTime.UtcNow.AddYears(2),
                //Issuer = "MyWebsite.com",
                //Audience = "MyWebsite.com",
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);
            
            return new AuthenticationP() {Token = accessToken};
        }        
    }
}