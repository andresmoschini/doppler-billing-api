using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Billing.API.Services.Invoice
{
    public class SapServiceSettingsService : ISapServiceSettingsService
    {
        private readonly InvoiceProviderOptions _options;

        public SapServiceSettingsService(IOptions<InvoiceProviderOptions> options)
        {
            _options = options.Value;
        }

        public string GetSapSchema(string sapSystem)
        {
            // Check if exists a sap schema for the sap system
            if (!_options.ConfigsBySystem.TryGetValue(sapSystem, out var config))
            {
                throw new ArgumentException($"The sapSystem '{sapSystem}' is not supported. Only supports: {string.Join(", ", _options.ConfigsBySystem.Select(x => x.Key))}");
            }

            return config.Schema;
        }
    }
}
