using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Billing.API.Models
{
    public class InvoiceListItem
    {
        public string DocumentType { get; }
        public string DocumentNumber { get; }
        public string Product { get; }
        public string AccountId { get; }
        [JsonConverter(typeof(IsoDateConverter))]
        public DateTimeOffset CreationDate { get; }
        [JsonConverter(typeof(IsoDateConverter))]
        public DateTimeOffset DueDate { get; }
        [Obsolete("Use CreationDate or DueDate in place of this.")]
        [JsonConverter(typeof(IsoDateConverter))]
        public DateTimeOffset Date { get; }
        public string Currency { get; }
        public double Amount { get; }
        public double PaidToDate { get; }
        public double Balance => Amount - PaidToDate;

        [JsonIgnore]
        public int FileId { get; }

        public string Filename { get; }

        [JsonProperty(PropertyName = "_links")]
        public List<Link> Links { get; } = new List<Link>();

        public InvoiceListItem(string documentType, string documentNumber, string product, string accountId, DateTimeOffset creationDate, DateTimeOffset dueDate, DateTimeOffset date, string currency, double amount, double paidToDate, string filename, int fileId)
        {
            DocumentType = documentType;
            DocumentNumber = documentNumber;
            Product = product.EqualsIgnoreCase("CD") ? "Doppler"
                : product.EqualsIgnoreCase("CR") ? "Relay"
                : product.EqualsIgnoreCase("CM") ? "Client Manager"
                : product;
            AccountId = accountId;
            CreationDate = creationDate;
            DueDate = dueDate;
            Date = date;
            Currency = currency;
            Amount = amount;
            PaidToDate = paidToDate;
            Filename = filename;
            FileId = fileId;
        }
    }
}
