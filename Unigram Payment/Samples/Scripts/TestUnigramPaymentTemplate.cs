using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using UnigramPayment.Core;
using UnigramPayment.Runtime.Core;
using UnigramPayment.Runtime.Common;
using UnigramPayment.Runtime.Data;
using UnigramPayment.Runtime.Utils;
using TestExample.UI.PopUp;

namespace TestExample
{
    public sealed class TestUnigramPaymentTemplate : MonoBehaviour
    {
        [SerializeField, Space] private UnigramPaymentSDK _unigramPayment;
        [SerializeField, Space] private TextMeshProUGUI _debugBar;
        [SerializeField, Space] private Button _fastPayItemButton;
        [SerializeField] private Button _refundStarsButton;
        [SerializeField, Space] private SaleableItemsStorage _itemsStorage;
        [SerializeField, Space] private TestTransactionPendingPopUp _pendingTransactionPopUp;

        private SaleableItem _randomItemForPurchase;
        private PaymentReceiptData _itemPaymentReceipt;

        private const string DEBUG_PREFIX = "DEBUG INFO:";

        private void OnEnable()
        {
            _unigramPayment.OnInitialized += UnigramPaymentInitialized;

            _unigramPayment.OnSessionTokenRefreshed += SessionTokenRefreshed;
            _unigramPayment.OnSessionTokenRefreshFailed += SessionTokenRefreshFailed;

            _unigramPayment.OnInvoiceLinkCreated += PaymentInvoiceCreated;
            _unigramPayment.OnFullInvoiceLinkCreateFailed += PaymentInvoiceCreateFailed;

            _unigramPayment.OnItemPurchased += TargetItemPurchased;
            _unigramPayment.OnFullItemPurchaseFailed += TargetItemPurchaseFailed;

            _unigramPayment.OnRefundTransactionFinished += RefundTransactionFinished;
            _unigramPayment.OnFullRefundTransactionFailed += RefundTransactionFinishFailed;

            _unigramPayment.OnTimeTickLoaded += TimeTickLoaded;
        }

        private void OnDisable()
        {
            _fastPayItemButton.onClick.RemoveListener(FastPayItem);
            _refundStarsButton.onClick.RemoveListener(Refund);

            _unigramPayment.OnInitialized -= UnigramPaymentInitialized;

            _unigramPayment.OnSessionTokenRefreshed -= SessionTokenRefreshed;
            _unigramPayment.OnSessionTokenRefreshFailed -= SessionTokenRefreshFailed;

            _unigramPayment.OnInvoiceLinkCreated -= PaymentInvoiceCreated;
            _unigramPayment.OnFullInvoiceLinkCreateFailed -= PaymentInvoiceCreateFailed;

            _unigramPayment.OnItemPurchased -= TargetItemPurchased;
            _unigramPayment.OnFullItemPurchaseFailed -= TargetItemPurchaseFailed;

            _unigramPayment.OnRefundTransactionFinished -= RefundTransactionFinished;
            _unigramPayment.OnFullRefundTransactionFailed -= RefundTransactionFinishFailed;

            _unigramPayment.OnTimeTickLoaded -= TimeTickLoaded;
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            ConfigureButtons();

            SetInteractableStateByButton(_fastPayItemButton, false);
            SetInteractableStateByButton(_refundStarsButton, false);

            _unigramPayment.Initialize();
        }

        private void FastPayItem()
        {
            _pendingTransactionPopUp.Show();

            _unigramPayment.PayInvoice(_randomItemForPurchase);
        }

        private void Refund()
        {
            if (_itemPaymentReceipt == null)
            {
                Debug.LogWarning("Payment receipt for refund not found!");

                return;
            }

            _unigramPayment.Refund(_itemPaymentReceipt);
        }

        private void ConfigureButtons()
        {
            _fastPayItemButton.onClick.AddListener(FastPayItem);
            _refundStarsButton.onClick.AddListener(Refund);
        }

        private void SetInteractableStateByButton(
            Button button, bool isCanInteract)
        {
            button.interactable = isCanInteract;
        }

        private void SetRandomItemForPay()
        {
            var randomId = Random.Range(0, _itemsStorage.Items.Count);

            _randomItemForPurchase = _itemsStorage.Items[randomId];

            Debug.Log($"Claimed item with payload id: {_randomItemForPurchase.Id}");
        }

        private void UnigramPaymentInitialized(bool isSuccess)
        {
            if (isSuccess)
            {
#if !UNITY_EDITOR && UNITY_WEBGL
                Debug.Log($"Loaded telegram user data: ${JsonConvert.SerializeObject(WebAppAPIBridge.GetTelegramUser())}");
#endif

                SetRandomItemForPay();

                _debugBar.text = $"{DEBUG_PREFIX} `Unigram Payment` has been successfully initialized." +
                    $" The process of creating a payment link for an item with an id: {_randomItemForPurchase.Id} has started.";

                _unigramPayment.FetchTimeTick();

                return;
            }

            _debugBar.text = $"{DEBUG_PREFIX} Failed to initialize `Unigram Payment` for one of the reasons: " +
                "API server is not running, client's secret key value is not filled in, no internet connection.";
        }

        private void SessionTokenRefreshed()
        {
            _debugBar.text = $"{DEBUG_PREFIX} The session token, to connect to the server API," +
                $" has been successfully updated.: {_unigramPayment.JwtToken}";

            SetInteractableStateByButton(_fastPayItemButton, true);
            SetInteractableStateByButton(_refundStarsButton, false);
        }

        private void SessionTokenRefreshFailed()
        {
            _debugBar.text = $"{DEBUG_PREFIX} Failed to update " +
                $"the client session token for some reason";

            SetInteractableStateByButton(_fastPayItemButton, false);
            SetInteractableStateByButton(_refundStarsButton, false);
        }

        private void PaymentInvoiceCreated(string itemPayloadId, string url)
        {
            _debugBar.text = $"{DEBUG_PREFIX} The link to purchase the" +
                $" test item {itemPayloadId} has been successfully generated: {url}";

            SetInteractableStateByButton(_fastPayItemButton, true);
            SetInteractableStateByButton(_refundStarsButton, false);
        }

        private void PaymentInvoiceCreateFailed(
            string itemPayloadId, ErrorTypes reason)
        {
            _debugBar.text = $"{DEBUG_PREFIX} Failed to create " +
                $"a payment link for item {itemPayloadId} by reason: {reason}";
        }

        private void TargetItemPurchased(
            PaymentReceiptData receipt)
        {
            _itemPaymentReceipt = receipt;

            _pendingTransactionPopUp.Hide();

            _debugBar.text = $"{DEBUG_PREFIX} The item with identifier " +
                $"{_itemPaymentReceipt.InvoicePayload} was successfully " +
                $"purchased for {_itemPaymentReceipt.Amount} stars by " +
                $"the buyer with telegram id {_itemPaymentReceipt.BuyerId}";

            SetInteractableStateByButton(_fastPayItemButton, false);
            SetInteractableStateByButton(_refundStarsButton, true);

            SetRandomItemForPay();
        }

        private void TargetItemPurchaseFailed(
            SaleableItem failedPurchaseItem, ErrorTypes reason)
        {
            _pendingTransactionPopUp.Hide();

            _debugBar.text = $"{DEBUG_PREFIX} Failed to purchase " +
                $"an item {failedPurchaseItem.Name} by reason: {reason}";
        }

        private void RefundTransactionFinished(
            string transactionId, bool isSuccess)
        {
            if (isSuccess)
            {
                _debugBar.text = $"{DEBUG_PREFIX} The process of refunding " +
                    $"the purchased stars through the transaction with the identifier " +
                    $"`{_unigramPayment.LastRefundedTransaction}` has been completed successfully";

                SetInteractableStateByButton(_fastPayItemButton, true);
                SetInteractableStateByButton(_refundStarsButton, false);

                return;
            }

            _debugBar.text = $"{DEBUG_PREFIX} Couldn't get a refund for the stars I bought" +
                " for one reason, they may have already been refunded.";
        }

        private void RefundTransactionFinishFailed(
            string transactionId, ErrorTypes reason)
        {
            _debugBar.text = $"{DEBUG_PREFIX} Couldn't get a refund " +
                $"for the stars I bought, reason: {reason}";
        }

        private void TimeTickLoaded(long unixTimeTick)
        {
            _debugBar.text = $"{DEBUG_PREFIX} Current server time tick: {unixTimeTick}";
        }
    }
}