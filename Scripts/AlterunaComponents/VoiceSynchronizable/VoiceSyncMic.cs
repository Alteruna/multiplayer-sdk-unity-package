using System;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Alteruna.Multiplayer.Unity
{
	public partial class VoiceSynchronizable
	{

		/// <summary>
		/// Interface for audio input.
		/// </summary>
		public interface IAudioInput
		{
			
			/// <summary>
			/// Supported platforms for audio input.
			/// </summary>
			SupportedPlatforms SupportedPlatforms { get; }
			
			/// <summary>
			///   <para>A list of available microphone devices, identified by name.</para>
			/// </summary>
			string[] devices { get; }

			/// <summary>
			///   <para>Stops recording.</para>
			/// </summary>
			/// <param name="deviceName">The name of the device.</param>
			void End(string deviceName);

			/// <summary>
			///   <para>Get the position in samples of the recording.</para>
			/// </summary>
			/// <param name="deviceName">The name of the device.</param>
			int GetPosition(string deviceName);

			/// <summary>
			///   <para>Start Recording with device.</para>
			/// </summary>
			/// <param name="deviceName">The name of the device.</param>
			/// <param name="loop">Indicates whether the recording should continue recording if lengthSec is reached, and wrap around and record from the beginning of the AudioClip.</param>
			/// <param name="lengthSec">Is the length of the AudioClip produced by the recording.</param>
			/// <param name="frequency">The sample rate of the AudioClip produced by the recording.</param>
			/// <returns>
			///   <para>The function returns null if the recording fails to start.</para>
			/// </returns>
			AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency);
		}
		
		/// <summary>
		/// Unity microphone audio input.
		/// </summary>
		public class UnityMicrophone : IAudioInput
		{
			
			/// <summary>
			/// Supported platforms for audio input.
			/// </summary>
			public SupportedPlatforms SupportedPlatforms => SupportedPlatforms.NonWebGL;
			
			/// <summary>
			///   <para>A list of available microphone devices, identified by name.</para>
			/// </summary>
			public string[] devices => UnityEngine.Microphone.devices;
			
			/// <summary>
			///   <para>Stops recording.</para>
			/// </summary>
			/// <param name="deviceName">The name of the device.</param>
			public void End(string deviceName) => UnityEngine.Microphone.End(deviceName);
			
			/// <summary>
			///   <para>Get the position in samples of the recording.</para>
			/// </summary>
			/// <param name="deviceName">The name of the device.</param>
			public int GetPosition(string deviceName) => UnityEngine.Microphone.GetPosition(deviceName);
			
			/// <summary>
			///   <para>Start Recording with device.</para>
			/// </summary>
			/// <param name="deviceName">The name of the device.</param>
			/// <param name="loop">Indicates whether the recording should continue recording if lengthSec is reached, and wrap around and record from the beginning of the AudioClip.</param>
			/// <param name="lengthSec">Is the length of the AudioClip produced by the recording.</param>
			/// <param name="frequency">The sample rate of the AudioClip produced by the recording.</param>
			/// <returns>
			///   <para>The function returns null if the recording fails to start.</para>
			/// </returns>
			public AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency) => UnityEngine.Microphone.Start(deviceName, loop, lengthSec, frequency);
		}
		
		/// <summary>
		/// Supported platforms for audio input.
		/// </summary>
		[Flags]
		public enum SupportedPlatforms : byte
		{
			None = 0,
			All = 3,
			WebGL = 1,
			NonWebGL = 2
		}
	}
}