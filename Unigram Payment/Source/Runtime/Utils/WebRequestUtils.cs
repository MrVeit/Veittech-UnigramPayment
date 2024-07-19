using System.Text;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnigramPayment.Runtime.Data;
using UnigramPayment.Runtime.Utils.Debugging;

namespace UnigramPayment.Runtime.Utils
{
    public sealed class WebRequestUtils
    {
        public const UnityWebRequest.Result SUCCESS = UnityWebRequest.Result.Success;
        public const UnityWebRequest.Result IN_PROGRESS = UnityWebRequest.Result.InProgress;
        public const UnityWebRequest.Result CONNECTION_ERROR = UnityWebRequest.Result.ConnectionError;
        public const UnityWebRequest.Result PROTOCOL_ERROR = UnityWebRequest.Result.ProtocolError;
        public const UnityWebRequest.Result DATA_PROCESSING_ERROR = UnityWebRequest.Result.DataProcessingError;

        public const string HEADER_CONTENT_TYPE = "Content-Type";
        public const string HEADER_AUTHORIZATION = "Authorization";

        public const string HEADER_VALUE_APPLICATION_JSON = "application/json";
        public const string HEADER_VALUE_TEXT_PLAIN = "text/plain";

        public const string ERROR_JWT_SESSION_EXPIRED = "Unauthorized client, access denied.";

        public static void SetRequestHeader(UnityWebRequest webRequest,
            string header, string headerValue)
        {
            webRequest.SetRequestHeader(header, headerValue);
        }

        public static void SetUploadHandler(UnityWebRequest webRequest,
            UploadHandlerRaw uploadHandler)
        {
            webRequest.uploadHandler = uploadHandler;
        }

        public static void SetDownloadHandler(UnityWebRequest webRequest,
            DownloadHandler downloadHandler)
        {
            webRequest.downloadHandler = downloadHandler;
        }

        public static UploadHandlerRaw GetUploadHandlerRaw(byte[] uploadData)
        {
            return new UploadHandlerRaw(uploadData);
        }

        public static byte[] GetBytesFromJsonUTF8(string json)
        {
            return Encoding.UTF8.GetBytes(json);
        }

        public static string GetAuthorizationHeaderValue(string token)
        {
            return $"Bearer {token}";
        }

        public static string ParseErrorReasonFromResponse(UnityWebRequest request)
        {
            string message = string.Empty;

            if (string.IsNullOrEmpty(request.downloadHandler.text))
            {
                UnigramPaymentLogger.LogError($"Failed to load the response from the server for response: {request.url}");

                return message;
            }

            message = JsonConvert.DeserializeObject<FailedResponseData>(request.downloadHandler.text).ErrorMessage;

            return message;
        }

        public static string ParseTelegramId(string providerPaymentChargeId)
        {
            int underscoreId = providerPaymentChargeId.IndexOf('_');

            if (underscoreId >= 0)
            {
                return providerPaymentChargeId.Substring(0, underscoreId);
            }

            UnigramPaymentLogger.LogWarning("Telegram Id could not be detected");

            return providerPaymentChargeId;
        }
    }
}