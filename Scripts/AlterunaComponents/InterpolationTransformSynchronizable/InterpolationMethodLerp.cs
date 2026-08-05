using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	public partial class InterpolationTransformSynchronizable
	{
		private class InterpolationMethodLerp : InterpolationMethodLerpRelative
		{
			private protected Vector3 BasePosition;
			private protected Quaternion BaseRotation;
			private float _scaledTime;

			public InterpolationMethodLerp(Transform targetTransform) : base(targetTransform) { }

			public override void MovePosition(Vector3 position)
			{
				TimeF = Time.fixedUnscaledDeltaTime;
				BasePosition = TargetTransform.position;
				TargetPosition = position;
				InterpolatePosition = true;
				IsSleeping = false;
			}

			public override void MoveRotation(Quaternion rotation)
			{
				TimeF = Time.fixedUnscaledDeltaTime;
				BaseRotation = TargetTransform.rotation;
				TargetRotation = rotation;
				InterpolateRotation = true;
				IsSleeping = false;
			}

			public override void Interpolate()
			{
				if (IsSleeping) return;

				if ((TimeF -= Time.deltaTime) <= 0)
				{
					TargetTransform.position = TargetPosition;
					TargetTransform.rotation = TargetRotation;
					InterpolatePosition = InterpolateRotation = false;
					IsSleeping = true;
					return;
				}

				_scaledTime = Mathf.InverseLerp(Time.fixedUnscaledDeltaTime, 0, TimeF);

				if (InterpolatePosition)
					TargetTransform.position = Vector3.Lerp(BasePosition, TargetPosition, _scaledTime);

				if (InterpolateRotation)
					TargetTransform.rotation = Quaternion.Lerp(BaseRotation, TargetRotation, _scaledTime);
			}
		}
	}
}