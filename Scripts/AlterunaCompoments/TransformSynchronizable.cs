using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AlterunaCompoments
{
	/// <inheritdoc/>
	[AddComponentMenu("Alteruna/Transform/Transform Synchronizable", 0)]
	[DisallowMultipleComponent]
	[MovedFrom(true, "Alteruna.Multiplayer", "Alteruna")]
	public sealed class TransformSynchronizable : Alteruna.Multiplayer.TransformSynchronizable { }
}