using System;
using System.Collections.Generic;
using System.Linq;
using Alteruna.Multiplayer.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using Alteruna.UnityEditor.MultiClientSimulation;
#endif


namespace Alteruna.Multiplayer.Unity.InputSynchronizable
{
	/// <summary>
	/// Synchronize inputs (255 buttons and 255 axis maximum)
	/// The input vales will update on this and other clients simultaneously.
	/// </summary>
	/// <remarks>
	///	<img src="../images/Alteruna.InputSynchronizable.png" />
	/// </remarks>
	/// <example>
	/// Sync inputs and move transform based on those inputs.
	/// Note that this does not sync position, after a while the positions could become unsynced.
	/// <code>
	/// using UnityEngine;
	/// using Alteruna;
	/// 
	/// public class SyncedPlayerMovement : MonoBehaviour
	/// {
	///		//reference to a InputSynchronizable object in the scene with a avatar.
	///		public InputSynchronizable InputSync;
	///		public float Speed = 5;
	///	
	///		private void Start() {
	///			InputSync.AddAxis(new[] {"Horizontal", "Vertical"});
	///		}
	///	
	///		private void Update() {
	///			float scaledSpeed = Speed * Time.deltaTime;
	///			transform.Translate(
	///				scaledSpeed * InputSync.AxesValues[0],
	///				scaledSpeed * InputSync.AxesValues[1],
	///				0);
	///		}
	///	 }
	/// </code>
	/// </example>
	/// <seealso cref="SyncedKey"/>
	/// <seealso cref="SyncedAxis"/>
	[AddComponentMenu("Alteruna/Avatar/Input Synchronizable"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class InputSynchronizable : CommunicationBridge, IInput
	{
		/// <summary>
		/// Get synced button values by index
		/// </summary>
		public bool[] KeyValues => _currentKeys;
		
		/// <summary>
		/// Get synced axes values by index
		/// </summary>
		public float[] AxesValues => _currentAxes;

		/// <summary>
		/// Whether to use local input or use reply as input.
		/// When false, all clients including the sender will receive inputs simultaneously. (assuming identical connection)
		/// </summary>
		[Tooltip("When false, all clients including the sender will receive inputs simultaneously. (assuming identical connection)")]
		public bool UseLocalInput = true;

		/// <summary>
		/// List of KeyCodes to track.
		/// </summary>
		[SerializeField] private KeyCode[] keys = Array.Empty<KeyCode>();

		/// <summary>
		/// List of axes to track.
		/// </summary>
		[SerializeField] private string[] axes = Array.Empty<string>();

		/// <summary>
		/// Event for changes in key inputs.
		/// passes <c>KeyCode</c> and state.
		/// </summary>
		public UnityEvent<KeyCode, bool> OnKeyUpdate => _onKeyUpdate;

		[FormerlySerializedAs("OnKeyUpdate"), SerializeField]
		private UnityEvent<KeyCode, bool> _onKeyUpdate;
		
		/*
		/// <summary>
		/// Ignore all inputs.
		/// </summary>
		[NonSerialized]
		public bool LockInput;
		*/

		[NonSerialized] internal User Possesor;

		[NonSerialized] private bool[] _localKeys = Array.Empty<bool>();
		[NonSerialized] private float[] _localAxes = Array.Empty<float>();

		[NonSerialized] private bool[] _currentKeys = Array.Empty<bool>();
		[NonSerialized] private float[] _currentAxes = Array.Empty<float>();

		[NonSerialized] private byte _keysLength;
		[NonSerialized] private byte _axisLength;

		[NonSerialized] private readonly List<byte> _data = new List<byte>();

		[NonSerialized] private string _procedureName;

		/// <summary>
		/// Add a key to the <c>InputSynchronizable</c>
		/// </summary>
		/// <param name="keyCode"><c>KeyCode</c> of the target key</param>
		public void AddKey(KeyCode keyCode)
		{
			if (_keysLength >= 255)
			{
				throw new IndexOutOfRangeException("Cannot have more than 255 registered keys");
			}

			keys = keys.Concat(new[] { keyCode }).ToArray();
			_localKeys = _localKeys.Concat(new bool[1]).ToArray();
			_currentKeys = _currentKeys.Concat(new bool[1]).ToArray();
			_keysLength++;
		}

		/// <summary>
		/// Add a array of keys to the <c>InputSynchronizable</c>
		/// </summary>
		/// <param name="keyCodes">Array of <c>KeyCode</c> to target</param>
		public void AddKey(KeyCode[] keyCodes)
		{
			int l = keyCodes.Length;
			if (_keysLength + l > 255)
			{
				throw new IndexOutOfRangeException("Cannot have more than 255 registered keys");
			}

			keys = keys.Concat(keyCodes).ToArray();
			_localKeys = _localKeys.Concat(new bool[l]).ToArray();
			_currentKeys = _currentKeys.Concat(new bool[l]).ToArray();
			_keysLength += (byte)l;
		}

		/// <summary>
		/// Add a axis to the <c>InputSynchronizable</c>
		/// </summary>
		/// <param name="newAxis">string of the target axis</param>
		public void AddAxis(string newAxis)
		{
			if (_axisLength >= 255)
			{
				throw new IndexOutOfRangeException("Cannot have more than 255 registered axes");
			}

			axes = axes.Concat(new[] { newAxis }).ToArray();
			_localAxes = _localAxes.Concat(new float[1]).ToArray();
			_currentAxes = _currentAxes.Concat(new float[1]).ToArray();
			_axisLength++;
		}

		/// <summary>
		/// Add a array of axes to the <c>InputSynchronizable</c>
		/// </summary>
		/// <param name="newAxes">strings of the target axes</param>
		public void AddAxis(string[] newAxes)
		{
			int l = newAxes.Length;
			if (_keysLength + l > 255)
			{
				throw new IndexOutOfRangeException("Cannot have more than 255 registered axes");
			}

			axes = axes.Concat(newAxes).ToArray();
			_localAxes = _localAxes.Concat(new float[l]).ToArray();
			_currentAxes = _currentAxes.Concat(new float[l]).ToArray();
			_axisLength += (byte)l;
		}

		/// <summary>
		/// Get index of a registered <c>keyCode</c>.
		/// If the target <c>keyCode</c> dos not exist it returns <c>-1</c>
		/// </summary>
		/// <param name="keyCode">target</param>
		/// <returns><c>index</c> on success, <c>-1</c> on fail.</returns>
		public int GetIndexOfKey(KeyCode keyCode)
		{
			for (byte i = 0; i < _keysLength; i++)
			{
				if (keys[i] == keyCode)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Attempts to get index of a registered <c>keyCode</c>.
		/// If the target <c>keyCode</c> dos not exist, return <c>false</c> and <c>index</c> will be 0
		/// </summary>
		/// <param name="keyCode">target</param>
		/// <param name="index">Index of target registered <c>keyCode</c></param>
		/// <returns>True on success</returns>
		public bool TryGetIndexOfKey(KeyCode keyCode, out int index)
		{
			index = GetIndexOfKey(keyCode);
			if (index == -1)
			{
				index = 0;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Get index of a registered <c>keyCode</c>.
		/// If the target <c>keyCode</c> dos not exist it returns <c>-1</c>
		/// </summary>
		/// <param name="targetAxis">target</param>
		/// <returns><c>index</c> on success, <c>-1</c> on fail.</returns>
		public int GetIndexOfAxis(string targetAxis)
		{
			for (byte i = 0; i < _axisLength; i++)
			{
				if (axes[i] == targetAxis)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Attempts to get index of a registered <c>keyCode</c>.
		/// If the target <c>keyCode</c> dos not exist, return <c>false</c> and <c>index</c> will be 0
		/// </summary>
		/// <param name="targetAxis">target</param>
		/// <param name="index">Index of target registered <c>keyCode</c></param>
		/// <returns>True on success</returns>
		public bool TryGetIndexOfAxis(string targetAxis, out int index)
		{
			index = GetIndexOfAxis(targetAxis);
			if (index == -1)
			{
				index = 0;
				return false;
			}

			return true;
		}

		public void Awake()
		{
#if UNITY_EDITOR
			ClientDisplayWindow.InputComponent = GetType();
#endif
			InternalAwake();
		}

		private void InternalAwake()
		{
			_keysLength = (byte)keys.Length;
			_axisLength = (byte)axes.Length;
			_localKeys = new bool[_keysLength];
			_currentKeys = new bool[_keysLength];
			_localAxes = new float[_axisLength];
			_currentAxes = new float[_axisLength];
		}

		public override void Possessed(bool isMe, User user)
		{
			if (Possesor == user) return;
			
			Possesor = user;
			enabled = isMe;
			// Creating a unique name for the procedure.
			_procedureName = "InputSync" + user.Index;
			SetMultiplayerComponent();
			Multiplayer.RegisterRemoteProcedure(_procedureName, ReceiveInput);
		}
		
		public override void Unpossessed()
		{
			enabled = false;
		}
		
		/*
		/// <summary>
		/// Set all inputs to default values and lock any future inputs.
		/// </summary>
		/// see <see cref="LockInput"/>
		public void ReleaseAndLockInput()
		{
			LockInput = true;
			
			_data.Clear();
			
			for (byte i = 0; i < _keysLength; i++)
			{
				if (_localKeys[i])
				{
					_data.AddRange(new[]
					{
						i, (byte)4
					});
					_localKeys[i] = false;
					if (UseLocalInput)
					{
						_currentKeys[i] = false;
						OnKeyUpdate.Invoke(keys[i], false);
					}
				}
			}
			
			for (byte i = 0; i < _axisLength; i++)
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (0 != _localAxes[i])
				{
					_data.AddRange(new[]
					{
						i, (byte)1 
					});
					_localAxes[i] = 0;
					if (UseLocalInput)
					{
						_currentAxes[i] = 0;
					}
				}
			}

			if (_data.Count > 0)
			{
				var args = new ProcedureParameters();
				args.Set("input", _data.ToArray());
				Multiplayer.InvokeRemoteProcedure(_procedureName, UseLocalInput ? UserId.All : UserId.AllInclusive, args);
			}
		}
		*/

		public void Update() => InternalUpdate();

		private void InternalUpdate()
		{
			//if (LockInput) return;
			
			_data.Clear();
			// check for changes in key inputs
			for (byte i = 0; i < _keysLength; i++)
			{
				bool key = Input.GetKey(keys[i]);
				if (key != _localKeys[i])
				{
					_data.AddRange(new[]
					{
						i, key ? (byte)5 : (byte)4
					});
					_localKeys[i] = key;
					if (UseLocalInput)
					{
						_currentKeys[i] = key;
						OnKeyUpdate.Invoke(keys[i], key);
					}
				}
			}

			// check for changes in axis inputs
			for (byte i = 0; i < _axisLength; i++)
			{
				float key = Input.GetAxisRaw(axes[i]);
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (key != _localAxes[i])
				{
					_data.AddRange(new[]
					{
						i, key == 0f ? (byte)1 : key > 0f ? (byte)2 : (byte)0
					});
					_localAxes[i] = key;
					if (UseLocalInput)
					{
						_currentAxes[i] = key;
					}
				}
			}

			if (_data.Count > 0 && Multiplayer)
			{
				var args = new ProcedureParameters();
				args.Set("input", _data.ToArray());
				Multiplayer.InvokeRemoteProcedure(_procedureName, UseLocalInput ? UserId.All : UserId.AllInclusive, args);
			}
		}

		private void ReceiveInput(ushort fromUser, ProcedureParameters parameters, uint callId,
			ITransportStreamReader processor) => ReceiveInput(parameters.Get("input", Array.Empty<byte>()));

		private void ReceiveInput(byte[] data)
		{
			for (int i = 0, l = data.Length; i < l; i += 2)
			{
				if ((data[i + 1] & 4) == 0)
				{
					_currentAxes[data[i]] = data[i + 1] - 1;
				}
				else
				{
					byte le = data[i];
					OnKeyUpdate.Invoke(keys[le], _currentKeys[le] = (data[i + 1] & 1) == 1);
				}
			}
		}
	}
}