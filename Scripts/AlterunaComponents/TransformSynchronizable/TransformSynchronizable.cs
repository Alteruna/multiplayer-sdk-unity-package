using System;
using System.Reflection;
using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Core.MethodArguments;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Class <c>TransformSynchronizable</c> defines a component which synchronizes its game objects transform with other clients in the Playroom.
	/// </summary>
	/// <remarks>
	///	<img src="../images/Alteruna.TransformSynchronizable.png" />
	/// </remarks>
	[DisallowMultipleComponent, AddComponentMenu("Alteruna/Transform/Transform Synchronizable", 0), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class TransformSynchronizable : TransformSynchronizableCommon
	{
		private const double MARGIN_POS_SQR = 1E-5f;
		private const double MARGIN_ANGULAR_SIN_SQR = 3.0461740160633344E-10f; // 0.001 degrees
		private const double MARGIN_SCALE_SQR = 1E-6f;
		
		private const double DEG_TO_RAD = 0.017453292;
		
		/// <summary>
		/// Set exactly what can and cannot be synced.
		/// </summary>
		[SerializeField, Tooltip("Set what can and cannot be synced.")]
		private TransformSyncConstraint SyncedAxes = TransformSyncConstraint.Everything;

		// Data
		[NonSerialized] private Vector3 _oldPosition = Vector3.zero;
		[NonSerialized] private Vector3 _oldRotation = Vector3.zero;
		[NonSerialized] private Vector3 _oldScale = Vector3.one;

		[NonSerialized] private IPackageOrderValidator _posId, _rotId, _scaleId;
		
		public new void Awake()
		{
			base.Awake();
			if (Reliability == Reliability.Unreliable)
			{
				_posId = new PackageOrderValidatorNone();
				_rotId = new PackageOrderValidatorNone();
				_scaleId = new PackageOrderValidatorNone();
			}
			else
			{
				_posId = new PackageOrderValidator();
				_rotId = new PackageOrderValidator();
				_scaleId = new PackageOrderValidator();
			}
		}

		public override void DisassembleData(Reader reader, UnserializeInfo info)
		{
			Transform t = transform;

			byte changed = reader.ReadByte();

			if (UseGlobalPosition)
			{
				// Position
				if ((changed & 1) != 0)
				{
					if (_posId.Validate(reader))
					{
						_oldPosition = t.position;

						if ((SyncedAxes & TransformSyncConstraint.PositionX) != 0) _oldPosition.x = reader.ReadFloat();
						if ((SyncedAxes & TransformSyncConstraint.PositionY) != 0) _oldPosition.y = reader.ReadFloat();
						if ((SyncedAxes & TransformSyncConstraint.PositionZ) != 0) _oldPosition.z = reader.ReadFloat();

						t.position = _oldPosition;
					}
				}

				// Rotation
				if ((changed & 2) != 0)
				{
					if (_rotId.Validate(reader))
					{
						_oldRotation = t.eulerAngles;

						if ((SyncedAxes & TransformSyncConstraint.RotationX) != 0) _oldRotation.x = reader.ReadFloat();
						if ((SyncedAxes & TransformSyncConstraint.RotationY) != 0) _oldRotation.y = reader.ReadFloat();
						if ((SyncedAxes & TransformSyncConstraint.RotationZ) != 0) _oldRotation.z = reader.ReadFloat();

						t.eulerAngles = _oldRotation;
					}
				}
			}
			else
			{
				// Position
				if ((changed & 1) != 0)
				{
					if (_posId.Validate(reader))
					{
						_oldPosition = t.localPosition;

						if ((SyncedAxes & TransformSyncConstraint.PositionX) != 0) _oldPosition.x = reader.ReadFloat();
						if ((SyncedAxes & TransformSyncConstraint.PositionY) != 0) _oldPosition.y = reader.ReadFloat();
						if ((SyncedAxes & TransformSyncConstraint.PositionZ) != 0) _oldPosition.z = reader.ReadFloat();

						t.localPosition = _oldPosition;
					}
				}

				// Rotation
				if ((changed & 2) != 0)
				{
					if (_rotId.Validate(reader))
					{
						_oldRotation = t.localRotation.eulerAngles;

						if ((SyncedAxes & TransformSyncConstraint.RotationX) != 0) _oldRotation.x = reader.ReadFloat();
						if ((SyncedAxes & TransformSyncConstraint.RotationY) != 0) _oldRotation.y = reader.ReadFloat();
						if ((SyncedAxes & TransformSyncConstraint.RotationZ) != 0) _oldRotation.z = reader.ReadFloat();

						t.localRotation = Quaternion.Euler(_oldRotation);
					}
				}
			}

			// Scale
			if ((changed & 4) != 0)
			{
				if (_scaleId.Validate(reader))
				{
					_oldScale = t.localScale;

					if ((SyncedAxes & TransformSyncConstraint.ScaleX) != 0) _oldScale.x = reader.ReadFloat();
					if ((SyncedAxes & TransformSyncConstraint.ScaleY) != 0) _oldScale.y = reader.ReadFloat();
					if ((SyncedAxes & TransformSyncConstraint.ScaleZ) != 0) _oldScale.z = reader.ReadFloat();

					t.localScale = _oldScale;
				}
			}
		}

		public override void AssembleData(Writer writer, SerializeInfo info)
		{
			Transform t = transform;
			byte changed = 0;

			bool changedScale = ((SyncedAxes & TransformSyncConstraint.Scale) != 0) && (info.ForceSync || ScaleOutsideRange(_oldScale, t.localScale));
			if (changedScale) changed |= 4;

			if (UseGlobalPosition)
			{
				bool changedPosition = ((SyncedAxes & TransformSyncConstraint.Position) != 0) && (info.ForceSync || PosOutsideRange(_oldPosition, t.position));
				bool changedRotation = ((SyncedAxes & TransformSyncConstraint.Rotation) != 0) && (info.ForceSync || RotOutsideRange(_oldRotation, t.eulerAngles));
				if (changedPosition) changed |= 1;
				if (changedRotation) changed |= 2;
				writer.Write(changed);
				
				// Position
				if (changedPosition)
				{
					_posId.Append(writer);

					_oldPosition = t.position;

					if ((SyncedAxes & TransformSyncConstraint.PositionX) != 0) writer.Write(_oldPosition.x);
					if ((SyncedAxes & TransformSyncConstraint.PositionY) != 0) writer.Write(_oldPosition.y);
					if ((SyncedAxes & TransformSyncConstraint.PositionZ) != 0) writer.Write(_oldPosition.z);
				}

				// Rotation
				if (changedRotation)
				{
					_rotId.Append(writer);

					_oldRotation = t.eulerAngles;

					if ((SyncedAxes & TransformSyncConstraint.RotationX) != 0) writer.Write(_oldRotation.x);
					if ((SyncedAxes & TransformSyncConstraint.RotationY) != 0) writer.Write(_oldRotation.y);
					if ((SyncedAxes & TransformSyncConstraint.RotationZ) != 0) writer.Write(_oldRotation.z);
				}
			}
			else
			{
				bool changedPosition = ((SyncedAxes & TransformSyncConstraint.Position) != 0) && (info.ForceSync || PosOutsideRange(_oldPosition, t.localPosition));
				bool changedRotation = ((SyncedAxes & TransformSyncConstraint.Rotation) != 0) && (info.ForceSync || RotOutsideRange(_oldRotation, t.localRotation.eulerAngles));
				if (changedPosition) changed |= 1;
				if (changedRotation) changed |= 2;
				writer.Write(changed);
				
				// Position
				if (changedPosition)
				{
					_posId.Append(writer);

					_oldPosition = t.localPosition;

					if ((SyncedAxes & TransformSyncConstraint.PositionX) != 0) writer.Write(_oldPosition.x);
					if ((SyncedAxes & TransformSyncConstraint.PositionY) != 0) writer.Write(_oldPosition.y);
					if ((SyncedAxes & TransformSyncConstraint.PositionZ) != 0) writer.Write(_oldPosition.z);
				}

				// Rotation
				if (changedRotation)
				{
					_rotId.Append(writer);

					_oldRotation = t.localRotation.eulerAngles;

					if ((SyncedAxes & TransformSyncConstraint.RotationX) != 0) writer.Write(_oldRotation.x);
					if ((SyncedAxes & TransformSyncConstraint.RotationY) != 0) writer.Write(_oldRotation.y);
					if ((SyncedAxes & TransformSyncConstraint.RotationZ) != 0) writer.Write(_oldRotation.z);
				}
			}

			// Scale
			if (changedScale)
			{
				_scaleId.Append(writer);

				_oldScale = t.localScale;

				if ((SyncedAxes & TransformSyncConstraint.ScaleX) != 0) writer.Write(_oldScale.x);
				if ((SyncedAxes & TransformSyncConstraint.ScaleY) != 0) writer.Write(_oldScale.y);
				if ((SyncedAxes & TransformSyncConstraint.ScaleZ) != 0) writer.Write(_oldScale.z);
			}
		}

		public void Update()
		{
			if (CanSync())
			{
				InternalUpdate();
			}
		}

		private void InternalUpdate()
		{
			Transform t = transform;

			if (!CommitPending)
			{
				if (UseGlobalPosition)
				{
					if (
						PosOutsideRange(_oldPosition, t.position) ||
						RotOutsideRange(_oldRotation, t.eulerAngles) ||
						ScaleOutsideRange(_oldScale, t.localScale))
					{
						Commit();
					}
				}
				else
				{
					if (
						PosOutsideRange(_oldPosition, t.localPosition) ||
						RotOutsideRange(_oldRotation, t.localRotation.eulerAngles) ||
						ScaleOutsideRange(_oldScale, t.localScale))
					{
						Commit();
					}
				}
			}

			SyncUpdate();
			UpdateFunctionScale();
		}

		public void Start()
		{
			Transform t = transform;

			if (UseGlobalPosition)
			{
				_oldPosition = t.position;
				_oldRotation = t.eulerAngles;
			}
			else
			{
				_oldPosition = t.localPosition;
				_oldRotation = t.localRotation.eulerAngles;
			}
			_oldScale = t.localScale;

			UpdateFunctionPos();
			UpdateFunctionRot();
			UpdateFunctionScale();
		}

		public override void Reset()
		{
			base.Reset();
			if (TryGetComponent(out TransformSynchronizable2D _))
			{
				Debug.LogError("Can not have both a TransformSynchronizable2D and a TransformSynchronizable in the same object");
				DestroyImmediate(this);
			}
		}
		
		Func<Vector3, Vector3, bool> PosOutsideRange = (lhs, rhs) =>
		{
			double num1 = lhs.x - rhs.x;
			double num2 = lhs.y - rhs.y;
			double num3 = lhs.z - rhs.z;
			return num1 * num1 + num2 * num2 + num3 * num3 > MARGIN_POS_SQR;
		};
		
		Func<Vector3, Vector3, bool> ScaleOutsideRange = (lhs, rhs) =>
		{
			double num1 = lhs.x - rhs.x;
			double num2 = lhs.y - rhs.y;
			double num3 = lhs.z - rhs.z;
			return num1 * num1 + num2 * num2 + num3 * num3 > MARGIN_SCALE_SQR;
		};
		
		Func<Vector3, Vector3, bool> RotOutsideRange = (lhs, rhs) =>
		{
			double num1 = Math.Sin(DEG_TO_RAD * lhs.x) - Math.Sin(DEG_TO_RAD * rhs.x);
			double num2 = Math.Sin(DEG_TO_RAD * lhs.y) - Math.Sin(DEG_TO_RAD * rhs.y);
			double num3 = Math.Sin(DEG_TO_RAD * lhs.z) - Math.Sin(DEG_TO_RAD * rhs.z);
			return num1 * num1 + num2 * num2 + num3 * num3 > MARGIN_SCALE_SQR;
		};

#region Update Function methods
		private void UpdateFunctionPos()
		{
			if ((SyncedAxes & TransformSyncConstraint.Position) == TransformSyncConstraint.Position)
			{
				PosOutsideRange = (lhs, rhs) =>
				{
					double num1 = lhs.x - rhs.x;
					double num2 = lhs.y - rhs.y;
					double num3 = lhs.z - rhs.z;
					return num1 * num1 + num2 * num2 + num3 * num3 > MARGIN_POS_SQR;
				};
			}
			else if ((SyncedAxes & TransformSyncConstraint.PositionX) == TransformSyncConstraint.PositionX)
			{
				if ((SyncedAxes & TransformSyncConstraint.PositionY) == TransformSyncConstraint.PositionY)
				{
					PosOutsideRange = (lhs, rhs) =>
					{
						double num1 = lhs.x - rhs.x;
						double num2 = lhs.y - rhs.y;
						return num1 * num1 + num2 * num2 > MARGIN_POS_SQR;
					};
				}
				else if ((SyncedAxes & TransformSyncConstraint.PositionZ) == TransformSyncConstraint.PositionZ)
				{
					PosOutsideRange = (lhs, rhs) =>
					{
						double num1 = lhs.x - rhs.x;
						double num3 = lhs.z - rhs.z;
						return num1 * num1 + num3 * num3 > MARGIN_POS_SQR;
					};
				}
				else
				{
					PosOutsideRange = (lhs, rhs) =>
					{
						double num1 = lhs.x - rhs.x;
						return num1 * num1 > MARGIN_POS_SQR;
					};
				}
			}
			else if ((SyncedAxes & TransformSyncConstraint.PositionY) == TransformSyncConstraint.PositionY)
			{
				if ((SyncedAxes & TransformSyncConstraint.PositionZ) == TransformSyncConstraint.PositionZ)
				{
					PosOutsideRange = (lhs, rhs) =>
					{
						double num2 = lhs.y - rhs.y;
						double num3 = lhs.z - rhs.z;
						return num2 * num2 + num3 * num3 > MARGIN_POS_SQR;
					};
				}
				else
				{
					PosOutsideRange = (lhs, rhs) =>
					{
						double num2 = lhs.y - rhs.y;
						return num2 * num2 > MARGIN_POS_SQR;
					};
				}
			}
			else if ((SyncedAxes & TransformSyncConstraint.PositionZ) == TransformSyncConstraint.PositionZ)
			{
				PosOutsideRange = (lhs, rhs) =>
				{
					double num3 = lhs.z - rhs.z;
					return num3 * num3 > MARGIN_POS_SQR;
				};
			}
			else
			{
				PosOutsideRange = (lhs, rhs) => false;
			}
		}

		private void UpdateFunctionScale()
		{
			if ((SyncedAxes & TransformSyncConstraint.Scale) == TransformSyncConstraint.Scale)
			{
				ScaleOutsideRange = (lhs, rhs) =>
				{
					double num1 = lhs.x - rhs.x;
					double num2 = lhs.y - rhs.y;
					double num3 = lhs.z - rhs.z;
					return num1 * num1 + num2 * num2 + num3 * num3 > MARGIN_SCALE_SQR;
				};
			}
			else if ((SyncedAxes & TransformSyncConstraint.ScaleX) == TransformSyncConstraint.ScaleX)
			{
				if ((SyncedAxes & TransformSyncConstraint.ScaleY) == TransformSyncConstraint.ScaleY)
				{
					ScaleOutsideRange = (lhs, rhs) =>
					{
						double num1 = lhs.x - rhs.x;
						double num2 = lhs.y - rhs.y;
						return num1 * num1 + num2 * num2 > MARGIN_SCALE_SQR;
					};
				}
				else if ((SyncedAxes & TransformSyncConstraint.ScaleZ) == TransformSyncConstraint.ScaleZ)
				{
					ScaleOutsideRange = (lhs, rhs) =>
					{
						double num1 = lhs.x - rhs.x;
						double num3 = lhs.z - rhs.z;
						return num1 * num1 + num3 * num3 > MARGIN_SCALE_SQR;
					};
				}
				else
				{
					ScaleOutsideRange = (lhs, rhs) =>
					{
						double num1 = lhs.x - rhs.x;
						return num1 * num1 > MARGIN_SCALE_SQR;
					};
				}
			}
			else if ((SyncedAxes & TransformSyncConstraint.ScaleY) == TransformSyncConstraint.ScaleY)
			{
				if ((SyncedAxes & TransformSyncConstraint.ScaleZ) == TransformSyncConstraint.ScaleZ)
				{
					ScaleOutsideRange = (lhs, rhs) =>
					{
						double num2 = lhs.y - rhs.y;
						double num3 = lhs.z - rhs.z;
						return num2 * num2 + num3 * num3 > MARGIN_SCALE_SQR;
					};
				}
				else
				{
					ScaleOutsideRange = (lhs, rhs) =>
					{
						double num2 = lhs.y - rhs.y;
						return num2 * num2 > MARGIN_SCALE_SQR;
					};
				}
			}
			else if ((SyncedAxes & TransformSyncConstraint.ScaleZ) == TransformSyncConstraint.ScaleZ)
			{
				ScaleOutsideRange = (lhs, rhs) =>
				{
					double num3 = lhs.z - rhs.z;
					return num3 * num3 > MARGIN_SCALE_SQR;
				};
			}
			else
			{
				ScaleOutsideRange = (lhs, rhs) => false;
			}
		}

		private void UpdateFunctionRot()
		{
			if ((SyncedAxes & TransformSyncConstraint.Rotation) == TransformSyncConstraint.Rotation)
			{
				RotOutsideRange = (lhs, rhs) =>
				{
					double num1 = Math.Sin(DEG_TO_RAD * lhs.x) - Math.Sin(DEG_TO_RAD * rhs.x);
					double num2 = Math.Sin(DEG_TO_RAD * lhs.y) - Math.Sin(DEG_TO_RAD * rhs.y);
					double num3 = Math.Sin(DEG_TO_RAD * lhs.z) - Math.Sin(DEG_TO_RAD * rhs.z);
					return num1 * num1 + num2 * num2 + num3 * num3 > MARGIN_ANGULAR_SIN_SQR;
				};
			}
			else if ((SyncedAxes & TransformSyncConstraint.RotationX) == TransformSyncConstraint.RotationX)
			{
				if ((SyncedAxes & TransformSyncConstraint.RotationY) == TransformSyncConstraint.RotationY)
				{
					RotOutsideRange = (lhs, rhs) =>
					{
						double num1 = Math.Sin(DEG_TO_RAD * lhs.x) - Math.Sin(DEG_TO_RAD * rhs.x);
						double num2 = Math.Sin(DEG_TO_RAD * lhs.y) - Math.Sin(DEG_TO_RAD * rhs.y);
						return num1 * num1 + num2 * num2 > MARGIN_ANGULAR_SIN_SQR;
					};
				}
				else if ((SyncedAxes & TransformSyncConstraint.RotationZ) == TransformSyncConstraint.RotationZ)
				{
					RotOutsideRange = (lhs, rhs) =>
					{
						double num1 = Math.Sin(DEG_TO_RAD * lhs.x) - Math.Sin(DEG_TO_RAD * rhs.x);
						double num3 = Math.Sin(DEG_TO_RAD * lhs.z) - Math.Sin(DEG_TO_RAD * rhs.z);
						return num1 * num1 + num3 * num3 > MARGIN_ANGULAR_SIN_SQR;
					};
				}
				else
				{
					RotOutsideRange = (lhs, rhs) =>
					{
						double num1 = Math.Sin(DEG_TO_RAD * lhs.x) - Math.Sin(DEG_TO_RAD * rhs.x);
						return num1 * num1 > MARGIN_ANGULAR_SIN_SQR;
					};
				}
			}
			else if ((SyncedAxes & TransformSyncConstraint.RotationY) == TransformSyncConstraint.RotationY)
			{
				if ((SyncedAxes & TransformSyncConstraint.RotationZ) == TransformSyncConstraint.RotationZ)
				{
					RotOutsideRange = (lhs, rhs) =>
					{
						double num2 = Math.Sin(DEG_TO_RAD * lhs.y) - Math.Sin(DEG_TO_RAD * rhs.y);
						double num3 = Math.Sin(DEG_TO_RAD * lhs.z) - Math.Sin(DEG_TO_RAD * rhs.z);
						return num2 * num2 + num3 * num3 > MARGIN_ANGULAR_SIN_SQR;
					};
				}
				else
				{
					RotOutsideRange = (lhs, rhs) =>
					{
						double num2 = Math.Sin(DEG_TO_RAD * lhs.y) - Math.Sin(DEG_TO_RAD * rhs.y);
						return num2 * num2 > MARGIN_ANGULAR_SIN_SQR;
					};
				}
			}
			else if ((SyncedAxes & TransformSyncConstraint.RotationZ) == TransformSyncConstraint.RotationZ)
			{
				RotOutsideRange = (lhs, rhs) =>
				{
					double num3 = Math.Sin(DEG_TO_RAD * lhs.z) - Math.Sin(DEG_TO_RAD * rhs.z);
					return num3 * num3 > MARGIN_ANGULAR_SIN_SQR;
				};
			}
			else
			{
				RotOutsideRange = (lhs, rhs) => false;
			}
		}
#endregion
	}
}