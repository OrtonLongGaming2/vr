using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectSpawnManager : MonoBehaviour
{
    public static NetworkObjectSpawnManager Singleton;
    private NetworkSpawner spawner;

    private List<QueuedNetworkObject> objectQueue = new List<QueuedNetworkObject>(); // list of queued Network Objects to spawn once ready

    [HideInInspector]
    public bool Ready = false; // if host has 

    [HideInInspector]
    public bool SceneLoaded = false; // if Game scene is loaded and all players are ready

    private void Awake()
    {
        Singleton = this;
    }

    public void SetSpawner(NetworkSpawner spawn) // called by host NetworkSpawner
    {
        spawner = spawn;
        if (spawner)
        {
            Ready = true;
        }
    }

    public void AllClientsLoaded() // called by NetworkGameManager (host) once all players have entered game scene and hit ready button
    {
        SceneLoaded = true;

        if (objectQueue.Count > 0) // dequeue any queued network objects
        {
            foreach (QueuedNetworkObject i in objectQueue)
            {
                SpawnObject(i.obj, i.pos, i.rot, i.perma, i.roomIDX, i.roomCallback);
            }

            objectQueue.Clear();
        }
    }

    /// <summary>
    /// Send a NetworkObject prefab to host NetworkSpawner, or queue until ready.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="perma"></param>
    public void SpawnObject(GameObject obj, Vector3 pos, Quaternion rot, bool perma = false, int roomIndex = -1, System.Action<GameObject> callback = null)
    {
        if (!Ready) return;

        if (!SceneLoaded)
        {
            QueueObject(new QueuedNetworkObject(obj, pos, rot, perma, roomIndex, callback)); // Create new queue object with request information, not scene loaded.
            return;
        }

        if (!perma)
        {
            spawner.SpawnNetworkObject(obj, pos, rot, roomIndex, callback);
        }
        else
        {
            spawner.SpawnPermanentObject(obj, pos, rot);
        }
    }    

    /// <summary>
    /// Queue a object spawn request until Scene is loaded and players are ready.
    /// </summary>
    /// <param name="obj"></param>
    private void QueueObject(QueuedNetworkObject obj)
    {
        objectQueue.Add(obj);
    }

    /// <summary>
    /// Called by NetworkGameManager - spawn a room - only called for host
    /// </summary>
    /// <param name="index"></param>
    public void SpawnRoom(int index) 
    {
        RoomData prevData = NetworkGameManager.Singleton.GetLastRoom(); // get previous room for end position
        RoomData data = NetworkGameManager.Singleton.roomsToSpawn.Dequeue(); // get new room from room queue

        SpawnObject(data.gameObject, prevData.endPos.position, prevData.endPos.rotation, false, index, NetworkGameManager.Singleton.RoomCreated); // spawn room using the previous room end position as reference, with callback to game manager
    }

    public void DespawnRoomObjects(int index)
    {
        spawner.DespawnRoomObjects(index);
    }

    public void SpawnRush()
    {
        spawner.SpawnRush();
    }
}

public class QueuedNetworkObject // Class for storing spawn requests in queue
{
    public GameObject obj;
    public Vector3 pos;
    public Quaternion rot;
    public bool perma;
    public int roomIDX;
    public System.Action<GameObject> roomCallback;

    public QueuedNetworkObject(GameObject gamObj, Vector3 position, Quaternion rotation, bool permanent, int roomIndex = -1, System.Action<GameObject> callback = null)
    {
        obj = gamObj;
        pos = position;
        rot = rotation;
        perma = permanent;
        if (roomIndex != -1)
        {
            roomIDX = roomIndex;
        }
        if (callback != null)
        {
            roomCallback = callback;
        }
    }
}
