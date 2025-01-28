using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class RefundItemBuyerData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user")]
        public TelegramUserData Info { get; set; }
    }
}