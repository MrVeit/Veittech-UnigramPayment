using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnigramPayment.Runtime.Core;
using UnigramPayment.Runtime.Data;
using UnigramPayment.Runtime.Common;
using UnigramPayment.Runtime.Utils;
using UnigramPayment.Runtime.Utils.Debugging;
using UnigramPayment.Storage.Data;

namespace UnigramPayment.Core
{
    internal static class BotAPIBridge
    {
        private const string POST_RESPONSE = UnityWebRequest.kHttpVerbPOST;

        private const string HEADER_CONTENT_TYPE = WebRequestUtils.HEADER_CONTENT_TYPE;
        private const string HEADER_AUTHORIZATION = WebRequestUtils.HEADER_AUTHORIZATION;
        private const string HEADER_VALUE_APPLICATION_JSON = WebRequestUtils.HEADER_VALUE_APPLICATION_JSON;
        private const string HEADER_VALUE_TEXT_PLAIN = WebRequestUtils.HEADER_VALUE_TEXT_PLAIN;

        private static readonly RuntimeAPIConfig RUNTIME_STORAGE = RuntimeAPIConfig.Load();

        private static readonly string API_SECRET_KEY = RUNTIME_STORAGE.ClientSecretKey;
        private static readonly string API_SERVER_LINK = RUNTIME_STORAGE.ServerUrl;

        internal static IEnumerator GetTime(Action<ServerTimeData> unixTickClaimed)
        {
            if (!IsExistServerLink())
            {
                unixTickClaimed?.Invoke(null);

                yield break;
            }

            var url = APIServerRequests.GetServerTimeLink(API_SERVER_LINK);

            using (UnityWebRequest request = new(url, UnityWebRequest.kHttpVerbGET))
            {
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;
                    var timeResponse = JsonConvert.DeserializeObject<ServerTimeData>(responseResult);

                    unixTickClaimed?.Invoke(timeResponse);

                    UnigramPaymentLogger.Log($"Server tick successfully claimed: {timeResponse.UnixTick}");

                    yield break;
                }

                var errorMessage = WebRequestUtils.ParseErrorReasonFromResponse(request);
                var code = request.responseCode;

                unixTickClaimed?.Invoke(null);

                UnigramPaymentLogger.LogError($"Failed to retrieve customer " +
                    $"transaction data, reason: {errorMessage}, status code: {code}");

                yield break;
            }
        }

        internal static IEnumerator AuthorizeClient(
            Action<string> authorizationTokenClaimed)
        {
            if (!IsExistProjectConfig())
            {
                authorizationTokenClaimed?.Invoke(null);

                yield break;
            }

            if (!IsExistClientSecretKey())
            {
                authorizationTokenClaimed?.Invoke(null);

                yield break;
            }

            if (!IsExistServerLink())
            {
                authorizationTokenClaimed?.Invoke(null);

                yield break;
            }

            var authorizationLink = APIServerRequests.GetAuthorizationLink(API_SERVER_LINK);

            using (UnityWebRequest request = new(authorizationLink, POST_RESPONSE))
            {
                var bodyRaw = WebRequestUtils.GetBytesFromJsonUTF8(API_SECRET_KEY);

                WebRequestUtils.SetUploadHandler(request,
                    WebRequestUtils.GetUploadHandlerRaw(bodyRaw));
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                WebRequestUtils.SetRequestHeader(request,
                    HEADER_CONTENT_TYPE, HEADER_VALUE_TEXT_PLAIN);

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;

                    var authToken = JsonConvert.DeserializeObject<AuthorizationTokenData>(responseResult);

                    authorizationTokenClaimed?.Invoke(authToken.Token);

                    UnigramPaymentLogger.Log($"Received JWT Token from API " +
                        $"server: {JsonConvert.SerializeObject(authToken)}");
                    UnigramPaymentLogger.Log("The client is authorized on the " +
                        "server, connection to the api server was successful.");

                    yield break;
                }

                UnigramPaymentLogger.LogError($"Failed to authorize " +
                    $"client on api server with message: {request.error}");

                authorizationTokenClaimed?.Invoke(null);

                yield break;
            }
        }

        internal static IEnumerator CreateInvoice(
            SaleableItem product, Action<string> invoiceLinkCreated, 
            Action<ErrorTypes> invoiceLinkCreateFailed)
        {
            if (!IsExistServerLink())
            {
                invoiceLinkCreated?.Invoke(null);

                yield break;
            }

            if (!IsExistSessionToken())
            {
                invoiceLinkCreated?.Invoke(null);

                yield break;
            }

            string url = APIServerRequests.GetInvoiceLink(API_SERVER_LINK);

            var invoice = new InvoiceData()
            {
                Payload = product.Id,
                Title = product.Name,
                Description = product.Description,
                Amount = product.Price
            };

            string jsonPayload = JsonConvert.SerializeObject(invoice);

            UnigramPaymentLogger.Log($"Product data for purchase: {jsonPayload}");

            using (UnityWebRequest request = new(url, POST_RESPONSE))
            {
                var bodyRaw = WebRequestUtils.GetBytesFromJsonUTF8(jsonPayload);

                WebRequestUtils.SetUploadHandler(request, 
                    WebRequestUtils.GetUploadHandlerRaw(bodyRaw));
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                WebRequestUtils.SetRequestHeader(request, 
                    HEADER_CONTENT_TYPE, HEADER_VALUE_APPLICATION_JSON);
                WebRequestUtils.SetRequestHeader(request, HEADER_AUTHORIZATION,
                    WebRequestUtils.GetAuthorizationHeaderValue(GetSessionToken()));

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;

                    var invoiceLink = JsonConvert.DeserializeObject<GeneratedInvoiceData>(responseResult).Url;

                    invoiceLinkCreated?.Invoke(invoiceLink);

                    UnigramPaymentLogger.Log($"The invoice for the purchase" +
                        $" of stars has been successfully created: {invoiceLink}");

                    yield break;
                }

                var code = request.responseCode;
                var errorMessage = WebRequestUtils.ParseErrorReasonFromResponse(request);

                UnigramPaymentLogger.LogError($"Failed to create invoice link, " +
                    $"reason: {errorMessage}, request code: {request.responseCode}");

                var isExpired = UpdateSessionIfExpired(errorMessage, invoiceLinkCreateFailed);

                if (!isExpired)
                {
                    invoiceLinkCreated?.Invoke(null);
                }

                yield break;
            }
        }

        internal static IEnumerator RefundPayment(long buyerId, string transactionId, 
            Action<bool> refundProcessFinished, Action<ErrorTypes> refundProcessFailed)
        {
            if (!IsExistServerLink())
            {
                refundProcessFinished?.Invoke(false);

                yield break;
            }

            if (!IsExistSessionToken())
            {
                refundProcessFinished?.Invoke(false);

                yield break;
            }

            var url = APIServerRequests.GetRefundStarsLink(API_SERVER_LINK);

            var refundData = new RefundProcessData()
            {
                UserId = buyerId,
                TransactionId = transactionId,
            };

            string jsonRefund = JsonConvert.SerializeObject(refundData);

            using (UnityWebRequest request = new(url, POST_RESPONSE))
            {
                var bodyRaw = WebRequestUtils.GetBytesFromJsonUTF8(jsonRefund);

                WebRequestUtils.SetUploadHandler(request,
                    WebRequestUtils.GetUploadHandlerRaw(bodyRaw));
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                WebRequestUtils.SetRequestHeader(request, 
                    HEADER_CONTENT_TYPE, HEADER_VALUE_APPLICATION_JSON);
                WebRequestUtils.SetRequestHeader(request, HEADER_AUTHORIZATION,
                    WebRequestUtils.GetAuthorizationHeaderValue(GetSessionToken()));

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;

                    refundProcessFinished?.Invoke(true);

                    UnigramPaymentLogger.Log($"The stars previously purchased by" +
                        $"the user were successfully returned with status: {responseResult}");

                    yield break;
                }

                var code = request.responseCode;
                var errorMessage = WebRequestUtils.ParseErrorReasonFromResponse(request);

                UnigramPaymentLogger.LogError($"Failed to refund stars previously " +
                    $"purchased by the user due to the following reason: " +
                    $"{errorMessage}, response code: {code}");

                var isExpired = UpdateSessionIfExpired(errorMessage, refundProcessFailed);

                if (!isExpired)
                {
                    refundProcessFinished?.Invoke(false);
                }

                yield break;
            }
        }

        internal static IEnumerator GetPaymentReceipt(float delay, string userId, string itemId, 
            Action<PaymentReceiptData> paymentReceiptClaimed, Action<ErrorTypes> paymentReceiptLoadFailed)
        {
            if (!IsExistServerLink())
            {
                paymentReceiptClaimed?.Invoke(null);

                yield break;
            }

            if (!IsExistSessionToken())
            {
                paymentReceiptClaimed?.Invoke(null);

                yield break;
            }

            UnigramPaymentLogger.Log($"Running a delay `{delay}s` to " +
                $"successfully receive a payment check from the Telegram API");

            yield return new WaitForSeconds(delay);

            UnigramPaymentLogger.Log($"Receipt validation " +
                $"sended for item {itemId} by user {userId}");

            var url = APIServerRequests.GetPaymentReceiptLink(API_SERVER_LINK);

            var invoiceData = new BuyerInvoiceData()
            {
                TelegramId = userId,
                PurchasedItemId = itemId,
            };

            string jsonPayload = JsonConvert.SerializeObject(invoiceData);

            UnigramPaymentLogger.Log($"Product data for confirm purchase: {jsonPayload}");

            using (UnityWebRequest request = new(url, UnityWebRequest.kHttpVerbPOST))
            {
                var bodyRaw = WebRequestUtils.GetBytesFromJsonUTF8(jsonPayload);

                WebRequestUtils.SetUploadHandler(request,
                    WebRequestUtils.GetUploadHandlerRaw(bodyRaw));
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                WebRequestUtils.SetRequestHeader(request, 
                    HEADER_CONTENT_TYPE, HEADER_VALUE_APPLICATION_JSON);
                WebRequestUtils.SetRequestHeader(request, HEADER_AUTHORIZATION,
                    WebRequestUtils.GetAuthorizationHeaderValue(GetSessionToken()));

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;
                    var receipt = JsonConvert.DeserializeObject<PaymentReceiptData>(responseResult);

                    UnigramPaymentLogger.Log($"Customer transaction data " +
                        $"{responseResult} has been successfully uploaded.");

                    paymentReceiptClaimed?.Invoke(receipt);

                    yield break;
                }

                var code = request.responseCode;
                var errorResponse = request.downloadHandler.text;
                var errorMessage = WebRequestUtils.ParseErrorReasonFromResponse(request);

                UnigramPaymentLogger.LogError($"Failed to retrieve customer " +
                    $"transaction data, reason: {errorMessage}, status code: {code}");

                var errorData = JsonConvert.DeserializeObject<FailedResponseData>(errorResponse);
                var isExpired = UpdateSessionIfExpired(errorMessage, paymentReceiptLoadFailed);

                ResendFetchPurchaseReceiptIfNotFound(errorMessage, paymentReceiptLoadFailed);

                if (!isExpired)
                {
                    paymentReceiptClaimed?.Invoke(null);
                }

                yield break;
            }
        }

        internal static IEnumerator GetPurchaseHistory(long amount, long totalPass, 
            Action<PurchaseHistoryData> purchaseHistoryClaimed, Action<ErrorTypes> purchaseHistoryLoadFailed)
        {
            var transactionHistoryPending = GetTransactionHistory<PurchaseHistoryData>(
                amount, totalPass, APIServerRequests.GetPurchaseHistoryLink(
                API_SERVER_LINK), (purchaseHistory) =>
            {
                purchaseHistoryClaimed?.Invoke(purchaseHistory);
            },
            (errorReason) =>
            {
                purchaseHistoryLoadFailed?.Invoke(errorReason);
            });

            return transactionHistoryPending;
        }

        internal static IEnumerator GetRefundHistory(long amount, long totalPass, 
            Action<RefundHistoryData> refundHistoryClaimed, Action<ErrorTypes> refundHistoryLoadFailed)
        {
            var refundHistoryPending = GetTransactionHistory<RefundHistoryData>(
                amount, totalPass, APIServerRequests.GetRefundHistoryLink(
                API_SERVER_LINK), (refundHistory) =>
            {
                refundHistoryClaimed?.Invoke(refundHistory);
            },
            (errorReason) =>
            {
                refundHistoryLoadFailed?.Invoke(errorReason);
            });

            return refundHistoryPending;
        }

        private static IEnumerator GetTransactionHistory<T>(long amount, long totalPass, string apiUrl,
            Action<T> historyClaimed, Action<ErrorTypes> historyLoadFailed) where T : class
        {
            if (!IsExistServerLink())
            {
                yield break;
            }

            if (!IsExistSessionToken())
            {
                yield break;
            }

            var url = apiUrl;

            var historyPayload = new TransactionHistoryPayloadData()
            {
                Amount = amount,
                TotalPass = totalPass,
            };

            string jsonPayload = JsonConvert.SerializeObject(historyPayload);

            using (UnityWebRequest request = new(url, UnityWebRequest.kHttpVerbGET))
            {
                UnigramPaymentLogger.Log(apiUrl);

                var bodyRaw = WebRequestUtils.GetBytesFromJsonUTF8(jsonPayload);

                WebRequestUtils.SetUploadHandler(request,
                    WebRequestUtils.GetUploadHandlerRaw(bodyRaw));
                WebRequestUtils.SetDownloadHandler(request, new DownloadHandlerBuffer());

                WebRequestUtils.SetRequestHeader(request, 
                    HEADER_CONTENT_TYPE, HEADER_VALUE_APPLICATION_JSON);
                WebRequestUtils.SetRequestHeader(request, HEADER_AUTHORIZATION,
                    WebRequestUtils.GetAuthorizationHeaderValue(GetSessionToken()));

                yield return request.SendWebRequest();

                if (request.result == WebRequestUtils.SUCCESS)
                {
                    var responseResult = request.downloadHandler.text;
                    var historyData = JsonConvert.DeserializeObject<T>(responseResult);

                    historyClaimed?.Invoke(historyData);

                    yield break;
                }

                var code = request.responseCode;
                var errorMessage = WebRequestUtils.ParseErrorReasonFromResponse(request);

                UnigramPaymentLogger.LogError($"Failed to retrieve " +
                    $"transaction history, reason: {errorMessage}, status code: {code}");

                var isExpired = UpdateSessionIfExpired(errorMessage, historyLoadFailed);

                if (!isExpired)
                {
                    historyClaimed?.Invoke(null);
                }

                yield break;
            }
        }

        private static void ResendFetchPurchaseReceiptIfNotFound(
            string errorMessage, Action<ErrorTypes> resendResponseActivated)
        {
            if (errorMessage != WebRequestUtils.ERROR_TRANSACTION_RECEIPT_NOT_FOUND)
            {
                return;
            }

            UnigramPaymentLogger.LogError("Target transaction not " +
                "found, please resend response after delay.");

            resendResponseActivated?.Invoke(ErrorTypes.HistoryNotFound);
        }

        private static bool UpdateSessionIfExpired(
            string errorMessage, Action<ErrorTypes> errorCallback)
        {
            var isExpired = errorMessage == WebRequestUtils.ERROR_JWT_SESSION_EXPIRED;

            if (!isExpired)
            {
                return false;
            }

            UnigramPaymentLogger.LogWarning("Client session is expired, try to update...");

            UnigramPaymentSDK.Instance.RefreshToken();

            errorCallback?.Invoke(ErrorTypes.SessionExpired);

            return isExpired;
        }

        private static string GetSessionToken()
        {
            string token = UnigramPaymentSDK.Instance.JwtToken;

            if (string.IsNullOrEmpty(token))
            {
                UnigramPaymentLogger.LogError("Session token not exist, something wrong");

                return null;
            }

            UnigramPaymentLogger.Log($"Session token available, value: {token}");

            return token;
        }

        private static bool IsExistSessionToken()
        {
            return !string.IsNullOrEmpty(GetSessionToken());
        }

        private static bool IsExistServerLink()
        {
            if (!IsExistProjectConfig())
            {
                return false;
            }

            if (string.IsNullOrEmpty(API_SERVER_LINK))
            {
                UnigramPaymentLogger.LogError("No link to a valid api server " +
                    "was found, please fill in the required data and try again later.");

                return false;
            }

            return true;
        }

        private static bool IsExistClientSecretKey()
        {
            if (!IsExistProjectConfig())
            {
                return false;
            }

            if (string.IsNullOrEmpty(API_SECRET_KEY))
            {
                UnigramPaymentLogger.LogError("No client secret key value " +
                    "was found for api server requests, fill in the required data and try again later.");

                return false;
            }

            return true;
        }

        private static bool IsExistProjectConfig()
        {
            if (RUNTIME_STORAGE == null)
            {
                UnigramPaymentLogger.LogError("API server configuration is not detected, " +
                    "to configure it go to the 'Unigram Payment -> API Config' window and fill in the required data.");

                return false;
            }

            return true;
        }
    }
}
