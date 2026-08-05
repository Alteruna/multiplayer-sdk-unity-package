using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Unity;
using UnityEditor;
using UnityEngine;

namespace Alteruna.UnityEditor
{
	[CustomEditor(typeof(SyncedEventBase<>), true)]
	[CanEditMultipleObjects]
	public class SyncEventEditor : SynchronizableEditor
	{
		public SyncEventEditor() : base(false, false) { }

		public override void OnInspectorGUI()
		{
			DrawSynchronizable();
			if (Application.isPlaying && targets.Length == 1)
			{
				var obj = (ISyncedEventType)target;
				if (GUILayout.Button("Trigger Event")) obj.Invoke();
				var c = GUI.color;
				GUI.color = new Color(c.r, c.b, c.b, 0.5f);
				GUILayout.Label("Last Value: " + obj.ValueToString());
				GUI.color = c;
			}
			else
			{
				GUI.enabled = Application.isPlaying;
				if (GUILayout.Button("Trigger Event"))
					foreach (var o in targets)
						((ISyncedEvent)o).Invoke();
				GUI.enabled = true;
				GUILayout.Space(19);
			}

			DrawBase();
		}
	}
}