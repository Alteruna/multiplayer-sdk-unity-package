using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Sync a UnityEvent with a <c>float</c> parameter.
	/// </summary>
	/// <seealso cref="SyncedEventBase{T}"/>
	[AddComponentMenu("Alteruna/Event/Synced Event <float>"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class SyncedEventFloat : SyncedEventBase<float> { }
}