using UnigramPayment.Runtime.Data;

namespace UnigramPayment.Core.Common
{
    public interface IUnigramPaymentTransactionCallbacks
    {
        delegate void OnInvoiceLinkCreate(string url);
        delegate void OnInvoiceLinkCreateFail();
        delegate void OnItemPurchase(PaymentReceiptData receipt);
        delegate void OnItemPurchaseFail();
        delegate void OnRefundTransactionFinish(bool isSuccess);

        event OnInvoiceLinkCreate OnInvoiceLinkCreated;
        event OnInvoiceLinkCreateFail OnInvoiceLinkCreateFailed;
        event OnItemPurchase OnItemPurchased;
        event OnItemPurchaseFail OnItemPurchaseFailed;
        event OnRefundTransactionFinish OnRefundTransactionFinished;
    }
}