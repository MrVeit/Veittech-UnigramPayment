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

        delegate void OnPurchaseHistoryLoad(PurchaseHistoryData history);
        delegate void OnRefundHistoryLoad(RefundHistoryData history);

        event OnInvoiceLinkCreate OnInvoiceLinkCreated;
        event OnInvoiceLinkCreateFail OnInvoiceLinkCreateFailed;

        event OnItemPurchase OnItemPurchased;
        event OnItemPurchaseFail OnItemPurchaseFailed;

        event OnRefundTransactionFinish OnRefundTransactionFinished;

        event OnPurchaseHistoryLoad OnPurchaseHistoryLoaded;
        event OnRefundHistoryLoad OnRefundHistoryLoaded;
    }
}