using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class PaymentReceiptData
    {
        [JsonProperty("currency")]
        public string CurrencyType { get; set; }

        [JsonProperty("total_amount")]
        public int Amount { get; set; }

        [JsonProperty("invoice_payload")]
        public string InvoicePayload { get; set; }

        [JsonProperty("telegram_payment_charge_id")]
        public string TransactionId { get; set; }

        [JsonProperty("provider_payment_charge_id")]
        public string BuyerId { get; set; }
    }
}