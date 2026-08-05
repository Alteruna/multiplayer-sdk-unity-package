using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Alteruna.Multiplayer.Unity.InputSynchronizable
{
	/// <summary>
	/// Alternative way of implementing <c>InputSynchronizable</c>.
	/// </summary>
	/// <example>
	/// Setup to sync the A key and listen to related event.
	///	You can also just as easy get the value directly from the <c>SyncedKey.Value</c>.
	/// <code>
	///	using UnityEngine;
	///	using UnityEngine.Events;
	///	using Alteruna;
	///	
	///	public class MyInputClass : MonoBehaviour
	///	{
	///		// Reference to a InputSynchronizable.
	///		public InputSynchronizable Input;
	///		// Key field.
	///		private SyncedKey _myKey;
	///	
	///		void Awake()
	///		{
	///			// Setup key.
	///			_myKey = new SyncedKey(Input, KeyCode.A);
	///	
	///			// Listen to key event.
	///			_myKey.OnInputChanged.AddListener(KeyChange);
	///		}
	///	
	///		void KeyChange(SyncedKey key) {
	///			// This is the same value as _myKey.Value.
	///			Debug.Log(key.Value);
	///		}
	///	}
	/// </code>
	/// <br/>
	/// 
	///	SyncedKeys can also be set up by the inspector. but to work, they still need to be registered.
	///	<code>
	/// using UnityEngine;
	///	using Alteruna;
	///	
	///	public class MyJumpClass : MonoBehaviour
	///	{
	///		// Reference to a InputSynchronizable.
	///		public InputSynchronizable Input;
	///		// Jump input that we can setup in the inspector.
	///		public SyncedKey Jump;
	///		// Jump force.
	///		public float jumpForce = 10f;
	///	
	///		void Awake()
	///		{
	///			Jump.Register(Input);
	///		}
	///	
	///		private void Update()
	///		{
	///			if (Jump)
	///			{
	///				transform.Translate(0, Time.deltaTime * jumpForce, 0);
	///			}
	///		}
	///	}
	/// </code>
	/// <img src="../images/Alteruna.SyncedKey.MyJumpClass.png" alt="Inspector setup of SyncedAxis"/>
	/// </example>
	/// <seealso cref="InputSynchronizable"/>
	/// <seealso cref="SyncedAxis"/>
	[Serializable]
	public class SyncedKey
	{
		/// <summary>
		/// Registered Keycode input.
		/// On set, reregister if already registered.
		/// </summary>
		public KeyCode Key
		{
			get => _key;
			set
			{
				if (_isRegistered)
				{
					Register(InputManager, value);
				}
				else
				{
					_key = value;
				}
			}
		}

		/// <summary>
		/// key mode.
		/// </summary>
		public KeyMode mode = KeyMode.KeyPress;

		/// <summary>
		/// The raw value of target key unaffected by mode.
		/// </summary>
		[NonSerialized] public bool KeyState;

		/// <summary>
		/// Max time between taps for a valid double tap for the key mode doubleTap
		/// </summary>
		[Min(0.2f), Tooltip("Max time between taps for a valid double tap for the key mode doubleTap or toggleDoubleTap (in seconds)")]
		public float DoubleTapTime = 0.5f;

		/// <summary>
		/// Invokes when value get changed.
		/// </summary>
#if UNITY_2019
		public UnityEvent<SyncedKey> OnInputChanged = new SyncedKeyEvent();
#else
		public UnityEvent<SyncedKey> OnInputChanged = new UnityEvent<SyncedKey>();
#endif
		
#if UNITY_2019
		[Serializable]
		public class SyncedKeyEvent : UnityEvent<SyncedKey> { }
#endif

		/// <summary>
		/// Connected <c>IInput</c>.
		/// </summary>
		[NonSerialized] public IInput InputManager;

		/// <summary>
		/// Value of target input key.
		/// </summary>
		public bool Value
		{
			get
			{
				switch (mode)
				{
					case KeyMode.DoubleTap:
						if (_value)
						{
							_value = false;
							return true;
						}

						return false;
					case KeyMode.KeyDown:
						if (!_value && KeyState)
						{
							return _value = true;
						}

						return false;
					case KeyMode.KeyUp:
						if (!_value && !KeyState)
						{
							return _value = true;
						}

						return false;
					default:
						return _value;
				}
			}
		}

		[NonSerialized] private bool _value;
		[NonSerialized] private bool _isRegistered;
		[NonSerialized] private int _index;
		[NonSerialized] private float _lastPress;

		[SerializeField]
		private KeyCode _key = KeyCode.None;

		/// <summary>
		/// Register key and mode.
		/// </summary>
		/// <param name="inputManager">Target IInput.</param>
		/// <param name="key">Target key.</param>
		/// <param name="keyMode">Target key mode.</param>
		public SyncedKey(IInput inputManager, KeyCode key = KeyCode.None, KeyMode keyMode = KeyMode.KeyPress)
		{
			mode = keyMode;
			Register(inputManager, key);
		}

		/// <summary>
		/// Set key and mode without registering. 
		/// </summary>
		/// <param name="key">Target key.</param>
		/// <param name="keyMode">Target key mode.</param>
		public SyncedKey(KeyCode key, KeyMode keyMode = KeyMode.KeyPress)
		{
			_key = key;
			mode = keyMode;
		}

		/// <summary>
		/// Constructor for default values.
		/// </summary>
		public SyncedKey() { }

		/// <summary>
		/// Register key to a previously set <c>IInput</c>.
		/// </summary>
		public void Register()
		{
			if (InputManager == null)
			{
				throw new NullReferenceException("IInput cannot be null.");
			}

			Register(InputManager);
		}

		/// <summary>
		/// Register key to target <c>IInput</c>.
		/// </summary>
		/// <param name="inputManager">Target IInput.</param>
		public void Register(IInput inputManager, KeyCode key)
		{
			if (inputManager == InputManager && key == _key) return;
			_key = key;
			Register(inputManager);
		}

		/// <summary>
		/// Register key to target <c>IInput</c>.
		/// </summary>
		/// <param name="inputManager">Target IInput.</param>
		public void Register(IInput inputManager)
		{
			if (_isRegistered)
			{
				InputManager.OnKeyUpdate.RemoveListener(OnKey);
			}

			if (_key == KeyCode.None)
			{
				Deregister();
				InputManager = inputManager;
				return;
			}

			InputManager = inputManager;

			if (!inputManager.TryGetIndexOfKey(_key, out _index))
			{
				_index = inputManager.KeyValues.Length;
				inputManager.AddKey(_key);
			}

			if (!_isRegistered)
			{
				inputManager.OnKeyUpdate.AddListener(OnKey);
			}

			_value = KeyState = mode == KeyMode.KeyPress && InputManager.KeyValues[_index];

			_isRegistered = true;
		}


		/// <summary>
		/// Deregister from <c>IInput</c>.
		/// </summary>
		public void Deregister()
		{
			if (_isRegistered && InputManager != null)
			{
				InputManager.OnKeyUpdate.RemoveListener(OnKey);
			}

			_value = false;
			KeyState = false;
			_isRegistered = false;
		}

		private void OnKey(KeyCode key, bool status)
		{
			if (key != _key) return;
			KeyState = status;
			switch (mode)
			{
				case KeyMode.KeyPress:
					_value = status;
					break;
				case KeyMode.ToggleKeyDown:
					if (!status) return;
					_value = !_value;
					break;
				case KeyMode.ToggleKeyUp:
					if (status) return;
					_value = !_value;
					break;
				case KeyMode.ToggleDoubleTap:
					if (!status) return;
					if (_lastPress + DoubleTapTime >= Time.time)
					{
						_lastPress = 0;
						_value = !_value;
					}
					else
					{
						_lastPress = Time.time;
					}

					break;
				case KeyMode.KeyDown:
					if (!status) _value = false;
					break;
				case KeyMode.KeyUp:
					if (status) _value = false;
					break;
				case KeyMode.DoubleTap:
					if (!status) return;
					if (_lastPress + DoubleTapTime >= Time.time)
					{
						_lastPress = 0;
						_value = true;
					}
					else
					{
						_lastPress = Time.time;
					}

					break;
			}

			OnInputChanged.Invoke(this);
		}

		/// <summary>
		/// Get value of key.
		/// </summary>
		public static implicit operator bool(SyncedKey key) => key.Value;

		/// <summary>
		/// Get value of key.
		/// </summary>
		public static implicit operator int(SyncedKey key) => key.Value ? 1 : 0;

		/// <summary>
		/// Get value of key.
		/// </summary>
		public static implicit operator float(SyncedKey key) => key.Value ? 1f : 0f;

		/// <summary>
		/// Key behavior mode
		/// </summary>
		public enum KeyMode
		{
			/// <summary>
			/// True during the frame the user pressing down the key for the second time withing time defined in <c>DoubleTapTime</c>.
			/// </summary>
			DoubleTap,

			/// <summary>
			/// True during the frame the user starts pressing down the key.
			/// </summary>
			KeyDown,

			/// <summary>
			/// True while the user holds down the key.
			/// </summary>
			KeyPress,

			/// <summary>
			/// True during the frame the user releases the key.
			/// </summary>
			KeyUp,

			/// <summary>
			/// True during the frame the user pressing down the key for the second time withing time defined in <c>DoubleTapTime</c>.
			/// </summary>
			ToggleDoubleTap,

			/// <summary>
			/// Toggles the value when user starts pressing down the key.
			/// </summary>
			ToggleKeyDown,

			/// <summary>
			/// Toggles the value when user releases the key.
			/// </summary>
			ToggleKeyUp
		}
	}
}