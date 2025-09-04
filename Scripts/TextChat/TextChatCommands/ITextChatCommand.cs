using System.Linq;
using Alteruna.Multiplayer;
using UnityEngine;

namespace Alteruna.TextChatCommands
{
	public interface ITextChatCommand
	{
		string Command { get; }
		string Description { get; }
		string Usage { get; }
		bool IsCheat { get; }
		bool IgnoreCase { get; }
		string Execute(TextChatSynchronizable textChat, string[] args);
	}
	
	public static class TextChatCommandHelper
	{

		public static Transform LastTransformTarget = null;
		public static CommunicationBridgeUID LastUidTarget = null;
		
		static readonly string[] trueStr = new string[] {"TRUE", "1", "ON"};
		static readonly string[] falseStr = new string[] {"FALSE", "0", "OFF"};

		public static bool TryGetBoolArg(string[] args, out bool result, bool defaultValue = false)
		{
			result = defaultValue;
			return TrySetBoolArg(args, ref result);
		}
		
		public static bool TrySetBoolArg(string[] args, ref bool field)
		{
			if (args.Length == 0) return false;

			return TrySetBoolArg(args[0], ref field);
		}

		public static bool TryGetBoolArg(string arg, out bool result, bool defaultValue = false)
		{
			result = defaultValue;
			return TrySetBoolArg(arg, ref result);
		}
		
		public static bool TrySetBoolArg(string arg, ref bool field)
		{
			if (arg == null) return false;

			arg = arg.ToUpper();

			if (trueStr.Any(s => s == arg))
			{
				field = true;
				return true;
			}

			if (falseStr.Any(s => s == arg))
			{
				field = false;
				return true;
			}

			return false;
		}
		
		public static Transform GetPlayerTransform(TextChatSynchronizable textChat)
		{
#if ALTERUNA
			if (textChat.Multiplayer != null)
			{
				if (textChat.Multiplayer.Me == null)
				{
					textChat.LogError("No user information available.");
					return null;
				}

				var avatar = textChat.Multiplayer.GetAvatar();
				if (avatar == null)
				{
					textChat.LogError("No avatar found for local user.");
					return null;
				}
				return avatar.transform;
			}
			else
#endif
			{
				if (Camera.main == null)
				{
					textChat.LogError("No camera found.");
					return null;
				}
				Transform t = Camera.main.transform;
				while (t.parent != null)
				{
					t = t.parent;
				}

				return t;
			}
		}
	}
}