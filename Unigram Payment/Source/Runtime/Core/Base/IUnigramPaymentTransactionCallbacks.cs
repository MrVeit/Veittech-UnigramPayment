using UnigramPayment.Runtime.Data;

namespace UnigramPayment.Core.Common
{
    public interface IUnigramPaymentTransactionCallbacks
    {
        delegate void OnInvoiceLinkCreate(string itemPayloadId, string url);
        delegate void OnInvoiceLinkCreateFail(string itemPayloadId);
        delegate void OnItemPurchase(PaymentReceiptData receipt);
        delegate void OnItemPurchaseFail(SaleableItem failedPurchaseItem);
        delegate void OnRefundTransactionFinish(string transactionId, bool isSuccess);

        event OnInvoiceLinkCreate OnInvoiceLinkCreated;
        event OnInvoiceLinkCreateFail OnInvoiceLinkCreateFailed;

        event OnItemPurchase OnItemPurchased;
        event OnItemPurchaseFail OnItemPurchaseFailed;

        event OnRefundTransactionFinish OnRefundTransactionFinished;
    }
}