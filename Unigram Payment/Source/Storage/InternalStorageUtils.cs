using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnigramPayment.Storage.Utils
{
    public sealed class InternalStorageUtils
    {
#if UNITY_EDITOR
        public sealed class Target<T> where T: ScriptableObject
        {
            public static T CreateInstance()
            {
                return ScriptableObject.CreateInstance<T>();
            }

            public static T LoadAssetByPath(string folder, string path)
            {
                Directory.CreateDirectory(folder);

                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
        }

        public static void CreateAsset(Object asset, string path)
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        public static void SaveAsync<T>(T item) where T : ScriptableObject
        {
            EditorUtility.SetDirty(item);
        }
#endif
    }
}