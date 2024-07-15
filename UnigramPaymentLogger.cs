using System;
using UnityEngine;
using UnigramPayment.Runtime.Core;

namespace UnigramPayment.Utils.Debugging
{
    public sealed class UnigramPaymentLogger : MonoBehaviour
    {
        private static bool IsEnabled => UnigramPaymentSDK.Instance != null && UnigramPaymentSDK.Instance.IsDebugMode;

        public const string PREFIX = "[Unigram Payment] ";

        public static void Log(object message)
        {
            if (IsEnabled)
            {
                Debug.Log(PREFIX + message);
            }
        }

        public static void LogWarning(object message)
        {
            if (IsEnabled)
            {
                Debug.LogWarning(PREFIX + message);
            }
        }

        public static void LogError(object message)
        {
            if (IsEnabled)
            {
                Debug.LogError(PREFIX + message);
            }
        }

        public static void LogException(Exception exception)
        {
            if (IsEnabled)
            {
                Debug.LogException(exception);
            }
        }
    }
}