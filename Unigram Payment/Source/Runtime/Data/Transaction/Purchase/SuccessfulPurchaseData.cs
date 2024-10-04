using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class SuccessfulPurchaseData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("date")]
        public long UnixPurchaseDate { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("source")]
        public ItemBuyerData Buyer { get; set; }
    }
}