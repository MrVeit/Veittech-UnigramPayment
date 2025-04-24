namespace UnigramPayment.Runtime.Common
{
    public enum ErrorTypes
    {
        SessionExpired,
        AttemptsExpired,
        InvalidInvoiceLink,
        PurchaseWindowClosed,
        PurchaseFailed,
        HistoryNotFound,
        TransactionNotFound
    }
}