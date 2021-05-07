using Billing.API.Models;
using System.Collections.Generic;

namespace Billing.API.Services.Invoice
{
    public class InvoiceProviderOptions
    {
        public Dictionary<string, InvoiceConfig> ConfigsBySystem { get; set; }
        public bool UseDummyData { get; set; }
        public string DbConnectionString { get; set; } = string.Empty;
        public string SignatureHashKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
