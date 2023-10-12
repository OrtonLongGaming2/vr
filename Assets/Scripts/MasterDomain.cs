using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterDomain : MonoBehaviour
{
    public static LootDomain LootDomain;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        LootDomain = GetComponent<LootDomain>();
    }
}

