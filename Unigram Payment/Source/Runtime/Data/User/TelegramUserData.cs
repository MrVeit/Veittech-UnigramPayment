using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class TelegramUserData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [JsonProperty("is_premium")]
        public bool? IsHavePremium { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("language_code")]
        public string LanguageCode { get; set; }

#nullable enable
        [JsonProperty("last_name")]
        public string? LastName { get; set; }

        [JsonProperty("username")]
        public string? Username { get; set; }
#nullable restore
    }
}