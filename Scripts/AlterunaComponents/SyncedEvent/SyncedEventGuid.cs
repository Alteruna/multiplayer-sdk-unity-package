using System;
using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Sync a UnityEvent with a <c>Guid</c> parameter.
	/// </summary>
	/// <seealso cref="SyncedEventBase{T}"/>
	[AddComponentMenu("Alteruna/Event/Synced Event <Guid>"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class SyncedEventGuid : SyncedEventBase<Guid>
	{

		/// <summary>
		/// Invoke the event with the given argument.
		/// </summary>
		/// <param name="arg">passed object</param>
		public void Invoke(IUniqueID arg)
		{
			OnEvent.Invoke(Value = arg.UID);
			Multiplayer.Sync(this, Reliability);
		}
		
		/// <summary>
		/// Invoke without triggering local event with the given argument.
		/// </summary>
		/// <param name="arg">passed object</param>
		public void InvokeSilent(IUniqueID arg)
		{
			Multiplayer.Sync(this, Reliability);
		}
	}
}