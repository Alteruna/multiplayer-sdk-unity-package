using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AlterunaCompoments
{
	/// <inheritdoc/>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody2D))]
	[AddComponentMenu("Alteruna/Transform/Rigidbody 2D Synchronizable")]
	[MovedFrom(true, "Alteruna.Multiplayer", "Alteruna")]
	public sealed class Rigidbody2DSynchronizable : Alteruna.Multiplayer.Rigidbody2DSynchronizable { }
}