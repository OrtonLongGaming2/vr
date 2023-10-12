using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DebugNetworkItemSpawn : MonoBehaviour
{
    [SerializeField]
    public GameObject spawnPrefab;

    private void Start()
    {
        if (NetworkObjectSpawnManager.Singleton)
        {
            NetworkObjectSpawnManager.Singleton.SpawnObject(spawnPrefab, transform.position, transform.rotation);
        }
    }
}
