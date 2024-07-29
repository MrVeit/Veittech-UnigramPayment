using System;
using UnityEngine;
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

        /// <summary>
        /// Identifies if the Web App is open inside Telegram
        /// </summary>
        /// <returns></returns>
        public static bool IsTelegramWebApp()
        {
            return IsTelegramApp();
        }

        /// <summary>
        /// Returns true if the user's app supports a version of the Bot API that
        /// is equal to or higher than the version passed as the parameter.
        /// </summary>
        /// <param name="version">Bot API version to check</param>
        /// <returns></returns>
        public static bool IsVersionSupported(string version)
        {
            return IsSupportedVersionActive(version);
        }

        /// <summary>
        /// True, if the Mini App is expanded to the maximum available height. 
        /// False, if the Mini App occupies part of the screen and can be
        /// expanded to the full height using the ExpandApp() method.
        /// </summary>
        /// <returns></returns>
        public static bool IsAppExpanded()
        {
            return IsExpanded();
        }

        /// <summary>
        /// The version of the Bot API available in the user's Telegram app.
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            return GetCurrentVersion();
        }

        /// <summary>
        /// A method that expands the Mini App to the maximum available height. 
        /// To find out if the Mini App is expanded to the maximum height,
        /// refer to the value of the IsAppExpanded parameter
        /// </summary>
        public static void ExpandApp()
        {
            Expand();
        }

        /// <summary>
        /// A method that closes the Mini App.
        /// </summary>
        public static void CloseApp()
        {
            Close();
        }

        /// <summary>
        /// A method that opens a link in an external browser. The Mini App will not be closed.
        /// </summary>
        /// <param name="url">Link to open in an external browser</param>
        /// <param name="tryInstantView">Opening the specified link in Instant View mode, if possible</param>
        public static void OpenLinkExternal(string url, bool tryInstantView = false)
        {
            OpenExternalLink(url, tryInstantView);
        }

        /// <summary>
        /// Opening a variety of links within Telegram
        /// </summary>
        /// <param name="url">Link to open in telegram</param>
        public static void OpenTelegramDeepLink(string url)
        {
            OpenTelegramLink(url);
        }

        internal static void OpenPurchaseInvoice(string url, 
            Action<string, string> onInvoiceSuccessfullyPaid, Action<string> onInvoicePayFailed)
        {
            _onInvoiceSuccessfullyPaid = (status, paymentReceipt) =>
            {
                onInvoiceSuccessfullyPaid?.Invoke(status, paymentReceipt);

                _onInvoiceSuccessfullyPaid = null; 
                _onInvoicePayFailed = null;  
            };

            _onInvoicePayFailed = (status) =>
            {
                onInvoicePayFailed?.Invoke(status);

                _onInvoiceSuccessfullyPaid = null; 
                _onInvoicePayFailed = null;   
            };

            OpenInvoice(url, OnInvoiceSuccessfullyPaid, OnInvoicePayFailed);
        }
    }
}