using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	public partial class InterpolationTransformSynchronizable
	{
		private class InterpolationMethodSpring : InterpolationMethodLerpRelative
		{
			const float ANGULAR_VELOCITY_MULTIPLIER = 1f;
			const float LINER_VELOCITY_MULTIPLIER = 0.98f;
			const float SPRING_ANGULAR_STIFFNESS = 0.4f;
			const float SPRING_LINER_STIFFNESS = 0.2f;
			
			private Quaternion _angularVelocity;
			private float _deltaTime;
			private float _inDeltaTime;
			private float _inversedLinerDeltaTime;

			private float _linerDeltaTime;
			private Vector3 _velocity;

			public InterpolationMethodSpring(Transform targetTransform) : base(targetTransform) { }

			public override void MovePosition(Vector3 position)
			{
				//TODO: update expression
				//y = tan^(-1)((x×75.87872472458215611) °)×7.3
				TimeF = Mathf.Atan(Vector3.Distance(TargetTransform.position, position) * 75.878724724582156114f) *
				        7.3f;
				TargetPosition = position;
				InterpolatePosition = true;
				IsSleeping = false;
			}

			public override void SetPosition(Vector3 position)
			{
				TargetTransform.position = TargetPosition = position;
				InterpolatePosition = false;
				IsSleeping = !InterpolateRotation;
				_velocity = Vector3.zero;
			}

			public override void MoveRotation(Quaternion rotation)
			{
				//TODO: update expression
				//y = tan^(-1)((x×75.87872472458215611) °)×7.3
				TimeF = Mathf.Atan(Quaternion.Angle(TargetTransform.rotation, rotation) * 75.878724724582156114f) *
				        7.3f;
				TargetRotation = rotation;
				InterpolateRotation = true;
				IsSleeping = false;
			}

			public override void SetRotation(Quaternion rotation)
			{
				TargetTransform.rotation = TargetRotation = rotation;
				InterpolateRotation = false;
				IsSleeping = !InterpolatePosition;
				_angularVelocity = Quaternion.identity;
			}

			public override void Interpolate()
			{
				if (IsSleeping) return;

				if ((TimeF -= Time.deltaTime) <= 0)
				{
					TargetTransform.position = TargetPosition;
					TargetTransform.rotation = TargetRotation;
					InterpolatePosition = false;
					InterpolateRotation = false;
					IsSleeping = true;
					return;
				}

				if (InterpolatePosition)
				{
					_linerDeltaTime = Time.deltaTime * SPRING_LINER_STIFFNESS;
					_inversedLinerDeltaTime = 1 - _linerDeltaTime;

					var currentPosition = TargetTransform.position;
					_velocity = (TargetPosition - currentPosition) * _linerDeltaTime +
					            _velocity * _inversedLinerDeltaTime;
					_velocity *= LINER_VELOCITY_MULTIPLIER;

					TargetTransform.position = currentPosition + _velocity;
				}

				if (InterpolateRotation)
				{
					_deltaTime = Time.deltaTime * SPRING_ANGULAR_STIFFNESS;
					_inDeltaTime = 1 - _deltaTime;

					var currentRotation = TargetTransform.rotation;
					_angularVelocity = new Quaternion(
						(TargetRotation.x - currentRotation.x) * _deltaTime + _angularVelocity.x * _inDeltaTime,
						(TargetRotation.y - currentRotation.y) * _deltaTime + _angularVelocity.y * _inDeltaTime,
						(TargetRotation.z - currentRotation.z) * _deltaTime + _angularVelocity.z * _inDeltaTime,
						(TargetRotation.w - currentRotation.w) * _deltaTime + _angularVelocity.w * _inDeltaTime
					);

					currentRotation.x += _angularVelocity.x * ANGULAR_VELOCITY_MULTIPLIER;
					currentRotation.y += _angularVelocity.y * ANGULAR_VELOCITY_MULTIPLIER;
					currentRotation.z += _angularVelocity.z * ANGULAR_VELOCITY_MULTIPLIER;
					currentRotation.w += _angularVelocity.w * ANGULAR_VELOCITY_MULTIPLIER;

					TargetTransform.rotation = currentRotation;
				}
			}
		}
	}
}