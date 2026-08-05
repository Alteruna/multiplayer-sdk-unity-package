using System;
using System.Reflection;
using Alteruna.Multiplayer.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Synchronizable Animator component.
	/// </summary>
	/// <remarks>
	///	In most cases, you should avoid synchronizing the animations as they are usually not deterministic and can be performed from actions directly.
	/// For example, instead of playing walk animation, you should consider animating based on the velocity of the character locally.
	/// </remarks>
	/// <example>
	///
	///	AnimationSynchronizable is used together with the Unity Animator.
	/// Adding a AnimationSynchronizable component to a GameObject will automatically add the Animator component.
	///
	/// To sync using the AnimationSynchronizable component, you need to call the method from the AnimationSynchronizable component.
	/// 
	///	<code>
	/// using UnityEngine;
	///
	/// [RequireComponent(typeof(Alteruna.AnimationSynchronizable))]
	///	public class MyAnimatedObj : MonoBehaviour
	///	{
	///		private Alteruna.AnimationSynchronizable _aniSync;
	///	
	///		// We can optimize by precalculating the hash of the animation state.
	///		private int JumpId = Animator.StringToHash("Jump");
	///	
	///		private void Start()
	///		{
	///			_aniSync = GetComponent&lt;Alteruna.AnimationSynchronizable&gt;();
	///
	///			// We can get the Unity Animator from the AnimationSynchronizable component.
	///			if (_aniSync.Animator.isHuman) print("Humanoid");
	///		}
	///	
	///		private void Update()
	///		{
	///			// Play animation
	///			if (Input.GetKeyDown(KeyCode.Space))
	///			{
	///				// play for all clients
	///				_aniSync.Play(JumpId);
	///			}
	///		}
	///	}
	/// </code>
	/// </example>
	[AddComponentMenu("Alteruna/AnimationSynchronizable", 0)]
	[MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class AnimationSynchronizable : AttributesSync
	{
		[FormerlySerializedAs("_animator"), HideInInspector]
		public Animator Animator;
		
		/// <summary>
		/// If true, only commit new states in SetBool, SetInteger, and SetFloat methods.
		/// </summary>
		[Tooltip("If true, only commit new states in SetBool, SetInteger, and SetFloat methods.")]
		public bool OnlyCommitNewStates;

		private static int _setLayerWeightSyncedByteFloat;
		private static int _setLayerWeightSyncedIntFloat;
		
		private static int _playSyncedInt;
		private static int _playSyncedIntInt;
		private static int _playSyncedIntByte;
		private static int _playSyncedIntIntFloat;
		private static int _playSyncedIntByteFloat;

		private static int _setBoolSynced;
		private static int _setIntegerSyncedInt;
		private static int _setIntegerSyncedByte;
		private static int _setFloatSynced;

		private static int _setTriggerSynced;
		private static int _resetTriggerSynced;
		private static int _setTargetSynced;
		private static int _setLookAtPositionSynced;

		private static int _startPlaybackSynced;
		private static int _stopPlaybackSynced;

		private static int _updateSynced;
		private static int _rebindSynced;


		private void Awake()
		{
			// Try to get Animator if not assigned so that a user can more easily accesses it.
			if (!Animator)
			{
				TryGetComponent(out Animator);
			}
		}


		private void Start()
		{
			if (!Animator)
			{
				if (!TryGetComponent(out Animator))
				{
					enabled = false;
					throw new Exception("AnimationSynchronizable require an assigned Animator or to have a Animator on the same object.");
				}
			}
			
			if (_updateSynced != 0) return;

			_setLayerWeightSyncedByteFloat = GetMethodAttributeId(nameof(SetLayerWeightByteFloat));
			_setLayerWeightSyncedIntFloat = GetMethodAttributeId(nameof(SetLayerWeightIntFloat));
			
			_playSyncedInt = GetMethodAttributeId(nameof(PlaySyncedInt));
			_playSyncedIntInt = GetMethodAttributeId(nameof(PlaySyncedIntInt));
			_playSyncedIntByte = GetMethodAttributeId(nameof(PlaySyncedIntByte));
			_playSyncedIntIntFloat = GetMethodAttributeId(nameof(PlaySyncedIntIntFloat));
			_playSyncedIntByteFloat = GetMethodAttributeId(nameof(PlaySyncedIntByteFloat));

			_setBoolSynced = GetMethodAttributeId(nameof(SetBoolSynced));
			_setIntegerSyncedInt = GetMethodAttributeId(nameof(SetIntegerSyncedInt));
			_setIntegerSyncedByte = GetMethodAttributeId(nameof(SetIntegerSyncedByte));
			_setFloatSynced = GetMethodAttributeId(nameof(SetFloatSynced));

			_setTriggerSynced = GetMethodAttributeId(nameof(SetTriggerSynced));
			_resetTriggerSynced = GetMethodAttributeId(nameof(ResetTriggerSynced));
			_setTargetSynced = GetMethodAttributeId(nameof(SetTargetSynced));
			_setLookAtPositionSynced = GetMethodAttributeId(nameof(SetLookAtPositionSynced));

			_startPlaybackSynced = GetMethodAttributeId(nameof(StartPlaybackSynced));
			_stopPlaybackSynced = GetMethodAttributeId(nameof(StopPlaybackSynced));

			_updateSynced = GetMethodAttributeId(nameof(UpdateSynced));
			_rebindSynced = GetMethodAttributeId(nameof(RebindSynced));
		}

		public new void Reset()
		{
			base.Reset();
			Reliability = Reliability.Reliable;
			OnlyCommitNewStates = true;
			if (!Animator)
			{
				if (!TryGetComponent(out Animator))
				{
					Animator = gameObject.AddComponent<Animator>();
				}
			}
		}

#region SetLayerWeight

		/// <summary>
		///   <para>Sets the weight of the layer at the given index.</para>
		/// </summary>
		/// <param name="layerIndex">The layer index.</param>
		/// <param name="weight">The new layer weight.</param>
		public void SetLayerWeight(int layer, float weight)
		{
			if (layer < 0 || layer > 255)
			{
				BroadcastRemoteMethod(_setLayerWeightSyncedIntFloat, layer, weight);
			}
			else
			{
				BroadcastRemoteMethod(_setLayerWeightSyncedByteFloat, (byte)layer, weight);
			}
		}
		
		/// <summary>
		///   <para>Sets the weight of the layer at the given index but only if equal or above the required delta.</para>
		/// </summary>
		/// <param name="layerIndex">The layer index.</param>
		/// <param name="weight">The new layer weight.</param>
		/// <param name="requiredDelta">The minimum weight required to set the layer weight.</param>
		/// <returns>true if the layer weight was updated, false otherwise.</returns>
		public bool SetLayerWeight(int layer, float weight, float requiredDelta)
		{
			if (Animator.GetLayerWeight(layer) < requiredDelta) return false;
			SetLayerWeight(layer, weight);
			return true;
		}
		
		[SynchronizableMethod]
		private void SetLayerWeightByteFloat(byte layer, float weight) => Animator.SetLayerWeight(layer, weight);

		[SynchronizableMethod]
		private void SetLayerWeightIntFloat(int layer, float weight) => Animator.SetLayerWeight(layer, weight);

#endregion

#region Play

		public void Play(string stateName) => Play(Animator.StringToHash(stateName));
		public void Play(string stateName, int layer) => Play(Animator.StringToHash(stateName), layer);
		public void Play(string stateName, int layer, float normalizedTime) => Play(Animator.StringToHash(stateName), layer, normalizedTime);

		public void Play(int stateNameHash) => BroadcastRemoteMethod(_playSyncedInt, stateNameHash);

		public void Play(int stateNameHash, int layer)
		{
			if (layer < 0 || layer > 255)
			{
				BroadcastRemoteMethod(_playSyncedIntInt, stateNameHash, layer);
			}
			else
			{
				BroadcastRemoteMethod(_playSyncedIntByte, stateNameHash, (byte)layer);
			}
		}

		public void Play(int stateNameHash, int layer, float normalizedTime)
		{
			if (layer < 0 || layer > 255)
			{
				BroadcastRemoteMethod(_playSyncedIntIntFloat, stateNameHash, layer, normalizedTime);
			}
			else
			{
				BroadcastRemoteMethod(_playSyncedIntByteFloat, stateNameHash, (byte)layer, normalizedTime);
			}
		}

		[SynchronizableMethod]
		private void PlaySyncedInt(int stateNameHash) => Animator.Play(stateNameHash);

		[SynchronizableMethod]
		private void PlaySyncedIntInt(int stateNameHash, int layer) => Animator.Play(stateNameHash, layer);

		[SynchronizableMethod]
		private void PlaySyncedIntByte(int stateNameHash, byte layer) => Animator.Play(stateNameHash, layer);

		[SynchronizableMethod]
		private void PlaySyncedIntIntFloat(int stateNameHash, int layer, float normalizedTime) => Animator.Play(stateNameHash, layer, normalizedTime);

		[SynchronizableMethod]
		private void PlaySyncedIntByteFloat(int stateNameHash, byte layer, float normalizedTime) => Animator.Play(stateNameHash, layer, normalizedTime);

#endregion

#region Set

		public void SetBool(string name, bool value) => SetBool(Animator.StringToHash(name), value);
		public void SetInteger(string name, int value) => SetInteger(Animator.StringToHash(name), value);
		public void SetInteger(string name, byte value) => SetInteger(Animator.StringToHash(name), value);
		public void SetFloat(string name, float value) => SetFloat(Animator.StringToHash(name), value);

		public void SetBool(int id, bool value)
		{
			if (!OnlyCommitNewStates || Animator.GetBool(id) != value) BroadcastRemoteMethod(_setBoolSynced, id, value);
		}

		public void SetInteger(int id, int value)
		{
			if (value <= byte.MaxValue) SetInteger(id, (byte)value);
			else if (!OnlyCommitNewStates || Animator.GetInteger(id) != value) BroadcastRemoteMethod(_setIntegerSyncedInt, id, value);
		}
		
		public void SetInteger(int id, byte value)
		{
			if (!OnlyCommitNewStates || Animator.GetInteger(id) != value) BroadcastRemoteMethod(_setIntegerSyncedInt, id, value);
		}

		public void SetFloat(int id, float value)
		{
			if (!OnlyCommitNewStates || !Mathf.Approximately(Animator.GetFloat(id), value)) BroadcastRemoteMethod(_setFloatSynced, id, value);
		}

		[SynchronizableMethod]
		private void SetBoolSynced(int id, bool value) => Animator.SetBool(id, value);

		[SynchronizableMethod]
		private void SetIntegerSyncedInt(int id, int value) => Animator.SetInteger(id, value);
		
		[SynchronizableMethod]
		private void SetIntegerSyncedByte(int id, byte value) => Animator.SetInteger(id, value);

		[SynchronizableMethod]
		private void SetFloatSynced(int id, float value) => Animator.SetFloat(id, value);

#endregion

#region SetMultiple
/*
		public class SetMultipleParams
		{
			internal byte Bools;
			internal byte Bytes;
			internal byte Ints;
			internal byte Floats;
			internal readonly Dictionary<int, object> MultipleParams = new Dictionary<int, object>();
			
			public void Add(int id, bool value)
			{
				if (MultipleParams.ContainsKey(id))
				{
					MultipleParams[id] = value;
				}
				else
				{
					MultipleParams.Add(id, value);
					Bools++;
				}
			}
			
			public void Add(int id, byte value)
			{
				if (MultipleParams.ContainsKey(id))
				{
					MultipleParams[id] = value;
				}
				else
				{
					MultipleParams.Add(id, value);
					Bytes++;
				}
			}
			
			public void Add(int id, int value)
			{
				if (value <= byte.MaxValue) Add(id, (byte)value);
				else if (MultipleParams.ContainsKey(id))
				{
					MultipleParams[id] = value;
				}
				else
				{
					MultipleParams.Add(id, value);
					Ints++;
				}
			}
			
			public void Add(int id, float value)
			{
				if (MultipleParams.ContainsKey(id))
				{
					MultipleParams[id] = value;
				}
				else
				{
					MultipleParams.Add(id, value);
					Floats++;
				}
			}
			
			public void Clear()
			{
				MultipleParams.Clear();
				Bools = 0;
				Bytes = 0;
				Ints = 0;
				Floats = 0;
			}
		}
		
		public void SetMultiple(SetMultipleParams parameters)
		{
			if (parameters.MultipleParams.Count == 0) return;
			int l = parameters.MultipleParams.Count;
			int[] ids = new int[l];
			object[] values = new object[l];
			int i = 0;
			foreach (var kvp in parameters.MultipleParams)
			{
				ids[i] = kvp.Key;
				values[i] = kvp.Value;
				i++;
			}
			BroadcastRemoteMethod(_setBoolSynced, parameters.Bools, parameters.Bytes, parameters.Ints, parameters.Floats, ids, values);
		}

		[SynchronizableMethod]
		private void SetMultipleSynced(byte bools, byte bytes, byte ints, byte floats, int[] ids, object[] values)
		{
			int l = ids.Length;
			for (int i = 0, b = 0, by = 0, inT = 0, f = 0; i < l; i++)
			{
				if (b < bools)
				{
					Animator.SetBool(ids[i], (bool)values[i]);
					b++;
				}
				else if (by < bytes)
				{
					Animator.SetInteger(ids[i], (byte)values[i]);
					by++;
				}
				else if (inT < ints)
				{
					Animator.SetInteger(ids[i], (int)values[i]);
					inT++;
				}
				else if (f < floats)
				{
					Animator.SetFloat(ids[i], (float)values[i]);
					f++;
				}
			}
		}
*/
#endregion

#region Trigger

		public void SetTrigger(string name) => SetTrigger(Animator.StringToHash(name));
		public void ResetTrigger(string name) => SetFloat(Animator.StringToHash(name), 0);
		public void SetTrigger(int id) => BroadcastRemoteMethod(_setTriggerSynced, id);
		public void ResetTrigger(int id) => SetFloat(_resetTriggerSynced, 0);

		[SynchronizableMethod]
		private void SetTriggerSynced(int id) => Animator.SetTrigger(id);
		[SynchronizableMethod]
		private void ResetTriggerSynced(int id) => Animator.ResetTrigger(id);
		

#endregion

#region SetTarget

		public void SetTarget(AvatarTarget targetIndex, float targetNormalizedTime) => BroadcastRemoteMethod(_setTargetSynced, targetIndex, targetNormalizedTime);

		[SynchronizableMethod]
		private void SetTargetSynced(AvatarTarget targetIndex, float targetNormalizedTime) => Animator.SetTarget(targetIndex, targetNormalizedTime);

#endregion

#region SetLookAtPosition

		public void SetLookAtPosition(Vector3 lookAtPosition) => BroadcastRemoteMethod(_setLookAtPositionSynced, lookAtPosition);

		[SynchronizableMethod]
		private void SetLookAtPositionSynced(Vector3 lookAtPosition) => Animator.SetLookAtPosition(lookAtPosition);

#endregion

#region StartPlayback

		public void StartPlayback() => BroadcastRemoteMethod(_startPlaybackSynced);

		[SynchronizableMethod]
		private void StartPlaybackSynced() => Animator.StartPlayback();

#endregion

#region StopPlayback

		public void StopPlayback() => BroadcastRemoteMethod(_stopPlaybackSynced);

		[SynchronizableMethod]
		private void StopPlaybackSynced() => Animator.StopPlayback();

#endregion

#region Update

		public void AnimatorUpdate(float deltaTime) => BroadcastRemoteMethod(_updateSynced, deltaTime);

		[SynchronizableMethod]
		private void UpdateSynced(float deltaTime) => Animator.Update(deltaTime);

#endregion

#region Rebind

		public void Rebind() => BroadcastRemoteMethod(_rebindSynced);

		[SynchronizableMethod]
		private void RebindSynced() => Animator.Rebind();

#endregion
	}
}