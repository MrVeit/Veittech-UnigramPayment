using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnigramPayment.Runtime.Data
{
    public sealed class PurchaseHistoryData 
    {
        [JsonProperty("transactions")]
        public List<SuccessfulPurchaseData> Transactions { get; set; }
    }
}