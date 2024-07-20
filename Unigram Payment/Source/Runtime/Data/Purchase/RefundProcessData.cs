using System;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class RefundProcessData
    {
        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }
    }
}