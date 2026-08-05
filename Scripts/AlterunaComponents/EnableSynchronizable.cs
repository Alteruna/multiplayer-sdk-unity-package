using System;
using System.Reflection;
using Alteruna.Multiplayer.Core;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// A component for synchronizing the enabling or disabling of a GameObject across multiple clients in a multiplayer environment.
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("Alteruna/Object/Enable Synchronizable")]
	[MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class EnableSynchronizable : AttributesSync
	{
		// Private field to track ownership status.
		[NonSerialized] private bool _owned = true;

		/// <summary>
		/// Called when the object is possessed. Sets the ownership status of the GameObject.
		/// </summary>
		/// <param name="isMe">Boolean indicating if the current user is the owner.</param>
		/// <param name="user">The user who is possessing the object.</param>
		public override void Possessed(bool isMe, User user)
		{
			_owned = isMe;
		}

		/// <summary>
		/// Invoked when the GameObject is enabled. Registers the object for synchronization and, if owned by the current user and in a multiplayer room, requests enabling the GameObject for all users.
		/// </summary>

		public override void OnEnable()
		{
			Register();
			if (_owned && Multiplayer.InRoom)
				InvokeRemoteMethod(nameof(RemoteEnable), UserId.All);
		}

		/// <summary>
		/// Invoked when the GameObject is disabled. If owned by the current user and in a multiplayer room, requests disabling the GameObject for all users.
		/// </summary>

		private void OnDisable()
		{
			if (_owned && Multiplayer.InRoom)
				InvokeRemoteMethod(nameof(RemoteDisable), UserId.All);
		}

		/// <summary>
		/// A synchronizable method that remotely disables the GameObject.
		/// This method is intended to be invoked across the network to ensure consistent state among all clients.
		/// </summary>
		[SynchronizableMethod]
		private void RemoteDisable()
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// A synchronizable method that remotely enables the GameObject.
		/// This method is intended to be invoked across the network to ensure consistent state among all clients.
		/// </summary>
		[SynchronizableMethod]
		private void RemoteEnable()
		{
			gameObject.SetActive(true);
		}
		
		public new void Reset()
		{
			base.Reset();
			Reliability = Reliability.Reliable;
		}
	}
}