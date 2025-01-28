using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class GeneratedInvoiceData
    {
        [JsonProperty("invoiceLink")]
        public string Url { get; set; }
    }
}