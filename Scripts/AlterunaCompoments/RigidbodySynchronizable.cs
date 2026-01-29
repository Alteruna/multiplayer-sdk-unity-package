using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AlterunaCompoments
{
	/// <inheritdoc/>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody))]
	[AddComponentMenu("Alteruna/Transform/Rigidbody Synchronizable")]
	[MovedFrom(true, "Alteruna.Multiplayer", "Alteruna")]
	public sealed class RigidbodySynchronizable : Alteruna.Multiplayer.RigidbodySynchronizable { }
}