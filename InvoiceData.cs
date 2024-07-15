using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class InvoiceData
    {
        public const string CURRENCY_TYPE = "XTR";

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("provider_token")]
        public string ProviderToken { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("prices")]
        public List<PriceData> Prices { get; set; }

        [JsonProperty("photo_url")]
        public string URL { get; set; }

        [JsonProperty("photo_width")]
        public int Width { get; set; }

        [JsonProperty("photo_height")]
        public int Height { get; set; }
    }
}