using UnityEngine;

namespace UnigramPayment.Runtime.Data
{
    [CreateAssetMenu(fileName = "Saleable Item", menuName = "Unigram Payment/Saleable Item")]
    public sealed class SaleableItem : ScriptableObject
    {
        [field: SerializeField, Space] public string Id { get; private set; }
        [field: SerializeField, Space] public string Name { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField, Space] public int Price { get; private set; }
    }
}