using UnityEngine;
using UnigramPayment.Utils.Debugging;
using TMPro;
using System.Collections;
using Newtonsoft.Json;
using UnigramPayment.Runtime.Data;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using UnigramPayment.Core;

namespace UnigramPayment.Runtime.Core
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/MrVeit/Veittech-UnigramPayment/blob/master/README.md")]
    public sealed class UnigramPaymentSDK : MonoBehaviour
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
        [Tooltip("Enable if you want to test the SDK without having to create your items for purchase")]
        [SerializeField, Space] private bool _testMode;
        [Tooltip("Enable if you want to activate SDK logging for detailed analysis")]
        [SerializeField] private bool _debugMode;
        [Tooltip("Turn it off if you want to do your own cdk initialization in your scripts")]
        [SerializeField, Space] private bool _initializeOnAwake;
        [SerializeField, Space] private TextMeshProUGUI _logInfo;

        private string _testProductName = "Test Product";
        private string _testDescription = "If you buy this product, you'll be happy all day today!";

        private int _testProductAmount = 0;

        private string _lastInvoiceLink;

        public bool IsDebugMode => _debugMode;

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

        }

        public void Purchase()
        {

        }

        public void Refund()
        {

        }

        public void CreateInvoice()
        {
            StartCoroutine(CreateInvoiceLink());
        }

        public void OpenInvoice()
        {
            WebAppAPIBridge.OpenPurchaseInvoice(_lastInvoiceLink, (status, resultPayment) =>
            {
                Debug.Log($"Success purchase with result: {status}, data: {resultPayment}");
            },
            (paymentStatus) =>
            {
                Debug.LogError($"Faied purchase with status: {paymentStatus}");
            });
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

                UnigramPaymentLogger.LogError($"Another instance is detected on the scene, running delete...");

                Destroy(gameObject);
            }
        }

        private IEnumerator CreateInvoiceLink()
        {
            string botToken = "7372318645:AAH-lOi_1m_NhE0eNxNdQWo29O9ZTgmI7BM";

            string apiUrl = "https://api.telegram.org/bot";
            string finalUrl = $"{apiUrl}{botToken}/createInvoiceLink";

            Debug.Log(finalUrl);

            var invoice = new InvoiceData()
            {
                Title = "TestTitle",
                Description = "TestDesc",
                Payload = "Custom-Payload",
                ProviderToken = "",
                Currency = InvoiceData.CURRENCY_TYPE,
                Prices = new List<PriceData>()
                { 
                    new()
                    {
                        Label = "ProductLabel",
                        Amount = 10
                    }
                },
                URL = "https://saunposium.logarithm.games/icon.png",
                Height = 512,
                Width = 512
            };

            string jsonPayload = JsonConvert.SerializeObject(invoice);

            Debug.Log(jsonPayload);

            using (UnityWebRequest request = new(finalUrl, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var responseResult = request.downloadHandler.text;

                    _logInfo.text = $"Invoice link created: {responseResult}";

                    _lastInvoiceLink = JsonConvert.DeserializeObject<InvoiceLinkData>(responseResult).Link;

                    Debug.Log(_lastInvoiceLink);
                }
                else
                {
                    Debug.LogError($"Failed to create invoice link: {request.error}, request code: {request.responseCode}");
                }
            }
        }
    }
}