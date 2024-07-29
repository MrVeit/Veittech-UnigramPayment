using System;
using UnityEngine;
using UnigramPayment.Core;
using UnigramPayment.Core.Common;
using UnigramPayment.Runtime.Data;
using UnigramPayment.Runtime.Common;
using UnigramPayment.Runtime.Utils;
using UnigramPayment.Runtime.Utils.Debugging;

namespace UnigramPayment.Runtime.Core
{
    [AddComponentMenu("Unigram Payment/Unigram Payment SDK")]
    [SelectionBase]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/MrVeit/Veittech-UnigramPayment/blob/master/README.md")]
    public sealed class UnigramPaymentSDK : MonoBehaviour, 
        IUnigramPaymentCallbacks, IUnigramPaymentTransactionCallbacks
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
        [Tooltip("Enable if you want to activate SDK logging for detailed analysis")]
        [SerializeField, Space] private bool _debugMode;
        [Tooltip("Turn it off if you want to do your own cdk initialization in your scripts")]
        [SerializeField] private bool _initializeOnAwake;
        [Tooltip("Store all items for purchase, to add new ones call 'Create -> Unigram Payment -> Saleable Item' and add it to the list.")]
        [SerializeField, Space] private SaleableItemsStorage _itemsStorage;

        private SaleableItem _currentPurchaseItem;

        private PaymentReceiptData _lastPaymentReceipt;

        /// <summary>
        /// Access token to the API server to work with the payment module.
        /// </summary>
        public string JwtToken { get; private set; }

        /// <summary>
        /// Last previously generated invoice reference
        /// </summary>
        public string LastInvoiceLink { get; private set; }

        /// <summary>
        /// Link to the last previously successfully revoked telegram stars payment transaction
        /// </summary>
        public string LastRefundedTransaction { get; private set; }

        public bool IsDebugMode => _debugMode;

        /// <summary>
        /// Callback that is called when initialization of cdk is completed
        /// </summary>
        public event IUnigramPaymentCallbacks.OnUnigramConnectInitialize OnInitialized;

        /// <summary>
        /// A callback that is called if the valid authorization
        /// token has expired and has been updated to a new one
        /// </summary>

        public event IUnigramPaymentCallbacks.OnSessionTokenRefresh OnSessionTokenRefreshed;

        /// <summary>
        /// Callback, which is called if the valid authorization
        /// token has expired and could not be renewed due to one of the following reasons
        /// </summary>
        public event IUnigramPaymentCallbacks.OnSessionTokenRefreshFail OnSessionTokenRefreshFailed;

        /// <summary>
        /// A callback that is invoked when a payment invoice is successfully created with a link provided
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnInvoiceLinkCreate OnInvoiceLinkCreated;

        /// <summary>
        /// A callback that is invoked if the creation of a payment link fails
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnInvoiceLinkCreateFail OnInvoiceLinkCreateFailed;

        /// <summary>
        /// A callback that is invoked when an item is 
        /// successfully purchased with the submission of a receipt 
        /// that contains: buyer id, transaction id, and amount spent telegram stars.
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnItemPurchase OnItemPurchased;

        /// <summary>
        /// A callback that is invoked when an item purchase 
        /// fails or the purchase window closes on a previously generated invoice link
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnItemPurchaseFail OnItemPurchaseFailed;

        /// <summary>
        /// A callback that is called when the process of 
        /// returning previously spent stars on an item in the project is completed
        /// </summary>
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

        /// <summary>
        /// Initialize the Unigram Payment sdk if you want to do it manually. 
        /// Subscribe to the `OnInitialized` event to get the initialization status
        /// </summary>
        public void Initialize()
        {
            AuthorizeClient((tokenClaimed) =>
            {
                OnInitialize(true);
            },
            () =>
            {
                OnInitialize(false);
            });
        }

        /// <summary>
        /// Create an invoice by transferring an item from the item storage for purchase.
        /// Before creating the invoice, you must create and add the item to the item store, otherwise the creation process will be canceled with an error.
        /// </summary>
        /// <param name="item">Item configuration for purchase</param>
        public void CreateInvoice(SaleableItem item)
        {
            var saleableItem = UnigramUtils.FindItemInItemsStorage(_itemsStorage, item);

            if (saleableItem == null)
            {
                UnigramPaymentLogger.LogError("The invoice link creation process is canceled, the item for purchase is not found in the vault.");

                return;
            }

            _currentPurchaseItem = item;

            StartCoroutine(BotAPIBridge.CreateInvoice(item, (invoiceLink) =>
            {
                LastInvoiceLink = invoiceLink;

                if (LastInvoiceLink == null)
                {
                    OnInvoiceLinkCreateFail(item.Id);

                    return;
                }

                OnInvoiceLinkCreate(item.Id, LastInvoiceLink);
            }));
        }

        /// <summary>
        /// This call opens the invoice that was previously created when `CreateInvoice(SaleableItem item)` was called.
        /// </summary>
        /// <param name="invoiceUrl">Generated payment link</param>
        public void OpenInvoice(string invoiceUrl)
        {
            OpenPurchaseInvoice(invoiceUrl, (status, message) =>
            {
                UnigramPaymentLogger.Log($"Transaction finished with status: {status}, data: {message}");

                if (status == PaymentStatus.paid)
                {
                    GetPaymentReceipt((receipt) =>
                    {
                        if (receipt == null)
                        {
                            OnItemPurchaseFail(_currentPurchaseItem);

                            return;
                        }

                        OnItemPurchase(receipt);
                    });
                }
                else if (status is PaymentStatus.cancelled or PaymentStatus.failed)
                {
                    OnItemPurchaseFail(_currentPurchaseItem);
                }
            });
        }

        /// <summary>
        /// Starting the process of returning previously purchased stars
        /// </summary>
        /// <param name="receipt">Link to check for payment of previously purchased telegram stars</param>
        public void Refund(PaymentReceiptData receipt)
        {
            var parsedTelegramId = WebRequestUtils.ParseTelegramId(receipt.BuyerId);
            var telegramId = int.Parse(parsedTelegramId);

            LastRefundedTransaction = receipt.TransactionId;

            StartCoroutine(BotAPIBridge.RefundPayment(telegramId, receipt.TransactionId, (isSuccess) =>
            {
                OnRefundTransactionFinish(LastRefundedTransaction, isSuccess);

                UnigramPaymentLogger.Log($"Refund process finished with result: {isSuccess}");
            }));
        }

        /// <summary>
        /// Updating the API server access token for continuous work with the payment module.
        /// Subscribe to the `OnSessionTokenRefreshed` event to handle its successful update to a new one.
        /// </summary>
        public void RefreshToken()
        {
            UnigramPaymentLogger.Log("A request to update the server access token has been activated.");

            AuthorizeClient((tokenClaimed) =>
            {
                OnSessionTokenRefresh();
            },
            () =>
            {
                OnSessionTokenRefshFail();
            });
        }

        private void AuthorizeClient(Action<string> accessTokenClaimed,
            Action accessTokenClaimFailed)
        {
            StartCoroutine(BotAPIBridge.AuthorizeClient((authToken) =>
            {
                JwtToken = authToken;

                if (JwtToken == null)
                {
                    accessTokenClaimFailed?.Invoke();

                    return;
                }

                UnigramPaymentLogger.Log($"Loaded jwt token: {JwtToken}");

                accessTokenClaimed?.Invoke(JwtToken);

                UnigramPaymentLogger.Log("Unigram Payment SDK has been successfully initialized, connection to the server has been made.");
            }));
        }


        private void OpenPurchaseInvoice(string invoiceLink, Action<PaymentStatus, string> invoiceClosed)
        {
            if (!UnigramUtils.IsSupportedNativeOpen())
            {
                Application.OpenURL(invoiceLink);

                UnigramPaymentLogger.LogWarning("Native opening of payment request in Telegram Stars is not supported in Editor." +
                    " Make a WebGL build to get the result of native transaction status events.");

                return;
            }

            WebAppAPIBridge.OpenPurchaseInvoice(invoiceLink,
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
        private void OnSessionTokenRefshFail() => OnSessionTokenRefreshFailed?.Invoke();

        private void OnInvoiceLinkCreate(string itemId,
            string invoiceUrl) => OnInvoiceLinkCreated?.Invoke(itemId, invoiceUrl);
        private void OnInvoiceLinkCreateFail(string itemId) => OnInvoiceLinkCreateFailed?.Invoke(itemId);

        private void OnItemPurchase(PaymentReceiptData receipt) => OnItemPurchased?.Invoke(receipt);
        private void OnItemPurchaseFail(SaleableItem failedPurchaseItem) =>
            OnItemPurchaseFailed?.Invoke(failedPurchaseItem);

        private void OnRefundTransactionFinish(string transactionId,
            bool isSuccess) => OnRefundTransactionFinished?.Invoke(transactionId, isSuccess);
    }
}