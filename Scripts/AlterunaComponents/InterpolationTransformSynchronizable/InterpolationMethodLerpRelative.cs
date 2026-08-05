using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	public partial class InterpolationTransformSynchronizable
	{
		private class InterpolationMethodLerpRelative : InterpolationMethodNone
		{
			private protected Vector3 TargetPosition;
			private protected Quaternion TargetRotation;
			private protected float TimeF;

			public InterpolationMethodLerpRelative(Transform targetTransform) : base(targetTransform)
			{
				TargetPosition = TargetTransform.position;
				TargetRotation = TargetTransform.rotation;
			}

			public override void MovePosition(Vector3 position)
			{
				//y = tan^(-1)((x×75.87872472458215611) °)×7.33733469039886
				TimeF = Mathf.Atan(Vector3.Distance(TargetTransform.position, position) * 75.878724724582156114f) *
				        7.33733469039886f;
				TargetPosition = position;
				InterpolatePosition = true;
				IsSleeping = false;
			}

			public override void SetPosition(Vector3 position)
			{
				TargetTransform.position = TargetPosition = position;
				InterpolatePosition = false;
				IsSleeping = !InterpolateRotation;
			}

			public override void MoveRotation(Quaternion rotation)
			{
				//y = tan^(-1)((x×75.87872472458215611) °)×7.33733469039886
				TimeF = Mathf.Atan(Quaternion.Angle(TargetTransform.rotation, rotation) * 75.878724724582156114f) *
				        7.33733469039886f;
				TargetRotation = rotation;
				InterpolateRotation = true;
				IsSleeping = false;
			}
			
			public override void MoveRotation(Vector3 rotation) => MoveRotation(Quaternion.Euler(rotation));

			public override void SetRotation(Quaternion rotation)
			{
				TargetTransform.rotation = TargetRotation = rotation;
				InterpolateRotation = false;
				IsSleeping = !InterpolatePosition;
			}

			public override void Interpolate()
			{
				if (IsSleeping) return;

				TimeF -= Time.deltaTime;
				if (InterpolatePosition)
				{
					TargetTransform.position = Vector3.Lerp(
						TargetTransform.position,
						TargetPosition,
						Time.deltaTime*2
					);
					if (TimeF <= 0)
					{
						TargetTransform.position = TargetPosition;
						InterpolatePosition = false;
						IsSleeping = !InterpolateRotation;
					}
				}

				if (InterpolateRotation)
				{
					TargetTransform.rotation = Quaternion.Lerp(
						TargetTransform.rotation,
						TargetRotation,
						Time.deltaTime*2
					);
					if (TimeF <= 0)
					{
						TargetTransform.rotation = TargetRotation;
						InterpolateRotation = false;
						IsSleeping = !InterpolatePosition;
					}
				}
			}
		}
	}
}