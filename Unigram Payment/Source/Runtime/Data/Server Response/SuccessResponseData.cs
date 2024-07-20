using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class SuccessResponseData
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}