using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class RefundHistoryData
    {

        [JsonProperty("transactions")]
        public List<RefundTransactionData> Transactions { get; set; }
    }
}