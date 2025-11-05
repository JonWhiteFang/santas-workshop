#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SantasWorkshop.Utilities;

namespace SantasWorkshop.Editor
{
    /// <summary>
    /// Custom property drawer for ReadOnlyAttribute.
    /// Displays the field in the Inspector but disables editing.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
#endif
