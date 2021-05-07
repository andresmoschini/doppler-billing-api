using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Billing.API.DopplerSecurity
{
    public class IsOwnResourceAuthorizationHandler : AuthorizationHandler<DopplerAuthorizationRequirement>
    {
        private readonly ILogger<IsOwnResourceAuthorizationHandler> _logger;

        public IsOwnResourceAuthorizationHandler(ILogger<IsOwnResourceAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DopplerAuthorizationRequirement requirement)
        {
            if (requirement.AllowOwnResource && IsOwnResource(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool IsOwnResource(AuthorizationHandlerContext context)
        {
            var tokenUserId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!(context.Resource is AuthorizationFilterContext resource))
            {
                _logger.LogWarning("Is not possible access to Resource information.");
                return false;
            }

            if (!resource.RouteData.Values.TryGetValue("clientId", out var clientId) || clientId?.ToString() != tokenUserId)
            {
                _logger.LogWarning("The IdUser into the token is different that in the route. The user hasn't permissions.");
                return false;
            }
            // TODO: check token Issuer information, to validate right origin

            return true;
        }
    }
}
