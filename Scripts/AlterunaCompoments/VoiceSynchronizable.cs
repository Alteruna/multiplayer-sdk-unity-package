using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AlterunaCompoments
{
	/// <inheritdoc/>
	[DisallowMultipleComponent]
	[AddComponentMenu("Alteruna/Audio/Voice Synchronizable")]
	[MovedFrom(true, "Alteruna.Multiplayer", "Alteruna")]
	public sealed class VoiceSynchronizable : Alteruna.Multiplayer.VoiceSynchronizable { }
}