using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class RefundHistoryData
    {
        [JsonProperty("transactions")]
        public List<RefundTransactionData> Transactions { get; set; }
    }
}