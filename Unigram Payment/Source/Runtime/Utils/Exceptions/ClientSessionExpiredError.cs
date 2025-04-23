using System;

namespace UnigramPayment.Runtime.Utils.Debugging
{
    public sealed class ClientSessionExpiredError : Exception
    {
        public sealed override string Message => "Client session has expired, " +
            "update the master key based token and try again.";

        public ClientSessionExpiredError()
        {
            UnigramPaymentLogger.LogError(Message);
        }
    }
}