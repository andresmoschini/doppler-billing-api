using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Billing.API.DopplerSecurity
{
    public class ConfigureDopplerSecurityOptions : IConfigureOptions<DopplerSecurityOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly IFileProvider _fileProvider;

        public ConfigureDopplerSecurityOptions(IConfiguration configuration, IHostingEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _fileProvider = webHostEnvironment.ContentRootFileProvider;
        }

        private static string ReadToEnd(IFileInfo fileInfo)
        {
            using (var stream = fileInfo.CreateReadStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static RsaSecurityKey ParseXmlString(string xmlString)
        {
            using (var rsaProvider = new RSACryptoServiceProvider())
            {
                rsaProvider.FromXmlStringAlt(xmlString);
                var rsaParameters = rsaProvider.ExportParameters(false);
                return new RsaSecurityKey(RSA.Create(rsaParameters));
            }
        }

        public void Configure(DopplerSecurityOptions options)
        {
            var path = _configuration.GetValue(
                DopplerSecurityDefaults.PUBLIC_KEYS_FOLDER_CONFIG_KEY,
                DopplerSecurityDefaults.PUBLIC_KEYS_FOLDER_DEFAULT_CONFIG_VALUE);
            var files = _fileProvider.GetDirectoryContents(path).Where(x => !x.IsDirectory);
            var publicKeys = files
                .Select(ReadToEnd)
                .Select(ParseXmlString)
                .ToArray();

            options.SkipLifetimeValidation = false;
            options.SigningKeys = publicKeys;
        }
    }
}
