using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.EventArgument;
using TMPro;

namespace Alteruna
{
	public class RoomMenu : CommunicationBridge
	{
		private const string FORMAT_USER_COUNT = " ({0}/{1})";
		private const string TEXT_CONNECTING = "Connecting";
		private const string TEXT_IN_ROOM = "In Room ";
		private const string TEXT_MISSING_MULTIPLAYER = "Missing Multiplayer Component";
		private const string TEXT_NOT_CONNECTED = "Not Connected";
		private const string TEXT_OFFLINE = "Offline";
		private const string TEXT_RECONNECTING = "Reconnecting";
		private const string TEXT_ROOMS = "Rooms";
		private const string TEXT_STARTED = "Started";

		[SerializeField] private TMP_Text TitleText;
		[SerializeField] private GameObject LANEntryPrefab;
		[SerializeField] private GameObject WANEntryPrefab;
		[SerializeField] private GameObject ContentContainer;
		[SerializeField] private Button StartButton;
		[SerializeField] private Button LeaveButton;

		public bool ShowUserCount = false;

		// manual refresh can be done by calling Multiplayer.RefreshRoomList();
		public bool AutomaticallyRefresh = true;
		public float RefreshInterval = 5.0f;

		private readonly List<RoomObject> _roomObjects = new List<RoomObject>();
		private float _refreshTime;

		private int _count;
		private string _connectionMessage = TEXT_CONNECTING;
		private float _statusTextTime;
		private int _roomI = -1;


		private void Start()
		{
			if (Multiplayer == null)
			{
				Multiplayer = FindObjectOfType<Multiplayer.MultiplayerManager>();
			}

			if (Multiplayer == null)
			{
				Debug.LogError("Unable to find a active object of type Multiplayer.");
				if (TitleText != null) TitleText.text = TEXT_MISSING_MULTIPLAYER;
				enabled = false;
			}
			else
			{
				Multiplayer.OnStarted.AddListener(Started);
				Multiplayer.OnConnected.AddListener(Connected);
				Multiplayer.OnDisconnected.AddListener(Disconnected);
				Multiplayer.OnRoomListUpdated.AddListener(RoomListUpdated);
				Multiplayer.OnRoomJoined.AddListener(RoomJoined);
				Multiplayer.OnRoomLeft.AddListener(RoomLeft);

				StartButton.onClick.AddListener(() =>
				{
					Multiplayer.CreateRoom();
					_refreshTime = RefreshInterval;
				});

				LeaveButton.onClick.AddListener(() =>
				{
					Multiplayer.CurrentRoom?.Leave();
					_refreshTime = RefreshInterval;
				});

				if (TitleText != null)
				{
					ResponseCode blockedReason = Multiplayer.GetLastBlockResponse();
					if (blockedReason != ResponseCode.NaN)
					{
						string str = blockedReason.ToString();
						str = string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
						TitleText.text = str;
					}
					else
					{
						TitleText.text = TEXT_CONNECTING;
					}
				}

				// if already connected
				if (Multiplayer.IsConnected)
				{
					Connected(Multiplayer);
					return;
				}
			}

			StartButton.interactable = false;
			LeaveButton.interactable = false;
		}

		private void FixedUpdate()
		{
			if (!Multiplayer.enabled)
			{
				TitleText.text = TEXT_OFFLINE;
			}
			else if (Multiplayer.IsConnected)
			{
				if (!AutomaticallyRefresh || (_refreshTime += Time.fixedDeltaTime) < RefreshInterval) return;
				_refreshTime -= RefreshInterval;

				Multiplayer.RefreshRoomList();

				if (TitleText == null) return;

				ResponseCode blockedReason = Multiplayer.GetLastBlockResponse();

				if (blockedReason == ResponseCode.NaN) return;

				string str = blockedReason.ToString();
				str = string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
				TitleText.text = str;
			}
			else if (!Multiplayer.IsConnecting)
			{
				TitleText.text = TEXT_NOT_CONNECTED;
				
				if (!AutomaticallyRefresh || (_refreshTime += Time.fixedDeltaTime) < RefreshInterval) return;
				_refreshTime -= RefreshInterval;

				Multiplayer.RefreshRoomList();
			}
			else if ((_statusTextTime += Time.fixedDeltaTime) >= 1)
			{
				_statusTextTime -= 1;
				ResponseCode blockedReason = Multiplayer.GetLastBlockResponse();
				if (blockedReason != ResponseCode.NaN)
				{
					string str = blockedReason.ToString();
					str = string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
					TitleText.text = str;
					return;
				}

				switch (_count)
				{
					case 0:
						TitleText.text = _connectionMessage + ".  ";
						break;
					case 1:
						TitleText.text = _connectionMessage + ".. ";
						break;
					default:
						TitleText.text = _connectionMessage + "...";
						_count = -1;
						break;
				}

				_count++;
			}
		}

		public bool JoinRoom(string roomName, ushort password = 0)
		{
			roomName = roomName.ToLower();
			if (Multiplayer != null && Multiplayer.IsConnected)
			{
				foreach (var room in Multiplayer.AvailableRooms)
				{
					if (room.DisplayName.ToLower() == roomName)
					{
						room.Join(password);
						return true;
					}
				}
			}

			return false;
		}

		private void Started(StartedEvent args)
		{
			if (Multiplayer.ApplicationData.ServerMode == API.GameClientApi.ProjectServerMode.Single_room)
			{
				Connected(args.Controller);
			}
			else if (TitleText != null && TitleText.text == TEXT_OFFLINE)
			{
				TitleText.text = TEXT_STARTED;
			}
		}

		private void Connected(ConnectedEvent args) => Connected(args.Controller);
		private void Connected(Multiplayer.MultiplayerManager controller)
		{
			// if already connected to room
			if (controller.InRoom)
			{
				RoomJoined(controller.CurrentRoom);
				return;
			}

			StartButton.interactable = true;
			LeaveButton.interactable = false;

			if (TitleText != null)
			{
				TitleText.text = TEXT_ROOMS;
			}
		}

		private void Disconnected(DisconnectedEvent args)
		{
			StartButton.interactable = false;
			LeaveButton.interactable = false;

			_connectionMessage = TEXT_RECONNECTING;
			if (TitleText != null)
			{
				TitleText.text = TEXT_RECONNECTING;
			}
		}

		private void RoomJoined(RoomJoinedEvent args)
		{
			RoomJoined(args.Room);
		}
		
		private void RoomJoined(Room room)
		{
			StartButton.interactable = false;
			LeaveButton.interactable = true;

			if (TitleText != null)
			{
				TitleText.text = TEXT_IN_ROOM + room.DisplayName;
			}
		}

		private void RoomLeft(RoomLeftEvent args)
		{
			_roomI = -1;

			StartButton.interactable = true;
			LeaveButton.interactable = false;

			if (TitleText != null)
			{
				TitleText.text = TEXT_ROOMS;
			}
		}

		private void RoomListUpdated(RoomListUpdatedEvent args)
		{
			RoomListUpdated(args.Controller);
		}

		private void RoomListUpdated(Multiplayer.MultiplayerManager multiplayer)
		{
			if (ContentContainer == null) return;
			TitleText.text = TEXT_ROOMS;

			RemoveExtraRooms(multiplayer);

			for (int i = 0; i < multiplayer.AvailableRooms.Count; i++)
			{
				Room room = multiplayer.AvailableRooms[i];
				RoomObject entry;

				if (_roomObjects.Count > i)
				{
					if (room.Local != _roomObjects[i].Lan)
					{
						Destroy(_roomObjects[i].GameObject);
						entry = new RoomObject(Instantiate(WANEntryPrefab, ContentContainer.transform), room.SessionID, room.Local);
						_roomObjects[i] = entry;
					}
					else
					{
						entry = _roomObjects[i];
						entry.Button.onClick.RemoveAllListeners();
					}
				}
				else
				{
					entry = new RoomObject(Instantiate(WANEntryPrefab, ContentContainer.transform), room.SessionID, room.Local);
					_roomObjects.Add(entry);
				}

				if (
					// Hide private rooms.
					room.Private && room.SessionID != _roomI ||
					// Hide locked rooms.
					room.IsLocked ||
					// Hide full rooms.
					room.CurrentUsers > room.MaxUsers
				)
				{
					entry.GameObject.SetActive(false);
					entry.GameObject.name = room.DisplayName;
					continue;
				}

				string newName = room.DisplayName;
				if (ShowUserCount)
				{
					newName += string.Format(FORMAT_USER_COUNT, room.CurrentUsers, room.MaxUsers);
				}

				if (entry.GameObject.name != newName)
				{
					entry.GameObject.name = newName;
					entry.Text.text = newName;
				}

				entry.GameObject.SetActive(true);

				if (room.SessionID == _roomI)
				{
					entry.Button.interactable = false;
				}
				else
				{
					entry.Button.interactable = true;
					entry.Button.onClick.AddListener(() =>
					{
						room.Join();
						RoomListUpdated(multiplayer);
					});
				}
			}
		}

		private void RemoveExtraRooms(Multiplayer.MultiplayerManager multiplayer)
		{
			int l = _roomObjects.Count;
			if (multiplayer.AvailableRooms.Count < l)
			{
				for (int i = 0; i < l; i++)
				{
					if (multiplayer.AvailableRooms.All(t => t.SessionID != _roomObjects[i].SessionID))
					{
						Destroy(_roomObjects[i].GameObject);
						_roomObjects.RemoveAt(i);
						i--;
						l--;
						if (multiplayer.AvailableRooms.Count >= l) return;
					}
				}
			}
		}

		public new void Reset()
		{
			base.Reset();
			EnsureEventSystem.Ensure(true);
		}

		private struct RoomObject
		{
			public readonly GameObject GameObject;
			public readonly TMP_Text Text;
			public readonly Button Button;
			public readonly uint SessionID;
			public readonly bool Lan;

			public RoomObject(GameObject obj, uint sessionID, bool lan = false)
			{
				GameObject = obj;
				Text = obj.GetComponentInChildren<TMP_Text>();
				Button = obj.GetComponentInChildren<Button>();
				SessionID = sessionID;
				Lan = lan;
			}
		}
	}
}