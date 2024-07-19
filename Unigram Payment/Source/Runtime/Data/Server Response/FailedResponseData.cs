using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class FailedResponseData
    {
        [JsonProperty("error")]
        public string ErrorMessage { get; set; }
    }
}