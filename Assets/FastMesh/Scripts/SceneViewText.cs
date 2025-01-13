using UnityEngine;
using UnityEditor;

namespace FastMesh_Example
{
    [ExecuteInEditMode]
    public class SceneViewText : MonoBehaviour
    {
        public bool isShow = true;
        string text2 = "These 3D models, all created with \"Fast Mesh - 3D Asset Creation Tool\" (click)";
        Color backgroundColor = Color.white;
        Color textColor = Color.black;

#if UNITY_EDITOR
        private void OnEnable()
        {
            // SceneView iþlemleri yalnýzca Editor'de geçerli
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            // Editor'de SceneView event'ini kaldýrýyoruz
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (isShow == false) return;

            // Editor GUI iþlemleri burada yapýlacak
            Handles.BeginGUI();
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 20;
            style.normal.textColor = textColor;
            style.wordWrap = true;

            float width = 420f;
            float height = 50f;
            float x = (sceneView.position.width - width) / 2f;
            float y = 10f;

            GUI.color = backgroundColor;
            GUI.DrawTexture(new Rect(x - 10, y - 10, width + 20, height + 20), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (GUI.Button(new Rect(x, y, width, height), text2, style))
            {
#if UNITY_WEBGL
                    Debug.Log("WebGL doesn't support Application.OpenURL. Please open it manually.");
#else
                    Application.OpenURL("https://assetstore.unity.com/packages/slug/288711");
#endif
            }

            Handles.EndGUI();
        }
#endif
    }
}
