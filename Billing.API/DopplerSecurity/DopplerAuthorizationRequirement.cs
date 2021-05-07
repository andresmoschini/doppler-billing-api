using Microsoft.AspNetCore.Authorization;

namespace Billing.API.DopplerSecurity
{
    public class DopplerAuthorizationRequirement : IAuthorizationRequirement
    {
        public bool AllowSuperUser { get; set; }
        public bool AllowOwnResource { get; set; }
        public bool AllowSignedPaths { get; set; }
    }
}
