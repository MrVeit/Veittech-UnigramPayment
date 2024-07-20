namespace UnigramPayment.Core.Common
{
    public interface IUnigramPaymentCallbacks
    {
        delegate void OnUnigramConnectInitialize(bool isSuccess);

        delegate void OnSessionTokenRefresh();
        delegate void OnSessionTokenRefreshFail();

        event OnUnigramConnectInitialize OnInitialized;

        event OnSessionTokenRefresh OnSessionTokenRefreshed;
        event OnSessionTokenRefreshFail OnSessionTokenRefreshFailed;
    }
}