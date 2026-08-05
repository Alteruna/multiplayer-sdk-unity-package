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
	/// Common Transform Synchronizable methods.
	/// </summary>
	[DisallowMultipleComponent]
	public abstract class TransformSynchronizableCommon : Synchronizable
	{
		private const float MAX_REFRESH_RATE = 120f;
		private const float MIN_REFRESH_RATE = 2.777777777777778e-4f;

		/// <summary>
		/// Sync global position and rotation, otherwise use local.
		/// </summary>
		[Tooltip("Sync global position and rotation, otherwise use local.")]
		public bool UseGlobalPosition;

		/// <summary>
		/// When enabled, it will only automatically sync from the lowest id user.
		/// </summary>
		/// <para>
		///	Does not apply if possessed (placed in or under an Alteruna Avatar).
		/// </para>
#if TRINITY_REGISTERED
		[Tooltip("When enabled, it will only automatically sync from the lowest id user. (does not apply if placed in or under an Alteruna Avatar)")]
#else
		[HideInInspector]
#endif
		[SerializeField] protected bool OnlySyncFromRoomOwner;

		[NonSerialized] private protected bool _force;
		[NonSerialized] private protected bool _isPossesed;
		[NonSerialized] private protected bool _isMe;

		/// <summary>
		/// Set How often to automatically sync data.
		/// Can be set between once every hour and 120 times per second.
		/// </summary>
		[Range(MIN_REFRESH_RATE, MAX_REFRESH_RATE), SerializeField, Tooltip("How often to automatically sync data.")]
		private float refreshRate = 30f;

		[NonSerialized] private float _lastSyncTime;
		[NonSerialized] private float _timeBetweenSyncs;

		/// <summary>
		/// Set How often to automatically sync data.
		/// Can be set between once every hour and 120 times per second.
		/// </summary>
		public float RefreshRate
		{
			get => refreshRate;
			set => _timeBetweenSyncs = 1f / (refreshRate = Mathf.Clamp(value, MIN_REFRESH_RATE, MAX_REFRESH_RATE));
		}

		public void Awake()
		{
			_timeBetweenSyncs = 1 / refreshRate;
		}

		public void OnValidate()
		{
			_timeBetweenSyncs = 1 / refreshRate;
		}

		protected bool CanSync()
		{
#if TRINITY_REGISTERED
			if (_isPossesed) {
				//if (!_isMe) return false;
			}
			else
			{
				if (OnlySyncFromRoomOwner && !Multiplayer.Me.IsHost()) return false;
			}
#else
			//if (!_isMe) return false;
#endif
			float delta = Time.unscaledTime - _lastSyncTime;
			if (delta >= _timeBetweenSyncs)
			{
				if (delta > _timeBetweenSyncs * 2)
				{
					_lastSyncTime = Time.unscaledTime;
				}
				else
				{
					_lastSyncTime += _timeBetweenSyncs;
				}

				return true;
			}

			return false;
		}

		private protected bool ValidateDataId(byte stored, byte incoming)
		{
			return stored == 0 || stored <= incoming && stored >= incoming - 4 || stored > 252 && incoming <= (byte)(stored + 4);
		}

		private protected byte AppendDataId(byte id)
		{
			if (id == 255) id += 2;
			else id++;
			return id;
		}


		/// <summary>
		/// Set position of transform and sync it to all clients.
		/// </summary>
		/// <param name="pos">new position</param>
		public void Teleport(Vector3 pos)
		{
			if (transform.position == pos) return;

			transform.position = pos;
			Commit();
			SyncUpdate();
		}

		/// <summary>
		/// Set position of transform and sync it to all clients.
		/// </summary>
		/// <param name="pos">new position</param>
		public void Teleport(Vector2 pos)
		{
			var t = transform;
			if ((Vector2)t.position == pos) return;

			t.position = new Vector3(pos.x, pos.y, t.position.z);
			Commit();
			SyncUpdate();
		}

		public override void AssembleData(Writer writer, SerializeInfo info) { }

		public override void DisassembleData(Reader reader, UnserializeInfo info) { }

		public override void Possessed(bool isMe, User user)
		{
			OnlySyncFromRoomOwner = false;
			_isPossesed = true;
			_isMe = isMe;
			enabled = isMe;
		}

		public override void Unpossessed()
		{
			_isMe = false;
			_isPossesed = false;
			enabled = false;
		}

		public override void Reset()
		{
			base.Reset();
			if (TryGetComponent(out RigidbodySynchronizableCommon _))
			{
				Debug.LogError("Can not have both a RigidbodySynchronizable and a TransformSynchronizable in the same object");
				DestroyImmediate(this);
			}

#if !TRINITY_EVALUATE
			else if (TryGetComponent(out InterpolationTransformSynchronizable _))
			{
				Debug.LogError("Can not have both a InterpolationTransformSynchronizable and a TransformSynchronizable in the same object");
				DestroyImmediate(this);
			}
#endif
		}

		/// <summary>
		/// Flags for setting what axis to sync.
		/// </summary>
		[Flags]
		public enum TransformSyncConstraint
		{
			None = 0,
			Everything = 511,
			Position = 7,
			PositionX = 1,
			PositionY = 2,
			PositionZ = 4,
			Rotation = 56,
			RotationX = 8,
			RotationY = 16,
			RotationZ = 32,
			Scale = 448,
			ScaleX = 64,
			ScaleY = 128,
			ScaleZ = 256,
		}

		/// <summary>
		/// Flags for setting what axis to sync.
		/// </summary>
		[Flags, Serializable]
		public enum Transform2DAxes
		{
			None = 0,
			Everything = 203,
			Position = 3,
			PositionX = 1,
			PositionY = 2,
			Rotation = 8,
			RotationZ = 8,
			Scale = 192,
			ScaleX = 64,
			ScaleY = 128,
		}
	}
}