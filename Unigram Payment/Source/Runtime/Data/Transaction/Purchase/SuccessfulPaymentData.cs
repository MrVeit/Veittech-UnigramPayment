using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class SuccessfulPaymentData
    {
        [JsonProperty("id")]
        public string TransactionId { get; set; }

        [JsonProperty("buyerId")]
        public long BuyerId { get; set; }

        [JsonProperty("itemId")]
        public string PayloadItem { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }
    }
}