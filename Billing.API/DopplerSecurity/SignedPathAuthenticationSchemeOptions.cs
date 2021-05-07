using Microsoft.AspNetCore.Authentication;

namespace Billing.API.DopplerSecurity
{
    public class SignedPathAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string QueryStringParameterName { get; set; } = DopplerSecurityDefaults.SIGNED_PATH_QUERY_STRING_PARAMETER_NAME;
    }
}
