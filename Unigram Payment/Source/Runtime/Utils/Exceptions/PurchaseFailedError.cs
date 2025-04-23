using System;

namespace UnigramPayment.Runtime.Utils.Debugging
{
    public sealed class PurchaseFailedError : Exception
    {
        public sealed override string Message => "User refused to pay for the product";

        public PurchaseFailedError()
        {
            UnigramPaymentLogger.LogError(Message);
        }
    }
}