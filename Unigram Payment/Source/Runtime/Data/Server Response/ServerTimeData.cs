using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class ServerTimeData
    {
        [JsonProperty("tick")]
        public long UnixTick { get; set; }
    }
}