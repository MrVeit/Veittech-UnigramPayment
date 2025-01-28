using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class RefundProcessData
    {
        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }
    }
}