using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class RoomData : NetworkBehaviour
{
    public int RoomIndex = -1; // the rooms index, used for the door number
    public List<RoomNetworkObject> roomNetworkObjs = new List<RoomNetworkObject>(); // network objects to spawn alongside room, normally keys and furniture
    public Transform endPos; // end position used for spawning new room
    public List<Transform> rushPositions; // rush positions used when instantiating network positions - despawns with room

    [HideInInspector]
    public List<Transform> networkRushPositions = new List<Transform>(); // rush positions instantiated over network - dont despawn with rooms

    [HideInInspector]
    public NetworkVariable<bool> Dark = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // if the room is dark or not

    public void SetDark() // called by host - set room to be dark
    {
        Dark.Value = true;
        GetComponent<Animator>().SetTrigger("LightsOut");

        PlayDarkSoundClientRpc(); // play dark room opened sound for all players
    }

    public void Flicker() // called by host - set room to flicker to signal rush
    {
        GetComponent<Animator>().SetTrigger("Flicker");
    }

    [ClientRpc]
    public void PlayDarkSoundClientRpc() // called for all players, plays dark room sound.
    {
        GameObject.Find("DarkRoomSound").GetComponent<AudioSource>().Play();
    }
}

/// <summary>
/// Custom Class used to specify network objects to spawn as part of room - set in MonoBehavior
/// </summary>
[Serializable]
public class RoomNetworkObject
{
    public GameObject prefab;
    public Transform positioning;
}
