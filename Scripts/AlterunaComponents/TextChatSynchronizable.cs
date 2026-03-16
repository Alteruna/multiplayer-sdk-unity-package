using System;
using UnityEngine;

namespace AlterunaComponents
{
	/// <inheritdoc/>
	[AddComponentMenu("Alteruna/TextChatSynchronizable", 0)]
	public sealed class TextChatSynchronizable : Alteruna.Multiplayer.TextChatSynchronizable { }
}

namespace Alteruna.TextChatCommands
{
	[Obsolete("Use Alteruna.Multiplayer.ITextChatCommand instead")]
	public interface ITextChatCommand : Alteruna.Multiplayer.ITextChatCommand { }
}