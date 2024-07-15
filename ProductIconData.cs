using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class ProductIconData
    {
        [JsonProperty("photo_url")]
        public string URL { get; set; }

        [JsonProperty("photo_width")]
        public int Width { get; set; }

        [JsonProperty("photo_height")]
        public int Height { get; set; }
    }
}