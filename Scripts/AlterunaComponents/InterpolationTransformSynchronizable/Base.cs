using System;
using System.Collections.Generic;
using System.Reflection;
using Alteruna.Multiplayer.Core.MethodArguments;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	
	/// <summary>
	/// Interpolate transform position and rotation using selected interpolation method.
	/// </summary>
	/// <remarks>
	///	<img src="../images/Alteruna.InterpolationTransformSynchronizable.png" />
	/// </remarks>
	[DisallowMultipleComponent, AddComponentMenu("Alteruna/Transform/Interpolation Transform Synchronizable"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public partial class InterpolationTransformSynchronizable : Synchronizable
	{

		/// <summary>
		/// Behavior of transform when set locally.
		/// </summary>
		public LocalBehaviourType LocalBehaviour = LocalBehaviourType.InterpolationMethod;
		
		/// <summary>
		/// Behavior of transform
		/// </summary>
		public InterpolationMethodType InterpolationMethod;

		/// <summary>
		/// Enabling this can reduces the perceived latency.
		/// It is intended to be used when <c>MovePosition</c> is frequently called.
		/// </summary>
		public bool ClientPrediction;

		[NonSerialized]
		private readonly List<OutgoingEvent> _outgoingEvents = new List<OutgoingEvent>();

		[NonSerialized]
		private InterpolationMethodType _oldInterpolate;

		[NonSerialized]
		private InterpolationMethodNone _interpolationMethod;
		[NonSerialized]
		private InterpolationMethodNone _interpolationMethodLocal;

		[NonSerialized]
		private Vector3 _velosity = new Vector3();
		[NonSerialized]
		private Vector3 _oldPos = new Vector3();

		public void Awake()
		{
#if UNITY_EDITOR
			_oldInterpolate = InterpolationMethod;
#endif
			if (_interpolationMethod == null) SetInterpolationMethod(InterpolationMethod, false);
		}


		private void Start()
		{
			_oldPos = transform.position;
		}

		public void Update()
		{
			_interpolationMethod.Interpolate();
		}

		public void FixedUpdate()
		{
			if (ClientPrediction)
			{
				var newPos = transform.position;
				_velosity = (newPos - _oldPos);
				_oldPos = newPos;
			}
			
			SyncUpdate();
		}

		public void OnValidate()
		{
			// during runtime, if interpolate get changed, run SetInterpolationMethod in order to apply the change
			if (_interpolationMethod != null && InterpolationMethod != _oldInterpolate)
			{
				_oldInterpolate = InterpolationMethod;
				SetInterpolationMethod(InterpolationMethod);
				Debug.Log("Changed interpolation method");
			}
		}
		
		public override void Reset()
		{
			base.Reset();
			if (TryGetComponent(out TransformSynchronizable _))
			{
				Debug.LogError("Can not have both a InterpolationTransformSynchronizable and a TransformSynchronizable in the same object");
				DestroyImmediate(this);
			}
			else if (TryGetComponent(out RigidbodySynchronizable _)  || TryGetComponent(out Rigidbody2DSynchronizable _))
			{
				Debug.LogError("Can not have both a InterpolationTransformSynchronizable and a RigidbodySynchronizable in the same object");
				DestroyImmediate(this);
			}
		}

		private void SetInterpolationMethod(InterpolationMethodType method, bool commit)
		{
			if (InterpolationMethod == method && _interpolationMethod != null)
			{
				return;
			}
			
			InterpolationMethod = method;
			switch (InterpolationMethod)
			{
				case InterpolationMethodType.None:
					_interpolationMethod = new InterpolationMethodNone(transform);
					break;
				case InterpolationMethodType.Lerp:
					_interpolationMethod = new InterpolationMethodLerp(transform);
					break;
				case InterpolationMethodType.LerpRelative:
					_interpolationMethod = new InterpolationMethodLerpRelative(transform);
					break;
				case InterpolationMethodType.SmoothDamp:
					_interpolationMethod = new InterpolationMethodSmoothDamp(transform);
					break;
				case InterpolationMethodType.Spring:
					_interpolationMethod = new InterpolationMethodSpring(transform);
					break;
				case InterpolationMethodType.Extrapolate:
					_interpolationMethod = new InterpolationMethodExtrapolate(transform);
					break;
				default:
					_interpolationMethod = new InterpolationMethodNone(transform);
					break;
			}

			SetLocalBehaviour(LocalBehaviour);
			
			if (commit)
			{
				_outgoingEvents.Add(new OutgoingEvent((byte)(InterpolationMethod + 4)));
				Commit();
				SyncUpdate();
			}
		}

		/// <summary>
		/// <para>Set interpolation method of interpolation transform synchronizable to interpolation method</para>
		/// </summary>
		/// <param name="method">The interpolation method for the interpolation transform synchronizable</param>
		public void SetInterpolationMethod(InterpolationMethodType method) => SetInterpolationMethod(method, true);

		/// <summary>
		/// <para>Set local behaviour of interpolation transform synchronizable to local behaviour</para>
		/// </summary>
		/// <param name="behaviourTypee local behaviour for the interpolation transform synchronizable</param>
		public void SetLocalBehaviour(LocalBehaviourType behaviourType)
		{
			LocalBehaviour = behaviourType;
			
			if (LocalBehaviour == LocalBehaviourType.InterpolationMethod)
			{
				_interpolationMethodLocal = _interpolationMethod;
			} 
			else if (LocalBehaviour == LocalBehaviourType.None && (_interpolationMethodLocal == null || _interpolationMethodLocal.GetType() != typeof(InterpolationMethodNone)))
			{
				_interpolationMethodLocal = new InterpolationMethodNone(transform);
			}
		}

		public override void AssembleData(Writer writer, SerializeInfo info)
		{
			int mask = 0;
			var length = _outgoingEvents.Count;
			byte count = 0;
			// find number of unique events
			for (int i = length - 1; i >= 0; i--)
			{
				if (((1 << _outgoingEvents[i].Type) & mask) == 0)
				{
					mask |= (1 << _outgoingEvents[i].Type);
					count++;
				}
			}
			writer.Write(count);
			mask = 0;
			byte count2 = 0;
			// send last unique event
			for (int i = length - 1; count2 < count; i--)
			{
				if (((1 << _outgoingEvents[i].Type) & mask) == 0)
				{
					mask |= (1 << _outgoingEvents[i].Type);
					_outgoingEvents[i].Write(writer);
					count2++;
				}
			}
			_outgoingEvents.Clear();
		}

		public override void DisassembleData(Reader reader, UnserializeInfo info)
		{
			var length = reader.ReadByte();
			for (var i = 0; i < length; i++)
			{
				var syncType = reader.ReadByte();
				if (syncType > 3)
				{
					SetInterpolationMethod((InterpolationMethodType)(syncType - 4), false);
					return;
				}

				switch (syncType)
				{
					case 0:
						_interpolationMethod.MovePosition(reader.ReadVector3());
						break;
					case 1:
						_interpolationMethod.SetPosition(reader.ReadVector3());
						break;
					case 2:
						_interpolationMethod.MoveRotation(reader.ReadVector3());
						break;
					case 3:
						_interpolationMethod.SetRotation(reader.ReadVector3());
						break;
				}
			}
		}
	}
}