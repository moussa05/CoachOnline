using CoachOnline.Interfaces;
using CoachOnline.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CoachOnline.Helpers
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUser _userService;

        public BasicAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUser userService)
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();

            // skip authentication if endpoint has [AllowAnonymous] attribute but check for 2 methods
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null && 
                !(Request.Path.Value == "/api/Player/OpenCourse" || Request.Path.Value == "/api/Player/OpenEpisode"
                || (Request.Path.Value.ToLower().Contains("/api/player/course/") && Request.Path.Value.ToLower().Contains("/comments") && Request.Method == "GET")
                )            
                )
            { 
                return AuthenticateResult.NoResult(); 
            }
            else if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null 
                && (Request.Path.Value == "/api/Player/OpenCourse" || Request.Path.Value == "/api/Player/OpenEpisode"
                  || (Request.Path.Value.ToLower().Contains("/api/player/course/") && Request.Path.Value.ToLower().Contains("/comments") && Request.Method == "GET")))
            {
                if (!Request.Headers.ContainsKey("Authorization"))
                {
                    var claimsAn = new[] {
                    new Claim(ClaimTypes.NameIdentifier, ""),
                    new Claim(ClaimTypes.Role,  ""),
                    new Claim(ClaimTypes.Email, "")
                };
                    var identityAn = new ClaimsIdentity(claimsAn, Scheme.Name);
                    var principalAn = new ClaimsPrincipal(identityAn);
                    var ticketAn = new AuthenticationTicket(principalAn, Scheme.Name);
                    return AuthenticateResult.Success(ticketAn);
                }
                else
                {
                    var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                    if (string.IsNullOrEmpty(authHeader.Parameter))
                    {
                        var claimsAn = new[] {
                    new Claim(ClaimTypes.NameIdentifier, ""),
                    new Claim(ClaimTypes.Role,  ""),
                    new Claim(ClaimTypes.Email, "")
                };
                        var identityAn = new ClaimsIdentity(claimsAn, Scheme.Name);
                        var principalAn = new ClaimsPrincipal(identityAn);
                        var ticketAn = new AuthenticationTicket(principalAn, Scheme.Name);
                        return AuthenticateResult.Success(ticketAn);
                    }
                }
            }

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                
                    return AuthenticateResult.Fail("Missing Authorization Header");
                
            }



            User user = null;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                //var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                //var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                //var username = credentials[0];
                //var password = credentials[1];
                //user = await _userService.Authenticate(username, password);
                user = await _userService.Authenticate(authHeader.Parameter);
            }
            catch
            {
               
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

        
            if (user == null)
                return AuthenticateResult.Fail("Invalid User Token");
                //return AuthenticateResult.Fail("Invalid Username or Password");

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role,  user.UserRole.ToString()),
                new Claim(ClaimTypes.Email, user.EmailAddress)        
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
