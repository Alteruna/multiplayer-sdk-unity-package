using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AlterunaComponents
{
	/// <inheritdoc/>
	[DisallowMultipleComponent]
	[AddComponentMenu("Alteruna/Audio/Voice Synchronizable")]
	[MovedFrom(true, "Alteruna.Multiplayer", "Alteruna")]
	public sealed class VoiceSynchronizable : Alteruna.Multiplayer.VoiceSynchronizable { }
}