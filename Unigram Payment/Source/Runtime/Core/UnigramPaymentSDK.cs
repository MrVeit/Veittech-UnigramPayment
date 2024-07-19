using System;
using UnityEngine;
using UnigramPayment.Core;
using UnigramPayment.Core.Common;
using UnigramPayment.Runtime.Data;
using UnigramPayment.Runtime.Common;
using UnigramPayment.Runtime.Utils;
using UnigramPayment.Runtime.Utils.Debugging;
using UnigramPayment.Storage.Data;

namespace UnigramPayment.Runtime.Core
{
    [AddComponentMenu("Unigram Payment/Unigram Payment SDK")]
    [SelectionBase]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/MrVeit/Veittech-UnigramPayment/blob/master/README.md")]
    public sealed class UnigramPaymentSDK : MonoBehaviour, IUnigramPaymentCallbacks, IUnigramPaymentTransactionCallbacks
    {
        private static readonly object _lock = new();

        private static UnigramPaymentSDK _instance;

        public static UnigramPaymentSDK Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<UnigramPaymentSDK>();
                    }
                }

                return _instance;
            }
        }

        [Header("SDK Settings"), Space]
        [Tooltip("A link to your api server, with which the Unity application will communicate to make payments (for a production build to the server, you must have a domain and a certificate to connect over HTTPS)")]
        [SerializeField] private string _apiServerUrl;
        [Tooltip("A client  secret key that knows the Unity application and API server to verify the signature and allow access to the API.")]
        [SerializeField] private string _clientSecretKey;
        [Tooltip("Enable if you want to activate SDK logging for detailed analysis")]
        [SerializeField, Space] private bool _debugMode;
        [Tooltip("Turn it off if you want to do your own cdk initialization in your scripts")]
        [SerializeField] private bool _initializeOnAwake;
        [Tooltip("Store all items for purchase, to add new ones call 'Create -> Unigram Payment -> Saleable Item' and add it to the list.")]
        [SerializeField, Space] private SaleableItemsStorage _itemsStorage;

        private PaymentReceiptData _lastPaymentReceipt;

        public string JwtToken { get; private set; }
        public string LastInvoiceLink { get; private set; }
        public string LastRefundedTransaction { get; private set; }

        public string ClientSecretKey => _clientSecretKey;
        public string ApiServerUrl => _apiServerUrl;

        public bool IsDebugMode => _debugMode;

        public event IUnigramPaymentCallbacks.OnUnigramConnectInitialize OnInitialized;
        public event IUnigramPaymentCallbacks.OnSessionTokenRefresh OnSessionTokenRefreshed;

        public event IUnigramPaymentTransactionCallbacks.OnInvoiceLinkCreate OnInvoiceLinkCreated;
        public event IUnigramPaymentTransactionCallbacks.OnInvoiceLinkCreateFail OnInvoiceLinkCreateFailed;

        public event IUnigramPaymentTransactionCallbacks.OnItemPurchase OnItemPurchased;
        public event IUnigramPaymentTransactionCallbacks.OnItemPurchaseFail OnItemPurchaseFailed;

        public event IUnigramPaymentTransactionCallbacks.OnRefundTransactionFinish OnRefundTransactionFinished;

        private void Awake()
        {
            CreateInstance();

            if (!_initializeOnAwake)
            {
                return;
            }

            Initialize();
        }

        public void Initialize()
        {
            StartCoroutine(BotAPIBridge.AuthorizeClient(RuntimeAPIConfig.Load(), (authToken) =>
            {
                JwtToken = authToken;

                if (JwtToken == null)
                {
                    OnInitialize(false);

                    return;
                }

                OnInitialize(true);

                UnigramPaymentLogger.Log("Unigram Payment SDK has been successfully initialized, connection to the server has been made.");
            }));
        }

        public void CreateInvoice(SaleableItem item)
        {
            var saleableItem = UnigramUtils.FindItemInItemsStorage(_itemsStorage, item);

            if (saleableItem == null)
            {
                UnigramPaymentLogger.LogWarning("The invoice link creation process is canceled, the item for purchase is not found in the vault.");

                return;
            }

            StartCoroutine(BotAPIBridge.CreateInvoice(item, (invoiceLink) =>
            {
                LastInvoiceLink = invoiceLink;

                if (LastInvoiceLink == null)
                {
                    OnInvoiceLinkCreateFail();

                    return;
                }

                OnInvoiceLinkCreate(LastInvoiceLink);
            }));
        }

        public void OpenInvoice(string invoiceUrl)
        {
            OpenPurchaseInvoice((status, message) =>
            {
                UnigramPaymentLogger.Log($"Transaction finished with status: {status}, data: {message}");

                if (status == PaymentStatus.paid)
                {
                    GetPaymentReceipt((receipt) =>
                    {
                        if (receipt == null)
                        {
                            OnItemPurchaseFail();

                            return;
                        }

                        OnItemPurchase(receipt);
                    });
                }
                else if (status is PaymentStatus.cancelled or PaymentStatus.failed)
                {
                    OnItemPurchaseFail();
                }
            });
        }

        public void Refund(PaymentReceiptData receipt)
        {
            var parsedTelegramId = WebRequestUtils.ParseTelegramId(receipt.BuyerId);
            var telegramId = int.Parse(parsedTelegramId);

            LastRefundedTransaction = receipt.TransactionId;

            StartCoroutine(BotAPIBridge.RefundPayment(telegramId, receipt.TransactionId, (isSuccess) =>
            {
                OnRefundTransactionFinish(isSuccess);

                UnigramPaymentLogger.Log($"Refund process finished with result: {isSuccess}");
            }));
        }

        public void RefreshToken()
        {
            UnigramPaymentLogger.Log("A request to update the server access token has been activated.");

            StartCoroutine(BotAPIBridge.AuthorizeClient(RuntimeAPIConfig.Load(), (jwtToken) =>
            {
                JwtToken = jwtToken;

                OnSessionTokenRefresh();

                UnigramPaymentLogger.Log("Token has been successfully updated and is ready for use");
            }));
        }

        private void OpenPurchaseInvoice(Action<PaymentStatus, string> invoiceClosed)
        {
            if (!UnigramUtils.IsSupportedNativeOpen())
            {
                Application.OpenURL(LastInvoiceLink);

                UnigramPaymentLogger.LogWarning("Native opening of payment request in Telegram Stars is not supported in Editor." +
                    " Make a WebGL build to get the result of native transaction status events.");

                return;
            }

            WebAppAPIBridge.OpenPurchaseInvoice(LastInvoiceLink,
            (status, resultPayment) =>
            {
                invoiceClosed?.Invoke(UnigramUtils.ParsePaymentStatusAfterPurchase(status), resultPayment);

                UnigramPaymentLogger.Log($"Success purchase with result: {status}, data: {resultPayment}");
            },
            (paymentStatus) =>
            {
                invoiceClosed?.Invoke(UnigramUtils.ParsePaymentStatusAfterPurchase(paymentStatus), null);

                UnigramPaymentLogger.LogError($"Faied purchase with status: {paymentStatus}");
            });
        }

        private void GetPaymentReceipt(Action<PaymentReceiptData> paymentReceiptDataClaimed)
        {
            StartCoroutine(BotAPIBridge.GetPaymentReceipt((receipt) =>
            {
                _lastPaymentReceipt = receipt;

                paymentReceiptDataClaimed?.Invoke(_lastPaymentReceipt);

                UnigramPaymentLogger.Log($"Received data about the transaction {receipt.TransactionId} made with the identifier {receipt.InvoicePayload}");
            }));
        }

        private void CreateInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = this;

                    DontDestroyOnLoad(gameObject);

                    return;
                }

                Destroy(gameObject);

                UnigramPaymentLogger.LogError($"Another instance is detected on the scene, running delete...");
            }
        }

        private void OnInitialize(bool isSuccess) => OnInitialized?.Invoke(isSuccess);
        private void OnSessionTokenRefresh() => OnSessionTokenRefreshed?.Invoke();

        private void OnInvoiceLinkCreate(string invoiceUrl) => OnInvoiceLinkCreated?.Invoke(invoiceUrl);
        private void OnInvoiceLinkCreateFail() => OnInvoiceLinkCreateFailed?.Invoke();

        private void OnItemPurchase(PaymentReceiptData receipt) => OnItemPurchased?.Invoke(receipt);
        private void OnItemPurchaseFail() => OnItemPurchaseFailed?.Invoke();

        private void OnRefundTransactionFinish(bool isSuccess) => OnRefundTransactionFinished?.Invoke(isSuccess);
    }
}