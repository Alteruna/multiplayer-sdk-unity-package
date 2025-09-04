#if ALTERUNA
using Alteruna.Multiplayer.Core;
using UnityEngine;

namespace Alteruna.TextChatCommands
{
	public class CommandRoom : ITextChatCommand
	{
		public string Command { get; } = "room";
		public string Description { get; } = "Get information about current room.";
		public string Usage { get; } = "/room";
		public bool IsCheat { get; } = true;
		public bool IgnoreCase { get; } = true;
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		public static void Init() => TextChatSynchronizable.Commands.Add(new CommandRoom());

		public string Execute(TextChatSynchronizable textChat, string[] args)
		{
			if (textChat.Multiplayer == null)
			{
				textChat.LogError("No valid Multiplayer component.");
				return null;
			}

			if (!textChat.Multiplayer.InRoom)
			{
				textChat.LogError("Not in a room.");
				return null;
			}
			
			Room r = textChat.Multiplayer.CurrentRoom;
			return $"Room: {r.DisplayName}@{r.SessionID}\nPlayers: {r.CurrentUsers}\nMax players:{r.MaxUsers}\nPrivate: {r.Private}\nOn demand: {r.OnDemand}\nPassword: {r.IsPasswordProtected}\nState: {r.State.ToString()}\n";
		}
	}
}
#endif