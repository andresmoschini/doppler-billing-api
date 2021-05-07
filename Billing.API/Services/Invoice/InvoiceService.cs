using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Billing.API.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sap.Data.Hana;

namespace Billing.API.Services.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ILogger<InvoiceService> _logger;
        private readonly IOptions<InvoiceProviderOptions> _options;
        private readonly ISapServiceSettingsService _sapServiceSettingsService;

        public InvoiceService(ILogger<InvoiceService> logger, IOptions<InvoiceProviderOptions> options, ISapServiceSettingsService sapServiceSettingsService)
        {
            _logger  = logger;
            _options = options;
            _sapServiceSettingsService = sapServiceSettingsService;
        }

        public async Task<PaginatedResult<InvoiceListItem>> GetInvoices(string clientPrefix, int clientId, int page, int pageSize, string sortColumn, bool sortAsc)
        {
            var sapSystems = _options.Value.ConfigsBySystem;
            IQueryable<InvoiceListItem> invoices = new List<InvoiceListItem>().AsQueryable();

            // Get invoices for all supported sap system because the client can be in more than one sap system.
            // For example: One client exists in SAP AR and then it changes the billing system to QBL then the client is created in the SAP US
            // for it reason we need get the invoices for all supported sap system.
            foreach (var sapSystem in sapSystems)
            {
                var dt = await GetInvoiceRecords(clientPrefix, clientId, sapSystem.Key, null);

                invoices = invoices.Union(
                    dt.Select().Select(dr => new InvoiceListItem(
                    dr.Field<string>("DocumentType"),
                    dr.Field<string>("DocumentNumber"),
                    clientPrefix,
                    clientId.ToString(),
                    dr.Field<DateTime>("CreateDate").ToDateTimeOffSet(),
                    dr.Field<DateTime>("DocDueDate").ToDateTimeOffSet(),
                    dr.Field<DateTime>("SendDate").ToDateTimeOffSet(),
                    dr.Field<string>("DocCur"),
                    dr.Field<decimal>("DocTotal").ToDouble(),
                    dr.Field<decimal>("PaidToDate").ToDouble(),
                    $"invoice_{sapSystem.Key}_{dr.Field<DateTime>("SendDate"):yyyy-MM-dd}_{dr.Field<int>("AbsEntry")}.{dr.Field<string>("FileExt")}",
                    dr.Field<int>("AbsEntry"))).AsQueryable());
            }

            // TODO: When we can get the data from database we will try to move the pagination into the sql query
            var invoiceSorted = GetInvoicesSorted(invoices, sortColumn, sortAsc).ToList();
            var paginatedInvoices = invoiceSorted;

            if ((page > 0) && (pageSize > 0))
            {
                paginatedInvoices = invoiceSorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            return new PaginatedResult<InvoiceListItem> { Items = paginatedInvoices, TotalItems = invoiceSorted.Count };
        }

        public async Task<byte[]> GetInvoiceFile(string clientPrefix, int clientId, string sapSystem, int fileId)
        {
            var dt = await GetInvoiceRecords(clientPrefix, clientId, sapSystem, fileId);

            var dr = dt.Select().FirstOrDefault();

            if (dr == null)
                return null;

            var link = $"{dr.Field<string>("trgtPath")}\\{dr.Field<string>("FileName")}.{dr.Field<string>("FileExt")}";

            return await File.ReadAllBytesAsync(link);
        }

        public async Task<string> TestSapConnection()
        {
            try
            {
                using (var conn = new HanaConnection(_options.Value.DbConnectionString))
                {

                    conn.Open();

                    var schema = _sapServiceSettingsService.GetSapSchema("AR");

                    var query = $"select * from {schema}.oeml";

                    var da = new HanaDataAdapter(query, conn);

                    var dt = new DataTable("Invoices");

                    da.Fill(dt);

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Hana connection");

                return ex.Message;
            }

            return "Successfully";
        }

        public async Task<string> TestSapUsConnection()
        {
            try
            {
                using (var conn = new HanaConnection(_options.Value.DbConnectionString))
                {

                    conn.Open();

                    var schema = _sapServiceSettingsService.GetSapSchema("US");

                    var query = $"select * from {schema}.oeml";

                    var da = new HanaDataAdapter(query, conn);

                    var dt = new DataTable("Invoices");

                    da.Fill(dt);

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Hana connection");

                return ex.Message;
            }

            return "Successfully";
        }

        private static IEnumerable<InvoiceListItem> GetInvoicesSorted(IQueryable<InvoiceListItem> invoices, string sortColumn, bool sortAsc)
        {
            return invoices.OrderBy(sortColumn + (!sortAsc ? " descending" : ""));
        }

        private async Task<DataTable> GetInvoiceRecords(string clientPrefix, int clientId, string sapSystem, int? fileId = null)
        {
            using (var conn = new HanaConnection(_options.Value.DbConnectionString))
            {
                conn.Open();

                var query = string.Empty;

                /* Invoices */
                query += CreateInvoiceQuery("FC", clientPrefix, clientId, sapSystem, fileId);

                query += " UNION ";

                /* Credit Notes */
                query += CreateInvoiceQuery("NC", clientPrefix, clientId, sapSystem, fileId);

                var da = new HanaDataAdapter(query, conn);
                var dt = new DataTable("Invoices");

                da.Fill(dt);

                conn.Close();

                return dt;
            }
        }

        private string CreateInvoiceQuery(string documentType, string clientPrefix, int clientId, string sapSystem, int? fileId = null)
        {
            var schema = _sapServiceSettingsService.GetSapSchema(sapSystem);
            var query = string.Empty;

            var queryData = documentType == "FC"
                ? new
                {
                    ObjType = 13,
                    AmountFactor = 1,
                    Table = "OINV"
                }
                : new
                {
                    ObjType = 14,
                    AmountFactor = -1,
                    Table = "ORIN"
                };

            query += "  SELECT";
            query += "     AT1.\"AbsEntry\" ,";
            query += "     OEM.\"CardCode\" ,";
            query += "     OEM.\"CardName\" ,";
            query += "     OEM.\"SendDate\" ,";
            query += "     OEM.\"SendTime\" ,";
            query += "     cast(OEM.\"DocEntry\" AS NVARCHAR) AS \"DocEntry\" ,";
            query += $"    '{documentType}' AS \"DocumentType\", ";
            query += "     IFNULL(INV.\"Letter\" || '-' || (Right(INV.\"PTICode\", 4) || '-' || Right('00000000' || COALESCE(INV.\"FolNumFrom\", '0'), 8)), CAST(INV.\"DocNum\" AS VARCHAR(10))) AS \"DocumentNumber\", ";
            query += $"    INV.\"DocTotal\" * {queryData.AmountFactor} AS \"DocTotal\" ,";
            query += $"    INV.\"PaidToDate\" * {queryData.AmountFactor} AS \"PaidToDate\" ,"; ;
            query += "      INV.\"CreateDate\" ,";
            query += "      INV.\"DocDueDate\" ,";
            query += "     cast(INV.\"DocCur\" AS NVARCHAR)   AS \"DocCur\" ,";
            query += "     cast(AT1.\"trgtPath\" AS NVARCHAR) AS \"trgtPath\" ,";
            query += "     cast(AT1.\"FileName\" AS NVARCHAR) AS \"FileName\" ,";
            query += "     cast(AT1.\"FileExt\" AS NVARCHAR)  AS \"FileExt\" ";
            query += $" FROM {schema}.{queryData.Table} INV";
            query += "  INNER JOIN ( SELECT";
            query += "         T0.\"DocEntry\" ,";
            query += "         max(T0.\"AtcEntry\") AS \"AtcEntry\" ,";
            query += "         max(T0.\"AbsEntry\") AS \"AbsEntry\" ";
            query += $"        FROM {schema}.oeml T0 ";
            query += $"        WHERE T0.\"ObjType\" = '{queryData.ObjType}' AND ";
            query += $"              (T0.\"CardCode\" = '{clientPrefix}{clientId:0000000000000}' OR T0.\"CardCode\" LIKE '{clientPrefix}{clientId:00000000000}.%') ";
            query += "         GROUP BY T0.\"DocEntry\" ) x ON x.\"DocEntry\" = INV.\"DocEntry\" ";
            query += $" INNER JOIN {schema}.ATC1 AT1 ON x.\"AtcEntry\" = AT1.\"AbsEntry\" AND AT1.\"Line\" = 1 ";
            query += $" INNER JOIN {schema}.oeml OEM ON OEM.\"AbsEntry\" = x.\"AbsEntry\" ";
            query += $" WHERE INV.\"ObjType\" = '{queryData.ObjType}' AND ";
            query += $"       (INV.\"CardCode\" = '{clientPrefix}{clientId:0000000000000}' OR INV.\"CardCode\" LIKE '{clientPrefix}{clientId:00000000000}.%')";

            if (fileId.HasValue)
                query += $" AND (AT1.\"AbsEntry\" = '{fileId}')";

            return query;

        }
    }
}
