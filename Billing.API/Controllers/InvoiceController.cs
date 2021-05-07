using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Billing.API.DopplerSecurity;
using Billing.API.Models;
using Billing.API.Services.Invoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Billing.API.Controllers
{
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private static readonly Regex InvoiceFilenameRegex = new Regex(@"^invoice_(([A-Z]{2})_)?\d{4}-\d{2}-\d{2}_(\d+)\.pdf$");

        private readonly ILogger<InvoiceController> _logger;
        private readonly IInvoiceService _invoiceService;
        private readonly CryptoHelper _cryptoHelper;
        private readonly IOptions<InvoiceProviderOptions> _options;
        private readonly ISapServiceSettingsService _sapServiceSettingsService;

        private const string _defaultSapSystem = "AR";

        public InvoiceController(ILogger<InvoiceController> logger, IInvoiceService invoiceService, CryptoHelper cryptoHelper, IOptions<InvoiceProviderOptions> options, ISapServiceSettingsService sapServiceSettingsService)
        {
            _logger = logger;
            _invoiceService = invoiceService;
            _cryptoHelper = cryptoHelper;
            _options = options;
            _sapServiceSettingsService = sapServiceSettingsService;
        }

        [HttpGet]
        [Authorize]
        [Route("/accounts/{origin}/{clientId:int:min(0)}/invoices/")]
        public async Task<IActionResult> GetInvoices([FromRoute] string origin,
            [FromRoute] int clientId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortColumn = "AccountId",
            [FromQuery] bool sortAsc = true)
        {
            _logger.LogDebug("Getting invoices for {0} client {1}", origin, clientId);

            if (!TryGetClientPrefix(origin, out var clientPrefix))
                return BadRequest();

            var response = await _invoiceService.GetInvoices(clientPrefix, clientId, page, pageSize, sortColumn, sortAsc);

            foreach (var invoice in response.Items)
            {
                var urlPath = $"/accounts/{origin}/{clientId}/invoices/{invoice.Filename}";

                invoice.Links.Add(new Link
                {
                    Rel = "file",
                    Href = $"{_options.Value.BaseUrl}{urlPath}?_s={_cryptoHelper.GenerateSignature(urlPath)}",
                    Description = "Download invoice"
                });
            }

            return Ok(response);
        }

        [HttpGet]
        [Authorize(DopplerSecurityDefaults.DEFAULT_OR_SIGNED_PATHS_POLICY)]
        [Route("/accounts/{origin}/{clientId:int:min(1)}/invoices/{filename}")]
        public async Task<IActionResult> GetInvoiceFile([FromRoute] string origin, [FromRoute] int clientId, [FromRoute] string filename, [FromQuery(Name = "_s")] string signature)
        {
            try
            {
                _logger.LogDebug("Getting invoice for {0} client {1} filename {2}", origin, clientId, filename);

                if (!TryGetClientPrefix(origin, out var clientPrefix))
                    return BadRequest();

                var match = InvoiceFilenameRegex.Match(filename);

                if (!match.Success)
                    return BadRequest();

                var fileId = match.Groups[3].Value.ToInt32();
                var sapSystem = !string.IsNullOrEmpty(match.Groups[2].Value) ? match.Groups[2].Value : _defaultSapSystem;

                ValidateSapSystem(sapSystem);

                var response = await _invoiceService.GetInvoiceFile(clientPrefix, clientId, sapSystem, fileId);

                if (response == null)
                    return NotFound();

                return File(response, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("/testSap")]
        public async Task<IActionResult> TestSapConnection()
        {
            var response = await _invoiceService.TestSapConnection();

            return Ok(response);
        }

        [HttpGet]
        [Route("/testSapUs")]
        public async Task<IActionResult> TestSapUsConnection()
        {
            var response = await _invoiceService.TestSapUsConnection();

            return Ok(response);
        }

        private static bool TryGetClientPrefix(string origin, out string clientPrefix)
        {
            switch (origin.ToLowerInvariant())
            {
                case "doppler":
                    clientPrefix = "CD";

                    break;
                case "relay":
                    clientPrefix = "CR";

                    break;
                case "clientmanager":
                    clientPrefix = "CM";

                    break;
                default:
                    clientPrefix = string.Empty;

                    return false;
            }

            return true;
        }

        private void ValidateSapSystem(string sapSystem)
        {
            _sapServiceSettingsService.GetSapSchema(sapSystem);
        }
    }
}
