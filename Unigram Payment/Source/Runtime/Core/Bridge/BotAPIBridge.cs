using System;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnigramPayment.Runtime.Core;
using UnigramPayment.Runtime.Data;
using UnigramPayment.Runtime.Common;
using UnigramPayment.Runtime.Utils;
using UnigramPayment.Storage.Data;
using UnigramPayment.Runtime.Utils.Debugging;

namespace UnigramPayment.Core
{
    public static class BotAPIBridge
    {
        private const string POST_RESPONSE = UnityWebRequest.kHttpVerbPOST;

        private const string HEADER_CONTENT_TYPE = WebRequestUtils.HEADER_CONTENT_TYPE;
        private const string HEADER_AUTHORIZATION = WebRequestUtils.HEADER_AUTHORIZATION;

        private const string HEADER_VALUE_APPLICATION_JSON = WebRequestUtils.HEADER_VALUE_APPLICATION_JSON;
        private const string HEADER_VALUE_TEXT_PLAIN = WebRequestUtils.HEADER_VALUE_TEXT_PLAIN;

        public static IEnumerator AuthorizeClient(
            RuntimeAPIConfig apiConfig, Action<string> authorizationTokenClaimed)
        {
            if (!IsExistProjectConfig())
            {
                authorizationTokenClaimed?.Invoke(null);

                yield break;
            }

            if (!IsExisClientSecretKey())
            {
                authorizationTokenClaimed?.Invoke(null);

                yield break;
            }

            if (!IsExistServerLink())
            {
                authorizationTokenClaimed?.Invoke(null);

                yield break;
            }

            var authorizationLink = APIServerRequests.GetAuthorizationLink(apiConfig.ServerUrl);

            using (UnityWebRequest request = new(authorizationLink, POST_RESPONSE))
            {
                var bodyRaw = WebRequestUtils.GetBytesFromJsonUTF8(apiConfig.ClientSecretKey);

                WebRequestUtils.SetUploadHandler(request, WebRequestUtils.GetUploadHandlerRaw(bodyRaw));
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                WebRequestUtils.SetRequestHeader(request, HEADER_CONTENT_TYPE, HEADER_VALUE_TEXT_PLAIN);

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;
                    var authToken = JsonConvert.DeserializeObject<AuthorizationTokenData>(responseResult);

                    authorizationTokenClaimed?.Invoke(authToken.Token);

                    UnigramPaymentLogger.Log($"Received JWT Token from API server: {JsonConvert.SerializeObject(authToken)}");
                    UnigramPaymentLogger.Log("The client is authorized on the server, connection to the api server was successful.");

                    yield break;
                }
                else
                {
                    UnigramPaymentLogger.LogError($"Failed to authorize client on api server with message: {request.error}");

                    authorizationTokenClaimed?.Invoke(null);

                    yield break;
                }
            }
        }

        public static IEnumerator CreateInvoice(SaleableItem product,
            Action<string> invoiceLinkCreated)
        {
            string apiServerLink = RuntimeAPIConfig.Load().ServerUrl;

            if (!IsExistServerLink())
            {
                yield break;
            }

            string url = APIServerRequests.GetInvoiceLink(apiServerLink);

            var invoice = new InvoiceData()
            {
                Payload = product.Id,
                Title = product.Name,
                Description = product.Description,
                Amount = product.Price
            };

            var jwtToken = UnigramPaymentSDK.Instance.JwtToken;
            string jsonPayload = JsonConvert.SerializeObject(invoice);

            UnigramPaymentLogger.Log($"Product data for purchase: {jsonPayload}");

            using (UnityWebRequest request = new(url, POST_RESPONSE))
            {
                var bodyRaw = WebRequestUtils.GetBytesFromJsonUTF8(jsonPayload);

                WebRequestUtils.SetUploadHandler(request, WebRequestUtils.GetUploadHandlerRaw(bodyRaw));
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                WebRequestUtils.SetRequestHeader(request, HEADER_CONTENT_TYPE, HEADER_VALUE_APPLICATION_JSON);
                WebRequestUtils.SetRequestHeader(request, HEADER_AUTHORIZATION,
                    WebRequestUtils.GetAuthorizationHeaderValue(jwtToken));

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;

                    var invoiceLink = JsonConvert.DeserializeObject<GeneratedInvoiceData>(responseResult).Url;

                    invoiceLinkCreated?.Invoke(invoiceLink);

                    UnigramPaymentLogger.Log($"The invoice for the purchase of stars has been successfully created: {invoiceLink}");

                    yield break;
                }
                else
                {
                    var errorMessage = WebRequestUtils.ParseErrorReasonFromResponse(request);
                    var code = request.responseCode;

                    RefreshTokenIfSessionExpired(errorMessage);

                    invoiceLinkCreated?.Invoke(null);

                    UnigramPaymentLogger.LogError($"Failed to create invoice link, reason: {errorMessage}," +
                        $" request code: {request.responseCode}");

                    yield break;
                }
            }
        }

        public static IEnumerator RefundPayment(int buyerId,
            string transactionId, Action<bool> refundProcessFinished)
        {
            var apiServerLink = RuntimeAPIConfig.Load().ServerUrl;

            if (!IsExistServerLink())
            {
                yield break;
            }

            var url = APIServerRequests.GetRefundStarsLink(apiServerLink);

            var refundData = new RefundProcessData()
            {
                UserId = buyerId,
                TransactionId = transactionId,
            };

            var jwtToken = UnigramPaymentSDK.Instance.JwtToken;
            string jsonRefund = JsonConvert.SerializeObject(refundData);

            using (UnityWebRequest request = new(url, POST_RESPONSE))
            {
                var bodyRaw = WebRequestUtils.GetBytesFromJsonUTF8(jsonRefund);

                WebRequestUtils.SetUploadHandler(request, WebRequestUtils.GetUploadHandlerRaw(bodyRaw));
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                WebRequestUtils.SetRequestHeader(request, HEADER_CONTENT_TYPE, HEADER_VALUE_APPLICATION_JSON);
                WebRequestUtils.SetRequestHeader(request, HEADER_AUTHORIZATION,
                    WebRequestUtils.GetAuthorizationHeaderValue(jwtToken));

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;

                    refundProcessFinished?.Invoke(true);

                    UnigramPaymentLogger.Log($"The stars previously purchased by the user were successfully returned with status: {responseResult}");

                    yield break;
                }
                else
                {
                    var errorMessage = WebRequestUtils.ParseErrorReasonFromResponse(request);
                    var code = request.responseCode;

                    RefreshTokenIfSessionExpired(errorMessage);

                    refundProcessFinished?.Invoke(false);

                    UnigramPaymentLogger.LogError($"Failed to refund stars previously " +
                        $"purchased by the user due to the following reason: " +
                        $"{errorMessage}, response code: {code}");

                    yield break;
                }
            }
        }

        public static IEnumerator GetPaymentReceipt(Action<PaymentReceiptData> paymentReceiptClaimed)
        {
            var apiServerLink = RuntimeAPIConfig.Load().ServerUrl;

            if (!IsExistServerLink())
            {
                yield break;
            }

            var url = APIServerRequests.GetPaymentReceiptLink(apiServerLink);

            var jwtToken = UnigramPaymentSDK.Instance.JwtToken;

            using (UnityWebRequest request = new(url, UnityWebRequest.kHttpVerbGET))
            {
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                WebRequestUtils.SetRequestHeader(request, HEADER_AUTHORIZATION,
                    WebRequestUtils.GetAuthorizationHeaderValue(jwtToken));

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;
                    var receipt = JsonConvert.DeserializeObject<PaymentReceiptData>(responseResult);

                    paymentReceiptClaimed?.Invoke(receipt);

                    UnigramPaymentLogger.Log($"Customer transaction data {receipt} has been successfully uploaded.");

                    yield break;
                }
                else
                {
                    var errorMessage = WebRequestUtils.ParseErrorReasonFromResponse(request);
                    var code = request.responseCode;

                    RefreshTokenIfSessionExpired(errorMessage);

                    paymentReceiptClaimed?.Invoke(null);

                    UnigramPaymentLogger.LogError($"Failed to retrieve customer transaction data, reason: {errorMessage}, status code: {code}");

                    yield break;
                }
            }
        }

        private static void RefreshTokenIfSessionExpired(string errorMessage)
        {
            if (errorMessage == WebRequestUtils.ERROR_JWT_SESSION_EXPIRED)
            {
                UnigramPaymentSDK.Instance.RefreshToken();

                UnigramPaymentLogger.LogError("The session token has expired.");
            }
        }

        private static bool IsExistServerLink()
        {
            if (!IsExistProjectConfig())
            {
                return false;
            }

            var config = RuntimeAPIConfig.Load();

            if (string.IsNullOrEmpty(config.ServerUrl))
            {
                UnigramPaymentLogger.LogError("No link to a valid api server was found, please fill in the required data and try again later.");

                return false;
            }

            return true;
        }

        private static bool IsExisClientSecretKey()
        {
            if (!IsExistProjectConfig())
            {
                return false;
            }

            var config = RuntimeAPIConfig.Load();

            if (string.IsNullOrEmpty(config.ClientSecretKey))
            {
                UnigramPaymentLogger.LogError("No client secret key value was found for api server requests," +
                    " fill in the required data and try again later.");

                return false;
            }

            return true;
        }

        private static bool IsExistProjectConfig()
        {
            var config = RuntimeAPIConfig.Load();

            if (config == null)
            {
                UnigramPaymentLogger.LogError("API server configuration is not detected, " +
                    "to configure it go to the 'Unigram Payment -> API Config' window and fill in the required data.");

                return false;
            }

            return true;
        }
    }
}
