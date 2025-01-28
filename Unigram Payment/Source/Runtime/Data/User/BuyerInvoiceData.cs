using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class BuyerInvoiceData
    {
        [JsonProperty("userId")]
        public string TelegramId { get; set; }

        [JsonProperty("itemId")]
        public string PurchasedItemId { get; set; }
    }
}