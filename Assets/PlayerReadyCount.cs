using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerReadyCount : MonoBehaviour
{
    public TextMeshProUGUI txt;
    private int PlayerCount;

    private void Start()
    {
        PlayerCount = FindObjectsOfType<NetworkPlayer>().Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (txt)
        {
            if (NetworkGameManager.Singleton)
            {
                txt.text = NetworkGameManager.Singleton.LoadedPlayers.Value.ToString() + "/" + PlayerCount.ToString(); //NetworkManager.Singleton.ConnectedClientsIds.Count.ToString();
            }
        }
    }
}
