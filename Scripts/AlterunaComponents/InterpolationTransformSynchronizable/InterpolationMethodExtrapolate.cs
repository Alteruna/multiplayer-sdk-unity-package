using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	public partial class InterpolationTransformSynchronizable
	{
		private class InterpolationMethodExtrapolate : InterpolationMethodLerp
		{

			private Vector3 _oldPos;
			private Vector3 _oldRot;

			public InterpolationMethodExtrapolate(Transform targetTransform) : base(targetTransform)
			{
				_oldPos = targetTransform.position;
				_oldRot = targetTransform.eulerAngles;
			}

			public override void MovePosition(Vector3 position)
			{
				TimeF = Time.fixedUnscaledDeltaTime;
				BasePosition = TargetTransform.position;
				TargetPosition = position+position-_oldPos;
				_oldPos = position;
				InterpolatePosition = true;
				IsSleeping = false;
			}

			public override void MoveRotation(Quaternion rotation) => MoveRotation(rotation.eulerAngles);

			public override void MoveRotation(Vector3 rotation)
			{
				TimeF = Time.fixedUnscaledDeltaTime;
				BaseRotation = TargetTransform.rotation;
				TargetRotation = Quaternion.Euler(rotation+rotation-_oldRot);
				_oldRot = rotation;
				InterpolateRotation = true;
				IsSleeping = false;
			}
		}
	}
}