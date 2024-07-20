using UnityEngine;
using UnityEditor;
using UnigramPayment.Storage.Common;
using UnigramPayment.Editor.ConfigWindow;

namespace UnigramPayment.Editor.Common
{
    public sealed class TopBarMenu : ScriptableObject
    {
        [MenuItem("Unigram Payment/API Config", false, 0)]
        public static void OpenProductsStorage()
        {
            APIConfigWindow.Open();
        }

        [MenuItem("Unigram Payment/Documentation", false, 1)]
        public static void OpenDocumentation()
        {
            Application.OpenURL(EditorConsts.DOCUMENTATION_URL);
        }

        [MenuItem("Unigram Payment/Telegram Bot", false, 2)]
        public static void OpenTestBot()
        {
            Application.OpenURL(EditorConsts.TELEGRAM_BOT_URL);
        }
    }
}