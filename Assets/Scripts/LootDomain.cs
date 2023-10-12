using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootDomain : MonoBehaviour
{
    private List<LootItem> drawerItems;

    public List<GameObject> DrawerItemPrefabs;
    public List<float> DrawerItemChances;

    public List<LootItem> DrawerItems { get { return drawerItems; } }

    private LootItem nullItem;

    private void Awake()
    {
        drawerItems = new List<LootItem>();
        nullItem = new LootItem("null");

        foreach (GameObject i in DrawerItemPrefabs)
        {
            drawerItems.Add(new LootItem(DrawerItemChances[DrawerItemPrefabs.IndexOf(i)], i));
        }
    }

    public LootItem GetRandomItem(List<LootItem> excludes)
    {
        float randomNumber = Random.Range(0, 101);
        List<LootItem> possibleItems = new List<LootItem>();

        foreach (LootItem i in drawerItems)
        {
            if (randomNumber <= i.chance)
            {
                possibleItems.Add(i);
            }
        }

        if (possibleItems.Count > 0)
        {
            return possibleItems[Random.Range(0, possibleItems.Count)];
        }

        return nullItem;
    }
}

public class LootItem
{
    public float chance;
    public GameObject prefab;

    public LootItem(float Chance, GameObject Prefab)
    {
        chance = Chance;
        prefab = Prefab;
    }

    public LootItem(string Null)
    {
        chance = 0f;
        prefab = null;
    }
}
