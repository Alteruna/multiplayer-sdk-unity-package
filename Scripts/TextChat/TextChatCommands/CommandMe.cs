using UnityEngine;

namespace Alteruna.TextChatCommands
{
	public class CommandMe : ITextChatCommand
	{
		public string Command { get; } = "me";
		public string Description { get; } = "Get name and id of yourself. If you have a avatar, it will be set as target.";
		public string Usage { get; } = "/me";
		public bool IsCheat { get; } = false;
		public bool IgnoreCase { get; } = true;
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		public static void Init() => TextChatSynchronizable.Commands.Add(new CommandMe());

		public string Execute(TextChatSynchronizable textChat, string[] args)
		{
			var t = TextChatCommandHelper.GetPlayerTransform(textChat);
			if (t != null)
			{
				TextChatCommandHelper.LastTransformTarget = t;
				Vector3 pos = t.position;
				
#if ALTERUNA
				return $"{textChat.Multiplayer.Me}\n x:{pos.x} y:{pos.y} z:{pos.z}";
#else
				return $"x:{pos.x} y:{pos.y} z:{pos.z}";
#endif
			}

#if ALTERUNA
			return textChat.Multiplayer.Me;
#else
			return null;
#endif
		}
	}
}