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
    public sealed class UnigramPaymentSDK : MonoBehaviour, IUnigramPaymentCallbacks,
        IUnigramPaymentTransactionCallbacks, IUnigramPaymentUtilsCallbacks
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
        [Tooltip("Delay before sending a request to the backend to receive a check when making a payment." +
            "Telegram API may not immediately send a request to the bot to trigger a successful payment and receive a check.")]
        [SerializeField, Range(5.0f, 60.0f), Space] private float _receivePaymentCheckDelay;
        [Tooltip("Number of retries to process the request. IMPORTANT: At the moment, it is used only for loading a payment check from the backend.")]
        [SerializeField, Range(1, 10)] private int _resendAttemptsAmount;

        private SaleableItem _currentPurchaseItem;

        private PaymentReceiptData _lastPaymentReceipt;

        private int _currentResendAttemptsAmount;

        public bool IsInitialized { get; private set; }

        public string JwtToken { get; private set; }

        public string LastInvoiceLink { get; private set; }

        public string LastRefundedTransaction { get; private set; }

        public bool IsDebugMode => _debugMode;

        /// <summary>
        /// A callback that is called when initialization of cdk is completed
        /// </summary>
        public event IUnigramPaymentCallbacks.OnUnigramConnectInitialize OnInitialized;

        /// <summary>
        /// A callback that is called if the valid authorization
        /// token has expired and has been updated to a new one
        /// </summary>

        public event IUnigramPaymentCallbacks.OnSessionTokenRefresh OnSessionTokenRefreshed;

        /// <summary>
        /// A callback, which is called if the valid authorization
        /// token has expired and could not be renewed due to one of the following reasons
        /// </summary>
        public event IUnigramPaymentCallbacks.OnSessionTokenRefreshFail OnSessionTokenRefreshFailed;

        /// <summary>
        /// A callback that is invoked when a payment invoice 
        /// is successfully created with a link provided
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnInvoiceLinkCreate OnInvoiceLinkCreated;

        /// <summary>
        /// A callback that is invoked if the creation of a payment link fails
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnInvoiceLinkCreateFail OnInvoiceLinkCreateFailed;

        /// <summary>
        /// A callback that is called if the creation of 
        /// a payment link failed with a specific reason
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnInvoiceLinkCreateFailDetailed OnFullInvoiceLinkCreateFailed;

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
        /// A callback that is called when the purchase of an item 
        /// fails or the purchase window closes due to a previously
        /// created invoice link with a specific reason
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnItemPurchaseFailDetailed OnFullItemPurchaseFailed;

        /// <summary>
        /// A callback that is called when the process of 
        /// returning previously spent stars on an item in the project is completed
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnRefundTransactionFinish OnRefundTransactionFinished;

        /// <summary>
        /// A callback that is called when the return of 
        /// the previous payment failed with a specific reason
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnRefundTransactionFail OnFullRefundTransactionFailed;

        /// <summary>
        /// A callback that is called when the history of 
        /// purchases made has been successfully loaded
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnPurchaseHistoryLoad OnPurchaseHistoryLoaded;

        /// <summary>
        /// A callback that is called when the history of
        /// completed purchases could not be successfully loaded
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnPurchaseHistoryFail OnPurchaseHistoryLoadFailed;

        /// <summary>
        /// A callback that is called when a history of 
        /// of purchase returns has been successfully loaded
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnRefundHistoryLoad OnRefundHistoryLoaded;

        /// <summary>
        /// A callback that is called when the purchase 
        /// return history could not be successfully loaded
        /// </summary>
        public event IUnigramPaymentTransactionCallbacks.OnRefundHistoryFail OnRefundHistoryLoadFailed;

        /// <summary>
        /// A callback that returns a tick of the 
        /// current UTC time per second
        /// </summary>
        public event IUnigramPaymentUtilsCallbacks.OnTimeTickLoad OnTimeTickLoaded;

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
        /// Initializes the Unigram Payment sdk if you want to do it manually. 
        /// Subscribe to the `OnInitialized` event to get the initialization status.
        /// </summary>
        public void Initialize()
        {
            AuthorizeClient(() =>
            {
                IsInitialized = true;

                OnInitialize(true);
            },
            () =>
            {
                OnInitialize(false);
            });
        }

        /// <summary>
        /// Creates an invoice, based on the configuration from the item store for purchase.
        /// WARNING: You must create and add an item to the item store before creating an invoice, 
        /// otherwise the creation process will be canceled with an error.
        /// </summary>
        /// <param name="item">Item configuration for purchase</param>
        public void CreateInvoice(SaleableItem item)
        {
            CreateTargetInvoice(item, null);
        }

        /// <summary>
        /// Performs native opening of an invoice that was created earlier. 
        /// </summary>
        /// <param name=“invoiceUrl”>Generated invoice link</param>
        /// <param name="itemId">Item configuration for purchase</param>
        public void OpenInvoice(string invoiceUrl, string itemId)
        {
            TryPayInvoiceIfConfirmed(invoiceUrl, itemId);
        }

        /// <summary>
        /// Creates a link to an invoice from the configuration 
        /// and opens it instantly after it is successfully created. 
        /// WARNING: Enable the wait screen before calling this method to maintain a clear 
        /// UX for the user (due to potential response delays from your server as well as the Telegram API.
        /// </summary>
        /// <param name="item">Item configuration for purchase</param>
        public void PayInvoice(SaleableItem item)
        {
            CreateTargetInvoice(item, () =>
            {
                UnigramPaymentLogger.Log("Invoice link by " +
                    "config created, try to open...");

                TryPayInvoiceIfConfirmed(LastInvoiceLink,
                    _currentPurchaseItem.Id);
            });
        }

        /// <summary>
        /// Starts the process of returning previously purchased 
        /// stars if the payment is detected by Telegram itself
        /// </summary>
        /// <param name="receipt">Link to check for payment of previously purchased telegram stars</param>
        public void Refund(PaymentReceiptData receipt)
        {
            LastRefundedTransaction = receipt.TransactionId;

            var buyerId = long.Parse(receipt.BuyerId);
            var transactionId = receipt.TransactionId;

            StartCoroutine(BotAPIBridge.RefundPayment(buyerId, transactionId,
            (isSuccess) =>
            {
                OnRefundTransactionFinish(LastRefundedTransaction, isSuccess);

                UnigramPaymentLogger.Log($"Refund process finished with result: {isSuccess}");
            },
            (errorReason) =>
            {
                UnigramPaymentLogger.LogError($"Failed to refund transaction, reason: {errorReason}");

                OnRefundTransactionFail(transactionId, ErrorTypes.SessionExpired);
            }));
        }

        /// <summary>
        /// Updates the API server access token for continuous operation of the payment module.
        /// Subscribe to the `OnSessionTokenRefreshed` event to handle its successful update to a new one.
        /// </summary>
        public void RefreshToken()
        {
            UnigramPaymentLogger.Log("A request to update " +
                "the server access token has been activated.");

            AuthorizeClient(() =>
            {
                OnSessionTokenRefresh();
            },
            () =>
            {
                OnSessionTokenRefreshFail();
            });
        }

        /// <summary>
        /// Requests the specified number of successful transactions.
        /// IMPORTANT: The Telegram bot api starts counting from the first transaction recorded by the bot, not the most recent.
        /// </summary>
        /// <param name=“amount”>Number of transactions to download, displayed up to 100 if no value is specified.</param>.
        /// <param name=“totalPass”>Number of skips between transactions.
        /// Useful parameter for approaching the last transactions from the list (thanks to the great Telegram Bot API for such a crutch)</param>.
        public void GetPurchaseHistory(
            int amount = 0, int totalPass = 0)
        {
            StartCoroutine(BotAPIBridge.GetPurchaseHistory(amount, totalPass,
            (history) =>
            {
                var transactionAmount = history.Transactions.Count;

                UnigramPaymentLogger.Log($"Purchase history successfully " +
                    $"claimed with transactions amount: {transactionAmount}");

                OnPurchaseHistoryLoad(history);
            },
            (errorReason) =>
            {
                UnigramPaymentLogger.LogError($"Failed to load purchase " +
                    $"history, reason: {errorReason}");

                OnPurchaseHistoryLoadFail(errorReason);
            }));
        }

        /// <summary>
        /// Requests the specified number of successful returns.
        /// IMPORTANT: The Telegram bot api starts counting from the first transaction recorded by the bot, not the most recent.
        /// </summary>
        /// <param name=“amount”>Number of transactions to load, displayed up to 100 if no value is specified. </param>
        /// <param name=“totalPass”>Number of skips between transactions.
        /// Useful parameter for approaching the last transactions from the list (thanks to the great Telegram Bot API for such a crutch)</param>
        public void GetRefundHistory(
            int amount = 0, int totalPass = 0)
        {
            StartCoroutine(BotAPIBridge.GetRefundHistory(amount, totalPass,
                (history) =>
            {
                var transactionAmount = history.Transactions.Count;

                UnigramPaymentLogger.Log($"Refund history successfully " +
                    $"claimed with transactions amount: {transactionAmount}");

                OnRefundHistoryLoad(history);
            },
                (errorReason) =>
            {
                UnigramPaymentLogger.LogError($"Failed to load refund " +
                    $"history, reason: {errorReason}");

                OnRefundHistoryLoadFail(errorReason);
            }));
        }

        /// <summary>
        /// Loads the current server tick in seconds
        /// </summary>
        public void FetchTimeTick()
        {
            StartCoroutine(BotAPIBridge.GetTime((timeTickData) =>
            {
                if (timeTickData == null)
                {
                    OnTimeTickLoad(0);

                    UnigramPaymentLogger.LogWarning("Failed to fetch " +
                        "current server time tick");

                    return;
                }

                var timeTick = timeTickData.UnixTick;

                UnigramPaymentLogger.Log($"The current server time tick is: {timeTick}");

                OnTimeTickLoad(timeTick);
            }));
        }

        private void AuthorizeClient(Action accessTokenClaimed,
            Action accessTokenClaimFailed)
        {
            StartCoroutine(BotAPIBridge.AuthorizeClient((authToken) =>
            {
                JwtToken = authToken;

                if (string.IsNullOrEmpty(JwtToken))
                {
                    accessTokenClaimFailed?.Invoke();

                    return;
                }

                UnigramPaymentLogger.Log($"Loaded new session token: {JwtToken}");

                accessTokenClaimed?.Invoke();

                UnigramPaymentLogger.Log("Unigram Payment SDK " +
                    "has been successfully initialized");
            }));
        }

        private void CreateTargetInvoice(
            SaleableItem item, Action invoiceCreated)
        {
            var saleableItem = UnigramUtils.FindItemInItemsStorage(_itemsStorage, item);

            if (saleableItem == null)
            {
                UnigramPaymentLogger.LogError("The invoice link creation " +
                    "process is canceled, the item for purchase is not found in the vault.");

                return;
            }

            _currentPurchaseItem = item;

            StartCoroutine(BotAPIBridge.CreateInvoice(
            item, (invoiceLink) =>
            {
                if (string.IsNullOrEmpty(invoiceLink))
                {
                    UnigramPaymentLogger.LogWarning("Invoice link validation failed");

                    OnInvoiceLinkCreateFail(item.Id, ErrorTypes.InvalidInvoiceLink);

                    return;
                }

                LastInvoiceLink = invoiceLink;

                if (invoiceCreated != null)
                {
                    invoiceCreated?.Invoke();
                }

                OnInvoiceLinkCreate(item.Id, LastInvoiceLink);
            },
            (errorReason) =>
            {
                OnInvoiceLinkCreateFail(item.Id, errorReason);
            }));
        }

        private void TryPayInvoiceIfConfirmed(
            string invoiceUrl, string itemId)
        {
            if (_receivePaymentCheckDelay <= 0)
            {
                UnigramPaymentLogger.LogWarning($"Transaction delay is 0, " +
                    $"please activate the basic delay of 15 seconds.");

                _receivePaymentCheckDelay = 15f;
            }

            OpenPurchaseInvoice(invoiceUrl, (status, message) =>
            {
                UnigramPaymentLogger.Log($"Transaction finished " +
                    $"with status: {status}, extra data: {message}");

                if (status is PaymentStatus.paid)
                {
                    UnigramPaymentLogger.Log($"Local purchase event " +
                        $"finished with status: {status}, start load payment receipt");

                    var userId = WebAppAPIBridge.GetTelegramUser().Id.ToString();

                    GetPaymentReceipt(_receivePaymentCheckDelay,
                        userId, itemId, (receipt) =>
                    {
                        if (receipt != null)
                        {
                            OnItemPurchase(receipt);

                            return;
                        }

                        OnItemPurchaseFail(_currentPurchaseItem, ErrorTypes.AttemptsExpired);
                    });

                    return;
                }

                var parsedError = UnigramUtils.ParseErrorFromStatus(status);

                UnigramPaymentLogger.LogError($"Failed to pay opened " +
                    $"invoice by item {itemId}, reason: {parsedError}");

                OnItemPurchaseFail(_currentPurchaseItem, parsedError);
            });
        }

        private void OpenPurchaseInvoice(string invoiceLink,
            Action<PaymentStatus, string> invoiceClosed)
        {
            if (!UnigramUtils.IsSupportedNativeOpen())
            {
                invoiceClosed?.Invoke(PaymentStatus.cancelled, null);

                UnigramPaymentLogger.LogWarning("Native opening of payment request in " +
                    "Telegram Stars is not supported in Editor. Make a WebGL build to " +
                    "get the result of native transaction status events.");

                return;
            }

            WebAppAPIBridge.OpenPurchaseInvoice(invoiceLink,
            (status, resultPayment) =>
            {
                var parsedStatus = UnigramUtils.ParsePaymentStatusAfterPurchase(status);

                if (parsedStatus == PaymentStatus.paid)
                {
                    invoiceClosed?.Invoke(parsedStatus, resultPayment);

                    UnigramPaymentLogger.Log($"Success purchase with " +
                        $"result: {status}, data: {resultPayment}");

                    return;
                }

                UnigramPaymentLogger.LogWarning($"Failed to purchase item " +
                    $"by link {invoiceLink}, payment status: {parsedStatus}");

                invoiceClosed?.Invoke(parsedStatus, null);
            },
            (paymentStatus) =>
            {
                var parsedStatus = UnigramUtils.ParsePaymentStatusAfterPurchase(paymentStatus);

                invoiceClosed?.Invoke(parsedStatus, null);

                UnigramPaymentLogger.LogError($"Failed purchase " +
                    $"with status: {paymentStatus}");
            });
        }

        private void GetPaymentReceipt(float delay, string userId, string itemId,
            Action<PaymentReceiptData> paymentReceiptDataClaimed)
        {
            UnigramPaymentLogger.Log($"Starting load purchase " +
                $"receipt for payload: {userId} with delay: {delay}");

            StartCoroutine(BotAPIBridge.GetPaymentReceipt(delay, userId, itemId,
            (receipt) =>
            {
                _lastPaymentReceipt = receipt;

                _currentResendAttemptsAmount = 0;

                paymentReceiptDataClaimed?.Invoke(_lastPaymentReceipt);

                UnigramPaymentLogger.Log($"Received data about the " +
                    $"transaction {receipt.TransactionId} made with " +
                    $"the identifier {receipt.InvoicePayload}");
            },
            (errorReason) =>
            {
                UnigramPaymentLogger.LogWarning($"Starting resend response " +
                    $"for fetch payment receipt with attemp: " +
                    $"{_currentResendAttemptsAmount}");

                _currentResendAttemptsAmount++;

                if (_currentResendAttemptsAmount > _resendAttemptsAmount)
                {
                    paymentReceiptDataClaimed?.Invoke(null);

                    _currentResendAttemptsAmount = 0;

                    UnigramPaymentLogger.LogError($"Failed to receive " +
                        $"a check for payment for some reason");
                }

                GetPaymentReceipt(delay, userId, itemId, paymentReceiptDataClaimed);
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

                if (_instance != null)
                {
                    UnigramPaymentLogger.LogError($"Another instance " +
                        $"is detected on the scene, running delete...");

                    Destroy(gameObject);
                }
            }
        }

        private void OnInitialize(bool isSuccess) => 
            OnInitialized?.Invoke(isSuccess);

        private void OnSessionTokenRefresh() => 
            OnSessionTokenRefreshed?.Invoke();
        private void OnSessionTokenRefreshFail() => 
            OnSessionTokenRefreshFailed?.Invoke();

        private void OnInvoiceLinkCreate(string itemId, string invoiceUrl) =>
            OnInvoiceLinkCreated?.Invoke(itemId, invoiceUrl);
        private void OnInvoiceLinkCreateFail(string itemId) => 
            OnInvoiceLinkCreateFailed?.Invoke(itemId);
        private void OnInvoiceLinkCreateFail(string itemId, ErrorTypes reason)
        {
            OnInvoiceLinkCreateFail(itemId);

            OnFullInvoiceLinkCreateFailed?.Invoke(itemId, reason);
        }

        private void OnItemPurchase(PaymentReceiptData receipt) => 
            OnItemPurchased?.Invoke(receipt);
        private void OnItemPurchaseFail(SaleableItem failedPurchaseItem) =>
            OnItemPurchaseFailed?.Invoke(failedPurchaseItem);
        private void OnItemPurchaseFail(SaleableItem failedPurchaseItem, ErrorTypes reason)
        {
            OnItemPurchaseFail(_currentPurchaseItem);

            OnFullItemPurchaseFailed?.Invoke(failedPurchaseItem, reason);
        }

        private void OnRefundTransactionFinish(string transactionId,
            bool isSuccess) => OnRefundTransactionFinished?.Invoke(transactionId, isSuccess);
        private void OnRefundTransactionFail(string transactionId, ErrorTypes reason) =>
            OnFullRefundTransactionFailed?.Invoke(transactionId, reason);

        private void OnPurchaseHistoryLoad(PurchaseHistoryData history) => 
            OnPurchaseHistoryLoaded?.Invoke(history);
        private void OnPurchaseHistoryLoadFail(ErrorTypes reason) => 
            OnPurchaseHistoryLoadFailed?.Invoke(reason);

        private void OnRefundHistoryLoad(RefundHistoryData history) => 
            OnRefundHistoryLoaded?.Invoke(history);
        private void OnRefundHistoryLoadFail(ErrorTypes reason) => 
            OnRefundHistoryLoadFailed?.Invoke(reason);

        private void OnTimeTickLoad(long timeTick) => 
            OnTimeTickLoaded?.Invoke(timeTick);
    }
}