using System;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Can be implemented to create custom commands.
	/// </summary>
	/// <remarks>
	///	<c>ITextChatCommand</c> is used to create slash commands that can be used with <see cref="TextChatSynchronizable"/>.<br/><br/>
	/// 
	/// There are several built-in commands in Alteruna.
	/// One of them is <c>"/execute"</c>.
	/// It can be used to run command as a different user.<br/>
	/// <c>/execute as allInclusive say Hello everyone!</c><br/>
	/// This would make all players say "Hello everyone!" in the chat.
	/// Note that <c>allInclusive</c> can be replaced by the shorthand <c>alli</c>.
	/// </remarks>
	/// <example>
	///	Here we have an example adding a simple command to the text chat.
	/// <code>
	/// public class CommandPrintLine : ITextChatCommand
	/// {
	///		public string Command { get; } = "printline";
	///		public string Description { get; } = "print a message to local chat.";
	///		public string Usage { get; } = "/printLine &lt;msg&gt;";
	///		public bool IsCheat { get; } = false;
	///		public bool IgnoreCase { get; } = true;
	/// 
	///		// Register command
	///		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	///		public static void Init() =&gt; TextChatSynchronizable.Commands.Add(new CommandPrintLine());
	///
	///		public string Execute(TextChatSynchronizable textChat, string[] args)
	///		{
	///			if (args.Length &lt; 0)
	///			{
	///				// Log error to the chat and return an empty response
	///				textChat.LogError("No message");
	///				return null;
	///			}
	///
	///			// Join arguments as a singular string and return it to log it.
	///			return string.Join(" ", args);
	///		}
	/// }
	/// </code>
	///
	///	Here's an example of accessing MultiplayerManager.
	/// 
	/// <code>
	/// public class DisplayUserCommand : ITextChatCommand
	/// {
	///		public string Command =&gt; "displayuser";
	///		public string Description =&gt; "displays username and ID of the user who used the command.";
	///		public string Usage =&gt; "/displayuser";
	///		public bool IsCheat =&gt; false;
	///		public bool IgnoreCase =&gt; true;
	/// 
	///		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	///		public static void Init() =&gt; TextChatSynchronizable.Commands.Add(new DisplayUserCommand());
	///
	///		public string Execute(TextChatSynchronizable textChat, string[] args)
	///		{
	///			// We can access MultiplayerManager to get the user who ran the command.
	///			User user = textChat.Multiplayer.GetUser();
	///
	///			// We return the message we want the TextChatSynchronizable the send to the chat.
	///			// The message will also be sent to the Unity console.
	///			return $"{user.Name} with ID {user.Index} ran the /displayuser command!";
	///		}
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="TextChatSynchronizable"/>
	public interface ITextChatCommand
	{
		/// <summary>
		/// The written command name. Inputting a slash following with the command name runs the command.
		/// </summary>
		string Command { get; }
		
		/// <summary>
		/// The description that is displayed when running /help.
		/// </summary>
		string Description { get; }
		
		/// <summary>
		/// Displays how the command is written to run properly.
		/// </summary>
		string Usage { get; }
		
		/// <summary>
		/// Determines whether the command is a cheat or not. A cheat can be considered a developer shortcut.
		/// </summary>
		bool IsCheat { get; }
		
		/// <summary>
		/// Determines whether capital letters are taken into consideration or not.
		/// </summary>
		bool IgnoreCase { get; }
		
		/// <summary>
		/// Called when command is executed by a user.
		/// </summary>
		/// <param name="textChat"></param>
		/// <param name="args">Arguments that the user has input after the slash command.</param>
		/// <returns>The message that will be sent to Unity console as well as the text chat.</returns>
		string Execute(TextChatSynchronizable textChat, string[] args);
	}
}

namespace Alteruna.TextChatCommands
{
	[Obsolete("Use Alteruna.Unity.Multiplayer.ITextChatCommand instead")]
	public interface ITextChatCommand : Multiplayer.Unity.ITextChatCommand { }
}