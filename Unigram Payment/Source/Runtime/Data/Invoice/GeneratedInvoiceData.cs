using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class GeneratedInvoiceData
    {
        [JsonProperty("invoiceLink")]
        public string Url { get; set; }
    }
}