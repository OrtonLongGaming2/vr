using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Spawned by NetworkSpawner
/// </summary>
public class NetworkGameManager : NetworkBehaviour // Has to be owned by host
{
    public NetworkVariable<int> LoadedPlayers = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public static NetworkGameManager Singleton;

    //public override void OnNetworkSpawn() // set singleton
    //{
    //    base.OnNetworkSpawn();
    //    Singleton = this;
    //}

    private void Awake() // set singleton
    {
        if (!Singleton)
        {
            Singleton = this;
        }
    }

    public void CompletedLoad() // Player pressed ready button
    {
        CompletedLoadServerRpc(); // tell host to change network variable
    }

    [ServerRpc(RequireOwnership = false)]
    public void CompletedLoadServerRpc()
    {
        LoadedPlayers.Value = (LoadedPlayers.Value + 1); // change network variable

        if (LoadedPlayers.Value >= NetworkManager.Singleton.ConnectedClientsIds.Count) // if all players ready
        {
            ReleaseSceneBarrierClientRpc(); // allow network objects to be spawned
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
}
