using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using UnigramPayment.Core;
using UnigramPayment.Runtime.Core;
using UnigramPayment.Runtime.Data;
using UnigramPayment.Runtime.Utils;
using TestExample.UI.PopUp;

namespace TestExample
{
    public sealed class TestUnigramPaymentTemplate : MonoBehaviour
    {
        [SerializeField, Space] private UnigramPaymentSDK _unigramPayment;
        [SerializeField, Space] private TextMeshProUGUI _debugBar;
        [SerializeField, Space] private Button _purchaseItemButton;
        [SerializeField] private Button _refundStarsButton;
        [SerializeField, Space] private SaleableItemsStorage _itemsStorage;
        [SerializeField, Space] private TestTransactionPendingPopUp _pendingTransactionPopUp;

        private SaleableItem _randomItemForPurchase;
        private PaymentReceiptData _itemPaymentReceipt;

        private string _latestInvoiceLink;

        private const string DEBUG_PREFIX = "DEBUG INFO:";

        private void OnEnable()
        {
            _unigramPayment.OnInitialized += UnigramPaymentInitialized;
            _unigramPayment.OnSessionTokenRefreshed += SessionTokenRefreshed;

            _unigramPayment.OnInvoiceLinkCreated += PaymentInvoiceCreated;
            _unigramPayment.OnInvoiceLinkCreateFailed += PaymentInvoiceCreateFailed;

            _unigramPayment.OnItemPurchased += TargetItemPurchased;
            _unigramPayment.OnItemPurchaseFailed += TargetItemPurchaseFailed;

            _unigramPayment.OnRefundTransactionFinished += RefundTransactionFinished;
        }

        private void OnDisable()
        {
            _purchaseItemButton.onClick.RemoveListener(PurchaseItem);
            _refundStarsButton.onClick.RemoveListener(Refund);

            _unigramPayment.OnInitialized -= UnigramPaymentInitialized;

            _unigramPayment.OnInvoiceLinkCreated -= PaymentInvoiceCreated;
            _unigramPayment.OnInvoiceLinkCreateFailed -= PaymentInvoiceCreateFailed;

            _unigramPayment.OnItemPurchased -= TargetItemPurchased;
            _unigramPayment.OnItemPurchaseFailed -= TargetItemPurchaseFailed;

            _unigramPayment.OnRefundTransactionFinished -= RefundTransactionFinished;
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            ConfigureButtons();

            SetInteractableStateByButton(_purchaseItemButton, false);
            SetInteractableStateByButton(_refundStarsButton, false);

            _unigramPayment.Initialize();
        }

        private void PurchaseItem()
        {
            _pendingTransactionPopUp.Show();

            _unigramPayment.OpenInvoice(_latestInvoiceLink, _randomItemForPurchase.Id);
        }

        private void Refund()
        {
            _unigramPayment.Refund(_itemPaymentReceipt);
        }

        private void ConfigureButtons()
        {
            _purchaseItemButton.onClick.AddListener(PurchaseItem);
            _refundStarsButton.onClick.AddListener(Refund);
        }

        private void SetInteractableStateByButton(Button button, bool isCanInteract)
        {
            button.interactable = isCanInteract;
        }

        private void UnigramPaymentInitialized(bool isSuccess)
        {
            if (isSuccess)
            {
#if !UNITY_EDITOR && UNITY_WEBGL
                Debug.Log($"Loaded telegram user data: ${JsonConvert.SerializeObject(WebAppAPIBridge.GetTelegramUser())}");
#endif

                _randomItemForPurchase = _itemsStorage.Items[
                    Random.Range(0, _itemsStorage.Items.Count - 1)];

                Debug.Log($"Claimed item with payload id: {_randomItemForPurchase.Id}");

                _unigramPayment.CreateInvoice(_randomItemForPurchase);

                _debugBar.text = $"{DEBUG_PREFIX} `Unigram Payment` has been successfully initialized." +
                    $" The process of creating a payment link for an item with an id: {_randomItemForPurchase.Id} has started.";

                return;
            }

            _debugBar.text = $"{DEBUG_PREFIX} Failed to initialize `Unigram Payment` for one of the reasons: " +
                "API server is not running, client's secret key value is not filled in, no internet connection.";
        }

        private void SessionTokenRefreshed()
        {
            _debugBar.text = $"{DEBUG_PREFIX} The session token, to connect to the server API," +
                $" has been successfully updated.: {_unigramPayment.JwtToken}";

            SetInteractableStateByButton(_purchaseItemButton, true);
            SetInteractableStateByButton(_refundStarsButton, false);
        }

        private void PaymentInvoiceCreated(string itemPayloadId, string url)
        {
            _latestInvoiceLink = url;

            _debugBar.text = $"{DEBUG_PREFIX} The link to purchase the" +
                $" test item {itemPayloadId} has been successfully generated: {url}";

            SetInteractableStateByButton(_purchaseItemButton, true);
        }

        private void PaymentInvoiceCreateFailed(string itemPayloadId)
        {
            _debugBar.text = $"{DEBUG_PREFIX} Failed to create a payment link for" +
                $" item {itemPayloadId} for one of the following reasons:" +
                 $" SDK is not initialized, API server is not started, incorrectly filled in product data.";
        }

        private void TargetItemPurchased(PaymentReceiptData receipt)
        {
            _itemPaymentReceipt = receipt;

            _pendingTransactionPopUp.Hide();

            _debugBar.text = $"{DEBUG_PREFIX} The item with identifier {_itemPaymentReceipt.InvoicePayload} " +
                $"was successfully purchased for {_itemPaymentReceipt.Amount} " +
                $"stars by the buyer with telegram id {_itemPaymentReceipt.BuyerId}";

            SetInteractableStateByButton(_purchaseItemButton, false);
            SetInteractableStateByButton(_refundStarsButton, true);
        }

        private void TargetItemPurchaseFailed(SaleableItem failedPurchaseItem)
        {
            _pendingTransactionPopUp.Hide();

            _debugBar.text = $"{DEBUG_PREFIX} Failed to purchase an item {failedPurchaseItem.Name}" +
                $" for one of the following reasons: SDK not initialized," +
                $" API server not configured, or incorrect item data entered.";
        }

        private void RefundTransactionFinished(string transactionId, bool isSuccess)
        {
            if (isSuccess)
            {
                _debugBar.text = $"{DEBUG_PREFIX} The process of refunding the purchased stars through the transaction with" +
                    $" the identifier `{_unigramPayment.LastRefundedTransaction}` has been completed successfully";

                SetInteractableStateByButton(_purchaseItemButton, true);
                SetInteractableStateByButton(_refundStarsButton, false);

                return;
            }

            _debugBar.text = $"{DEBUG_PREFIX} Couldn't get a refund for the stars I bought" +
                " for one reason, they may have already been refunded.";
        }
    }
}