#if ALTERUNA
using Alteruna.Multiplayer.Core.MethodArguments;
using UnityEngine;

namespace Alteruna.TextChatCommands
{
	public class CommandCreateRoom : ITextChatCommand
	{
		public string Command { get; } = "createRoom";
		public string Description { get; } = "Create room. All arguments are optional.";
		public string Usage { get; } = "/createRoom name:<string> customData:<string> sceneID:<number> password:<number> maxUsers:<number> private:<bool> onDemand:<bool> joinRoom:<bool>";
		public bool IsCheat { get; } = true;
		public bool IgnoreCase { get; } = true;
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		public static void Init() => TextChatSynchronizable.Commands.Add(new CommandCreateRoom());

		public string Execute(TextChatSynchronizable textChat, string[] args)
		{

			if (textChat.Multiplayer == null)
			{
				textChat.LogError("No valid Multiplayer component.");
				return null;
			}

			if (textChat.Multiplayer.InRoom)
			{
				textChat.LogError("You are already in a room.");
				return null;
			}

			RoomArgs rArgs = new RoomArgs();

			foreach (string arg in args)
			{
				string[] split = arg.Split(':');
				if (split.Length != 2)
				{
					textChat.LogError("Invalid argument: " + arg);
					return null;
				}
				switch (split[0].ToUpper())
				{
					case "NAME":
						rArgs.DisplayName = split[1];
						break;
					// ReSharper disable once StringLiteralTypo
					case "CUSTOMDATA":
						rArgs.CustomData = split[1];
						break;
					// ReSharper disable once StringLiteralTypo
					case "SCEENEID":
						if (!int.TryParse(split[1], out int sceneId))
						{
							textChat.LogError("Invalid argument: " + arg);
							return null;
						}
						rArgs.SceneId = sceneId;
						break;
					case "PASSWORD":
						if (!ushort.TryParse(split[1], out ushort password))
						{
							textChat.LogError("Invalid argument: " + arg);
							return null;
						}
						rArgs.PinCode = password;
						break;
					// ReSharper disable once StringLiteralTypo
					case "MAXPLAYERS":
					// ReSharper disable once StringLiteralTypo
					case "MAXUSERS":
						if (!ushort.TryParse(split[1], out ushort maxUsers))
						{
							textChat.LogError("Invalid argument: " + arg);
							return null;
						}
						rArgs.MaxUsers = maxUsers;
						break;
					case "PRIVATE":
						if (!TextChatCommandHelper.TryGetBoolArg(split[1], out bool v))
						{
							textChat.LogError("Invalid argument: " + arg);
							return null;
						}
						rArgs.Private = v;
						break;
					// ReSharper disable once StringLiteralTypo
					case "ONDEMAND":
						if (!TextChatCommandHelper.TryGetBoolArg(split[1], out bool onDemand))
						{
							textChat.LogError("Invalid argument: " + arg);
							return null;
						}
						rArgs.OnDemand = onDemand;
						break;
					// ReSharper disable once StringLiteralTypo
					case "JOINROOM":
						if (!TextChatCommandHelper.TryGetBoolArg(split[1], out bool joinRoom))
						{
							textChat.LogError("Invalid argument: " + arg);
							return null;
						}
						rArgs.JoinRoom = joinRoom;
						break;
					default:
						textChat.LogError("Invalid argument: " + arg);
						return null;
				}
			}

			textChat.Multiplayer.CreateRoom(rArgs);
			return null;
		}
	}
}
#endif