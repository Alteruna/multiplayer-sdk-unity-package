using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	public partial class InterpolationTransformSynchronizable
	{

		// ReSharper disable once InconsistentNaming
		/// <summary>
		///   <para>The world space position of the Transform.</para>
		///   <para>On set, moves position.</para>
		/// </summary>
		public Vector3 position
		{
			get => transform.position;
			set => MovePosition(value);
		}
		
		/// <summary>
		///   <para>Moves the transform towards position.</para>
		/// </summary>
		/// <param name="pos">Provides the new position for the transform object.</param>
		public void MovePosition(Vector3 pos)
		{
			_interpolationMethodLocal.MovePosition(pos);
			if (ClientPrediction)
			{
				pos += _velosity;
			}
			_outgoingEvents.Add(new OutgoingEvent(0, pos));
			Commit();
		}

		/// <summary>
		///   <para>Sets the transform to a position.</para>
		/// </summary>
		/// <param name="pos">Provides the new position for the transform object.</param>
		public void SetPosition(Vector3 pos)
		{
			_outgoingEvents.Add(new OutgoingEvent(1, pos));
			_interpolationMethodLocal.SetPosition(pos);
			Commit();
		}

		/// <summary>
		///   <para>Rotates the transform to rotation.</para>
		/// </summary>
		/// <param name="rot">The new rotation for the transform.</param>
		public void MoveRotation(Vector3 rot)
		{
			_outgoingEvents.Add(new OutgoingEvent(2, rot));
			_interpolationMethodLocal.MoveRotation(rot);
			Commit();
		}

		/// <summary>
		///   <para>Rotates the transform to rotation.</para>
		/// </summary>
		/// <param name="rot">The new rotation for the transform.</param>
		public void MoveRotation(Quaternion rot)
		{
			_outgoingEvents.Add(new OutgoingEvent(2, rot.eulerAngles));
			_interpolationMethodLocal.MoveRotation(rot);
			Commit();
		}

		/// <summary>
		///   <para>Set the rotation the transform to rotation.</para>
		/// </summary>
		/// <param name="rot">The new rotation for the transform.</param>
		public void SetRotation(Vector3 rot)
		{
			_outgoingEvents.Add(new OutgoingEvent(3, rot));
			_interpolationMethodLocal.SetRotation(rot);
			Commit();
		}

		/// <summary>
		///   <para>Set the rotation the transform to rotation.</para>
		/// </summary>
		/// <param name="rot">The new rotation for the transform.</param>
		public void SetRotation(Quaternion rot)
		{
			_outgoingEvents.Add(new OutgoingEvent(3, rot.eulerAngles));
			_interpolationMethodLocal.SetRotation(rot);
			Commit();
		}
	}
}