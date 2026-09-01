using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Sync a UnityEvent with a <c>string</c> parameter.
	/// </summary>
	/// <seealso cref="SyncedEventBase{T}"/>
	[AddComponentMenu("Alteruna/Event/Synced Event <string>"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class SyncedEventString : SyncedEventBase<string> { }
}