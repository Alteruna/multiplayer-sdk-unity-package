using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Sync a UnityEvent with a <c>byte</c> parameter.
	/// </summary>
	/// <seealso cref="SyncedEventBase{T}"/>
	[AddComponentMenu("Alteruna/Event/Synced Event <byte>"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class SyncedEventByte : SyncedEventBase<byte> { }
}