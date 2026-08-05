using Alteruna.Multiplayer.Core;
using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Instantiate a prefab as a child from an array.
	/// If the avatar index goes beyond the length of the array, it will loop.
	/// </summary>
	/// <remarks>
	///	<img src="../images/Alteruna.UniqueAvatarChild.png" />
	/// </remarks>
	[HelpURL("https://docs.v2.alteruna.com/html/T_Alteruna_Multiplayer_Unity_UniqueAvatarChild.htm")]
	[AddComponentMenu("Alteruna/Avatar/Unique Avatar Child"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class UniqueAvatarChild : CommunicationBridge
	{
		/// <summary>
		/// The array of prefabs to spawn as children.
		/// When index exceeds the length, loop.
		/// </summary>
		public GameObject[] Prefabs;

		private GameObject _prefab;
		private int _lastId = -1;

		public override void Possessed(bool isMe, User user)
		{
			SetPrefab(user);
		}

		/// <summary>
		/// Set child prefab to target id. Wraps around if <c>Prefabs</c> is less than id.
		/// If child prefab already is set, replace it.
		/// </summary>
		/// <param name="id">User index</param>
		public void SetPrefab(ushort id)
		{
			// If the correct prefab already have been spawned
			bool prefabExist = _prefab;
			if (_lastId == id && prefabExist)
			{
				//id unchanged and prefab already exists
				return;
			}

			// if wrong prefab exist, destroy it.
			if (prefabExist)
			{
				Destroy(_prefab);
			}
			
			InstantiateAndSetLayer(Prefabs[id % Prefabs.Length]);
		}
		
		/// <summary>
		/// Set child prefab to target user's index. Wraps around if <c>Prefabs</c> is less than id.
		/// If child prefab already is set, replace it.
		/// </summary>
		/// <param name="user">target index</param>
		public void SetPrefab(User user) => SetPrefab(user.Index);

		/// <summary>
		/// Instantiate a new child prefab and destroy exising object.
		/// </summary>
		/// <param name="obj">prefab or object ti use as new child</param>
		public void OverwritePrefab(GameObject obj)
		{
			// if wrong prefab exist, destroy it.
			if (_prefab)
			{
				Destroy(_prefab);
			}

			_lastId = -1;

			InstantiateAndSetLayer(obj);
		}

		private void InstantiateAndSetLayer(GameObject obj)
		{
			_prefab = Instantiate(obj, transform);
			int layer = transform.gameObject.layer;
			_prefab.gameObject.layer = layer;
			SetLayerOnAllChildren(_prefab.transform, layer);
		}

		/// <summary>
		/// Get current avatar child object.
		/// </summary>
		/// <returns>avatar child game object</returns>
		public GameObject GetAvatarChild() => _prefab;

		/// <summary>
		/// Attempt to get current avatar child object.
		/// </summary>
		/// <param name="avatarChild">avatar child game object</param>
		/// <returns>true when avatar child exists</returns>
		public bool TryGetAvatarChild(out GameObject avatarChild)
		{
			if (_prefab)
			{
				avatarChild = _prefab;
				return true;
			}

			avatarChild = null;
			return false;
		}
		
		private void SetLayerOnAllChildren(Transform obj, int layer)
		{
			foreach (Transform child in obj)
			{
				child.gameObject.layer = layer;
                
				if (child.childCount > 0)
					SetLayerOnAllChildren(child, layer);
			}
		}
	}
}