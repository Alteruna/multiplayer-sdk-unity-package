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
	/// Control and synchronizes an object's position through physics simulation.
	/// </summary>
	/// <remarks>
	///	<img src="../images/Alteruna.RigidbodySynchronizable.png" />
	/// </remarks>
	/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.html"/>
	[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody)), AddComponentMenu("Alteruna/Transform/Rigidbody Synchronizable"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class RigidbodySynchronizable : RigidbodySynchronizableCommon
	{

		private RigidbodyConstraints _constraints;

		/// <summary>
		/// Rigidbody to synchronize.
		/// </summary>
		public Rigidbody Rigidbody;

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
			get => Rigidbody.useGravity;
			set
			{
				Rigidbody.useGravity = value;
				QueForNextUpdate();
			}
		}

		public override void Awake()
		{
			if (Rigidbody == null)
				Rigidbody = GetComponent<Rigidbody>();
			_constraints = ~Rigidbody.constraints;
			base.Awake();
		}

		public override void OnCollisionEnter(Collision collision)
		{
			if (isKinematic) return;
			if (!Avatar.UsingAvatars)
			{
				base.OnCollisionEnter(collision);
				return;
			}

			if (!AllowCollisionToAssumeOwner || (collision.gameObject.layer & IgnoredLayers) != 0) return;
			// When locked ownership of collider to different client, don't sync collision.
			if (collision.rigidbody != null && collision.rigidbody.TryGetComponent(out RigidbodySynchronizable rbs) && !rbs.AllowCollisionToAssumeOwner)
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
		/// The velocity vector of the rigidbody. It represents the rate of change of Rigidbody position.
		/// </summary>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody-velocity.html"/>
		// ReSharper disable once InconsistentNaming
		public Vector3 velocity
		{
			get => Rigidbody.velocity;
			set
			{
				float delta = Vector3.Distance(Rigidbody.velocity, value);
				Rigidbody.velocity = value;
				if (delta < 0.0271828f) { }
				else if (delta < 0.271828f) QueForNextUpdate();
				else ControlledForcedSync();
			}
		}

		/// <summary>
		/// The angular velocity vector of the rigidbody measured in radians per second.
		/// </summary>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody-angularVelocity.html"/>
		// ReSharper disable once InconsistentNaming
		public Vector3 angularVelocity
		{
			get => Rigidbody.angularVelocity;
			set
			{
				float delta = Vector3.Distance(Rigidbody.angularVelocity, value);
				Rigidbody.angularVelocity = value;
				if (delta < 0.0314159f) { }
				else if (delta < 0.314159f) QueForNextUpdate();
				else ControlledForcedSync();
			}
		}

		/// <summary>
		///	The position of the rigidbody.
		/// </summary>
		/// <example>
		///	<c>RigidbodySynchronizable.position</c> allows you to get and set the position of a Rigidbody using the physics engine and sync it immediately to other clients.<br/>
		/// If you change the position of a rigidbody using <c>RigidbodySynchronizable.position</c>, the transform will be updated after the next physics simulation step.<br/>
		/// This is faster than updating the position using <c>Transform.position</c>, as the latter will not trigger a sync packet and cause all attached Colliders to recalculate their positions relative to the Rigidbody.
		///
		/// If you want to continuously move a rigidbody use MovePosition instead, which takes interpolation into account.
		/// </example>
		/// <seealso cref="MovePosition"/>
		// ReSharper disable once InconsistentNaming
		public Vector3 position
		{
			get => Rigidbody.position;
			set
			{
				Rigidbody.position = value;
				if (!Rigidbody.isKinematic) Rigidbody.velocity = Vector3.zero;
				ControlledForcedSync();
			}
		}

		/// <summary>
		///   <para>The rotation of the Rigidbody.</para>
		/// </summary>
		/// <example>
		///	Use <c>RigidbodySynchronizable.rotation</c> to get and set the rotation of a Rigidbody using the physics engine.<br/>
		///
		/// Changing the rotation of a Rigidbody using <c>RigidbodySynchronizable.rotation</c> updates the Transform after the next physics simulation step and sync it immediately to other clients.<br/>
		/// This is faster than updating the rotation using <c>Transform.rotation</c>, as the latter will not trigger a sync packet and causes all attached Colliders to recalculate their rotation relative to the Rigidbody, whereas Rigidbody.rotation sets the values directly to the physics system.
		///
		/// If you want to continuously rotate a rigidbody use MoveRotation instead, which takes interpolation into account.
		/// </example>
		/// <seealso cref="MoveRotation"/>
		// ReSharper disable once InconsistentNaming
		public Quaternion rotation
		{
			get => Rigidbody.rotation;
			set
			{
				Rigidbody.rotation = value;
				if (!Rigidbody.isKinematic) Rigidbody.angularVelocity = Vector3.zero;
				ControlledForcedSync();
			}
		}

		/// <summary>
		///   <para>Moves the kinematic Rigidbody towards position.</para>
		/// </summary>
		/// <param name="position">Provides the new position for the Rigidbody object.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.MovePosition.html"/>
		// ReSharper disable once ParameterHidesMember
		public void MovePosition(Vector3 position)
		{
			Rigidbody.MovePosition(position);
			ControlledForcedSync();
		}

		/// <summary>
		///    <para>Moves the kinematic Rigidbody to a new position.</para>
		/// </summary>
		/// <param name="position">Provides the new position for the Rigidbody object.</param>
		/// <seealso cref="position"/>
		// ReSharper disable once ParameterHidesMember
		public void SetPosition(Vector3 position) => this.position = position;

		/// <summary>
		///   <para>Rotates the rigidbody to rotation.</para>
		/// </summary>
		/// <param name="rot">The new rotation for the Rigidbody.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.MoveRotation.html"/>
		public void MoveRotation(Quaternion rot)
		{
			Rigidbody.MoveRotation(rot);
			ControlledForcedSync();
		}

		/// <summary>
		///   <para>Set the rotation of the rigidbody to new rotation.</para>
		/// </summary>
		/// <param name="rot">The new rotation for the Rigidbody.</param>
		/// <seealso cref="rotation"/>
		public void SetRotation(Quaternion rot) => this.rotation = rot;

		/// <summary>
		/// Adds a force to the Rigidbody.
		/// </summary>
		/// <example>
		/// This example applies an Impulse force along the Z axis to the GameObject's Rigidbody.
		///	<code>
		///	using UnityEngine;
		///	public class Example : MonoBehaviour
		///	{
		///		public float thrust = 1.0f;
		///		public RigidbodySynchronizable rb;
		///	
		///		void Start()
		///		{
		///			rb.AddForce(0, 0, thrust, ForceMode.Impulse);
		///		}
		///	}
		/// </code>
		/// </example>
		/// <param name="x">Size of force along the world x-axis.</param>
		/// <param name="y">Size of force along the world y-axis.</param>
		/// <param name="z">Size of force along the world z-axis.</param>
		/// <param name="mode">	Type of force to apply.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.AddForce.html"/>
		public void AddForce(float x, float y, float z, ForceMode mode = ForceMode.Force) =>
			AddForce(new Vector3(x, y, z), mode);

		/// <summary>
		/// Adds a force to the Rigidbody.
		/// </summary>
		/// <example>
		///	Force is applied continuously along the direction of the force vector. Specifying the ForceMode mode allows the type of force to be changed to an Acceleration, Impulse or Velocity Change.
		/// <code>
		/// using UnityEngine;
		/// public class Example : MonoBehaviour
		/// {
		/// 	public RigidbodySynchronizable RigidbodySync;
		/// 	public float m_Thrust = 20f;
		/// 
		/// 	void FixedUpdate()
		/// 	{
		/// 		if (Input.GetButton("Jump"))
		/// 		{
		/// 			//Apply a force to this Rigidbody in direction of this GameObjects up axis
		/// 			RigidbodySync.AddForce(transform.up * m_Thrust);
		/// 		}
		/// 	}
		/// }
		/// </code>
		/// </example>
		/// <param name="force">Force vector in world coordinates.</param>
		/// <param name="mode">	Type of force to apply.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.AddForce.html"/>
		public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
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

			ControlledForcedSync();
		}

		/// <summary>
		/// Adds a torque to the rigidbody.
		/// </summary>
		/// <example>
		/// Force can be applied only to an active rigidbody. If a GameObject is inactive, AddTorque has no effect.
		///	Wakes up the Rigidbody by default. If the torque size is zero then the Rigidbody will not be woken up.
		///	</example>
		/// <param name="x">Size of torque along the world x-axis.</param>
		/// <param name="y">Size of torque along the world y-axis.</param>
		/// <param name="z">Size of torque along the world z-axis.</param>
		/// <param name="mode">The type of torque to apply.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.AddTorque.html"/>
		public void AddTorque(float x, float y, float z, ForceMode mode = ForceMode.Force) => AddTorque(new Vector3(x, y, z), mode);

		/// <summary>
		/// Adds a torque to the rigidbody.
		/// </summary>
		/// <example>
		///	Force can be applied only to an active rigidbody. If a GameObject is inactive, AddTorque has no effect.
		/// <code>
		///	// Rotate an object around its Y (upward) axis in response to
		///	// left/right controls.
		///	using UnityEngine;
		///	using System.Collections;
		///	
		///	public class ExampleClass : MonoBehaviour
		///	{
		///		public float torque;
		///		public RigidbodySynchronizable rb;
		///	
		///		void FixedUpdate()
		///		{
		///			float turn = Input.GetAxis("Horizontal");
		///			rb.AddTorque(transform.up * torque * turn);
		///		}
		///	}
		/// </code>
		/// </example>
		/// <param name="torque">Torque vector in world coordinates.</param>
		/// <param name="mode">	The type of torque to apply.</param>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.AddTorque.html"/>
		public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
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

			ControlledForcedSync();
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
				writer.Write(Rigidbody.useGravity);
			}

			if ((flags & 2) != 0)
			{
				FullSyncId.Append(writer);

				if ((_constraints & RigidbodyConstraints.FreezePosition) != 0)
				{
					if ((_constraints & RigidbodyConstraints.FreezePositionX) != 0)
					{
						writer.Write(Rigidbody.position.x);
						writer.Write(Rigidbody.velocity.x);
					}

					if ((_constraints & RigidbodyConstraints.FreezePositionY) != 0)
					{
						writer.Write(Rigidbody.position.y);
						writer.Write(Rigidbody.velocity.y);
					}

					if ((_constraints & RigidbodyConstraints.FreezePositionZ) != 0)
					{
						writer.Write(Rigidbody.position.z);
						writer.Write(Rigidbody.velocity.z);
					}
				}

				if ((_constraints & RigidbodyConstraints.FreezeRotation) != 0)
				{
					Vector3 euler = Rigidbody.rotation.eulerAngles;
					Vector3 angular = Rigidbody.angularVelocity;

					if ((_constraints & RigidbodyConstraints.FreezeRotationX) != 0)
					{
						writer.Write(euler.x);
						writer.Write(angular.x);
					}

					if ((_constraints & RigidbodyConstraints.FreezeRotationY) != 0)
					{
						writer.Write(euler.y);
						writer.Write(angular.y);
					}

					if ((_constraints & RigidbodyConstraints.FreezeRotationZ) != 0)
					{
						writer.Write(euler.z);
						writer.Write(angular.z);
					}
				}
			}
			else
			{
				if ((_constraints & RigidbodyConstraints.FreezePosition) != 0)
				{
					if ((_constraints & RigidbodyConstraints.FreezePositionX) != 0)
					{
						writer.Write(Rigidbody.velocity.x);
					}

					if ((_constraints & RigidbodyConstraints.FreezePositionY) != 0)
					{
						writer.Write(Rigidbody.velocity.y);
					}

					if ((_constraints & RigidbodyConstraints.FreezePositionZ) != 0)
					{
						writer.Write(Rigidbody.velocity.z);
					}
				}

				if ((_constraints & RigidbodyConstraints.FreezeRotation) != 0)
				{
					Vector3 angular = Rigidbody.angularVelocity;
					if ((_constraints & RigidbodyConstraints.FreezeRotationX) != 0)
					{
						writer.Write(angular.x);
					}

					if ((_constraints & RigidbodyConstraints.FreezeRotationY) != 0)
					{
						writer.Write(angular.y);
					}

					if ((_constraints & RigidbodyConstraints.FreezeRotationZ) != 0)
					{
						writer.Write(angular.z);
					}
				}
			}
		}

		public override void DisassembleData(Reader reader, UnserializeInfo info)
		{
			byte flags = reader.ReadByte();
			if (DisassembleFlags(flags)) return;

			bool fullSync = (flags & 2) != 0;

			// Sync settings
			if ((flags & 4) != 0)
			{
				_constraints = (RigidbodyConstraints)reader.ReadUshort();
				Rigidbody.constraints = _constraints;
				_constraints = ~_constraints;
				Rigidbody.interpolation = (RigidbodyInterpolation)reader.ReadByte();
				Rigidbody.mass = reader.ReadFloat();
				Rigidbody.drag = reader.ReadFloat();
				Rigidbody.angularDrag = reader.ReadFloat();
				Rigidbody.useGravity = reader.ReadBool();
			}


			if (fullSync)
			{
				if (!FullSyncId.Validate(reader)) return;

				if ((_constraints & RigidbodyConstraints.FreezePosition) != 0)
				{
					Vector3 pos = Rigidbody.position;
					Vector3 vel = Rigidbody.velocity;
					if ((_constraints & RigidbodyConstraints.FreezePositionX) != 0)
					{
						pos.x = reader.ReadFloat();
						vel.x = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints.FreezePositionY) != 0)
					{
						pos.y = reader.ReadFloat();
						vel.y = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints.FreezePositionZ) != 0)
					{
						pos.z = reader.ReadFloat();
						vel.z = reader.ReadFloat();
					}

					if (ApplyAsTransform) transform.position = pos;
					else Rigidbody.MovePosition(pos);

					Rigidbody.velocity = vel;
				}

				if ((_constraints & RigidbodyConstraints.FreezeRotation) != 0)
				{
					Vector3 euler = Rigidbody.rotation.eulerAngles;
					Vector3 newAngularVelocity = Rigidbody.angularVelocity;
					if ((_constraints & RigidbodyConstraints.FreezeRotationX) != 0)
					{
						euler.x = reader.ReadFloat();
						newAngularVelocity.x = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints.FreezeRotationY) != 0)
					{
						euler.y = reader.ReadFloat();
						newAngularVelocity.y = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints.FreezeRotationZ) != 0)
					{
						euler.z = reader.ReadFloat();
						newAngularVelocity.z = reader.ReadFloat();
					}

					if (ApplyAsTransform) transform.eulerAngles = euler;
					else Rigidbody.MoveRotation(Quaternion.Euler(euler));

					Rigidbody.angularVelocity = newAngularVelocity;
				}
			}
			else
			{
				if ((_constraints & RigidbodyConstraints.FreezePosition) != 0)
				{
					Vector3 newVelocity = Rigidbody.velocity;
					if ((_constraints & RigidbodyConstraints.FreezePositionX) != 0)
					{
						newVelocity.x = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints.FreezePositionY) != 0)
					{
						newVelocity.y = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints.FreezePositionZ) != 0)
					{
						newVelocity.z = reader.ReadFloat();
					}

					Rigidbody.velocity = newVelocity;
				}

				if ((_constraints & RigidbodyConstraints.FreezeRotation) != 0)
				{
					Vector3 newAngularVelocity = Rigidbody.angularVelocity;
					if ((_constraints & RigidbodyConstraints.FreezeRotationX) != 0)
					{
						newAngularVelocity.x = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints.FreezeRotationY) != 0)
					{
						newAngularVelocity.y = reader.ReadFloat();
					}

					if ((_constraints & RigidbodyConstraints.FreezeRotationZ) != 0)
					{
						newAngularVelocity.z = reader.ReadFloat();
					}

					Rigidbody.angularVelocity = newAngularVelocity;
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
				Rigidbody = GetComponent<Rigidbody>();
		}

		public override float EstimateMinimumDataSentPerSecond()
		{
			if (Rigidbody == null)
			{
				Rigidbody = GetComponent<Rigidbody>();
			}

			return EstimateDataBase((int)Rigidbody.constraints);
		}
	}
}