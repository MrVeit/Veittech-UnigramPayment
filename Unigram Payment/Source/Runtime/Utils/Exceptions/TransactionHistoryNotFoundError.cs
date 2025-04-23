using System;

namespace UnigramPayment.Runtime.Utils.Debugging
{
    public sealed class TransactionHistoryNotFoundError : Exception
    {
        public sealed override string Message =>
            "Transaction history not found or unavailablee";

        public TransactionHistoryNotFoundError()
        {
            UnigramPaymentLogger.LogError(Message);
        }
    }
}