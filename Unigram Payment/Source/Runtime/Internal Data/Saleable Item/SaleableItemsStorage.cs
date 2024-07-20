using System.Collections.Generic;
using UnityEngine;

namespace UnigramPayment.Runtime.Data
{
    [CreateAssetMenu(fileName = "Items Storage", menuName = "Unigram Payment/Items Storage")]
    public sealed class SaleableItemsStorage : ScriptableObject
    {
        [field: SerializeField, Space] public List<SaleableItem> Items { get; private set; }
    }
}