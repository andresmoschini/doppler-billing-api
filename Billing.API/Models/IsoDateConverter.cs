using Newtonsoft.Json.Converters;

namespace Billing.API.Models
{
    public class IsoDateConverter  : IsoDateTimeConverter
    {
        public IsoDateConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
