using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class PriceData
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}