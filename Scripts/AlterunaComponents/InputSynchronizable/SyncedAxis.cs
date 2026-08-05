using System;
using System.Reflection;
using UnityEngine;

namespace Alteruna.Multiplayer.Unity.InputSynchronizable
{
	/// <summary>
	/// Alternative way of implementing <c>InputSynchronizable</c>.
	/// </summary>
	/// <example>
	/// We can setup a SyncedAxis in the inspector and register it in the Awake method.<br/>
	/// Setup to sync the Horizontal axis and use its value.
	/// <code>
	///	using Alteruna;
	///	using UnityEngine;
	///
	///	[RequireComponent(typeof(InputSynchronizable))]
	///	public class InputTest : MonoBehaviour
	///	{
	///		public float Speed = 5;
	///
	///		public SyncedAxis AxisX = new SyncedAxis("Horizontal");
	///		public SyncedAxis AxisY = new SyncedAxis("Vertical");
	///
	///		private InputSynchronizable _input;
	///	
	///		void Awake()
	///		{
	///			if (_input == null)
	///				_input = GetComponent&lt;InputSynchronizable&gt;();
	///		
	///			AxisX.Register(_input);
	///			AxisY.Register(_input);
	///		}
	///
	///		void FixedUpdate()
	///		{
	///			float scaledSpeed = Speed * Time.deltaTime;
	///			transform.Translate(
	///				scaledSpeed * AxisX.Value,
	///				scaledSpeed * AxisY.Value,
	///				0);
	///		}
	///
	///		private void Reset()
	///		{
	///			if (_input == null)
	///				_input = GetComponent&lt;InputSynchronizable&gt;();
	///		}
	///	}
	/// </code>
	/// <img src="../images/Alteruna.SyncedAxis.InputTest.png" alt="Inspector setup of SyncedAxis"/>
	/// </example>
	/// <seealso cref="InputSynchronizable"/>
	/// <seealso cref="SyncedKey"/>
	[Serializable]
	public class SyncedAxis
	{
		/// <summary>
		/// Target axis.
		/// </summary>
		public string Axis
		{
			get => _axis;
			set
			{
				_axis = value;
				if (_isRegistered)
				{
					Register(InputManager);
				}
			}
		}

		/// <summary>
		/// Raw value of axis.
		/// </summary>
		public float Value => _isRegistered ? InputManager.AxesValues[_index] : 0f;

		/// <summary>
		/// Connected <c>IInput</c>.
		/// </summary>
		[NonSerialized] public IInput InputManager;

		[NonSerialized] private bool _isRegistered;
		[NonSerialized] private int _index;

		[SerializeField]
		private string _axis;

		/// <summary>
		/// Register axis.
		/// </summary>
		/// <param name="inputManager"></param>
		/// <param name="axis"></param>
		public SyncedAxis(IInput inputManager, string axis = "None")
		{
			_axis = axis;
			Register(inputManager);
		}

		/// <summary>
		/// Set axis without registering.
		/// </summary>
		/// <param name="axis">Target axis.</param>
		public SyncedAxis(string axis)
		{
			_axis = axis;
		}

		/// <summary>
		/// Register key to target <c>IInput</c>.
		/// </summary>
		/// <param name="inputManager">Target IInput.</param>
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
		public void Register(IInput inputManager, string axis)
		{
			if (inputManager == InputManager && _axis == axis) return;
			_axis = axis;
			Register(inputManager);
		}

		/// <summary>
		/// Register key on target <c>IInput</c>.
		/// </summary>
		/// <param name="inputManager">Target IInput.</param>
		public void Register(IInput inputManager)
		{
			InputManager = inputManager;

			if (_axis == String.Empty || _axis.ToLower() == "none")
			{
				Deregister();
				return;
			}

			if (!inputManager.TryGetIndexOfAxis(_axis, out _index))
			{
				_index = inputManager.AxesValues.Length;
				inputManager.AddAxis(_axis);
			}

			_isRegistered = true;
		}

		/// <summary>
		/// Deregister from <c>IInput</c>.
		/// </summary>
		public void Deregister()
		{
			_isRegistered = false;
		}

		public static implicit operator bool(SyncedAxis axis) => axis._isRegistered && axis.InputManager.AxesValues[axis._index] != 0;

		public static implicit operator int(SyncedAxis axis) => (int)axis.Value;

		public static implicit operator float(SyncedAxis axis) => axis.Value;
	}
}