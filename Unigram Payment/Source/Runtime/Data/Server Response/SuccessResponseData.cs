using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class SuccessResponseData
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}