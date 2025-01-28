using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class AuthorizationTokenData
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}