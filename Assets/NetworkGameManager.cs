using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Spawned by NetworkSpawner
/// </summary>
public class NetworkGameManager : NetworkBehaviour // Has to be owned by host
{
    public NetworkVariable<int> LoadedPlayers = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // amount of players hit ready button
    public NetworkVariable<int> CurrentLoadedRoom = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // last loaded room index

    [SerializeField]
    private RoomData startRoomPrefab; // room to spawn at the start

    [SerializeField]
    private List<RoomData> loopableRoomPrefabs; // rooms that can spawn as filler between special rooms

    [SerializeField]
    private List<SpecialRoom> specialRooms; // rooms that have to spawn at certain numbers

    [HideInInspector]
    public Dictionary<int, RoomData> specialRoomsDict = new Dictionary<int, RoomData>(); // dictionary version

    [HideInInspector]
    public Queue<RoomData> roomsToSpawn = new Queue<RoomData>(); // rooms to spawn

    private List<GameObject> activeRooms = new List<GameObject>(); // rooms spawned - only kept up to date on host

    [SerializeField]
    public GameObject rushPrefab;

    public static NetworkGameManager Singleton; // static singleton for other components

    private void Awake() // set singleton
    {
        if (!Singleton)
        {
            Singleton = this;
        }
    }

    //
    // LOADING SCENE
    //

    public void CompletedLoad() // Player pressed ready button
    {
        CompletedLoadServerRpc(); // tell host to change network variable
    }

    [ServerRpc(RequireOwnership = false)]
    public void CompletedLoadServerRpc() // increase loaded players count, if all are loaded allow for network object spawning
    {
        LoadedPlayers.Value = (LoadedPlayers.Value + 1); // change network variable

        if (LoadedPlayers.Value >= NetworkManager.Singleton.ConnectedClientsIds.Count) // if all players ready
        {
            ReleaseSceneBarrierClientRpc(); // allow network objects to be spawned
            CreateRooms(); // queue rooms to spawn and spawn starter room
        }        
    }

    [ClientRpc]
    public void ReleaseSceneBarrierClientRpc() // run for each client - reset position allow network objective spawning
    {
        SceneBarrier.Singleton.gameObject.SetActive(false); // disable barrier preventing player motion
        GameObject.Find("XR Origin").transform.position = new Vector3(0, 0.022f, 0); // reset all players positions

        if (!IsHost) return;

        NetworkObjectSpawnManager.Singleton.AllClientsLoaded(); // host allow network objective spawning and spawn queued objects
    }

    //
    // ROOMS
    //

    private void CreateRooms() // create room queue - called after all players ready - only run for host
    {
        Debug.Log("making queue");

        NetworkObjectSpawnManager.Singleton.SpawnObject(startRoomPrefab.gameObject, new Vector3(0, 1, -1.377f), Quaternion.identity, false, 0, RoomCreated); // spawn starting room

        //create special rooms Dict from monobehavior inputs
        if (specialRooms.Count > 0)
        {
            foreach (SpecialRoom i in specialRooms)
            {
                Debug.Log("adding index " + i.roomIndex + " to special rooms");
                specialRoomsDict.Add(i.roomIndex, i.roomPrefab);
            }
        }

        // create 100 rooms starting at index after start room
        for (int i = 1; i < 100; i++)
        {
            if (specialRoomsDict.ContainsKey(i))
            {
                Debug.Log("enqueueing special with index " + i);
                roomsToSpawn.Enqueue(specialRoomsDict[i]); // queue special room for index
            }
            else
            {
                Debug.Log("enqueueing rand with index " + i);
                roomsToSpawn.Enqueue(loopableRoomPrefabs[Random.Range(0, (int)loopableRoomPrefabs.Count)]); // queue random filler room at index
            }
        }
    }

    public void DoorOpened(int doorIndex) // Player opened door - called by any client
    {
        //if (doorIndex <= CurrentLoadedRoom.Value) return; // if room is already loaded, return        

        //remove oldest room with there are 3 or more loaded rooms
        if (activeRooms.Count > 2) // temp value
        {
            DespawnOldestRoomServerRpc();
        }

        SpawnRoomServerRpc(doorIndex); // tell host to spawn room from queue with index

        SetCurrentRoomServerRpc(doorIndex); // tell host to change the current last loaded room index
    }

    [ServerRpc(RequireOwnership = false)] // call only for host
    public void SpawnRoomServerRpc(int doorIndex) //spawn room prefab - run for host only
    {
        if (doorIndex <= CurrentLoadedRoom.Value) return; // if room is already loaded, return        

        NetworkObjectSpawnManager.Singleton.SpawnRoom(doorIndex); // spawn room from queue with index        
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnOldestRoomServerRpc() // despawn oldest room - 3 or more
    {
        GameObject roomtoRemove = activeRooms[0]; // get oldest room
        activeRooms.Remove(roomtoRemove); // remove from active rooms list

        NetworkObjectSpawnManager.Singleton.DespawnRoomObjects(roomtoRemove.GetComponent<RoomData>().RoomIndex); // despawn room objects

        NetworkObject netObj = roomtoRemove.GetComponent<NetworkObject>(); // get network object
        netObj.Despawn(); // despawn room network object        
    }

    public void RoomCreated(GameObject obj) // callback - add room to active rooms list and set current room
    {
        Debug.Log("caalback received");

        activeRooms.Add(obj); // add just created room to list of active rooms 

        Debug.Log("added to active rooms");

        if (!IsHost) return;

        Debug.Log("checking rush chances");

        int rand = Random.Range(0, 4); // 1 in 4 (25%) chance to spawn rush (temp!)
        if (rand == 0)
        {
            NetworkObjectSpawnManager.Singleton.SpawnRush(); // spawn if chance hit
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCurrentRoomServerRpc(int idx) // change loaded room value
    {
        CurrentLoadedRoom.Value = idx; // change loaded room value
    }

    public RoomData GetLastRoom() // get most recently spawned roomdata
    {
        return activeRooms[(activeRooms.Count - 1)].GetComponent<RoomData>();
    }

    public List<GameObject> GetAllActiveRooms() // get all active roomdata
    {
        return activeRooms;
    }
}

//used for specifying special rooms in monobehavior
[System.Serializable]
public class SpecialRoom
{
    public RoomData roomPrefab;
    public int roomIndex;
}
