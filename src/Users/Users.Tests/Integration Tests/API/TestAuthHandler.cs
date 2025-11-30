using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Users.Tests.Integration_Tests.API
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string AuthenticationScheme = "TestScheme";
        public const string ClaimsHeader = "X-Test-Claims";

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(ClaimsHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail($"Missing header: {ClaimsHeader}"));
            }

            try
            {
                var base64Claims = Request.Headers[ClaimsHeader].ToString();
                var jsonClaims = Encoding.UTF8.GetString(Convert.FromBase64String(base64Claims));

                var claimsData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonClaims);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, claimsData!["UserId"]),
                    new Claim(ClaimTypes.Role, claimsData!["Role"])
                };

                var identity = new ClaimsIdentity(claims, AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to process test claims header.");
                return Task.FromResult(AuthenticateResult.Fail($"Claims processing failed: {ex.Message}"));
            }
        }
    }
}
