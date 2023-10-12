using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DresserController : NetworkBehaviour
{
    public List<Transform> itemSpawnPoints;
    public bool shouldSpawnLoot = true;

    private void Start()
    {
        if (IsHost || IsServer)
        {
            if (shouldSpawnLoot)
            {
                foreach (Transform i in itemSpawnPoints)
                {
                    InstantiateLootAtPoint(i, MasterDomain.LootDomain.GetRandomItem(new List<LootItem>()));
                }
            }
        }
    }

    private void InstantiateLootAtPoint(Transform point, LootItem item)
    {
        if (item.prefab != null)
        {
            if (NetworkObjectSpawnManager.Singleton.Ready)
            {
                NetworkObjectSpawnManager.Singleton.SpawnObject(item.prefab, point.position, item.prefab.transform.rotation);
            }
        }
    }
}
