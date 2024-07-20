using UnityEngine;
using UnigramPayment.Storage.Common;
using UnigramPayment.Storage.Utils;

namespace UnigramPayment.Storage.Data
{
    public sealed class APIConfig : ScriptableObject
    {
#if UNITY_EDITOR
        private static APIConfig _instance;

        public static APIConfig Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                _instance = InternalStorageUtils.Target<APIConfig>.LoadAssetByPath(FOLDER_PATH, FULL_PATH);

                if (_instance)
                {
                    return _instance;
                }

                _instance = InternalStorageUtils.Target<APIConfig>.CreateInstance();

                _instance._clientSecretKey = EditorConsts.BASE_CLIENT_SECRET_KEY;
                _instance._serverUrl = EditorConsts.BASE_API_URL;

                InternalStorageUtils.CreateAsset(_instance, FULL_PATH);

                return _instance;
            }
        }
#endif

        [SerializeField, Space] private string _clientSecretKey;
        [SerializeField, Space] private string _serverUrl;

        private const string FILE_NAME = EditorConsts.EDITOR_API_CONFIG_FILE_NAME;
        private const string FOLDER_PATH = EditorConsts.EDITOR_API_CONFIG_FULL_PATH;
        private const string FULL_PATH = FOLDER_PATH + "/" + FILE_NAME;

        public string SecretKey
        {
            get => _clientSecretKey;
            set => _clientSecretKey = value;
        }

        public string ServerUrl
        {
            get => _serverUrl;
            set => _serverUrl = value;
        }

#if UNITY_EDITOR
        public static void SaveAsync()
        {
            InternalStorageUtils.SaveAsync(_instance);
        }
#endif
    }
}