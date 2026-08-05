using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	public partial class InterpolationTransformSynchronizable
	{
		private class InterpolationMethodNone
		{
			private protected readonly Transform TargetTransform;
			private protected bool InterpolatePosition;
			private protected bool InterpolateRotation;
			public bool IsSleeping;

			public InterpolationMethodNone(Transform targetTransform) => TargetTransform = targetTransform;

			public virtual void MovePosition(Vector3 position)
			{
				TargetTransform.position = position;
			}

			public virtual void SetPosition(Vector3 position)
			{
				TargetTransform.position = position;
			}

			public virtual void MoveRotation(Vector3 rotation)
			{
				MoveRotation(Quaternion.Euler(rotation));
			}

			public virtual void MoveRotation(Quaternion rotation)
			{
				TargetTransform.rotation = rotation;
			}

			public virtual void SetRotation(Vector3 rotation)
			{
				SetRotation(Quaternion.Euler(rotation));
			}

			public virtual void SetRotation(Quaternion rotation)
			{
				TargetTransform.rotation = rotation;
			}

			public virtual void Interpolate() { }
		}
	}
}