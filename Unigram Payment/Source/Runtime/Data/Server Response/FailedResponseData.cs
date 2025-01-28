using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class FailedResponseData
    {
        [JsonProperty("error")]
        public string ErrorMessage { get; set; }
    }
}