using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;

namespace Billing.API.DopplerSecurity
{
    public class SignedPathAuthenticationHandler : AuthenticationHandler<SignedPathAuthenticationSchemeOptions>
    {
        private readonly ILogger<SignedPathAuthenticationHandler> _logger;
        private readonly CryptoHelper _cryptoHelper;

        public SignedPathAuthenticationHandler(
            IOptionsMonitor<SignedPathAuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            CryptoHelper cryptoHelper,
            ILogger<SignedPathAuthenticationHandler> logger)
            : base(options, loggerFactory, encoder, clock)
        {
            _logger = logger;
            _cryptoHelper = cryptoHelper;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Query.TryGetValue(Options.QueryStringParameterName, out var signature))
            {
                _logger.LogDebug("Cannot get the Signature from the query string.");
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (_cryptoHelper.GenerateSignature(Request.Path) != signature)
            {
                _logger.LogWarning("Wrong signature for requested path.");
                // TODO: Consider return Fail in place of NoResult when the
                // signature is wrong
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var identity = new ClaimsIdentity(
                authenticationType: DopplerSecurityDefaults.SIGNED_PATH_SCHEME,
                nameType: ClaimsIdentity.DefaultNameClaimType,
                roleType: ClaimsIdentity.DefaultRoleClaimType);

            identity.BootstrapContext = signature;
            identity.AddClaim(new Claim(DopplerSecurityDefaults.SIGNED_PATH_CLAIM_TYPE, Request.Path));
            var principal =  new ClaimsPrincipal(identity);

            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(
                    principal,
                    DopplerSecurityDefaults.SIGNED_PATH_SCHEME
            )));
        }
    }
}
