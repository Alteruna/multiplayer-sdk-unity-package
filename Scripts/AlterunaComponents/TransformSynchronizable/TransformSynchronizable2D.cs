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
	/// Class <c>TransformSynchronizable2D</c> defines a component which synchronizes its game objects transform with other clients in the Playroom.
	/// </summary>
	/// <remarks>
	///	<img src="../images/Alteruna.Transform2DSynchronizable.png" />
	/// </remarks>
	[DisallowMultipleComponent, AddComponentMenu("Alteruna/Transform/Transform 2D Synchronizable"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class TransformSynchronizable2D : TransformSynchronizableCommon
	{
		private const double MARGIN_LINEAR_SQR = 5E-4f; // default comparator for Vector2 is 9.999999439624929E-11f
		private const double MARGIN_ANGULAR_SIN_SQR = 3.0461740160633344E-10f; // 0.001 degrees
		
		private const double DEG_TO_RAD = 0.017453292;

		/// <summary>
		/// Set exactly what can and cannot be synced.
		/// </summary>
		[SerializeField, Tooltip("Set what can and cannot be synced.")]
		private Transform2DAxes SyncedAxes = Transform2DAxes.Everything;

		// Data
		[NonSerialized] private Vector2 _oldPosition = Vector2.zero;
		[NonSerialized] private float _oldRotation;
		[NonSerialized] private Vector2 _oldScale = Vector2.one;

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
				if ((changed & 1) != 0)
				{
					if (_posId.Validate(reader))
					{
						_oldPosition = t.position;

						if ((SyncedAxes & Transform2DAxes.PositionX) != 0) _oldPosition.x = reader.ReadFloat();
						if ((SyncedAxes & Transform2DAxes.PositionY) != 0) _oldPosition.y = reader.ReadFloat();

						t.position = _oldPosition;
					}
				}

				if ((changed & 2) != 0)
				{
					if (_rotId.Validate(reader))
					{
						_oldRotation = reader.ReadFloat();
						t.rotation = Quaternion.Euler(new Vector3(0, 0, _oldRotation));
					}
				}
			}
			else
			{
				if ((changed & 1) != 0)
				{
					if (_posId.Validate(reader))
					{
						_oldPosition = t.localPosition;

						if ((SyncedAxes & Transform2DAxes.PositionX) != 0) _oldPosition.x = reader.ReadFloat();
						if ((SyncedAxes & Transform2DAxes.PositionY) != 0) _oldPosition.y = reader.ReadFloat();

						t.localPosition = _oldPosition;
					}
				}

				if ((changed & 2) != 0)
				{
					if (_rotId.Validate(reader))
					{
						_oldRotation = reader.ReadFloat();
						t.localRotation = Quaternion.Euler(new Vector3(0, 0, _oldRotation));
					}
				}
			}

			if ((changed & 4) != 0)
			{
				if (_scaleId.Validate(reader))
				{
					_oldScale = t.localScale;

					if ((SyncedAxes & Transform2DAxes.ScaleX) != 0) _oldScale.x = reader.ReadFloat();
					if ((SyncedAxes & Transform2DAxes.ScaleY) != 0) _oldScale.y = reader.ReadFloat();

					t.localScale = new Vector3(_oldScale.x, _oldScale.y, t.localScale.z);
				}
			}
		}

		public override void AssembleData(Writer writer, SerializeInfo info)
		{
			Transform t = transform;
			byte changed = 0;
			
			bool changedScale = ((SyncedAxes & Transform2DAxes.Scale) != 0) && (info.ForceSync || OutsideMargin(_oldScale , t.localScale));
			if (changedScale) changed |= 4;

			if (UseGlobalPosition)
			{
				bool changedPosition = ((SyncedAxes & Transform2DAxes.Position) != 0) && (info.ForceSync || OutsideMargin(_oldPosition, t.position));
				bool changedRotation = ((SyncedAxes & Transform2DAxes.Rotation) != 0) && (info.ForceSync || OutsideMarginAngular(_oldRotation, t.eulerAngles.z));
				if (changedPosition) changed |= 1;
				if (changedRotation) changed |= 2;
				writer.Write(changed);
				
				// Position
				if (changedPosition)
				{
					_posId.Append(writer);

					_oldPosition = t.position;

					if ((SyncedAxes & Transform2DAxes.PositionX) != 0) writer.Write(_oldPosition.x);
					if ((SyncedAxes & Transform2DAxes.PositionY) != 0) writer.Write(_oldPosition.y);
				}

				// Rotation
				if (changedRotation)
				{
					_rotId.Append(writer);

					_oldRotation = t.eulerAngles.z;
					writer.Write(_oldRotation);
				}
			}
			else
			{
				bool changedPosition = ((SyncedAxes & Transform2DAxes.Position) == Transform2DAxes.Position) && (info.ForceSync || OutsideMargin(_oldPosition, t.localPosition));
				bool changedRotation = ((SyncedAxes & Transform2DAxes.Rotation) == Transform2DAxes.Rotation) && (info.ForceSync || OutsideMarginAngular(_oldRotation, t.localRotation.eulerAngles.z));
				if (changedPosition) changed |= 1;
				if (changedRotation) changed |= 2;
				writer.Write(changed);
				
				// Position
				if (changedPosition)
				{
					_posId.Append(writer);

					_oldPosition = t.localPosition;

					if ((SyncedAxes & Transform2DAxes.PositionX) != 0) writer.Write(_oldPosition.x);
					if ((SyncedAxes & Transform2DAxes.PositionY) != 0) writer.Write(_oldPosition.y);
				}

				// Rotation
				if (changedRotation)
				{
					_rotId.Append(writer);

					_oldRotation = t.localRotation.eulerAngles.z;
					writer.Write(_oldRotation);
				}
			}

			// Scale
			if (changedScale)
			{
				_scaleId.Append(writer);

				_oldScale = t.localScale;

				if ((SyncedAxes & Transform2DAxes.ScaleX) != 0) writer.Write(_oldScale.x);
				if ((SyncedAxes & Transform2DAxes.ScaleY) != 0) writer.Write(_oldScale.y);
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

			if (UseGlobalPosition)
			{
				if ((SyncedAxes & Transform2DAxes.Position) != 0 && OutsideMargin(_oldPosition, t.position) ||
				    (SyncedAxes & Transform2DAxes.Rotation) != 0 && OutsideMarginAngular(_oldRotation, t.eulerAngles.z) ||
				    (SyncedAxes & Transform2DAxes.Scale) != 0 && OutsideMargin(_oldScale, t.localScale))
				{
					Commit();
				}
			}
			else
			{
				if ((SyncedAxes & Transform2DAxes.Position) != 0 && OutsideMargin(_oldPosition, t.localPosition) ||
				    (SyncedAxes & Transform2DAxes.Rotation) != 0 && OutsideMarginAngular(_oldRotation, t.localRotation.eulerAngles.z) ||
				    (SyncedAxes & Transform2DAxes.Scale) != 0 && OutsideMargin(_oldScale, t.localScale))
				{
					Commit();
				}
			}

			SyncUpdate();
		}

		public void Start()
		{
			Transform t = transform;
			if (UseGlobalPosition)
			{
				_oldPosition = t.position;
				_oldRotation = t.rotation.z;
				_oldScale = t.localScale;
			}
			else
			{
				_oldPosition = t.localPosition;
				_oldRotation = t.localRotation.z;
				_oldScale = t.localScale;
			}
		}

		public override void Reset()
		{
			base.Reset();
			if (TryGetComponent(out TransformSynchronizable _))
			{
				Debug.LogError("Can not have both a TransformSynchronizable and a TransformSynchronizable2D in the same object");
				DestroyImmediate(this);
			}
		}
		
		private static bool OutsideMargin(Vector2 lhs, Vector2 rhs)
		{
			double num1 = lhs.x - rhs.x;
			double num2 = lhs.y - rhs.y;
			return num1 * num1 + num2 * num2 > MARGIN_LINEAR_SQR;
		}
		
		private static bool OutsideMarginAngular(float lhs, float rhs)
		{
			double num = Math.Sin(DEG_TO_RAD * lhs) - Math.Sin(DEG_TO_RAD * rhs);
			return num * num > MARGIN_ANGULAR_SIN_SQR;
		}
	}
}