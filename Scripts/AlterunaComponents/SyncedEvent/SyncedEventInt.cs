using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Sync a UnityEvent with a <c>int</c> parameter.
	/// </summary>
	/// <seealso cref="SyncedEventBase{T}"/>
	[AddComponentMenu("Alteruna/Event/Synced Event <int>"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class SyncedEventInt : SyncedEventBase<int> { }
}