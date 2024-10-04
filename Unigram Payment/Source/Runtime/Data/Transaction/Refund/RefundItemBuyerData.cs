using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class RefundItemBuyerData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user")]
        public TelegramUserData Info { get; set; }
    }
}