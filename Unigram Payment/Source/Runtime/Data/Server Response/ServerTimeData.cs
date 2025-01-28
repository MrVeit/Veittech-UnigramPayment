using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class ServerTimeData
    {
        [JsonProperty("tick")]
        public long UnixTick { get; set; }
    }
}