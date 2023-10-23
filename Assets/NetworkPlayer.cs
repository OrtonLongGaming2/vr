using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkPlayer : NetworkBehaviour
{
    public int currentRoom = -1;
    private RoomData currentRoomData = null;
    private bool InWardrobe = false;
    [SerializeField]
    private Color baseSkyboxColor = new Color(0.2627451f, 0.2f, 0.2196078f, 1);

    //disable player model visuals for self
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        //disable player model visuals for self

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer i in renderers)
        {
            i.enabled = false;
        }

        // add on select events for changing grabbed object parent

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
    }

    // when grabbing network object, set owner to player
    public void OnSelectGrabbable(SelectEnterEventArgs args)
    {
        NetworkObject selectedObject = args.interactableObject.transform.GetComponent<NetworkObject>();

        if (selectedObject == null)
        {
            selectedObject = args.interactableObject.transform.GetComponentInParent<NetworkObject>();
            if (selectedObject == null) return;
        }

        RequestGrabbableOwnershipServerRpc(OwnerClientId, selectedObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestGrabbableOwnershipServerRpc(ulong playerId, NetworkObjectReference netObjectRef)
    {
        if (netObjectRef.TryGet(out NetworkObject netObject))
        {
            netObject.ChangeOwnership(playerId);
        }
    }

    // ENEMIES / ROOMS

    // when colliding with a trigger on the NetworkTriggers layer
    public void OnTriggerEnter(Collider other) 
    {
        Debug.Log("collided with trigger! - " + other.gameObject.name);

        //die if touching rush and not in wardrobe
        Rush rushData = other.gameObject.GetComponent<Rush>();
        if (rushData && !InWardrobe)
        {
            GetComponent<DeathController>().Die();
            return;
        }

        //if the trigger is a room, check if dark or not and record current room index
        RoomData room = other.gameObject.GetComponent<RoomData>();
        if (room)
        {
            Debug.Log("collided with room - index: " + room.RoomIndex + " - dark?: " + room.Dark.Value);

            //set current room information
            currentRoom = room.RoomIndex;
            currentRoomData = room;

            //enable/disable screech and set ambient colore (to make dark rooms darker)
            if (currentRoomData.Dark.Value)
            {
                GetComponent<ScreechController>().SetActive(true);
                RenderSettings.ambientSkyColor = Color.black;
                DynamicGI.UpdateEnvironment();
            }
            else
            {
                GetComponent<ScreechController>().SetActive(false);
                RenderSettings.ambientSkyColor = baseSkyboxColor;
                DynamicGI.UpdateEnvironment();
            }
            return;
        }

        // if the trigger is a wardrobe door hitbox, open the door

        if (other.CompareTag("WardrobeDoorOpen"))
        {
            other.gameObject.GetComponent<DoorOpenHitbox>().OpenDoor();
            return;
        }

        // if the trigger is a wardrobe, set player as in a wardrobe

        if (other.CompareTag("Wardrobe"))
        {
            InWardrobe = true;
        }
    }

    // when leaving collision with a trigger on the NetworkTriggers layer
    private void OnTriggerExit(Collider other)
    {
        // if the trigger is a wardrobe door hitbox, close the door

        if (other.CompareTag("WardrobeDoorOpen"))
        {
            other.gameObject.GetComponent<DoorOpenHitbox>().CloseDoor();
            return;
        }

        // if the trigger is a wardrobe, set player as out of wardrobe

        if (other.CompareTag("Wardrobe"))
        {
            InWardrobe = false;
        }
    }
}
