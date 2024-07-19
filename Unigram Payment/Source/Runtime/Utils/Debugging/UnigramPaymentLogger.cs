using UnityEngine;
using UnigramPayment.Runtime.Core;

namespace UnigramPayment.Runtime.Utils.Debugging
{
    public sealed class UnigramPaymentLogger
    {
        private static bool IsEnabled => UnigramPaymentSDK.Instance != null && 
            UnigramPaymentSDK.Instance.IsDebugMode;

        public const string PREFIX = "Unigram Payment";

        public static void Log(object message)
        {
            if (IsEnabled)
            {
                Debug.Log($"[{PREFIX}] {message}");
            }
        }

        public static void LogWarning(object message)
        {
            if (IsEnabled)
            {
                Debug.LogWarning($"[{PREFIX}] {message}");
            }
        }

        public static void LogError(object message)
        {
            if (IsEnabled)
            {
                Debug.LogError($"[{PREFIX}] {message}");
            }
        }
    }
}