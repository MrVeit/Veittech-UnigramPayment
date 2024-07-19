using System;
using System.Linq;
using UnigramPayment.Runtime.Data;
using UnigramPayment.Runtime.Common;
using UnigramPayment.Runtime.Utils.Debugging;

namespace UnigramPayment.Runtime.Utils
{
    public sealed class UnigramUtils
    {
        public static PaymentStatus ParsePaymentStatusAfterPurchase(string paymentStatus)
        {
            var status = (PaymentStatus)Enum.Parse(typeof(PaymentStatus), paymentStatus);

            return status;
        }

        public static bool IsSupportedNativeOpen()
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            return false;
#endif

            return true;
        }

        public static SaleableItem FindItemInItemsStorage(SaleableItemsStorage storage, SaleableItem targetItem)
        {
            var foundedItem = storage.Items.FirstOrDefault(item => item.Id == targetItem.Id);

            if (foundedItem == null)
            {
                UnigramPaymentLogger.LogError("The item for purchase is not found in the vault, create it and try again later.");

                return null;
            }

            return foundedItem;
        }
    }
}