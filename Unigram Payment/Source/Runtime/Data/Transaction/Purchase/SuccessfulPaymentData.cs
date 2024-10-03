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
        public string BuyerId { get; set; }

        [JsonProperty("itemId")]
        public string PayloadItem { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }
    }
}