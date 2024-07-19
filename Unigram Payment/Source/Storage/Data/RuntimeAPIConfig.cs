using UnityEngine;
using UnigramPayment.Storage.Common;
using UnigramPayment.Storage.Utils;

namespace UnigramPayment.Storage.Data
{
    public sealed class RuntimeAPIConfig : ScriptableObject
    {
#if UNITY_EDITOR
        private static RuntimeAPIConfig _instance;

        public static RuntimeAPIConfig Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                _instance = InternalStorageUtils.Target<RuntimeAPIConfig>.LoadAssetByPath(FOLDER_PATH, FULL_PATH);

                if (_instance)
                {
                    return _instance;
                }

                _instance = InternalStorageUtils.Target<RuntimeAPIConfig>.CreateInstance();

                InternalStorageUtils.CreateAsset(_instance, FULL_PATH);

                return _instance;
            }
        }
#endif

        [field: SerializeField, Space] public string ClientSecretKey { get; set; }
        [field: SerializeField, Space] public string ServerUrl { get; set; }

        private const string FILE_NAME = EditorConsts.RUNTIME_API_CONFIG_NAME;
        private const string FOLDER_PATH = EditorConsts.RUNTIME_API_CONFIG_FULL_PATH;
        private const string FULL_PATH = FOLDER_PATH + "/" + FILE_NAME;

#if UNITY_EDITOR
        public static void SaveAsync()
        {
            InternalStorageUtils.SaveAsync(_instance);
        }
#endif

        public static RuntimeAPIConfig Load()
        {
            return Resources.Load<RuntimeAPIConfig>(
                $"{EditorConsts.RUNTIME_API_CONFIG_SHORT_PATH}{EditorConsts.RUNTIME_API_CONFIG_NAME_WITHOUT_FORMAT}");
        }
    }
}