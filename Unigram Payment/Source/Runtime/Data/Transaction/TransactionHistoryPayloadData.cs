using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class TransactionHistoryPayloadData
    {
        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("totalPass")]
        public long TotalPass { get; set; }
    }
}