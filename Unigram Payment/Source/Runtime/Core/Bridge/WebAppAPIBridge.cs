using System;
using System.Runtime.InteropServices;
using AOT;
using UnigramPayment.Runtime.Utils.Debugging;

namespace UnigramPayment.Core
{
    public static class WebAppAPIBridge
    {
        private static Action<string, string> _onInvoiceSuccessfullyPaid;
        private static Action<string> _onInvoicePayFailed;

        [DllImport("__Internal")]
        private static extern bool IsTelegramApp();

        [DllImport("__Internal")]
        private static extern bool IsSupportedVersionActive(string version);

        [DllImport("__Internal")]
        private static extern bool IsExpanded();

        [DllImport("__Internal")]
        private static extern string GetCurrentVersion();

        [DllImport("__Internal")]
        private static extern void Expand();

        [DllImport("__Internal")]
        private static extern void Close();

        [DllImport("__Internal")]
        private static extern void OpenTelegramLink(string url);

        [DllImport("__Internal")]
        private static extern void OpenExternalLink(string url, bool tryInstantView);

        [DllImport("__Internal")]
        private static extern void OpenInvoice(string invoiceUrl, 
            Action<string, string> invoiceSuccessfullyPaid, Action<string> invoicePayFailed);

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        private static void OnInvoiceSuccessfullyPaid(string status, string paymentReceipt)
        {
            UnigramPaymentLogger.Log($"{nameof(WebAppAPIBridge)}.{nameof(OnInvoiceSuccessfullyPaid)} invoked");

            _onInvoiceSuccessfullyPaid?.Invoke(status, paymentReceipt);
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnInvoicePayFailed(string status)
        {
            UnigramPaymentLogger.Log($"{nameof(WebAppAPIBridge)}.{nameof(OnInvoicePayFailed)} invoked");

            _onInvoicePayFailed?.Invoke(status);
        }

        public static bool IsTelegramWebApp()
        {
            return IsTelegramApp();
        }

        public static bool IsVersionSupported(string version)
        {
            return IsSupportedVersionActive(version);
        }

        public static bool IsAppExpanded()
        {
            return IsExpanded();
        }

        public static string GetVersion()
        {
            return GetCurrentVersion();
        }

        public static void ExpandApp()
        {
            Expand();
        }

        public static void CloseApp()
        {
            Close();
        }

        public static void OpenLinkExternal(string url, bool tryInstantView = false)
        {
            OpenExternalLink(url, tryInstantView);
        }

        public static void OpenTelegramDeepLink(string url)
        {
            OpenTelegramLink(url);
        }

        public static void OpenPurchaseInvoice(string url, 
            Action<string, string> onInvoiceSuccessfullyPaid, Action<string> onInvoicePayFailed)
        {
            _onInvoiceSuccessfullyPaid = onInvoiceSuccessfullyPaid;
            _onInvoicePayFailed = onInvoicePayFailed;

            OpenInvoice(url, OnInvoiceSuccessfullyPaid, OnInvoicePayFailed);
        }
    }
}