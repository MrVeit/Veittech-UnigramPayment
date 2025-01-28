using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class RefundTransactionData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("date")]
        public long UnixPurchaseDate { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("receiver")]
        public RefundItemBuyerData Buyer { get; set; }
    }
}