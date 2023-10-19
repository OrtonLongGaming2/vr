using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkSpawner : NetworkBehaviour
{
    private Dictionary<int, List<GameObject>> spawnedRoomObjects = new Dictionary<int, List<GameObject>>(); // for removing network objects when a room despawns.

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsHost) return;

        NetworkObjectSpawnManager.Singleton.SetSpawner(this); // give spawn manager networkspawner to allow spawning - host only to prevent duplicates
    }

    /// <summary>
    /// Spawn a Network Prefab to be destroyed with the scene
    /// </summary>
    /// <param name="netObj"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="newRoomIdx"></param>
    /// <param name="callback"></param>
    public void SpawnNetworkObject(GameObject netObj, Vector3 position, Quaternion rotation, int newRoomIdx, System.Action<GameObject> callback)
    {
        Debug.Log("Requested network object spawn - " + netObj.name); // log spawn request

        GameObject go = Instantiate(netObj, position, rotation); // instantiate in scene locally

        NetworkObject spawnedObj = go.GetComponent<NetworkObject>(); // get NetworkObject component to spawn

        if (spawnedObj == null) // gun is local for testing
        {
            return;
        }

        spawnedObj.Spawn(true); // spawn for clients

        //call spawn finished callback if necessary - used for NetworkGameManager
        if (callback != null)
        {
            callback.Invoke(go);
        }

        // spawn any other network objects that are part of the room - dressers, items, etc
        RoomData roomData = go.GetComponent<RoomData>();
        if (roomData)
        {
            go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, (go.transform.localPosition.z + -0.06f)); // offset room to prevent z-fighting between walls

            roomData.RoomIndex = newRoomIdx; // set room index - used for doors and keys

            foreach (RoomNetworkObject i in roomData.roomNetworkObjs) // create network objects for the objects in the room - will be despawned alongside the room
            {
                SpawnParentedNetworkObject(i.prefab, i.positioning.position, i.positioning.rotation, go.transform, newRoomIdx); // spawn objects for room
            }
        
            foreach (Transform i in roomData.rushPositions) // create network transforms for rush so that they will not despawn with the room
            {
                SpawnRushPositionTransform(roomData, i);
            }
        }
    }

    /// <summary>
    /// Create waypoints that do not despawn with rooms to avoid issues with Rush waypoint tweening.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="i"></param>
    private void SpawnRushPositionTransform(RoomData room, Transform i)
    {
        GameObject go = Instantiate(new GameObject("RushPosition"), i.position, Quaternion.identity); // create empty GameObject at each waypoint position
        NetworkObject netObj = go.AddComponent<NetworkObject>(); // add network object component
        netObj.Spawn(true); // spawn for clients
        room.networkRushPositions.Add(go.transform); // add to room list so Rush can access
    }

    /// <summary>
    /// Spawn prefab children for a room and manage items within. Also manages despawns for objects if parent room is despawned.
    /// </summary>
    /// <param name="netObj"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="parent"></param>
    /// <param name="currentRoom"></param>
    private void SpawnParentedNetworkObject(GameObject netObj, Vector3 position, Quaternion rotation, Transform parent, int currentRoom) // spawning Room Network Prefabs
    {
        GameObject go = Instantiate(netObj, position, rotation); // instantiate prefab in scene locally

        go.GetComponent<NetworkObject>().Spawn(true); // spawn for clients

        // setup door number and lock state if object is a door
        Door doorData = go.GetComponent<Door>();
        if (doorData)
        {
            doorData.SetRoomIndex(currentRoom + 1);
            doorData.SetLocked(true); // temp lock state
        }

        // setup key number if object is a key
        KeyController key = go.GetComponent<KeyController>();
        if (key)
        {
            key.SetRoomNumber(currentRoom + 1);
        }

        // managing despawn

        int indexToUse = parent.GetComponent<RoomData>().RoomIndex; // index for spawned room new object is child of
        
        if (spawnedRoomObjects.ContainsKey(indexToUse) == false) // add room key if needed
        {
            spawnedRoomObjects.Add(indexToUse, new List<GameObject>()); // add room index key to dictionary
        }

        spawnedRoomObjects[indexToUse].Add(go); // add new object to dictionary with room index - used for despawing objects within a room when that room is despawned.
    }

    /// <summary>
    /// Despawn all Network Objects that have been spawned for a room.
    /// </summary>
    /// <param name="index"></param>
    public void DespawnRoomObjects(int index)
    {
        if (spawnedRoomObjects.ContainsKey(index) == false) return;

        foreach(GameObject i in spawnedRoomObjects[index]) // get all network objects spawned for this room index
        {
            i.GetComponent<NetworkObject>().Despawn(); // despawn for all clients
        }

        spawnedRoomObjects[index].Clear(); // clear list to prevent despawning nulls
    }

    /// <summary>
    /// Spawn a Network Prefab to NOT be destroyed with the scene
    /// </summary>
    /// <param name="netObj"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void SpawnPermanentObject(GameObject netObj, Vector3 position, Quaternion rotation)
    {
        GameObject go = Instantiate(netObj, position, rotation); // instantiate in scene locally
        go.GetComponent<NetworkObject>().Spawn(false); // spawn for clients
    }

    /// <summary>
    /// Spawns the 'Rush' Monster and gets waypoints to tween between from active rooms.
    /// </summary>
    public void SpawnRush()
    {
        GameObject go = Instantiate(NetworkGameManager.Singleton.rushPrefab); // instantiate in scene locally

        NetworkObject spawnedObj = go.GetComponent<NetworkObject>(); // get network object component to spawn

        if (spawnedObj == null) // gun is local for testing
        {
            return;
        }

        spawnedObj.Spawn(true); // spawn for clients

        Rush rushData = go.GetComponent<Rush>(); // get rush component

        List<Transform> waypoints = new List<Transform>(); // create list to store and remove waypoints for rush
                
        foreach (GameObject i in NetworkGameManager.Singleton.GetAllActiveRooms()) // get all active rooms that are spawned in the network
        {
            if (i.GetComponent<RoomData>() == null) return;

            foreach (Transform x in i.GetComponent<RoomData>().networkRushPositions) // get all waypoints in the network for this room and add to waypoints list
            {
                waypoints.Add(x); // add waypoint to list for rush component
            }
        }

        rushData.SetPositionAndStart(waypoints[0], waypoints); // tell rush to start and give waypoints and start waypoint
    }
}
