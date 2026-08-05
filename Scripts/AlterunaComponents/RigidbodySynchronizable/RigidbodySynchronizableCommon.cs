using System;
using System.Reflection;
using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Core.MethodArguments;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Common Rigidbody synchronizable methods and.
	/// </summary>
	/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.html"/>
	[DisallowMultipleComponent]
	public abstract class RigidbodySynchronizableCommon : Synchronizable
	{
		private protected const int OVERHEAD = 21;

		/// <summary>
		/// Ignored layers will not cause the object trigger sync on collision.
		/// </summary>
		public static LayerMask IgnoredLayers;

		/// <summary>
		/// Delay from the last controlled forced sync to the next soft sync.
		/// </summary>
		private const float SOFT_SYNC_DELAY = 0.5f;

		/// <summary>
		/// How often to automatically sync data in skips of FixedUpdate.
		/// </summary>
		/// <remarks>
		///	Only for automatic updates. Changes to velocity, position, etc. will trigger more frequent updates.
		/// </remarks>
		[Min(1), Tooltip("How often to automatically sync data in skips of FixedUpdate.")]
		public int SyncEveryNUpdates = 10;

		/// <summary>
		/// Sync velocity and position every Nth sync.
		/// </summary>
		[Min(1), Tooltip("Sync velocity and position every Nth sync.")]
		public int FullSyncEveryNSync = 4;

		/// <summary>
		/// When true, this client will sync the object will to all other clients.
		/// </summary>
		/// <remarks>
		/// Only one client can control the object at a time.
		/// Enabling this will disable it on all other clients.
		/// </remarks>
		[Tooltip("When enabled, this client will sync the object will to all other clients.")]
		public bool SendData = true;

		/// <summary>
		/// When true, the object will be moved and rotated using its transform directly instead of using the physics engine.
		/// This is not recommended, but may resolve some issues where it doesn't sync correctly.
		/// </summary>
		[Tooltip("Move and rotate using transform instead of physics engine. (Not recommended)")]
		public bool ApplyAsTransform = false;

		// ReSharper disable once InconsistentNaming
		/// <summary>
		/// Controls whether physics affects the rigidbody.
		/// </summary>
		public abstract bool isKinematic { get; set; }

		// ReSharper disable once InconsistentNaming
		/// <summary>
		/// Controls whether gravity affects this rigidbody.
		/// </summary>
		public abstract bool useGravity { get; set; }

		/// <summary>
		/// When false, collisions will not cause the object to switch which client is simulated on.
		/// </summary>
		[HideInInspector] public bool AllowCollisionToAssumeOwner = true;

		[NonSerialized] private bool _isSoft = true;
		[NonSerialized] private bool _isFirstSync = true;


		private protected bool _fullSync;


		private protected bool _syncSettings;


		private protected bool _force;


		private protected bool _isPossesedAndNotPossesor;

		private static byte _counter = 0;


		private protected int _currentInterval = 1;


		private protected int _currentFullInterval = 1;

		[NonSerialized] private float _lastControlledForcedSync;

		[NonSerialized] private protected PackageOrderValidator FullSyncId;

		public virtual void Awake()
		{
			//if (Rigidbody == null)
			//	Rigidbody = GetComponent<Rigidbody>();
			//_constraints = ~Rigidbody.constraints;
			if (_counter == 0)
			{
				_counter = (byte)Random.Range(0, 256);
			}

			_currentInterval = _counter++;
			_lastControlledForcedSync = Time.time;
		}

		/// <summary>
		/// Syncs settings to all clients.
		/// Required for changing settings during runtime.
		/// </summary>
		public void SyncSettings()
		{
			if (_isPossesedAndNotPossesor) return;
			_syncSettings = true;
			_currentInterval = 0;
			_currentFullInterval = 0;
			_force = true;
			_fullSync = true;
			SendData = true;
		}

		public void FixedUpdate()
		{
			if ((_force || !IsSleeping()) && SendData && (_currentInterval++ % SyncEveryNUpdates) == 0)
			{
				SoftUpdate();
				_fullSync = (_currentFullInterval++ % FullSyncEveryNSync) == 0;
				Commit();
				SyncUpdate();
			}
		}

		/// <summary>
		/// Forces a sync even if not owned.
		/// </summary>
		/// <param name="fullSync">Sync absolute data in addition to velocity.</param>
		public void ForceUpdate(bool fullSync = true)
		{
			_isSoft = false;
			_fullSync = fullSync;
			Commit();
			SyncUpdate();
		}

		public new void OnEnable()
		{
			base.OnEnable();
			WakeUp();
		}

		private void SoftUpdate()
		{
			if (_isSoft)
			{
				if (_isFirstSync)
				{
					_isFirstSync = false;
				}
				else
				{
					_isSoft = false;
				}
			}
		}

		public virtual void OnCollisionEnter2D(Collision2D collision)
		{
			if ((collision.gameObject.layer & IgnoredLayers) != 0) return;
			ControlledSoftSync();
		}

		public virtual void OnCollisionEnter(Collision collision)
		{
			if ((collision.gameObject.layer & IgnoredLayers) != 0) return;
			ControlledSoftSync();
		}

		public override void Serialize(ITransportStreamWriter processor, SerializeInfo info)
		{
			_force = info.ForceSync;
			if (info.ForceSync && !_isSoft)
			{
				ControlledForcedSync();
				SoftUpdate();
			}

			base.Serialize(processor, info);
		}

		private protected virtual void ControlledForcedSync()
		{
			if (isKinematic)
			{
				_fullSync = true;
				SendData = true;
				Commit();
				SyncUpdate();
			}
			else
			{
				_currentInterval = 0;
				_currentFullInterval = 0;
				_fullSync = true;
				_lastControlledForcedSync = Time.time;
				if (!_isPossesedAndNotPossesor)
				{
					SendData = true;
				}
			}
		}

		private protected virtual void QueForNextUpdate()
		{
			_currentInterval = 0;
		}

		private protected virtual void ControlledForcedSyncForOwner()
		{
			if (!_isPossesedAndNotPossesor) ControlledForcedSync();
		}

		private protected virtual void ControlledSoftSync()
		{
			if (_isPossesedAndNotPossesor) return;
			_currentInterval = 0;
			if (_lastControlledForcedSync + SOFT_SYNC_DELAY > Time.time) return;
			_currentFullInterval = 0;
			_fullSync = true;
			if (AllowCollisionToAssumeOwner)
			{
				SendData = true;
			}
		}

		/// <summary>
		/// Is the rigidbody sleeping?
		/// </summary>
		/// <returns>true when rigidbody is sleeping.</returns>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.IsSleeping.html"/>
		// ReSharper disable once MemberCanBeProtected.Global
		public abstract bool IsSleeping();

		/// <summary>
		/// Forces a rigidbody to sleep at least one frame.
		/// </summary>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.Sleep.html"/>
		public abstract void Sleep();

		/// <summary>
		/// Forces a rigidbody to wake up.
		/// </summary>
		/// <seealso cref="https://docs.unity3d.com/ScriptReference/Rigidbody.WakeUp.html"/>
		public abstract void WakeUp();

		public override void Possessed(bool isMe, User user)
		{
			AllowCollisionToAssumeOwner = false;
			SendData &= isMe;
			_isPossesedAndNotPossesor = !isMe;
		}

		private protected byte GetFlags()
		{
			byte flags = (byte)(
				(_isSoft ? 1 : 0) +
				(_fullSync | _force ? 2 : 0) +
				(_syncSettings ? 4 : 0) +
				(isKinematic ? 8 : 0) +
				(useGravity ? 16 : 0)
			);

			_force = false;
			_fullSync = false;
			_syncSettings = false;

			return flags;
		}

		/// <summary>
		/// Disassemble bit flag byte
		/// </summary>
		/// <param name="flags">bit flags</param>
		/// <returns>True when other taking control and should ignore incoming</returns>
		private protected bool DisassembleFlags(byte flags)
		{
			if ((flags & 1) != 0)
			{
				if (_isSoft)
				{
					// sender was soft, this is soft, assume sender as owner.
					SendData = false;
					_isSoft = false;
				}
				else
				{
					// sender was soft this is not soft, ignore data, assume im owner.
					return true;
				}
			}
			else
			{
				// assume sender as owner.
				SendData = false;
				_isSoft = false;
				// make the collider not trigger ownership change for a little while.
				_lastControlledForcedSync = Time.time;
			}

			isKinematic = (flags & 8) != 0;
			useGravity = (flags & 16) != 0;

			return false;
		}


		public virtual float EstimateMinimumDataSentPerSecond() => 0f;

		private protected float EstimateDataBase(int constrains)
		{
			float output = 0;
			constrains = ~constrains;

			for (int i = 0; i < 6; i++)
			{
				if ((constrains & 1) != 0)
				{
					output += 4;
				}

				constrains >>= 1;
			}

			output = (output + 1 + OVERHEAD) / SyncEveryNUpdates + output / (SyncEveryNUpdates * FullSyncEveryNSync);

			return output / Time.fixedDeltaTime;
		}

		public override void Reset()
		{
			base.Reset();
			Reliability = Reliability.Reliable;
		}
	}
}