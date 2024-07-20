using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class InvoiceData
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("providerToken")]
        public string ProviderToken => "";

        [JsonProperty("currency")]
        public string Currency => "XTR";

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}