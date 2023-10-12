using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkPlayer : NetworkBehaviour
{
    //disable player model visuals for self
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer i in renderers)
        {
            i.enabled = false;
        }

        GameObject fatherLeft = GameObject.Find("Left Hand");
        if (fatherLeft)
        {
            fatherLeft.GetComponent<XRDirectInteractor>().selectEntered.AddListener(OnSelectGrabbable);
        }
        GameObject fatherRight = GameObject.Find("Right Hand");
        if (fatherRight)
        {
            fatherRight.GetComponent<XRDirectInteractor>().selectEntered.AddListener(OnSelectGrabbable);
        }

        //NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneEvent;

        //if (!IsHost) return;

        //GameObject debuggerino = GameObject.Find("Debuggerino");

        //List<GameObject> spawnPrefab = debuggerino.GetComponent<DebugNetworkItemSpawn>().spawnPrefab;
        //foreach (GameObject i in spawnPrefab)
        //{
        //    GameObject go = Instantiate(i, new Vector3(debuggerino.transform.position.x + Random.Range(-3f, 3f), debuggerino.transform.position.y, debuggerino.transform.position.z + Random.Range(-3f, 3f)), debuggerino.transform.rotation);
        //    go.GetComponent<NetworkObject>().Spawn();
        //}
    }

    //private void SceneEvent(SceneEvent eventType)
    //{
    //    if (eventType.SceneEventType == SceneEventType.LoadComplete)
    //    {
    //        NotifyCompleteServerRpc();
    //    }
    //}

    //[ServerRpc] // tell host im in
    //private void NotifyCompleteServerRpc()
    //{
    //    if (IsHost)
    //    {
    //        NetworkGameManager.Singleton.CompletedLoad(); // increment loaded players
    //    }
    //}

    public void OnSelectGrabbable(SelectEnterEventArgs args)
    {
        //if (IsOwner)
        //{
        NetworkObject selectedObject = args.interactableObject.transform.GetComponent<NetworkObject>();

        if (selectedObject == null)
        {
            selectedObject = args.interactableObject.transform.GetComponentInParent<NetworkObject>();
            if (selectedObject == null) return;
        }

        RequestGrabbableOwnershipServerRpc(OwnerClientId, selectedObject);
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestGrabbableOwnershipServerRpc(ulong playerId, NetworkObjectReference netObjectRef)
    {
        if (netObjectRef.TryGet(out NetworkObject netObject))
        {
            netObject.ChangeOwnership(playerId);
        }
    }
}
