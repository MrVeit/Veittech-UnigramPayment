using UnigramPayment.Runtime.Common;
using UnigramPayment.Runtime.Data;

namespace UnigramPayment.Core.Common
{
    public interface IUnigramPaymentTransactionCallbacks
    {
        delegate void OnInvoiceLinkCreate(string itemPayloadId, string url);
        delegate void OnInvoiceLinkCreateFail(string itemPayloadId);
        delegate void OnInvoiceLinkCreateFailDetailed(
            string itemPayloadId, ErrorTypes reason);

        delegate void OnItemPurchase(PaymentReceiptData receipt);
        delegate void OnItemPurchaseFail(SaleableItem failedPurchaseItem);
        delegate void OnItemPurchaseFailDetailed(
            SaleableItem failedPurchaseItem, ErrorTypes reason);

        delegate void OnRefundTransactionFinish(string transactionId, bool isSuccess);
        delegate void OnRefundTransactionFail(string transactionId, ErrorTypes reason);

        delegate void OnPurchaseHistoryLoad(PurchaseHistoryData history);
        delegate void OnPurchaseHistoryFail(ErrorTypes reason);

        delegate void OnRefundHistoryLoad(RefundHistoryData history);
        delegate void OnRefundHistoryFail(ErrorTypes reason);

        event OnInvoiceLinkCreate OnInvoiceLinkCreated;
        event OnInvoiceLinkCreateFail OnInvoiceLinkCreateFailed;
        event OnInvoiceLinkCreateFailDetailed OnFullInvoiceLinkCreateFailed;

        event OnItemPurchase OnItemPurchased;
        event OnItemPurchaseFail OnItemPurchaseFailed;
        event OnItemPurchaseFailDetailed OnFullItemPurchaseFailed;

        event OnRefundTransactionFinish OnRefundTransactionFinished;
        event OnRefundTransactionFail OnFullRefundTransactionFailed;

        event OnPurchaseHistoryLoad OnPurchaseHistoryLoaded;
        event OnPurchaseHistoryFail OnPurchaseHistoryLoadFailed;

        event OnRefundHistoryLoad OnRefundHistoryLoaded;
        event OnRefundHistoryFail OnRefundHistoryLoadFailed;
    }
}