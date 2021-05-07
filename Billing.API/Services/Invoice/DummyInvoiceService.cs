using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Billing.API.DopplerSecurity;
using Billing.API.Models;
using Microsoft.Extensions.Options;

namespace Billing.API.Services.Invoice
{
    public class DummyInvoiceService : IInvoiceService
    {
        private readonly CryptoHelper _cryptoHelper;
        private readonly IOptions<InvoiceProviderOptions> _options;

        public DummyInvoiceService(CryptoHelper cryptoHelper, IOptions<InvoiceProviderOptions> options)
        {
            _cryptoHelper = cryptoHelper;
            _options = options;
        }

        public async Task<PaginatedResult<InvoiceListItem>> GetInvoices(string clientPrefix, int clientId, int page, int pageSize, string sortColumn, bool sortAsc)
        {
            if (clientId <= 0)
                throw new ArgumentException();

            var invoices = GetDummyInvoices(clientPrefix, clientId, page, pageSize, sortColumn, sortAsc);

            return await Task.FromResult(invoices);
        }

        public Task<byte[]> GetInvoiceFile(string clientPrefix, int clientId, string sapSystem, int fileId)
        {
            var bytes = File.ReadAllBytes("sample.pdf");

            return Task.FromResult(bytes);
        }

        public async Task<string> TestSapConnection()
        {
            return await Task.FromResult("Successful");
        }

        public async Task<string> TestSapUsConnection()
        {
            return await Task.FromResult("Successful");
        }

        private PaginatedResult<InvoiceListItem> GetDummyInvoices(string clientPrefix, int clientId, int page, int pageSize, string sortColumn, bool sortAsc)
        {
            var invoices = Enumerable.Range(1, 50).Select(x => new InvoiceListItem(
                x == 50 ? "NC" : "FC",
                $"A-0001-00000000{x}",
                $"Prod {x}",
                $"{clientPrefix}{clientId:0000000000000}",
                new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, TimeSpan.Zero).AddDays(x),
                new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, TimeSpan.Zero).AddDays(x + 15),
                DateTime.Today.AddDays(x),
                "ARS",
                x == 50 ? -100 : 15500,
                x == 50 ? -100 : 1000,
                $"invoice_AR_{DateTime.Today.AddDays(x):yyyy-MM-dd}_{x}.pdf",
                x)
            ).AsQueryable();

            var invoiceSorted = GetInvoicesSorted(invoices, sortColumn, sortAsc).ToList();
            var paginatedInvoices = invoiceSorted;

            if (page > 0 && pageSize > 0)
            {
                paginatedInvoices = invoiceSorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            return new PaginatedResult<InvoiceListItem> { Items = paginatedInvoices, TotalItems = invoiceSorted.Count };
        }

        private static IEnumerable<InvoiceListItem> GetInvoicesSorted(IQueryable<InvoiceListItem> invoices, string sortColumn, bool sortAsc)
        {
            return invoices.OrderBy(sortColumn + (!sortAsc ? " descending" : ""));
        }
    }
}
