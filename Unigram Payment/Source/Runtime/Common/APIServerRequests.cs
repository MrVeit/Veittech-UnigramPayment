namespace UnigramPayment.Runtime.Common
{
    public sealed class APIServerRequests
    {
        public static string GetAuthorizationLink(string apiServerLink)
        {
            return $"{apiServerLink}/api/authenticate";
        }

        public static string GetInvoiceLink(string apiServerLink)
        {
            return $"{apiServerLink}/api/payment/create-invoice";
        }

        public static string GetRefundStarsLink(string apiServerLink)
        {
            return $"{apiServerLink}/api/payment/refund";
        }

        public static string GetPaymentReceiptLink(string apiServerLink)
        {
            return $"{apiServerLink}/api/payment/latest-order-receipt";
        }
    }
}