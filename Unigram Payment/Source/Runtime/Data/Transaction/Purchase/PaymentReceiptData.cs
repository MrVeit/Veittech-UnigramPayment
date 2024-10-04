using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class PaymentReceiptData
    {
        [JsonProperty("id")]
        public string TransactionId { get; set; }

        [JsonProperty("buyerId")]
        public string BuyerId { get; set; }

        [JsonProperty("itemId")]
        public string InvoicePayload { get; set; }

        [JsonProperty("price")]
        public string Amount { get; set; }
    }
}