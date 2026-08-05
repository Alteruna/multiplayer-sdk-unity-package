using UnityEngine;
using UnityEngine.Events;

namespace Alteruna.Multiplayer.Unity.InputSynchronizable
{
	/// <summary>
	/// Alteruna Input interface.
	/// Can be used to create a custom synced input system.
	/// </summary>
	/// <seealso cref="InputSynchronizable"/>
	/// <seealso cref="SyncedKey"/>
	/// <seealso cref="SyncedAxis"/>
	public interface IInput
	{
		/// <summary>
		/// Get synced button values by index
		/// </summary>
		bool[] KeyValues { get; }
		
		/// <summary>
		/// Get synced axes values by index
		/// </summary>
		float[] AxesValues { get; }

		/// <summary>
		/// Event for changes in key inputs.
		/// passes <c>KeyCode</c> and state.
		/// </summary>
		UnityEvent<KeyCode, bool> OnKeyUpdate { get; }

		/// <summary>
		/// Add a key to the <c>InputSynchronizable</c>
		/// </summary>
		/// <param name="keyCode"><c>KeyCode</c> of the target key</param>
		void AddKey(KeyCode keyCode);

		/// <summary>
		/// Add a array of keys to the <c>InputSynchronizable</c>
		/// </summary>
		/// <param name="keyCodes">Array of <c>KeyCode</c> to target</param>
		void AddKey(KeyCode[] keyCodes);

		/// <summary>
		/// Add a axis to the <c>InputSynchronizable</c>
		/// </summary>
		/// <param name="newAxis">string of the target axis</param>
		void AddAxis(string newAxis);

		/// <summary>
		/// Add a array of axes to the <c>InputSynchronizable</c>
		/// </summary>
		/// <param name="newAxes">strings of the target axes</param>
		void AddAxis(string[] newAxes);

		/// <summary>
		/// Get index of a registered <c>keyCode</c>.
		/// If the target <c>keyCode</c> dos not exist it returns <c>-1</c>
		/// </summary>
		/// <param name="keyCode">target</param>
		/// <returns><c>index</c> on success, <c>-1</c> on fail.</returns>
		int GetIndexOfKey(KeyCode keyCode);

		/// <summary>
		/// Attempts to get index of a registered <c>keyCode</c>.
		/// If the target <c>keyCode</c> dos not exist, return <c>false</c> and <c>index</c> will be 0
		/// </summary>
		/// <param name="keyCode">target</param>
		/// <param name="index">Index of target registered <c>keyCode</c></param>
		/// <returns>True on success</returns>
		bool TryGetIndexOfKey(KeyCode keyCode, out int index);

		/// <summary>
		/// Get index of a registered <c>keyCode</c>.
		/// If the target <c>keyCode</c> dos not exist it returns <c>-1</c>
		/// </summary>
		/// <param name="targetAxis">target</param>
		/// <returns><c>index</c> on success, <c>-1</c> on fail.</returns>
		int GetIndexOfAxis(string targetAxis);

		/// <summary>
		/// Attempts to get index of a registered <c>keyCode</c>.
		/// If the target <c>keyCode</c> dos not exist, return <c>false</c> and <c>index</c> will be 0
		/// </summary>
		/// <param name="targetAxis">target</param>
		/// <param name="index">Index of target registered <c>keyCode</c></param>
		/// <returns>True on success</returns>
		bool TryGetIndexOfAxis(string targetAxis, out int index);
		
	}
}