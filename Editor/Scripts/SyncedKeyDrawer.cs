using Alteruna.Multiplayer.Unity.InputSynchronizable;
using UnityEditor;
using UnityEngine;

namespace Alteruna.UnityEditor
{
	[CustomPropertyDrawer(typeof(SyncedKey))]
	public class SyncedKeyDrawer : PropertyDrawer
	{
		private const float Margin = 2f;
		private const float LeftMargin = 10f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty keyProp = property.FindPropertyRelative("_key");
			SerializedProperty modeProp = property.FindPropertyRelative("mode");

			if (keyProp.enumValueIndex != (int)KeyCode.None)
			{
				label = modeProp.enumValueIndex == (int)SyncedKey.KeyMode.KeyPress ? new GUIContent(label.text + " - " + keyProp.enumDisplayNames[keyProp.enumValueIndex], label.tooltip) : new GUIContent(label.text + " - " + keyProp.enumDisplayNames[keyProp.enumValueIndex] + " (" + modeProp.enumDisplayNames[modeProp.enumValueIndex] + ")", label.tooltip);
			}

			// Add label modifications here as you did earlier

			EditorGUI.BeginProperty(position, label, property);

			bool isExpandedBefore = property.isExpanded;
			property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);
			bool isChanged = isExpandedBefore != property.isExpanded;

			if (property.isExpanded)
			{
				position.y += EditorGUIUtility.singleLineHeight + Margin;

				float keyWidth = (position.width - LeftMargin) * 0.7f;
				float modeWidth = position.width - LeftMargin - keyWidth;

				Rect keyRect = new Rect(position.x + LeftMargin, position.y, keyWidth, EditorGUIUtility.singleLineHeight);
				Rect modeRect = new Rect(position.x + LeftMargin + keyWidth, position.y, modeWidth, EditorGUIUtility.singleLineHeight);

				EditorGUI.PropertyField(keyRect, keyProp, new GUIContent("Input", "Keycode and mode to use as input."));
				EditorGUI.PropertyField(modeRect, modeProp, GUIContent.none);

				SyncedKey.KeyMode mode = (SyncedKey.KeyMode)modeProp.enumValueIndex;
				if (mode == SyncedKey.KeyMode.DoubleTap || mode == SyncedKey.KeyMode.ToggleDoubleTap)
				{
					SerializedProperty doubleTapTimeProp = property.FindPropertyRelative("DoubleTapTime");
					position.y += EditorGUIUtility.singleLineHeight + Margin;
					EditorGUI.PropertyField(new Rect(position.x + LeftMargin, position.y, position.width - LeftMargin, EditorGUIUtility.singleLineHeight), doubleTapTimeProp);
				}

				SerializedProperty onInputChangedProp = property.FindPropertyRelative("OnInputChanged");
				position.y += EditorGUIUtility.singleLineHeight + Margin;
				EditorGUI.PropertyField(new Rect(position.x + LeftMargin, position.y, position.width, position.height), onInputChangedProp);
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty modeProp = property.FindPropertyRelative("mode");
			SyncedKey.KeyMode mode = (SyncedKey.KeyMode)modeProp.enumValueIndex;
			int lines = 1;
			float height = 0;

			if (property.isExpanded)
			{
				lines += 1; // Add a line for the key and mode fields

				if (mode == SyncedKey.KeyMode.DoubleTap || mode == SyncedKey.KeyMode.ToggleDoubleTap)
				{
					lines += 1; // Add an extra line for DoubleTapTime
				}

				SerializedProperty onInputChangedProp = property.FindPropertyRelative("OnInputChanged");
				height = EditorGUI.GetPropertyHeight(onInputChangedProp);
			}

			return EditorGUIUtility.singleLineHeight * lines + (lines - 1) * Margin + height;
		}
	}
}