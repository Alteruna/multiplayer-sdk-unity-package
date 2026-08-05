using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Unity.EventArgument;
using UnityEngine;
using UnityEngine.Events;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Event used for when you want something to only apply for a single client.
	/// </summary>
	/// <remarks>
	///	When on an avatar, the controller will be the avatar owner.
	/// Otherwise, the controller will be the client with the lowest user index.
	/// </remarks>
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class SingleClientEvent : CommunicationBridge
	{
		/// <summary>
		/// Gets if the local client is the controller.
		/// </summary>
		public bool IsControlled { get; private set; }

		private bool _isAvatar;
		private bool _haveCalled;

		/// <summary>
		/// Runs when the controlling client is changed.
		/// </summary>
		/// <remarks>
		///	True when controlled on this client.
		/// </remarks>
		[Tooltip("Runs when the controlling client is changed. True when controlled on this client.")]
		public UnityEvent<bool> OnClientChanged = new UnityEvent<bool>();

		public void Start()
		{
			Multiplayer.OnRoomJoined.AddListener(OnRoomJoined);
			Multiplayer.OnOtherUserJoined.AddListener(OnOtherUserJoined);
			Multiplayer.OnOtherUserLeft.AddListener(OnOtherUserLeft);
			Multiplayer.OnRoomLeft.AddListener(OnRoomLeft);
		}

		public void OnDestroy()
		{
			Multiplayer.OnRoomJoined.RemoveListener(OnRoomJoined);
			Multiplayer.OnOtherUserJoined.RemoveListener(OnOtherUserJoined);
			Multiplayer.OnOtherUserLeft.RemoveListener(OnOtherUserLeft);
			Multiplayer.OnRoomLeft.RemoveListener(OnRoomLeft);
		}

		private void OnOtherUserLeft(OtherUserLeftEvent args) => UpdateEvent(args.Controller);
		private void OnOtherUserJoined(OtherUserJoinedEvent args) => UpdateEvent(args.Controller);
		private void OnRoomLeft(RoomLeftEvent args) => UpdateEvent(args.Controller);
		private void OnRoomJoined(RoomJoinedEvent args) => UpdateEvent(args.Controller);


		public void UpdateEvent() => UpdateEvent(Multiplayer);

		public void UpdateEvent(MultiplayerManager m)
		{
			if (_isAvatar) return;
			bool isMe = m.LowestUserIndex == m.GetUser();
			if (_haveCalled && isMe == IsControlled) return;
			IsControlled = isMe;
			_haveCalled = true;
			OnClientChanged.Invoke(IsControlled);
		}

		public override void Possessed(bool isMe, User user)
		{
			if (_haveCalled && IsControlled == isMe) return;
			IsControlled = isMe;
			_isAvatar = true;
			_haveCalled = true;
			OnClientChanged.Invoke(IsControlled);
		}

		public override void Unpossessed()
		{
			if (IsControlled)
				OnClientChanged.Invoke(false);
			IsControlled = false;
		}
	}
}