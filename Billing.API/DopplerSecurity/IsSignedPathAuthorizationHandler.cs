using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Billing.API.DopplerSecurity
{
    public class IsSignedPathAuthorizationHandler : AuthorizationHandler<DopplerAuthorizationRequirement>
    {
        private readonly ILogger<IsSignedPathAuthorizationHandler> _logger;
        private readonly CryptoHelper _cryptoHelper;

        public IsSignedPathAuthorizationHandler(ILogger<IsSignedPathAuthorizationHandler> logger, CryptoHelper cryptoHelper)
        {
            _logger = logger;
            _cryptoHelper = cryptoHelper;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DopplerAuthorizationRequirement requirement)
        {
            if (requirement.AllowSignedPaths && IsValidSignature(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool IsValidSignature(AuthorizationHandlerContext context)
        {
            if (!(context.Resource is AuthorizationFilterContext resource))
            {
                _logger.LogWarning("Is not possible access to Resource information.");
                return false;
            }

            var signedPaths = context.User.FindAll(c => c.Type == DopplerSecurityDefaults.SIGNED_PATH_CLAIM_TYPE);

            if (!signedPaths.Any())
            {
                _logger.LogDebug("No signed paths in this request.");
                return false;
            }

            if (!signedPaths.Any(x => x.Value == resource.HttpContext.Request.Path))
            {
                _logger.LogDebug("Signed path does not match request's path.");
                return false;
            }

            return true;
        }
    }
}
