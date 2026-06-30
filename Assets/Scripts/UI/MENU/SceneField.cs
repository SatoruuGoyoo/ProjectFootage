using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneField
{
    [SerializeField] private Object sceneAsset;
    [SerializeField] private string sceneName = "";

    public string SceneName => sceneName;

    public static implicit operator string(SceneField sf) => sf.sceneName;

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var sceneAssetProp = property.FindPropertyRelative("sceneAsset");
            var sceneNameProp = property.FindPropertyRelative("sceneName");

            position = EditorGUI.PrefixLabel(position, label);

            var newScene = EditorGUI.ObjectField(position, sceneAssetProp.objectReferenceValue, typeof(SceneAsset), false);

            if (newScene != sceneAssetProp.objectReferenceValue)
            {
                sceneAssetProp.objectReferenceValue = newScene;
                sceneNameProp.stringValue = newScene != null ? newScene.name : "";
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}