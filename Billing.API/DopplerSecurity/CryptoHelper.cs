using System;
using System.Security.Cryptography;
using System.Text;
using Billing.API.Services.Invoice;
using Microsoft.Extensions.Options;

namespace Billing.API.DopplerSecurity
{
    public class CryptoHelper
    {
        private readonly string _signatureHashKey;

        public CryptoHelper(IOptions<InvoiceProviderOptions> options)
        {
            _signatureHashKey = options.Value.SignatureHashKey;
        }

        public string GenerateSignature(string payload)
        {
            var key = $"{payload}{_signatureHashKey}";

            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));

                return Convert.ToBase64String(bytes).ReplaceByEmpty("/", "+", "=");
            }
        }
    }
}
