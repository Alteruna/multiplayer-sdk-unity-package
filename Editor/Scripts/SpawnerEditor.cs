using System.Linq;
using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Unity;
using UnityEditor;
using UnityEngine;

namespace Alteruna.UnityEditor
{
	[CustomEditor(typeof(Spawner))]
	public class SpawnerEditor : Editor
	{
		bool _showPosition;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var spawner = target as Spawner;
			if (spawner != null)
			{
				var spawned = spawner.SpawnedObjects;
				if (spawned.Any())
				{
					_showPosition = EditorGUILayout.Foldout(_showPosition, "Spawned Objects");
					if (_showPosition)
					{
						EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
						foreach (var obj in spawned)
						{
							EditorGUILayout.LabelField(obj.Item2.ToString(), obj.Item1.name);
						}

						EditorGUILayout.EndVertical();
					}
				}
			}
		}
	}
}