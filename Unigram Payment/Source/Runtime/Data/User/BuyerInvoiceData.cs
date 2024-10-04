using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class BuyerInvoiceData
    {
        [JsonProperty("userId")]
        public string TelegramId { get; set; }

        [JsonProperty("itemId")]
        public string PurchasedItemId { get; set; }
    }
}