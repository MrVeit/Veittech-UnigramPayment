using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class ItemBuyerData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user")]
        public TelegramUserData Info { get; set; }

        [JsonProperty("invoice_payload")]
        public string ItemPayloadId { get; set; }
    }
}