using System;

namespace UnigramPayment.Runtime.Utils.Debugging
{
    public sealed class ResendAttemptsExpiredError : Exception
    {
        public sealed override string Message => 
            "Retry attempts expired, try again later";

        public ResendAttemptsExpiredError()
        {
            UnigramPaymentLogger.LogError(Message);
        }
    }
}