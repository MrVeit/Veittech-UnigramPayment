namespace UnigramPayment.Core.Common
{
    public interface IUnigramPaymentCallbacks
    {
        delegate void OnUnigramConnectInitialize(bool isSuccess);
        delegate void OnSessionTokenRefresh();

        event OnUnigramConnectInitialize OnInitialized;
        event OnSessionTokenRefresh OnSessionTokenRefreshed;
    }
}