using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AlterunaComponents
{
	/// <inheritdoc/>
	[DefaultExecutionOrder(-10)]
	[AddComponentMenu("Alteruna/​Multiplayer Manager", 0)]
	[MovedFrom(true, "Alteruna.Multiplayer", "Alteruna")]
	public sealed class MultiplayerManager : Alteruna.Multiplayer.Unity.MultiplayerManager { }
}