using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AlterunaCompoments
{
	/// <inheritdoc/>
	[DefaultExecutionOrder(-10)]
	[AddComponentMenu("Alteruna/​Multiplayer Manager", 0)]
	[MovedFrom(true, "Alteruna.Multiplayer", "Alteruna")]
	public sealed class MultiplayerManager : Alteruna.Multiplayer.MultiplayerManager { }

	/// <inheritdoc/>
	[Obsolete("Use MultiplayerManager instead of Multiplayer.\nOne can be created in the right-click menu.")]
	[DefaultExecutionOrder(-10)]
	[AddComponentMenu("")]
	[MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public sealed class Multiplayer : Alteruna.Multiplayer.MultiplayerManager
	{
		[ContextMenu("Create New Multiplayer Manager", false, 1_000_000)]
		public void CreateNewMultiplayerManager()
		{
			var go = gameObject;
			var newComp = go.AddComponent<MultiplayerManager>();

			newComp.UID = UID;
			newComp.UIDString = UIDString;
			newComp.AvatarPrefab = AvatarPrefab;
			newComp.AvatarSpawnLocation = AvatarSpawnLocation;
			newComp.AvatarSpawning = AvatarSpawning;
			newComp.SpawnAvatarPerIndex = SpawnAvatarPerIndex;
			newComp.AvatarSpawnLocations = AvatarSpawnLocations;
			newComp.Buckets = Buckets;
			newComp.LogLevel = LogLevel;
			newComp.OnConnected = OnConnected;
			newComp.OnDisconnected = OnDisconnected;
			newComp.OnNetworkError = OnNetworkError;
			newComp.OnRoomCreated = OnRoomCreated;
			newComp.OnRoomJoined = OnRoomJoined;
			newComp.OnRoomLeft = OnRoomLeft;
			newComp.OnRoomListUpdated = OnRoomListUpdated;
			newComp.OnOtherUserJoined = OnOtherUserJoined;
			newComp.OnOtherUserLeft = OnOtherUserLeft;
			newComp.OnJoinRejected = OnJoinRejected;
			newComp.OnPacketSent = OnPacketSent;
			newComp.OnPacketReceived = OnPacketReceived;
			newComp.OnLockRequested = OnLockRequested;
			newComp.OnLockAcquired = OnLockAcquired;
			newComp.OnLockDenied = OnLockDenied;
			newComp.OnLockUnlocked = OnLockUnlocked;
			newComp.OnForceSynced = OnForceSynced;
			newComp.OnForceSync = OnForceSync;
			newComp.OnRpcReceived = OnRpcReceived;
			newComp.OnRpcSent = OnRpcSent;
			newComp.OnRpcRegistered = OnRpcRegistered;
			newComp.OnForceSyncReply = OnForceSyncReply;
			newComp.OnStarted = OnStarted;
		}
	}
}