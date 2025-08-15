using Alteruna;
using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Core;

namespace Alteruna_Examples
{
	// This also works with other base classes like Synchronizable, AttributesSync, CommunicationBridgeUID, etc.
	public class ExampleOnlyForLocalAvatar : CommunicationBridge
	{
		
		private User _user;
		
		public override void Possessed(bool isMe, User user)
		{
			// We store the user that is possessing the avatar
			_user = user;

			// We enable the component only for the local avatar
			enabled = isMe;

			if (isMe) LocalStart();
			else RemoteStart();
		}
		
		/// <summary>
		/// This method is called only for the local avatar.
		/// </summary>
		private void LocalStart()
		{
			print($"Local avatar started: {_user}");
		}
		
		/// <summary>
		/// This method is called only for remote Avatars.
		/// </summary>
		private void RemoteStart()
		{
			print($"Remote avatar started: {_user}");
		}

		public void Update()
		{
			// Only run on the local avatar
		}
		
	}
}