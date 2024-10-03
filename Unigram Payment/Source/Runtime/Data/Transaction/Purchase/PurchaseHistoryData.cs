using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    [Serializable]
    public sealed class PurchaseHistoryData 
    {
        [JsonProperty("transactions")]
        public List<SuccessfulPurchaseData> Transactions { get; set; }
    }
}