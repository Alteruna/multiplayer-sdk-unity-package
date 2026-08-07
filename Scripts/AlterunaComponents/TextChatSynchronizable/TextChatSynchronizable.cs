using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Core.MethodArguments;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using Alteruna.Multiplayer.Unity.EventArgument;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Application = UnityEngine.Application;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Synchronizes text chat messages for all clients.
	/// Can also be used to run commands and cheats.
	/// </summary>
	/// <remarks>
	///	<c>TextChatSynchronizable</c> is a chat system that synchronizes with all clients in the room.
	/// It is also used with slash commands using the
	/// <see href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.html">TMP_InputField</see> for
	/// input and creating a class that implements <see cref="ITextChatCommand"/>.<br/><br/>
	/// <img src="../images/Doc.Prefabs.TextChat.png" />
	/// </remarks>
	/// <seealso cref="ITextChatCommand"/>
	/// <seealso cref="Synchronizable"/>
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	[AddComponentMenu("Alteruna/Other/TextChatSynchronizable", 0)]
	public class TextChatSynchronizable : Synchronizable
	{
		/// <summary>
		/// Commands registry.
		/// </summary>
		public static readonly List<ITextChatCommand> Commands = new List<ITextChatCommand>();

		/// <summary>
		/// Number of chat messages to buffer
		/// </summary>
		[Tooltip("Number of chat messages to buffer"), SerializeField]
		private int chatBuffer = 10;

		/// <summary>
		/// Writes events to text chat. Like user joined.
		/// </summary>
		[Tooltip("Writes events to text chat")]
		public bool LogSystemMessages = true;

		/// <summary>
		/// Sort messages based on senders timestamp.
		/// </summary>
		[Tooltip("Sort messages based on senders timestamp")]
		public bool UseTimeStamps = false;

		/// <summary>
		/// When true, show a message for the sender when sending the message.
		/// Otherwise, it shows when the message is created.
		/// </summary>
		[Tooltip("When true, show message for sender when sending the message. Otherwise, it shows when the message is created.")]
		public bool LogLocalOnSend = true;

		/// <summary>
		/// Write Unity errors in chat.
		/// </summary>
		[Tooltip("Write Unity errors in chat")]
		public bool LogErrors = false;

		/// <summary>
		/// When true, Show error when the command fails.
		/// </summary>
		[Tooltip("When false, Only says: command failed.")]
		public bool LogFullCommandErrors = false;

		/// <summary>
		/// Allow commands to be run in text chat.
		/// </summary>
		[Tooltip("Allow commands to be run in text chat")]
		public bool AllowCommands = true;

		/// <summary>
		/// Allow commands that are considered cheats to run.
		/// </summary>
		[Tooltip("Allow commands that are considered cheats")]
		public bool AllowCheats = false;

		/// <summary>
		/// Allows a room owner to enable cheats.
		/// </summary>
		/// <remarks>
		/// Always true in development builds and editor.
		/// </remarks>
		[Tooltip("Always true in development build and editor.")]
		public bool AllowHostToToggleCheats = true;

		/// <summary>
		/// Default <see cref="AllowCheats"/> in development environments.
		/// </summary>
		/// <remarks>
		///	development environments refers to editor and development builds.
		/// </remarks>
		[Tooltip("Development builds and Editor is considered dev environments.")]
		public bool EnableCheatsInDevEnvironments = true;

		/// <summary>
		/// Use rich text formatting.
		/// </summary>
		public bool UseRichText = true;

		/// <summary>
		/// Make names in chat bold.
		/// </summary>
		/// <remarks>
		/// Requires <see cref="UseRichText"/> to be enabled.
		/// </remarks>
		public bool BoldNames = true;

		/// <summary>
		/// Log chat messages to the console debug log.
		/// </summary>
		public bool LogChatInDebugLog = true;

		/// <summary>
		/// Text input field for chat messages.
		/// </summary>
		public TMP_InputField InputField;

		/// <summary>
		/// Input action to select chat input field.
		/// </summary>
		public InputAction SetInputActiveAction = new InputAction("SetInputActive", InputActionType.Button, "<Keyboard>/enter", interactions: "press");

		/// <summary>
		/// Event triggered when a text message is added to the chat with the new message.
		/// </summary>
		public UnityEvent<string> TextMsgAdded;

		/// <summary>
		/// Event triggered when the buffer needs to be updated.
		/// The whole buffer is sent as a argument.
		/// </summary>
		public UnityEvent<string> TextChatUpdate;

		[NonSerialized] private ChatEvent[] _chat = Array.Empty<ChatEvent>();
		[NonSerialized] private readonly List<ChatEvent> _outgoingMessages = new List<ChatEvent>();

		// We define the StringBuilder here to avoid creating a new one every time we need it.
		[NonSerialized] private readonly StringBuilder _sb = new StringBuilder();
		[NonSerialized] private readonly StringBuilder _sb2 = new StringBuilder();

		private static readonly Color c = new Color(1f, .2f, 0, 1);

		/// <summary>
		/// Function to get user color.
		/// </summary>
		/// <example>
		///	<code>
		/// //Set color function to generate a unique color with saturation and brightness of "Color.red"
		/// TextChatSynchronizable.GetUserColor = id => UniqueAvatarColor.HueFromId(Color.red, id);
		/// </code>
		/// </example>
		// ReSharper disable once FieldCanBeMadeReadOnly.Global MemberCanBePrivate.Global
		public static Func<ushort, Color> GetUserColor = id => UniqueAvatarColor.HueFromId(c, id);

		/// <summary>
		/// Get or change the max number of buffered chat lines.
		/// </summary>
		public int ChatBuffer
		{
			get => chatBuffer;
			set => UpdateBufferSize(chatBuffer = value);
		}

		/// <summary>
		/// Initializing buffer and subscribe events.
		/// </summary>
		public virtual void Start()
		{
			if (Debug.isDebugBuild || Application.isEditor)
			{
				AllowHostToToggleCheats = true;
				AllowCommands = true;

				if (EnableCheatsInDevEnvironments)
				{
					AllowCheats = true;
				}
			}

			_chat = new ChatEvent[chatBuffer];
			Multiplayer.OnRoomJoined.AddListener(OnRoomJoined);
			Multiplayer.OnRoomLeft.AddListener(OnRoomLeft);
			Multiplayer.OnOtherUserJoined.AddListener(OnOtherUserJoined);
			Multiplayer.OnOtherUserLeft.AddListener(OnOtherUserLeft);
			Application.logMessageReceived += HandleLog;

			PrepareInputAction();
			InputField.onSubmit.AddListener(SendChatMessage);
		}

		private void PrepareInputAction()
		{
			SetInputActiveAction.performed += OnSetInputActiveActionPerformed;
			SetInputActiveAction.Enable();
		}

		private void OnSetInputActiveActionPerformed(InputAction.CallbackContext ctx)
		{
			if (InputField != null && InputField.gameObject.activeInHierarchy)
			{
				InputField.ActivateInputField();
				InputField.Select();
			}
		}

		/// <summary>
		/// Deregister and unsubscribe from evets.
		/// </summary>
		public override void OnDestroy()
		{
			base.OnDestroy();
			Multiplayer.OnRoomJoined.RemoveListener(OnRoomJoined);
			Multiplayer.OnRoomLeft.RemoveListener(OnRoomLeft);
			Multiplayer.OnOtherUserJoined.RemoveListener(OnOtherUserJoined);
			Multiplayer.OnOtherUserLeft.RemoveListener(OnOtherUserLeft);
			Application.logMessageReceived -= HandleLog;
		}

		private void OnRoomJoined(RoomJoinedEvent args)
		{
			if (LogSystemMessages)
			{
				AddChatEventToBuffer(new ChatEvent("joined the room", UseTimeStamps, args.User.Index));
			}
		}

		private void OnRoomLeft(RoomLeftEvent args)
		{
			if (LogSystemMessages)
			{
				AddChatEventToBuffer(new ChatEvent("User left room", UseTimeStamps));
			}
		}

		private void OnOtherUserJoined(OtherUserJoinedEvent args)
		{
			if (LogSystemMessages)
			{
				AddChatEventToBuffer(new ChatEvent("joined the room", UseTimeStamps, args.User.Index));
			}
		}

		private void OnOtherUserLeft(OtherUserLeftEvent args)
		{
			if (LogSystemMessages)
			{
				AddChatEventToBuffer(new ChatEvent("left the room", UseTimeStamps, args.User.Index));
			}
		}

		private void HandleLog(string logString, string stackTrace, LogType type)
		{
			if (LogErrors && type == LogType.Error)
			{
				AddChatEventToBuffer(new ChatEvent(logString, UseTimeStamps), false);
			}
		}

		private void UpdateBufferSize(int size)
		{
			var temp = _chat;
			_chat = new ChatEvent[size];
			Array.Copy(temp, _chat, Math.Min(temp.Length, _chat.Length));
		}

		public void LogError(string msg, bool allowConsoleLog = true) =>
			AddChatEventToBuffer(new ChatEvent(UseRichText ? "<color=#ED4337>" + msg + "</color>" : msg, UseTimeStamps), allowConsoleLog);

		private void AddChatEventToBuffer(ChatEvent chatEvent, bool allowConsoleLog = true)
		{
			//shift array
			for (int i = chatBuffer - 2; i >= 0; i--)
			{
				_chat[i + 1] = _chat[i];
			}

			_chat[0] = chatEvent;

			//sort chat by time stamp
			if (UseTimeStamps)
			{
				for (int i = 1; i < chatBuffer && _chat[i].TimeStamp > _chat[i - 1].TimeStamp; i++)
				{
					chatEvent = _chat[i];
					_chat[i] = _chat[i - 1];
					_chat[i - 1] = chatEvent;
				}
			}

			var msg = chatEvent.ToString(this);
			if (LogChatInDebugLog && allowConsoleLog)
				Debug.Log(msg);
			TextMsgAdded.Invoke(msg);
			TextChatUpdate.Invoke(ToString());
		}

		public void SendChatMessage(string msg)
		{
			InputField.text = string.Empty;

			msg = msg.Trim();

			if (msg.Length == 0)
			{
				return;
			}

			if (AllowCommands && msg[0] == '/')
			{
				ExecuteCommand(msg, true);
				return;
			}

			SendChatMessageRaw(msg);
		}

		private void SendChatMessageRaw(string msg)
		{
			ChatEvent chatEvent = new ChatEvent(msg, UseTimeStamps, Multiplayer.Me);

			if (!LogLocalOnSend)
			{
				AddChatEventToBuffer(chatEvent);
			}

			if (!Multiplayer.InRoom)
			{
				return;
			}

			_outgoingMessages.Add(chatEvent);
			Multiplayer.Sync(this, Reliability);
		}

		private void ExecuteCommandAsAdmin(string msg, bool invoker = false)
		{
			bool temp = AllowCommands;
			AllowCommands = true;
			ExecuteCommand(msg, invoker);
			AllowCommands = temp;
		}

		private void ExecuteCommand(string msg, bool invoker = false)
		{
			//if (!AllowCommands) return;

			if (invoker)
				AddChatEventToBuffer(new ChatEvent(msg, UseTimeStamps));

			if (msg[0] == '/')
			{
				msg = msg.Substring(1);
			}

			int spaceIndex = msg.IndexOf(' ');
			string command = spaceIndex == -1 ? msg : msg.Substring(0, spaceIndex);
			string commandCi = command.ToUpperInvariant();
			string[] args = spaceIndex == -1 ? Array.Empty<string>() : msg.Substring(spaceIndex + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (commandCi == "HELP")
			{
				_sb.AppendLine("/boldNames <bool> : Enable/disable bold names");
				_sb.AppendLine("/chatBuffer <int> : Change max number of buffered chat lines");
				_sb.AppendLine("/clear : Clear chat");
				_sb.AppendLine("/help : Show available commands");
				_sb.AppendLine("/logErrors <bool> : Enable/disable logging of errors");
				_sb.AppendLine("/logOnSend <bool> : When true, show message for sender when sending the message. Otherwise it shows when the message in created.");
				_sb.AppendLine("/logSystemMessages <bool> : Enable/disable logging of system messages");
				_sb.AppendLine("/useRichText <bool> : Enable/disable rich names");
				_sb.AppendLine("/say <string> : Same as typing in chat but will not execute any command");
				if (AllowHostToToggleCheats)
					_sb.AppendLine("/testingCheats <bool>: Enable/disable cheats");
				if (AllowCheats)
					_sb.AppendLine("/execute as <all|allInclusive|user id|username> <command>: execute command for other users. The (as <target>) part can be repeated for multiple targets. The command should not start with a slash symbol and can include any number of args.");

				foreach (ITextChatCommand c in Commands)
				{
					if (c.IsCheat && !AllowCheats) continue;

					_sb.Append(c.Usage);
					_sb.Append(" : ");
					_sb.AppendLine(c.Description);
				}

				string s = _sb.ToString();
				_sb.Clear();
				AddChatEventToBuffer(new ChatEvent(s, UseTimeStamps));
			}
			else if (commandCi == "CLEAR")
			{
				for (int i = 0; i < _chat.Length; i++)
				{
					_chat[i] = new ChatEvent();
				}

				AddChatEventToBuffer(new ChatEvent("", UseTimeStamps));
			}
			// ReSharper disable once StringLiteralTypo
			else if (commandCi == "CHATBUFFER")
			{
				if (args.Length == 0)
				{
					AddChatEventToBuffer(new ChatEvent(chatBuffer.ToString(), UseTimeStamps));
				}
				else
				{
					if (int.TryParse(args[0], out int size))
					{
						ChatBuffer = size;
					}
					else
					{
						LogError("Invalid argument");
					}
				}
			}
			else if (commandCi == "SAY")
			{
				SendChatMessageRaw(string.Join(" ", args));
			}
			// ReSharper disable once StringLiteralTypo
			else if (ConfigCommand(commandCi == "CHATBUFFER", ref chatBuffer, args))
			{
				if (_chat.Length == chatBuffer) return;
				if (chatBuffer < 0)
				{
					chatBuffer = _chat.Length;
					LogError("Invalid argument");
					return;
				}

				Array.Resize(ref _chat, chatBuffer);
			}
			// ReSharper disable once StringLiteralTypo
			else if (ConfigCommand(commandCi == "BOLDNAMES", ref BoldNames, args)) { }
			// ReSharper disable once StringLiteralTypo
			else if (ConfigCommand(commandCi == "LOGERRORS", ref LogErrors, args)) { }
			// ReSharper disable once StringLiteralTypo
			else if (ConfigCommand(commandCi == "LOGONSEND", ref LogLocalOnSend, args)) { }
			// ReSharper disable once StringLiteralTypo
			else if (ConfigCommand(commandCi == "LOGSYSTEMMESSAGES", ref LogSystemMessages, args)) { }
			// ReSharper disable once StringLiteralTypo
			else if (ConfigCommand(commandCi == "USERICHTTEXT", ref UseRichText, args)) { }
			// ReSharper disable once StringLiteralTypo
			else if (commandCi == "TESTINGCHEATS")
			{
				if (AllowHostToToggleCheats && (!Multiplayer || !Multiplayer.IsConnected || Multiplayer.Me.IsHost()))
				{
					ConfigCommand(ref AllowCheats, args);
				}
				else
				{
					LogError("Not allowed");
				}
			}
			else if (commandCi == "EXECUTE")
			{
				if (!AllowCheats)
				{
					LogError("Not allowed");
					return;
				}

				// get targets
				int i = 0;
				List<ushort> targets = new List<ushort>();
				while (args.Length >= i + 2 && args[i].ToUpperInvariant() == "AS")
				{
					i++;
					string s = args[i].ToUpperInvariant();
					if (s == "ALL" || s == "ALLE" || s == "ALLEXCLUSIVE")
					{
						if (targets.Count == 1)
						{
							if (targets[0] < (ushort)UserId.AllInclusive)
							{
								targets[0] = (ushort)UserId.All;
							}
						}
						else
						{
							targets.Clear();
							targets.Add((ushort)UserId.All);
						}
					}
					else if (s == "ALLI" || s == "ALLINCLUSIVE")
					{
						if (targets.Count == 1 && targets[0] != (ushort)UserId.AllInclusive)
						{
							targets[0] = (ushort)UserId.AllInclusive;
						}
						else
						{
							targets.Clear();
							targets.Add((ushort)UserId.AllInclusive);
						}
					}
					else if (ushort.TryParse(args[i], out ushort id))
					{
						targets.Add(id);
					}
					else
					{
						targets.Add(Multiplayer.GetUser(args[i]).Index);
					}

					i++;
				}

				// command to be executed
				string c = string.Join(" ", args.Skip(i));
				string c2 = "";
				bool multi = false;

				int nextExecute = c.IndexOf("EXECUTE", StringComparison.InvariantCultureIgnoreCase);
				if (nextExecute != -1)
				{
					c2 = c.Substring(nextExecute + 7);
					c = c.Substring(0, nextExecute);
					multi = true;
				}

				// execute command

				if (targets.Count == 0)
				{
					ExecuteCommand(c);
					return;
				}

				if (targets[0] == (ushort)UserId.AllInclusive)
				{
					ExecuteCommand(c);
					targets[0] = (ushort)UserId.All;
				}
				else
				{
					ushort myId = Multiplayer.Me.Index;
					if (targets.Any(id => id == myId))
					{
						targets.Remove(myId);
						ExecuteCommand(c);
					}
				}

				// remote execute
				_outgoingMessages.Add(new ChatEvent('/' + c));

				if (multi)
				{
					_outgoingMessages.Add(new ChatEvent("/EXECUTE " + c2));
				}

				Multiplayer.Sync(this, targets, Reliability);
			}
			else
			{
				foreach (ITextChatCommand c in Commands)
				{
					if (c.IgnoreCase)
					{
						if (string.Equals(command, c.Command, StringComparison.InvariantCultureIgnoreCase))
						{
							Execute();
							return;
						}
					}
					else
					{
						if (command == c.Command)
						{
							Execute();
							return;
						}
					}

					void Execute()
					{
						if (c.IsCheat && !AllowCheats)
						{
							LogError("Not allowed");
							return;
						}

						try
						{
							string s = c.Execute(this, args);
							if (s != null)
								AddChatEventToBuffer(new ChatEvent(s, UseTimeStamps));
						}
						catch (Exception e)
						{
							if (LogFullCommandErrors)
							{
								LogError("Failed to execute command.\n" + e);
							}
							else
							{
								LogError("Failed to execute command.", false);
								Debug.LogError("Failed to execute command.\n" + e);
							}
						}
					}
				}

				LogError("Command not found");
			}
		}

		private bool ConfigCommand(bool condition, ref bool config, string[] args)
		{
			if (condition)
			{
				ConfigCommand(ref config, args);
				return true;
			}

			return false;
		}

		private bool ConfigCommand(ref bool config, string[] args)
		{
			if (args.Length == 0)
			{
				// Get current value.
				AddChatEventToBuffer(new ChatEvent(config.ToString(), UseTimeStamps));
				return true;
			}

			// Set new value.
			return TextChatCommandHelper.TrySetBoolArg(args[0], ref config);
		}

		private bool ConfigCommand(bool condition, ref int config, string[] args)
		{
			if (condition)
			{
				ConfigCommand(ref config, args);
				return true;
			}

			return false;
		}

		private bool ConfigCommand(ref int config, string[] args)
		{
			if (args.Length == 0)
			{
				// Get current value.
				AddChatEventToBuffer(new ChatEvent(config.ToString(), UseTimeStamps));
				return true;
			}

			// Set new value.
			if (Int32.TryParse(args[0], out int v))
			{
				config = v;
				return true;
			}

			LogError("Unable to parse argument");
			return false;
		}

		public override void AssembleData(Writer writer, SerializeInfo info)
		{
			int messageCount = Math.Min(_outgoingMessages.Count, byte.MaxValue);
			writer.Write((byte)messageCount);
			for (int i = 0; i < messageCount; i++)
			{
				if (LogLocalOnSend && !(AllowCommands && _outgoingMessages[i].IsCommand))
				{
					AddChatEventToBuffer(_outgoingMessages[i]);
				}

				_outgoingMessages[i].Write(writer);
			}

			_outgoingMessages.Clear();
		}

		public override void DisassembleData(Reader reader, UnserializeInfo info)
		{
			byte messageCount = reader.ReadByte();
			for (byte i = 0; i < messageCount; i++)
			{
				var e = new ChatEvent(reader);

				// execute received commands
				if (AllowCommands && e.Msg.Length > 0 && e.Msg[0] == '/')
				{
					ExecuteCommandAsAdmin(e.Msg);
					continue;
				}

				AddChatEventToBuffer(e);
			}
		}

		public override string ToString()
		{
			for (int i = chatBuffer - 1; i >= 0; i--)
			{
				_sb2.AppendLine(_chat[i].ToString(this));
			}

			string s = _sb2.ToString();
			_sb2.Clear();
			return s;
		}

		public new void Reset()
		{
			base.Reset();
			Reliability = Reliability.Reliable;
			EnsureEventSystem.Ensure(true);
		}

		private struct ChatEvent
		{
			public readonly ushort SenderId;
			public readonly int TimeStamp;
			public string Msg;
			private bool _compiled;

			public bool IsCommand => Msg.Length > 0 && Msg[0] == '/';

			public ChatEvent(string s, bool time, ushort senderId = UInt16.MaxValue) : this(s, time ? -1 : 0, senderId) { }

			public ChatEvent(string s = "", int time = -1, ushort senderId = UInt16.MaxValue)
			{
				Msg = s;
				SenderId = senderId;
				_compiled = false;
				if (time < 0)
				{
					var localNow = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
					var timeOffset = new DateTimeOffset(localNow, TimeZoneInfo.Utc.GetUtcOffset(localNow));
					TimeStamp = timeOffset.Millisecond + timeOffset.Second * 1000 + timeOffset.Minute * 60000 + timeOffset.Hour * 3600000;
				}
				else
				{
					TimeStamp = time;
				}
			}

			public ChatEvent(Reader reader)
			{
				_compiled = false;
				ChatEventFlags flags = (ChatEventFlags)reader.ReadByte();
				if (flags.HasFlag(ChatEventFlags.SenderId))
				{
					SenderId = reader.ReadUshort();
				}
				else
				{
					SenderId = UInt16.MaxValue;
				}

				if (flags.HasFlag(ChatEventFlags.TimeStamp))
				{
					TimeStamp = reader.ReadInt();
				}
				else
				{
					var localNow = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
					var timeOffset = new DateTimeOffset(localNow, TimeZoneInfo.Utc.GetUtcOffset(localNow));
					TimeStamp = timeOffset.Millisecond + timeOffset.Second * 1000 + timeOffset.Minute * 60000 + timeOffset.Hour * 3600000;
				}

				Msg = reader.ReadString();
			}

			public void Write(Writer writer)
			{
				ChatEventFlags flags = (ChatEventFlags)((SenderId == UInt16.MaxValue ? 0 : 1) | (TimeStamp == -1 || TimeStamp == 0 ? 0 : 2));
				writer.Write((byte)flags);
				if (flags.HasFlag(ChatEventFlags.SenderId))
				{
					writer.Write(SenderId);
				}

				if (flags.HasFlag(ChatEventFlags.TimeStamp))
				{
					writer.Write(TimeStamp);
				}

				writer.Write(Msg);
			}

			public string ToString(TextChatSynchronizable textChat)
			{
				if (string.IsNullOrEmpty(Msg))
				{
					return null;
				}

				if (_compiled || SenderId == UInt16.MaxValue)
				{
					return Msg;
				}

				string name = textChat.Multiplayer?.GetUser(SenderId)?.Name;
				if (string.IsNullOrEmpty(name)) name = "Unknown";

				// Add sender name to message
				if (textChat.UseRichText)
				{
					if (textChat.BoldNames)
					{
						textChat._sb.Append("<b>");
						AppendName(textChat, name, SenderId);
						textChat._sb.Append("</b>");
					}
					else
					{
						AppendName(textChat, name, SenderId);
					}

					static void AppendName(TextChatSynchronizable tc, string n, ushort senderId)
					{
						tc._sb.Append("<color=#");
						tc._sb.Append(ColorUtility.ToHtmlStringRGB(GetUserColor(senderId)));
						tc._sb.Append('>');
						tc._sb.Append(n);
						tc._sb.Append("</color>");
					}
				}
				else
				{
					textChat._sb.Append(name);
				}

				textChat._sb.Append(' ');
				textChat._sb.Append(Msg);
				string msg = textChat._sb.ToString();
				textChat._sb.Clear();

				// Store the user in memory so that it doesn't break when the user leaves
				if (!string.Equals(name, "unknown", StringComparison.InvariantCultureIgnoreCase))
				{
					Msg = msg;
					_compiled = true;
				}

				return msg;
			}

			[Flags]
			private enum ChatEventFlags : byte
			{
				SenderId = 1,
				TimeStamp = 2
			}
		}
	}
}