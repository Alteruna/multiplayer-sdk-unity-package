using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

namespace Alteruna.Multiplayer.Unity
{
	[ExecuteInEditMode, UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class EnsureEventSystem : MonoBehaviour
	{
		
#if UNITY_EDITOR
		private void Awake()
		{
			Ensure(true);
		}
#endif

		/// <summary>
		/// Ensures that there is an EventSystem in the scene.
		/// </summary>
		/// <returns>True when new EventSystem was created.</returns>
		public static bool Ensure(bool onlyInEditor = false)
		{
			if (onlyInEditor && !Application.isEditor)
			{
				return false;
			}
			
			// Check if there is already an EventSystem in the scene
			if (Object.FindObjectOfType<EventSystem>() == null)
			{
				// Create a new GameObject
				GameObject eventSystem = new GameObject("EventSystem");

				// Add EventSystem component
				eventSystem.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
	            // Add InputSystemUIInputModule component for the new Input System
	            eventSystem.AddComponent<InputSystemUIInputModule>();
				Debug.Log("InputSystemUIInputModule added.");
#else
				// Add StandaloneInputModule component for the old Input System
				eventSystem.AddComponent<StandaloneInputModule>();
				Debug.Log("StandaloneInputModule added.");
#endif
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}