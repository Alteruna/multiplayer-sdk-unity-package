using Alteruna.Multiplayer.Unity.InputSynchronizable;
using UnityEditor;
using UnityEngine;

namespace Alteruna.UnityEditor
{
    [CustomPropertyDrawer(typeof(SyncedAxis))]
    public class SyncedAxisDrawer : PropertyDrawer
    {
        private const float Margin = 2f;
        private const float LeftMargin = 10f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty axisProp = property.FindPropertyRelative("_axis");

            // Add label modifications here as you did earlier
            
            if (axisProp.stringValue != "")
            {
                label = new GUIContent(label.text + " - " + axisProp.stringValue, label.tooltip);
            }

            EditorGUI.BeginProperty(position, label, property);

            bool isExpandedBefore = property.isExpanded;
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);
            bool isChanged = isExpandedBefore != property.isExpanded;

            if (property.isExpanded)
            {
                position.y += EditorGUIUtility.singleLineHeight + Margin;
                
                Rect axisRect = new Rect(position.x + LeftMargin, position.y, position.width - LeftMargin, EditorGUIUtility.singleLineHeight);
                
                EditorGUI.BeginChangeCheck(); // Check for changes
                EditorGUI.PropertyField(axisRect, axisProp, new GUIContent("Axis", "Uses the same input as Input.GetAxis."));
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = 1;

            if (property.isExpanded)
            {
                lines += 1; // Add a line for the axis field
            }

            return EditorGUIUtility.singleLineHeight * lines + (lines - 1) * Margin;
        }
    }
}
