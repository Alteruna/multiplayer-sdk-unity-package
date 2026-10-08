using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Ensures the scene has an <c>EventSystem</c>, creating one with the appropriate input module if missing.
	/// </summary>
	/// <remarks>
	/// As an alternative to put Event System in prefabs, this component will create an EventSystem in the scene if one is not yet present.
	/// </remarks>
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
			
#if UNITY_6000_0_OR_NEWER
			bool EventSystemExists = FindFirstObjectByType<EventSystem>() == null;
#else
			bool EventSystemExists = FindObjectOfType<EventSystem>() == null;
#endif
			
			// Check if there is already an EventSystem in the scene
			if (EventSystemExists)
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