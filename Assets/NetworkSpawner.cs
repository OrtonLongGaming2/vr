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

        NetworkObjectSpawnManager.Singleton.SetSpawner(this); // give spawn manager networkspawner to allow spawning

        //NetworkManager.SceneManager.OnLoad += SwitchedSceneHost; // wait until new scene load before spawning game manager
    }

    //private void SwitchedSceneHost(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation) // spawn game manager once host started game
    //{
    //   SpawnPermanentObject(Resources.Load<GameObject>("NetworkGameManager"), Vector3.zero, Quaternion.identity); // spawn game manager
    //}

    /// <summary>
    /// Spawn a Network Prefab to be destroyed with the scene
    /// </summary>
    /// <param name="netObj"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void SpawnNetworkObject(GameObject netObj, Vector3 position, Quaternion rotation)
    {
        Debug.Log("Requested network object spawn - " + netObj.name);

        GameObject go = Instantiate(netObj, position, rotation); // instantiate in scene locally

        if (go.GetComponent<NetworkObject>() == null) // gun is local for testing
        {
            return;
        }

        go.GetComponent<NetworkObject>().Spawn(true); // spawn for clients

        // spawn any other network objects that are part of the room - dressers, items, etc
        RoomData roomData = go.GetComponent<RoomData>();
        if (roomData)
        {
            foreach (RoomNetworkObject i in roomData.roomNetworkObjs)
            {
                SpawnParentedNetworkObject(i.prefab, i.pos, Quaternion.Euler(i.eulerRot), i.scale, go.transform); // spawn with parenting, local position, and convert local rotation euler to Quaternion
            }
        }
    }

    private void SpawnParentedNetworkObject(GameObject netObj, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent) // spawning Room Network Prefabs
    {
        Vector3 offsetPos = (parent.transform.position + position); // prefab position with room position as offset

        GameObject go = Instantiate(netObj, new Vector3(offsetPos.x, position.y, offsetPos.z), rotation); // instantiate in scene locally with scale and room offset

        go.GetComponent<NetworkObject>().Spawn(true); // spawn for clients

        //go.transform.localScale = scale; // set scale

        int indexToUse = parent.GetComponent<RoomData>().RoomIndex; // index for spawned room new object is child of
        
        if (spawnedRoomObjects.ContainsKey(indexToUse) == false) // add room key if needed
        {
            spawnedRoomObjects.Add(indexToUse, new List<GameObject>());
        }

        spawnedRoomObjects[indexToUse].Add(go); // add new object to dictionary with room index - used for despawing room and objects within
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
}
