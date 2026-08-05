using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Unity;
using UnityEditor;
using UnityEngine;

namespace Alteruna.UnityEditor
{
	[CustomEditor(typeof(TextChatSynchronizable))]
	public class TextChatSynchronizableEditor : UniqueIDEditor
	{
		private bool _commandSettings;

		public override void OnInspectorGUI()
		{
			DrawUID();

			serializedObject.Update();

			var textChat = (TextChatSynchronizable)target;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("chatBuffer"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("UseTimeStamps"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("LogLocalOnSend"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("LogSystemMessages"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("LogErrors"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("LogFullCommandErrors"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("LogChatInDebugLog"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("UseRichText"));

			GUI.enabled = textChat.UseRichText;
			textChat.BoldNames = EditorGUILayout.Toggle("Bold Names", textChat.BoldNames);
			GUI.enabled = true;


			_commandSettings = EditorGUILayout.Foldout(_commandSettings, "Command Settings", true);
			if (_commandSettings)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowCommands"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableCheatsInDevEnvironments"));
				GUI.enabled = textChat.AllowCommands;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowCheats"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowHostToToggleCheats"));
				GUI.enabled = true;
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("InputField"));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("SetInputActiveAction"));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("TextMsgAdded"));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("TextChatUpdate"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}