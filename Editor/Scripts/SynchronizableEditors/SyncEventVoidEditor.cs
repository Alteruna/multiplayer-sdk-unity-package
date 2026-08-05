using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Unity;
using UnityEditor;
using UnityEngine;

namespace Alteruna.UnityEditor
{
	[CustomEditor(typeof(SyncedEventVoid), true)]
	[CanEditMultipleObjects]
	public class SyncEventVoidEditor : SynchronizableEditor
	{
		public SyncEventVoidEditor() : base(false, false) { }

		public override void OnInspectorGUI()
		{
			DrawSynchronizable();
			GUI.enabled = Application.isPlaying;
			if (GUILayout.Button("Trigger Event"))
				foreach (var o in targets)
					((ISyncedEvent)o).Invoke();
			GUI.enabled = true;
			GUILayout.Space(19);
			DrawBase();
		}
	}
}