using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Unity;
using UnityEditor;
using UnityEngine;

namespace Alteruna.UnityEditor
{
	[CustomEditor(typeof(RigidbodySynchronizableCommon), true), CanEditMultipleObjects]
	public class RigidbodySynchronizableCommonEditor : SynchronizableEditor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();

			var style = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Italic,
				normal = new GUIStyleState() { textColor = Color.grey }
			};
			if (targets.Length > 1)
			{
				float data = 0;
				for (int i = 0, l = targets.Length; i < l; i++)
				{
					data += ((RigidbodySynchronizableCommon)targets[i]).EstimateMinimumDataSentPerSecond();
				}
				EditorGUILayout.LabelField("Estimated data transfer of all selected is " + ((int)data / 100) / 10f + " Kb/s. (excluding transport headers)",
					style, GUILayout.ExpandWidth(true));
			}
			else
			{
				EditorGUILayout.LabelField("Estimated data transfer is " + ((RigidbodySynchronizableCommon)target).EstimateMinimumDataSentPerSecond() + " bytes per second. (excluding transport header)",
					style, GUILayout.ExpandWidth(true));
			}
		}
	}
}