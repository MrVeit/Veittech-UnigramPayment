using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class StartTelegramUserData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("language_code")]
        public string LanguageCode { get; set; }

        [JsonProperty("start_param")]
        public string CustomParam { get; set; }

        [JsonProperty("auth_date")]
        public string UnixAuthData { get; set; }

        [JsonProperty("hash")]
        public string AuthorizationHash { get; set; }
    }
}