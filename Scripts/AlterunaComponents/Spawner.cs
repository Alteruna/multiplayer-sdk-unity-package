using System;
using System.Collections.Generic;
using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Unity.EventArgument;
using UnityEngine;
using UnityEngine.Events;

namespace Alteruna.Multiplayer.Unity
{
    /// <summary>
    /// <c>Spawner</c> can instantiate and destroy objects on all clients in the Room simultaneously.
    /// </summary>
    /// <remarks>
    /// <img src="../images/Alteruna.Spawner.png" alt="The image shows an example of how the Spawner component looks in the inspector."/><br/>
    /// </remarks>
    /// <example>
    ///
    /// Here's an example of using the Spawner to spawn cubes and spheres using different positions and rotations.
    /// It also showcases how to despawn game objects.
    /// 
    /// <code>
    /// public class MySpawner : MonoBehaviour
    /// {
    ///     public Spawner spawner;
    /// 
    ///     public GameObject cubePrefab;
    ///     private int _cubeIndex;
    /// 
    ///     public GameObject spherePrefab;
    ///     private int _sphereIndex;
    ///
    ///     // Since we will create three cubes that we want at different positions and rotations, we create
    ///     // variables that hold that data.
    ///     private readonly Vector3[] _cubeSpawnPositions =
    ///     {
    ///         new Vector3(0, 0, 0),
    ///         new Vector3(1, 0, 1),
    ///         new Vector3(-1, 0, 1),
    ///     };
    ///
    ///     private readonly Vector3[] _cubeSpawnRotations =
    ///     {
    ///         new Vector3(0, 0, 0),
    ///         new Vector3(0, 45, 45),
    ///         new Vector3(0, 20, 50),
    ///     };
    ///
    ///     private GameObject _spawnedSphere;
    ///
    ///     private void Start()
    ///     {
    ///         // We add the prefabs to the SpawnableObjects list.
    ///         spawner.SpawnableObjects.Add(cubePrefab);
    /// 
    ///         // We also set the index, based on the order we put the prefabs in the list.
    ///         _cubeIndex = 0;
    ///
    ///         spawner.SpawnableObjects.Add(spherePrefab);
    ///         _sphereIndex = 1;
    ///     }
    /// 
    ///     public void SpawnSphereOnAllClients()
    ///     {
    ///         // We spawn the sphere prefab using its index in the SpawnableObjects array.
    ///         // Additionally, we store the spawned sphere so we can despawn it later.
    ///         _spawnedSphere = spawner.Spawn(_sphereIndex);
    /// 
    ///         // Alternatively, we can spawn the prefab using its name.
    ///         // spawner.Spawn("Sphere");
    ///     }
    ///
    ///     public void SpawnCubesOnAllClients()
    ///     {
    ///         for(int i = 0; i &lt; 3; i++)
    ///         {
    ///             // We spawn the cubes using different positions and rotations.
    ///             spawner.Spawn(_cubeIndex, _cubeSpawnPositions[i], _cubeSpawnRotations[i]);
    ///         }
    ///     }
    ///
    ///     public void DespawnSphereOnAllClients()
    ///     {
    ///         // We despawn the previously spawned sphere on all clients.
    ///         spawner.Despawn(_spawnedSphere);
    ///     }
    /// }
    /// </code>
    /// </example>
    [AddComponentMenu(""), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
    public class Spawner : CommunicationBridge
    {
        /// <summary>
        /// List of <c>GameObjects</c> which can be spawned during the game.
        /// </summary>
        [Tooltip("GameObjects witch are available for spawning.")]
        public List<GameObject> SpawnableObjects = new List<GameObject>();

        /// <summary>
        /// List of all currently spawned <c>GameObjects</c> in the Room.
        /// </summary>
        private readonly List<(GameObject, Guid, string)> _spawnedObjects = new List<(GameObject, Guid, string)>();
        
        /// <summary>
        /// List of all currently spawned <c>GameObjects</c> in the Room.
        /// </summary>
        public IReadOnlyList<(GameObject, Guid, string)> SpawnedObjects => _spawnedObjects;

        /// <summary>
        /// Invoked after <c>GameObject</c> has been spawned by a <c>User</c> in the Room.
        /// </summary>
        public UnityEvent<User, GameObject> OnObjectSpawn;

        /// <summary>
        /// Invoked before <c>GameObject</c> gets despawned by a <c>User</c> in the Room.
        /// </summary>
        public UnityEvent<User, GameObject> OnObjectDespawn;

        /// <summary>
        /// When true, spawn previously spawned objects on joining client(s).
        /// </summary>
        [HideInInspector]
        public bool ForceSync = true;
        
        /// <summary>
        /// Spawn an new game object from index for all <c>Users</c> in the Room.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        public GameObject Spawn(int index) =>
            Spawn(index, Vector3.zero, Vector3.zero, Vector3.one);

        /// <summary>
        /// Spawn an new game object from index for all <c>Users</c> in the Room with position.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(int index, Vector3 position) =>
            Spawn(index, position, Vector3.zero, Vector3.one);
        
        /// <summary>
        /// Spawn a new game object from index for all <c>Users</c> in the Room using position and eular angles rotation.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="rotation">The rotation which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(int index, Vector3 position, Quaternion rotation) =>
            Spawn(index, position, rotation.eulerAngles, Vector3.one);
        
        /// <summary>
        /// Spawn an new game object from index for all <c>Users</c> in the Room using position and rotation.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="rotation">The rotation which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(int index, Vector3 position, Vector3 rotation) =>
            Spawn(index, position, rotation, Vector3.one);
        
        /// <summary>
        /// Spawn an new object for all <c>Users</c> in the Room using position, rotation, and scale.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="rotation">The rotation which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="scale">The scale which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(int index, Vector3 position, Quaternion rotation, Vector3 scale) =>
            Spawn(index, position, rotation.eulerAngles, scale);
        
        /// <summary>
        /// Spawn an new game object from index for all <c>Users</c> in the Room using position, rotation, and scale.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="rotation">The rotation which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="scale">The scale which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(int index, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (index < 0 || index > SpawnableObjects.Count)
                return null;
            
            ProcedureParameters parameters = new ProcedureParameters();
            
            int mask = 0 + (position == Vector3.zero ? 0 : 2) + (rotation == Vector3.zero ? 0 : 4) + (scale == Vector3.one ? 0 : 8);
            parameters.Set("Index", index);
            
            GameObject newObj = Instantiate(SpawnableObjects[index], position, Quaternion.Euler(rotation), this.transform);
            
            CommunicationBridgeUID[] synchronizables = newObj.GetComponentsInChildren<CommunicationBridgeUID>(true);
            foreach (var t in synchronizables)
            {
                t.OverrideUID(Guid.NewGuid());
            }
            
            Guid objectUid = Guid.NewGuid();
            
            // Give the object a parent and then remove it after setup to properly register in devscenes
            newObj.transform.parent = null;

            newObj.transform.position = position;
            newObj.transform.rotation = Quaternion.Euler(rotation);
            newObj.transform.localScale = scale;
            _spawnedObjects.Add((newObj, objectUid, SpawnableObjects[index].name));

            if (newObj.TryGetComponent(out Avatar avatar))
            {
                if (!avatar.IsPossessed)
                {
                    Multiplayer.GetAvatars()?.Add(avatar);
                    avatar.Possessed(Multiplayer.Me);
                }
                    
                parameters.Set("Possessor", avatar.Possessor.Index);
            }
            
            return SpawnFinal(synchronizables, parameters, objectUid, (byte)mask, newObj, position, rotation, scale);
        }
        
        private GameObject SpawnOnOthers((GameObject obj, Guid uid, string path) item, ushort target = (ushort)UserId.All)
        {
            if (!TryGetWithNameOrPath(item.path, out GameObject obj, out int index))
            {
                throw new ArgumentException("Unable to load path or find object with name.\nGiven name/path: " + item.path);
            }
            
            int mask;
            ProcedureParameters parameters = new ProcedureParameters();

            if (index >= 0)
            {
                mask = 0;
                parameters.Set("Index", index);
            }
            else
            {
                mask = 1;
                parameters.Set("Path", item.path);
            }
            
            mask |= (item.obj.transform.position == Vector3.zero ? 0 : 2) + (item.obj.transform.eulerAngles == Vector3.zero ? 0 : 4) + (item.obj.transform.localScale == Vector3.one ? 0 : 8);

            if (item.obj.TryGetComponent(out Avatar avatar) && avatar.IsPossessed)
            {
                parameters.Set("Possessor", avatar.Possessor.Index);
            }
            
            return SpawnFinal(item.obj.GetComponentsInChildren<CommunicationBridgeUID>(true), 
                parameters, item.uid, (byte)mask, obj, item.obj.transform.position, item.obj.transform.eulerAngles, item.obj.transform.localScale, target);
        }
        
        /// <summary>
        /// Spawn a new object from name for all <c>Users</c> in the Room.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        public GameObject Spawn(string name) =>
            Spawn(name, Vector3.zero, Vector3.zero, Vector3.one);

        /// <summary>
        /// Spawn a new object from name for all <c>Users</c> in the Room with position.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(string name, Vector3 position) =>
            Spawn(name, position, Vector3.zero, Vector3.one);
        
        /// <summary>
        /// Spawn a new object from name for all <c>Users</c> in the Room using position and rotation.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="rotation">The rotation which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(string name, Vector3 position, Quaternion rotation) =>
            Spawn(name, position, rotation.eulerAngles, Vector3.one);
        
        /// <summary>
        /// Spawn a new object from name for all <c>Users</c> in the Room using position and euler angles rotation.
        /// </summary>
        /// <param name="index">The index of the SpawnableObject to spawn.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="rotation">The rotation which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(string name, Vector3 position, Vector3 rotation) =>
            Spawn(name, position, rotation, Vector3.one);
        
        /// <summary>
        /// Spawn a new object from name for all <c>Users</c> in the Room using position, rotation, and scale.
        /// </summary>
        /// <param name="name">name or Asset path.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="rotation">The rotation which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="scale">The scale which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(string name, Vector3 position, Quaternion rotation, Vector3 scale) => 
            Spawn(name, position, rotation.eulerAngles, scale);
        
        /// <summary>
        /// Spawn an new object from name for all <c>Users</c> in the Room using position, euler angles rotation, and scale.
        /// </summary>
        /// <param name="name">name or Asset path.</param>
        /// <param name="position">The position which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="rotation">The rotation which the <c>GameObject</c> will be spawned with.</param>
        /// <param name="scale">The scale which the <c>GameObject</c> will be spawned with.</param>
        public GameObject Spawn(string name, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (name == string.Empty)
                return null;
            
            if (!TryGetWithNameOrPath(name, out GameObject obj, out int index))
            {
                throw new ArgumentException("Unable to load path or find object with name.\nGiven name/path: " + name);
            }

            if (index >= 0)
            {
                return Spawn(index, position, rotation, scale);
            }
            
            ProcedureParameters parameters = new ProcedureParameters();
            
            int mask = 1 + (position == Vector3.zero ? 0 : 2) + (rotation == Vector3.zero ? 0 : 4) + (scale == Vector3.one ? 0 : 8);
            parameters.Set("Path", name);

            GameObject newObj = Instantiate(obj, position, Quaternion.Euler(rotation), transform);
            
            CommunicationBridgeUID[] synchronizables = newObj.GetComponentsInChildren<CommunicationBridgeUID>(true);
            foreach (var t in synchronizables)
            {
                t.OverrideUID(Guid.NewGuid());
            }
            
            Guid objectUID = Guid.NewGuid();
            
            // Give the object a parent and then remove it after setup to properly register in devscenes
            newObj.transform.parent = null;

            newObj.transform.position = position;
            newObj.transform.rotation = Quaternion.Euler(rotation);
            newObj.transform.localScale = scale;
            _spawnedObjects.Add((newObj, objectUID, name));
            
            if (newObj.TryGetComponent(out Avatar avatar))
            {
                if (!avatar.IsPossessed)
                {
                    Multiplayer.GetAvatars()?.Add(avatar);
                    avatar.Possessed(Multiplayer.Me);
                }
                    
                parameters.Set("Possessor", avatar.Possessor.Index);
            }
            
            return SpawnFinal(synchronizables, parameters, objectUID, (byte)mask, newObj, position, rotation, scale);
        }

        private GameObject SpawnFinal(CommunicationBridgeUID[] synchronizables, ProcedureParameters parameters, Guid objectUid, byte mask, GameObject newObj, Vector3 position, Vector3 rotation, Vector3 scale, ushort target = (ushort)UserId.All)
        {

            if (target == (ushort)UserId.AllInclusive)
            {
                target = (ushort)UserId.All;
            }
            
            parameters.Set("mask", mask);
            
            parameters.Set("UID", objectUid.ToString());
            
            if ((mask & 2) != 0)
            {
                parameters.Set("PosX", position.x);
                parameters.Set("PosY", position.y);
                parameters.Set("PosZ", position.z);
            }

            if ((mask & 4) != 0)
            {
                parameters.Set("RotX", rotation.x);
                parameters.Set("RotY", rotation.y);
                parameters.Set("RotZ", rotation.z);
            }

            if ((mask & 8) != 0)
            {
                parameters.Set("ScaleX", scale.x);
                parameters.Set("ScaleY", scale.y);
                parameters.Set("ScaleZ", scale.z);
            }
            
            for (int i = 0; i < synchronizables.Length; i++)
            {
                parameters.Set("GUID_" + i, synchronizables[i].GetUID().ToString());
            }

            if (target == (ushort)UserId.AllInclusive || target == Multiplayer.Me.Index)
            {
	            OnObjectSpawn.Invoke(Multiplayer.Me, newObj);
            }

            Multiplayer.InvokeRemoteProcedure(name + "_SpawnObject", target, parameters);

            return newObj;
        }

        /// <summary>
        /// Invoked when a <c>GameObject</c> has been despawned by a <c>User</c> in the Room.
        /// </summary>
        /// <param name="spawnedObject">The spawned <c>GameObject</c> to despawn.</param>
        public void Despawn(GameObject spawnedObject)
        {
            if (spawnedObject == null) return;
            
            for (int i = 0; i < _spawnedObjects.Count; i++)
            {
                if (_spawnedObjects[i].Item1 == spawnedObject)
                {
                    ProcedureParameters parameters = new ProcedureParameters();
                    parameters.Set("UID", _spawnedObjects[i].Item2.ToString());
                    GameObject obj = _spawnedObjects[i].Item1;
                    _spawnedObjects.RemoveAt(i);
                    OnObjectDespawn.Invoke(Multiplayer.Me, obj);
                    Destroy(obj);
                    Multiplayer.InvokeRemoteProcedure(name + "_DestroyObject", (ushort)UserId.All, parameters);
                    return;
                }
            }
        }

        private bool TryGetWithNameOrPath(string n, out GameObject obj, out int index)
        {
            index = -1;
            obj = null;
            
            if (n == string.Empty)
            {
                return false;
            }
            
            obj = Resources.Load(n, typeof(GameObject)) as GameObject;

            if (obj == null)
            {
                for (int i = 0; i < SpawnableObjects.Count; i++)
                {
                    if (SpawnableObjects[i].name == n)
                    {
                        obj = SpawnableObjects[i];
                        index = i;
                        return true;
                    }
                }
            }
            else
            {
                return true;
            }

            return false;
        }
        
        void SpawnObject(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
        {
            int mask = parameters.Get("mask", (byte)0);
            string uidS = parameters.Get("UID", "");
            if (uidS == string.Empty)
            {
                Debug.LogWarning("invalid spawn request.");
                return;
            }
            Guid objectUid = Guid.Parse(uidS);

            GameObject obj;
            
            if ((mask & 1) == 0)
            {
                int index = parameters.Get("Index", 0);
                obj = SpawnableObjects[index];
            }
            else
            {
                string n = parameters.Get("Path", "");
                if (!TryGetWithNameOrPath(n, out obj, out _))
                {
                    Debug.LogError("Unable to find object with name or path of: \""+n + "\"");
                    return;
                }
            }
            
            // Give the object a parent and then remove it after setup to properly register in devscenes
            GameObject newObj = Instantiate(obj, transform);
            newObj.transform.parent = null;
            
            if (newObj.TryGetComponent(out Avatar avatar))
            {
                if (parameters.Get("Possessor", out ushort possessor))
                {
                    avatar.Possessed(Multiplayer.GetUser(possessor));
                }
            }

            // Get parameter values
            if ((mask & 2) != 0)
            {
                newObj.transform.position =
                    new Vector3(
                        parameters.Get("PosX", 0f),
                        parameters.Get("PosY", 0f),
                        parameters.Get("PosZ", 0f)
                    );
            }
            else
            {
                newObj.transform.position = Vector3.zero;
            }

            if ((mask & 4) != 0)
            {
                newObj.transform.eulerAngles =
                    new Vector3(
                        parameters.Get("RotX", 0f),
                        parameters.Get("RotY", 0f),
                        parameters.Get("RotZ", 0f)
                    );
            }
            else
            {
                newObj.transform.rotation = Quaternion.identity;
            }

            if ((mask & 8) != 0)
            {
                newObj.transform.localScale =
                    new Vector3(
                        parameters.Get("ScaleX", 1f),
                        parameters.Get("ScaleY", 1f),
                        parameters.Get("ScaleZ", 1f)
                    );
            }
            else
            {
                newObj.transform.localScale = Vector3.one;
            }

            CommunicationBridgeUID[] synchronizables = newObj.GetComponentsInChildren<CommunicationBridgeUID>(true);
            for (int i = 0; i < synchronizables.Length; i++)
            {
                synchronizables[i].OverrideUID(Guid.Parse(parameters.Get("GUID_" + i, "")));
            }

            _spawnedObjects.Add((newObj, objectUid, obj.name));
            
            OnObjectSpawn.Invoke(Multiplayer.GetUser(fromUser), newObj);
        }

        void DestroyObject(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
        {
            Guid objectUid = Guid.Parse(parameters.Get("UID", ""));
            for (int i = 0; i < _spawnedObjects.Count; i++)
            {
                if (_spawnedObjects[i].Item2 == objectUid)
                {
                    if (_spawnedObjects[i].Item1 == null)
                    {
                        _spawnedObjects.RemoveAt(i);
                        return;
                    }

                    GameObject obj = _spawnedObjects[i].Item1;
                    _spawnedObjects.RemoveAt(i);
                    OnObjectDespawn.Invoke(Multiplayer.GetUser(fromUser), obj);
                    Destroy(obj);

                    return;
                }
            }
        }

        public void Start()
        {
            if (Multiplayer != null)
            {
                Multiplayer.RegisterRemoteProcedure(name + "_SpawnObject", SpawnObject);
                Multiplayer.RegisterRemoteProcedure(name + "_DestroyObject", DestroyObject);
                
                Multiplayer.OnOtherUserJoined.AddListener(OtherJoined);
                Multiplayer.OnRoomLeft.AddListener(LeftRoom);
            }
        }

        private void LeftRoom(RoomLeftEvent args)
        {
            for (int i = 0, l = _spawnedObjects.Count; i < l; i++)
            {
                if (_spawnedObjects[i].Item1 != null)
                {
                    Destroy(_spawnedObjects[i].Item1);
                }
            }
            _spawnedObjects.Clear();
            
        }

        private void OtherJoined(OtherUserJoinedEvent args)
        {
            if (!ForceSync || args.Controller.LowestUserIndex != args.Controller.Me.Index) return;

            for (int i = 0, l = _spawnedObjects.Count; i < l; i++)
            {
                if (_spawnedObjects[i].Item1 == null)
                {
                    _spawnedObjects.Remove(_spawnedObjects[i]);
                    OnObjectDespawn.Invoke(Multiplayer.Me, _spawnedObjects[i].Item1);
                }
                else
                {
                    SpawnOnOthers(_spawnedObjects[i], args.User.Index);
                }
            }
        }
    }
}