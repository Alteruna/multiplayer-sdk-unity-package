using System;
using System.Collections.Generic;
using System.Reflection;
using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Core.MethodArguments;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using Alteruna.UnityEditor.MultiClientSimulation;
#endif

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Synchronizes input actions across multiple users in a room using the new input system.
	/// </summary>
	/// <example>
	/// This example demonstrates how to use the <see cref="NewInputSync"/> component to synchronize an input action for horizontal movement.
	/// <code>
	/// using UnityEngine;
	/// using Alteruna.Multiplayer;
	/// 
	/// [RequireComponent(typeof(NewInputSync))]
	/// public class NewInputSyncTest : MonoBehaviour
	/// {
	/// 	public float MoveSpeed = 5f;
	/// 
	/// 	private NewInputSync _inputSync;
	/// 
	/// 	// Type specific action
	/// 	private NewInputSync.InputActionSync.Action&lt;float&gt; _horizontal;
	/// 
	/// 	// Unspecified type action
	/// 	private NewInputSync.InputActionSync _vertical;
	/// 
	/// 	private void Start()
	/// 	{
	/// 		var inputSync = GetComponent&lt;NewInputSync&gt;();
	/// 		// Get casted action
	/// 		_horizontal = inputSync.FindAction&lt;float&gt;("Horizontal");
	/// 		// Get unspecified type action
	/// 		_vertical = inputSync.FindAction("Vertical");
	/// 	}
	/// 
	/// 	private void Update()
	/// 	{
	/// 		transform.Translate(
	/// 			// Get value directly from the action
	/// 			_horizontal.GetValue() * MoveSpeed * Time.deltaTime,
	/// 			// Attempt to get value from the unspecified type action as float
	/// 			_vertical.GetValue&lt;float&gt;() * MoveSpeed * Time.deltaTime,
	/// 			0
	/// 		);
	/// 	}
	/// }
	/// </code>
	/// </example>
	[AddComponentMenu("Alteruna/Avatar/New Input Synchronizable"), DefaultExecutionOrder(-1)]
	public class NewInputSync : Synchronizable
	{
		public InputActionAsset inputActions;

		[NonSerialized] private InputActionSync[] _actions = Array.Empty<InputActionSync>();

		[NonSerialized] private bool local;
		[NonSerialized] private bool pending;

		[NonSerialized] private float t = 0;

#if UNITY_EDITOR
		public void Awake()
		{
			ClientDisplayWindow.InputComponent = GetType();
		}
#endif

		public void Start()
		{
			if (Multiplayer == null)
			{
				// offline mode.
				inputActions.Enable();
				enabled = false;
			}
		}

		void Update()
		{
			// only run on the local player
			if (!local) return;
			foreach (InputActionSync.ILocalAction action in _actions)
			{
				if (!action.Pending && action.Updated)
				{
					action.Pending = true;
					pending = true;
				}
			}

			if (t > 0)
			{
				t -= Time.unscaledDeltaTime;
			}

			if (t <= 0 && pending)
			{
				Multiplayer.Sync(this);
				t = 0.0083f; // Delay before sending the next input action
				pending = false;
			}
		}

		private void LateUpdate()
		{
			// only run on remote players
			if (local) return;
			foreach (InputActionSync action in _actions)
			{
				action.Updated = false;
			}
		}


		/// <summary>
		/// Finds the unique identifier of an input action by its name or identifier string.
		/// </summary>
		/// <param name="actionNameOrId">The name or string identifier of the input action to find.</param>
		/// <param name="throwIfNotFound">Specifies whether to throw an exception if the action is not found. Defaults to false.</param>
		/// <returns>The unique identifier (GUID) of the input action if found; otherwise, behavior depends on the value of <paramref name="throwIfNotFound"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="actionNameOrId"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="throwIfNotFound"/> is true and the
		/// action could not be found. -Or- If <paramref name="actionNameOrId"/> contains a slash but is missing
		/// either the action or the map name.</exception>
		public Guid FindActionId(string actionNameOrId, bool throwIfNotFound = false) =>
			inputActions.FindAction(actionNameOrId, throwIfNotFound).id;

		/// <summary>
		/// Retrieves an input action synchronization object corresponding to the specified unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the input action to retrieve.</param>
		/// <returns>The <see cref="InputActionSync"/> object associated with the specified identifier.</returns>
		/// <exception cref="ArgumentException">Thrown when no input action matches the specified identifier.</exception>
		public InputActionSync FindAction(Guid id)
		{
			foreach (var action in _actions)
			{
				if (action.Id == id)
				{
					return action;
				}
			}

			throw new ArgumentException($"Input action with ID {id} not found.", nameof(id));
		}

		/// <summary>
		/// Finds an input action synchronization object by its name or identifier string.
		/// </summary>
		/// <param name="actionNameOrId">The name or string identifier of the input action to find.</param>
		/// <returns>The <see cref="InputActionSync"/> object associated with the specified name or identifier.</returns>
		/// <exception cref="ArgumentException">Thrown when no input action matches the specified name or identifier.</exception>
		public InputActionSync FindAction(string actionNameOrId)
		{
			return FindAction(FindActionId(actionNameOrId, true));
		}

		/// <summary>
		/// Finds the synchronized input action associated with the specified name or identifier string and casts it to the specified type.
		/// </summary>
		/// <param name="actionNameOrId">The name or identifier of the input action to find.</param>
		/// <typeparam name="T"></typeparam>
		/// <returns>An <see cref="InputActionSync"/> instance representing the synchronized input action of given type.</returns>
		/// <exception cref="ArgumentException">Thrown if the specified input action cannot be found or if the name or identifier is invalid.</exception>
		public InputActionSync.Action<T> FindAction<T>(string actionNameOrId) where T : struct
		{
			var action = FindAction(actionNameOrId);
			if (action is InputActionSync.Action<T> typedAction)
			{
				return typedAction;
			}

			throw new ArgumentException($"Input action '{actionNameOrId}' is not of type {typeof(T).Name}.", nameof(actionNameOrId));
		}

		public override void Possessed(bool isMe, User user)
		{
			local = isMe;

			var actionsList = new List<InputActionSync>();
			foreach (var map in inputActions.actionMaps)
			{
				foreach (var action in map.actions)
				{
					actionsList.Add(InputActionSync.New(action, isMe));
				}
			}

			if (actionsList.Count > byte.MaxValue)
			{
				Debug.LogError("Too many input actions! The maximum is " + byte.MaxValue + ". Additional actions will be ignored.");
				actionsList.RemoveRange(byte.MaxValue, actionsList.Count - byte.MaxValue);
			}

			_actions = actionsList.ToArray();

			if (isMe)
			{
				inputActions.Enable();
				enabled = true;
			}
		}

		public override void Unpossessed()
		{
			local = false;
			_actions = Array.Empty<InputActionSync>();
		}

		public override void AssembleData(Writer writer, SerializeInfo info)
		{
			if (!local)
			{
				writer.Write((byte)0);
				return;
			}

			if (info.ForceSync)
			{
				writer.Write((byte)_actions.Length);

				foreach (var action in _actions)
				{
					action.AssembleData(writer);
				}

				return;
			}

			byte length = 0;
			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			foreach (InputActionSync.ILocalAction action in _actions)
			{
				if (action.Pending)
				{
					length++;
				}
			}

			writer.Write(length);

			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			for (byte i = 0, l = (byte)_actions.Length; i < l; i++)
			{
				var action = (InputActionSync.ILocalAction)_actions[i];
				if (action.Pending)
				{
					writer.Write(i);
					action.AssembleData(writer);
					length--;
					if (length == 0)
					{
						break;
					}
				}
			}
		}

		public override void DisassembleData(Reader reader, UnserializeInfo info)
		{
			if (_actions.Length == 0)
			{
				Possessed(false, null);
			}

			int length = reader.ReadByte();

			if (info.ForceSync || length >= _actions.Length)
			{
				if (length != _actions.Length)
				{
					Debug.LogError("InputSync: Received data length does not match the expected length. Expected: " + _actions.Length + ", Received: " + length);
					return;
				}

				foreach (var action in _actions)
				{
					action.DisassembleData(reader);
				}

				return;
			}

			for (length--; length >= 0; length--)
			{
				_actions[reader.ReadByte()].DisassembleData(reader);
			}
		}

#region Actions

		public abstract class InputActionSync
		{
			public virtual bool Updated { get; internal set; }
			public readonly Guid Id;
			public abstract T GetValue<T>() where T : struct;

			public abstract bool IsPressed();

			[Obfuscation(Exclude = true, ApplyToMembers = true)]
			internal abstract void AssembleData(Writer writer);
			[Obfuscation(Exclude = true, ApplyToMembers = true)]
			internal abstract void DisassembleData(Reader reader);

			private InputActionSync(UnityEngine.InputSystem.InputAction action)
			{
				Id = action.id;
			}

			public static InputActionSync New(UnityEngine.InputSystem.InputAction action, bool local)
			{
				if (local)
				{
					if (action.type == InputActionType.Button)
					{
						return new LocalActionBool(action);
					}

					switch (action.expectedControlType)
					{
						case "Axis":
						case "Analog":
							return new LocalActionFloat(action);
						case "Vector2":
							return new LocalActionVector2(action);
						case "Vector3":
							return new LocalActionVector3(action);
						case "Quaternion":
							return new LocalActionQuaternion(action);
						default:
							throw new ArgumentOutOfRangeException($"Unsupported control type: {action.expectedControlType}");
					}
				}

				if (action.type == InputActionType.Button)
				{
					return new RemoteActionBool(action);
				}

				switch (action.expectedControlType)
				{
					case "Axis":
					case "Analog":
						return new RemoteActionFloat(action);
					case "Vector2":
						return new RemoteActionVector2(action);
					case "Vector3":
						return new RemoteActionVector3(action);
					case "Quaternion":
						return new RemoteActionQuaternion(action);
					default:
						throw new ArgumentOutOfRangeException($"Unsupported control type: {action.expectedControlType}");
				}
			}

			public abstract class Action<T> : InputActionSync where T : struct
			{
				internal virtual T Value { get; set; }

				internal Action(InputAction action) : base(action) { }

				public virtual T GetValue() => Value;

				public override T2 GetValue<T2>()
				{
					if (typeof(T2) == typeof(T))
					{
						return (T2)(object)Value;
					}

					if (typeof(T2) == typeof(bool))
					{
						return (T2)(object)IsPressed();
					}

					if (typeof(T2) == typeof(float) || typeof(T2) == typeof(double) || typeof(T2) == typeof(decimal))
					{
						return (T2)(object)Convert.ToSingle(Value);
					}

					throw new InvalidCastException($"Cannot convert {typeof(T).Name} to {typeof(T2).Name}");
				}

				public override bool IsPressed()
				{
					if (typeof(T) == typeof(float))
					{
						return Convert.ToSingle(Value) > 0f;
					}

					if (typeof(T) == typeof(Vector2))
					{
						var v = (Vector2)(object)Value;
						return v.x != 0f || v.y != 0f;
					}

					if (typeof(T) == typeof(Vector3))
					{
						var v = (Vector3)(object)Value;
						return v.x != 0f || v.y != 0f || v.z != 0f;
					}

					if (typeof(T) == typeof(Vector4))
					{
						var v = (Vector4)(object)Value;
						return v.x != 0f || v.y != 0f || v.z != 0f || v.w != 0f;
					}

					if (typeof(T) == typeof(Quaternion))
					{
						var v = (Quaternion)(object)Value;
						return v.x != 0f || v.y != 0f || v.z != 0f || v.w != 0f;
					}

					throw new InvalidCastException($"Cannot determine if {typeof(T).Name} is pressed");
				}
			}

			internal interface ILocalAction
			{
				bool Pending { get; set; }
				bool Updated { get; }
				void AssembleData(Writer writer);
			}

			public abstract class InputActionSyncLocal<T1> : Action<T1>, ILocalAction where T1 : struct
			{
				internal override T1 Value => Action.ReadValue<T1>();
				private T1 _oldValue;

				public override bool Updated
				{
					get
					{
						if (!Value.Equals(_oldValue))
						{
							_oldValue = Value;
							return true;
						}

						return false;
					}
				}

				internal readonly InputAction Action;
				bool ILocalAction.Pending { get; set; }
				void ILocalAction.AssembleData(Writer writer) => AssembleData(writer);

				public override T1 GetValue() => Action.ReadValue<T1>();

				public override T GetValue<T>() => Action.ReadValue<T>();
				public override bool IsPressed() => Action.IsPressed();

				private protected InputActionSyncLocal(InputAction action) : base(action)
				{
					Action = action;
				}
			}

			public class RemoteActionBool : Action<bool>
			{
				public RemoteActionBool(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
				}

				internal override void DisassembleData(Reader reader)
				{
					Value = reader.ReadBool();
					Updated = true;
				}
			}

			public class RemoteActionFloat : Action<float>
			{
				public RemoteActionFloat(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
				}

				internal override void DisassembleData(Reader reader)
				{
					Value = reader.ReadFloat();
					Updated = true;
				}
			}

			public class RemoteActionVector2 : Action<Vector2>
			{
				public RemoteActionVector2(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
				}

				internal override void DisassembleData(Reader reader)
				{
					Value = reader.ReadVector2();
					Updated = true;
				}
			}

			public class RemoteActionVector3 : Action<Vector3>
			{
				public RemoteActionVector3(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
				}

				internal override void DisassembleData(Reader reader)
				{
					Value = reader.ReadVector3();
					Updated = true;
				}
			}

			public class RemoteActionQuaternion : Action<Quaternion>
			{
				public RemoteActionQuaternion(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
				}

				internal override void DisassembleData(Reader reader)
				{
					Value = reader.ReadQuaternion();
					Updated = true;
				}
			}

			public class LocalActionBool : InputActionSyncLocal<bool>
			{
				internal LocalActionBool(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
					((ILocalAction)this).Pending = false;
				}

				internal override void DisassembleData(Reader reader)
				{
					reader.ReadBool();
				}
			}

			public class LocalActionFloat : InputActionSyncLocal<float>
			{
				internal LocalActionFloat(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
					((ILocalAction)this).Pending = false;
				}

				internal override void DisassembleData(Reader reader)
				{
					reader.ReadFloat();
				}
			}

			public class LocalActionVector2 : InputActionSyncLocal<Vector2>
			{
				internal LocalActionVector2(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
					((ILocalAction)this).Pending = false;
				}

				internal override void DisassembleData(Reader reader)
				{
					reader.ReadVector2();
				}
			}

			public class LocalActionVector3 : InputActionSyncLocal<Vector3>
			{
				internal LocalActionVector3(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
					((ILocalAction)this).Pending = false;
				}

				internal override void DisassembleData(Reader reader)
				{
					reader.ReadVector3();
				}
			}

			public class LocalActionQuaternion : InputActionSyncLocal<Quaternion>
			{
				internal LocalActionQuaternion(InputAction action) : base(action) { }

				internal override void AssembleData(Writer writer)
				{
					writer.Write(Value);
					((ILocalAction)this).Pending = false;
				}

				internal override void DisassembleData(Reader reader)
				{
					reader.ReadQuaternion();
				}
			}
		}

#endregion
	}
}