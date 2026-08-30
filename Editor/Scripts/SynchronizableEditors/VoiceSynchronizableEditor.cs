using Alteruna.Multiplayer.Core.PacketProcessing;
using Alteruna.Multiplayer.Unity;
using UnityEditor;
using UnityEngine;

namespace Alteruna.UnityEditor
{
	[CustomEditor(typeof(VoiceSynchronizable), true)]
	[CanEditMultipleObjects]
	public class VoiceSynchronizableEditor : SynchronizableEditor
	{
		public VoiceSynchronizableEditor() : base(false, false) { }

		public override void OnInspectorGUI()
		{
			DrawSynchronizable();
			DrawBase();

			var component = (VoiceSynchronizable)target;
			if (component == null) return;
			if (Application.isPlaying)
			{
				if (component.IsSender)
				{
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Microphone: " + VoiceSynchronizable.DeviceName);

					if (component.Compression != CompressionMethod.None)
					{
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Compression: " + component.Compression.ToString());
						EditorGUILayout.LabelField("Current: " + component.CurrentSavedFromCompressionPer100Sampels.ToString("F1") + "%");
						EditorGUILayout.LabelField("Peek: " + component.StatisticsPeekSavedFromCompressionPer100Sampels.ToString("F1") + "%");

						long totalUncompressed = component.StatisticsTotalWritten + component.StatisticsTotalSaved;
						long totalCompressed = component.StatisticsTotalWritten;
						float average = 100f - (totalCompressed * 100) / (float)totalUncompressed;

						EditorGUILayout.LabelField("Average: " + average.ToString("F1") + "%");
					}
				}
			}
			else
			{
				AudioSettings.GetDSPBufferSize(out int bufferLength, out int numBuffers);
				int sendFrequency = Mathf.RoundToInt(component.SendFrequency * 1000);
				int bufferDelay = Mathf.RoundToInt(bufferLength * numBuffers * 1000 / (float)component.AudioFrequency);
				
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox("With the current settings, you sending with a delay of " + sendFrequency + "ms (excluding ping)\nand a playback buffer delay of " + bufferDelay + "ms. (See DSP Buffer Size in Project Settings/Audio. Delay is excluding overhead delay of audio streaming.)", MessageType.Info);
			}
		}
	}
}