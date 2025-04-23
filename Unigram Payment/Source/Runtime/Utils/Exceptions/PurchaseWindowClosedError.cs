using System;

namespace UnigramPayment.Runtime.Utils.Debugging
{
    public sealed class PurchaseWindowClosedError : Exception
    {
        public sealed override string Message => "Purchase window was closed " +
            "without payment, open it again and pay for the item";

        public PurchaseWindowClosedError()
        {
            UnigramPaymentLogger.LogWarning(Message);
        }
    }
}