using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AlterunaCompoments
{
	/// <inheritdoc/>
	[AddComponentMenu("Alteruna/Avatar/Avatar", 0)]
	[DisallowMultipleComponent]
	[MovedFrom(true, "Alteruna.Multiplayer", "Alteruna")]
	public sealed class Avatar : Alteruna.Multiplayer.Avatar { }
}