using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class AuthorizationTokenData
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}