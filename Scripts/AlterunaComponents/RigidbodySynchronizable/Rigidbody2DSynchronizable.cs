using System;
using System.Reflection;
using Alteruna.Multiplayer.Core.MethodArguments;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using UnityEngine;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// <c>Rigidbody2DSynchronizable</c> is a <c>Synchronizable</c> that synchronizes the state of a <c>Rigidbody2D</c> component.
	/// </summary>
	[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D)), AddComponentMenu("Alteruna/Transform/Rigidbody 2D Synchronizable"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class Rigidbody2DSynchronizable : RigidbodySynchronizableCommon
	{

		private RigidbodyConstraints2D _constraints;

		[NonSerialized] private float _gravityScale = 1;

		/// <summary>
		/// Rigidbody to synchronize.
		/// </summary>
		public Rigidbody2D Rigidbody;

		/// <inheritdoc />
		public override bool isKinematic
		{
			get => Rigidbody.isKinematic;
			set
			{
				Rigidbody.isKinematic = value;
				QueForNextUpdate();
			}
		}

		/// <inheritdoc />
		public override bool useGravity
		{
			get => Rigidbody.gravityScale != 0;
			set
			{
				if (value)
				{
					if (useGravity) Rigidbody.gravityScale = _gravityScale == 0 ? 1 : _gravityScale;
				}
				else
				{
					if (!useGravity) Rigidbody.gravityScale = 0;
				}
			}
		}

		// ReSharper disable once InconsistentNaming
		/// <summary>
		/// The degree to which this object is affected by gravity.
		/// </summary>
		public float gravityScale
		{
			get => Rigidbody.gravityScale;
			set => Rigidbody.gravityScale = _gravityScale = value;
		}

		public override void Awake()
		{
			if (Rigidbody == null)
				Rigidbody = GetComponent<Rigidbody2D>();
			_constraints = ~Rigidbody.constraints;
			_gravityScale = Rigidbody.gravityScale;
			base.Awake();
		}

		public override void OnCollisionEnter2D(Collision2D collision)
		{
			if (isKinematic) return;
			if (!Avatar.UsingAvatars)
			{
				base.OnCollisionEnter2D(collision);
				return;
			}

			if (!AllowCollisionToAssumeOwner || (collision.gameObject.layer & IgnoredLayers) != 0) return;
			// When locked ownership of collider to different client, don't sync collision.
			if (collision.rigidbody != null && collision.rigidbody.TryGetComponent(out Rigidbody2DSynchronizable rbs) && !rbs.AllowCollisionToAssumeOwner)
			{
				if (rbs.SendData)
				{
					SendData = !_isPossesedAndNotPossesor;
				}
				else
				{
					return;
				}
			}

			_currentInterval = 0;
			_currentFullInterval = 0;
			_fullSync = true;
		}

		/// <summary>
		///   <para>Linear velocity of the Rigidbody in units per second.</para>
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public Vector2 velocity
		{
			get => Rigidbody.velocity;
			set
			{
				float delta = Vector2.Distance(Rigidbody.velocity, value);
				Rigidbody.velocity = value;
				if (delta < 0.01618f) { }
				else if (delta < 0.1618f) QueForNextUpdate();
				else ControlledForcedSync();
			}
		}

		/// <summary>
		///   <para>Angular velocity in degrees per second.</para>
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public float angularVelocity
		{
			get => Rigidbody.angularVelocity;
			set
			{
				float delta = Mathf.Abs(Rigidbody.angularVelocity - value);
				Rigidbody.angularVelocity = value;
				if (delta < 0.0157079f) { }
				else if (delta < 0.157079f) QueForNextUpdate();
				else ControlledForcedSync();
			}
		}

		/// <summary>
		///   <para>The position of the rigidbody.</para>
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public Vector2 position
		{
			get => Rigidbody.position;
			set
			{
				Rigidbody.position = value;
				Rigidbody.velocity = Vector2.zero;
				ControlledForcedSync();
			}
		}

		/// <summary>
		///   <para>The rotation of the rigidbody.</para>
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public float rotation
		{
			get => Rigidbody.rotation;
			set
			{
				Rigidbody.rotation = value;
				Rigidbody.angularVelocity = 0;
				ControlledForcedSync();
			}
		}

		/// <summary>
		///   <para>Sets the rotation of the Rigidbody2D to angle (given in degrees).</para>
		/// </summary>
		/// <param name="angle">The rotation of the Rigidbody (in degrees).</param>
		public void SetRotation(float angle)
		{
			Rigidbody.SetRotation(angle);
			ControlledForcedSync();
		}

		/// <summary>
		///   <para>Moves the rigidbody to position.</para>
		/// </summary>
		/// <param name="position">The new position for the Rigidbody object.</param>
		// ReSharper disable once ParameterHidesMember
		public void MovePosition(Vector2 position)
		{
			Rigidbody.MovePosition(position);
			ControlledForcedSync();
		}

		/// <summary>
		///   <para>Rotates the Rigidbody to angle (given in degrees).</para>
		/// </summary>
		/// <param name="angle">The new rotation angle for the Rigidbody object.</param>
		public void MoveRotation(float angle)
		{
			Rigidbody.MoveRotation(angle);
			ControlledForcedSync();
		}

		/// <summary>
		/// Adds a force to the Rigidbody.
		/// </summary>
		/// <param name="x">Size of force along the world x-axis.</param>
		/// <param name="y">Size of force along the world y-axis.</param>
		/// <param name="mode">	Type of force to apply.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody2D.AddForce.html"/>
		public void AddForce(float x, float y, ForceMode mode = ForceMode.Force) =>
			AddForce(new Vector2(x, y), mode);

		/// <summary>
		/// Adds a force to the Rigidbody.
		/// </summary>
		/// <param name="force">Force vector in world coordinates.</param>
		/// <param name="mode">	Type of force to apply.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody2D.AddForce.html"/>
		public void AddForce(Vector2 force, ForceMode mode = ForceMode.Force)
		{
			switch (mode)
			{
				case ForceMode.Force:
					velocity += force * (Time.fixedDeltaTime / Rigidbody.mass);
					break;
				case ForceMode.Acceleration:
					velocity += force * Time.fixedDeltaTime;
					break;
				case ForceMode.Impulse:
					velocity += force / Rigidbody.mass;
					break;
				case ForceMode.VelocityChange:
					velocity += force;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		/// <summary>
		/// Adds a force to the Rigidbody.
		/// </summary>
		/// <param name="x">Size of force along the world x-axis.</param>
		/// <param name="y">Size of force along the world y-axis.</param>
		/// <param name="mode">	Type of force to apply.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody2D.AddForce.html"/>
		public void AddForce(float x, float y, ForceMode2D mode = ForceMode2D.Force) =>
			AddForce(new Vector2(x, y), mode);

		/// <summary>
		/// Adds a force to the Rigidbody.
		/// </summary>
		/// <param name="force">Force vector in world coordinates.</param>
		/// <param name="mode">	Type of force to apply.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody2D.AddForce.html"/>
		public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
		{
			switch (mode)
			{
				case ForceMode2D.Force:
					velocity += force * (Time.fixedDeltaTime / Rigidbody.mass);
					break;
				case ForceMode2D.Impulse:
					velocity += force / Rigidbody.mass;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		/// <summary>
		/// Adds a torque to the rigidbody.
		/// </summary>
		/// <param name="torque">Torque vector in world coordinates.</param>
		/// <param name="mode">	The type of torque to apply.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody2D.AddTorque.html"/>
		public void AddTorque(float torque, ForceMode mode = ForceMode.Force)
		{
			switch (mode)
			{
				case ForceMode.Force:
					angularVelocity += torque * (Time.fixedDeltaTime / Rigidbody.mass);
					break;
				case ForceMode.Acceleration:
					angularVelocity += torque * Time.fixedDeltaTime;
					break;
				case ForceMode.Impulse:
					angularVelocity += torque / Rigidbody.mass;
					break;
				case ForceMode.VelocityChange:
					angularVelocity += torque;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		private protected override void ControlledSoftSync()
		{
			if (!isKinematic)
			{
				base.ControlledSoftSync();
			}
		}

		/// <summary>
		/// Is the rigidbody sleeping?
		/// </summary>
		/// <returns>true when rigidbody is sleeping.</returns>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.IsSleeping.html"/>
		public override bool IsSleeping() => Rigidbody.IsSleeping();

		/// <summary>
		/// Forces a rigidbody to sleep at least one frame.
		/// </summary>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.Sleep.html"/>
		public override void Sleep() => Rigidbody.Sleep();

		/// <summary>
		/// Forces a rigidbody to wake up.
		/// </summary>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.WakeUp.html"/>
		public override void WakeUp()
		{
			Rigidbody.WakeUp();
			ControlledForcedSync();
		}

		public override void AssembleData(Writer writer, SerializeInfo info)
		{
			byte flags = GetFlags();
			writer.Write(flags);

			if ((flags & 4) != 0)
			{
				_constraints = Rigidbody.constraints;
				writer.Write((ushort)_constraints);
				_constraints = ~_constraints;
				writer.Write((byte)Rigidbody.interpolation);
				writer.Write(Rigidbody.mass);
				writer.Write(Rigidbody.drag);
				writer.Write(Rigidbody.angularDrag);
				writer.Write(Rigidbody.gravityScale);
			}

			// Sync settings
			if ((flags & 2) != 0)
			{
				FullSyncId.Append(writer);

				if ((_constraints & RigidbodyConstraints2D.FreezePositionX) != 0)
				{
					writer.Write(Rigidbody.position.x);
					writer.Write(Rigidbody.velocity.x);
				}

				if ((_constraints & RigidbodyConstraints2D.FreezePositionY) != 0)
				{
					writer.Write(Rigidbody.position.y);
					writer.Write(Rigidbody.velocity.y);
				}

				if ((_constraints & RigidbodyConstraints2D.FreezeRotation) != 0)
				{
					writer.Write(Rigidbody.rotation);
					writer.Write(Rigidbody.angularVelocity);
				}
			}
			else
			{
				if ((_constraints & RigidbodyConstraints2D.FreezePositionX) != 0)
				{
					writer.Write(Rigidbody.velocity.x);
				}

				if ((_constraints & RigidbodyConstraints2D.FreezePositionY) != 0)
				{
					writer.Write(Rigidbody.velocity.y);
				}

				if ((_constraints & RigidbodyConstraints2D.FreezeRotation) != 0)
				{
					writer.Write(Rigidbody.angularVelocity);
				}
			}
		}

		public override void DisassembleData(Reader reader, UnserializeInfo info)
		{
			byte flags = reader.ReadByte();
			if (DisassembleFlags(flags)) return;

			bool fullSync = (flags & 2) != 0;

			if ((flags & 4) != 0)
			{
				_constraints = (RigidbodyConstraints2D)reader.ReadUshort();
				Rigidbody.constraints = _constraints;
				_constraints = ~_constraints;
				Rigidbody.interpolation = (RigidbodyInterpolation2D)reader.ReadByte();
				Rigidbody.mass = reader.ReadFloat();
				Rigidbody.drag = reader.ReadFloat();
				Rigidbody.angularDrag = reader.ReadFloat();
				Rigidbody.gravityScale = reader.ReadFloat();
			}


			if (fullSync)
			{
				if (!FullSyncId.Validate(reader)) return;

				if ((_constraints & RigidbodyConstraints2D.FreezePosition) != 0)
				{
					Vector2 newPosition = Rigidbody.position;
					Vector2 newVelocity = Rigidbody.velocity;
					if ((_constraints & RigidbodyConstraints2D.FreezePositionX) != 0)
					{
						newPosition.x = reader.ReadFloat();
						newVelocity.x = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints2D.FreezePositionY) != 0)
					{
						newPosition.y = reader.ReadFloat();
						newVelocity.y = reader.ReadFloat();
					}

					if (ApplyAsTransform)
					{
						Transform t = transform;
						t.position = new Vector3(newPosition.x, newPosition.y, t.position.z);
					}
					else
					{
						Rigidbody.MovePosition(newPosition);
					}

					Rigidbody.velocity = newVelocity;
				}

				if ((_constraints & RigidbodyConstraints2D.FreezeRotation) != 0)
				{
					if (ApplyAsTransform)
					{
						Transform t = transform;
						t.eulerAngles = new Vector3(t.eulerAngles.x, t.eulerAngles.y, reader.ReadFloat());
					}
					else
					{
						Rigidbody.MoveRotation(reader.ReadFloat());
					}

					Rigidbody.angularVelocity = reader.ReadFloat();
				}
			}
			else
			{
				if ((_constraints & RigidbodyConstraints2D.FreezePosition) != 0)
				{
					Vector2 newVelocity = Rigidbody.velocity;
					if ((_constraints & RigidbodyConstraints2D.FreezePositionX) != 0)
					{
						newVelocity.x = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints2D.FreezePositionY) != 0)
					{
						newVelocity.y = reader.ReadFloat();
					}

					Rigidbody.velocity = newVelocity;
				}

				if ((_constraints & RigidbodyConstraints2D.FreezeRotation) != 0)
				{
					Rigidbody.angularVelocity = reader.ReadFloat();
				}
			}
		}

		public override void Reset()
		{
			if (TryGetComponent(out TransformSynchronizable _))
			{
				Debug.LogError("Can not have both a RigidbodySynchronizable and a TransformSynchronizable in the same object");
				DestroyImmediate(this);
				return;
			}

#if !TRINITY_EVALUATE
			if (TryGetComponent(out InterpolationTransformSynchronizable _))
			{
				Debug.LogError("Can not have both a InterpolationTransformSynchronizable and a RigidbodySynchronizable in the same object");
				DestroyImmediate(this);
				return;
			}
#endif

			base.Reset();
			if (Rigidbody == null)
				Rigidbody = GetComponent<Rigidbody2D>();
		}

		public override float EstimateMinimumDataSentPerSecond()
		{
			if (Rigidbody == null)
			{
				Rigidbody = GetComponent<Rigidbody2D>();
			}

			return EstimateDataBase((int)Rigidbody.constraints);
		}
	}
}