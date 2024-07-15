using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class InvoiceLinkData
    {
        [JsonProperty("ok")]
        public bool Status { get; set; }

        [JsonProperty("result")]
        public string Link { get; set; }
    }
}