using UnityEngine;
using UnityEditor;
using UnigramPayment.Storage.Data;

namespace UnigramPayment.Editor.ConfigWindow
{
    public sealed class APIConfigWindow : EditorWindow
    {
        private APIConfig _editorStorage => APIConfig.Instance;

        private string _editorSecretKey
        {
            get => APIConfig.Instance.SecretKey;
            set => APIConfig.Instance.SecretKey = value;
        }

        private string _editorApiUrl
        {
            get => APIConfig.Instance.ServerUrl;
            set => APIConfig.Instance.ServerUrl = value;
        }

        private void OnDestroy()
        {
            var runtimeStorage = RuntimeAPIConfig.Instance;

            UpdateRuntimeStorage();

            APIConfig.SaveAsync();
            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            GUILayout.Space(5);

            LaberHeaderField("PROJECT API CONFIG");

            GUILayout.BeginHorizontal("box",
                GUILayout.Width(position.width), GUILayout.Height(position.height));

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Client Secret Key:", GUILayout.Width(200));
            _editorSecretKey = GUILayout.TextField(_editorSecretKey, 30, GUILayout.MaxWidth(position.width - 15));

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Server Url:", GUILayout.Width(200));
            _editorApiUrl = GUILayout.TextField(_editorApiUrl, GUILayout.MaxWidth(position.width - 15));

            GUILayout.Space(5);

            EditorGUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        public static void Open()
        {
            var config = GetWindow(typeof(APIConfigWindow));

            config.titleContent = new GUIContent("Project API Config");
            config.minSize = new Vector2(500, 150);
            config.maxSize = new Vector2(750, 150);

            config.Show();
        }

        private static void LaberHeaderField(string headerName)
        {
            EditorGUILayout.LabelField(headerName, new GUIStyle(EditorStyles.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,

                normal = new GUIStyleState()
                {
                    textColor = new Color(0.47f, 0.9f, 0.9f)
                },

                alignment = TextAnchor.MiddleCenter

            }, GUILayout.Height(20));

            HorizontalLine(new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    background = EditorGUIUtility.whiteTexture
                },
                margin = new RectOffset(0, 0, 10, 5),
                fixedHeight = 2
            });
        }

        private static string TextRow(string fieldTitle, string text, GUILayoutOption laberWidth,
            GUILayoutOption textFieldWidthOption = null)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(fieldTitle), laberWidth);

            text = textFieldWidthOption == null
                ? GUILayout.TextField(text) :
                GUILayout.TextField(text, textFieldWidthOption);

            GUILayout.EndHorizontal();

            return text;
        }

        private static void HorizontalLine(GUIStyle lineStyle)
        {
            var color = GUI.color;

            GUI.color = Color.grey;
            GUILayout.Box(GUIContent.none, lineStyle);
            GUI.color = color;
        }

        private void UpdateRuntimeStorage()
        {
            var storage = RuntimeAPIConfig.Load();

            storage.ClientSecretKey = _editorSecretKey;
            storage.ServerUrl = _editorApiUrl;

            RuntimeAPIConfig.SaveAsync();

            Debug.Log("[Unigram Payment] The API configuration of the project has been successfully saved!");
        }
    }
}