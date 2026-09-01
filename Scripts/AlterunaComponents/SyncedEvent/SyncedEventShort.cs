using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Sync a UnityEvent with a <c>short</c> parameter.
	/// </summary>
	/// <seealso cref="SyncedEventBase{T}"/>
	[AddComponentMenu("Alteruna/Event/Synced Event <short>"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class SyncedEventShort : SyncedEventBase<short> { }
}