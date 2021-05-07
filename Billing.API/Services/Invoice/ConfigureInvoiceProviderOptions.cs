using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Billing.API.Services.Invoice
{
    public class ConfigureInvoiceProviderOptions : IConfigureOptions<InvoiceProviderOptions>
    {
        private const string DEFAULT_CONFIGURATION_SECTION_NAME = "Invoice";

        private readonly IConfiguration _configuration;

        public ConfigureInvoiceProviderOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(InvoiceProviderOptions options)
        {
            _configuration.Bind(DEFAULT_CONFIGURATION_SECTION_NAME, options);
        }
    }
}
