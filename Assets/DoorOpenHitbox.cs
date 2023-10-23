using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenHitbox : MonoBehaviour
{
    public Animator animator; //animator referernce for monobehavior

    public void OpenDoor() // called by NetworkPlayer, opens wardrobe door when player is close
    {
        animator.SetBool("Open", true);
    }

    public void CloseDoor() // called by NetworkPlayer, closes wardrobe door when player leaves range/enters wardrobe
    {
        animator.SetBool("Open", false);
    }
}
