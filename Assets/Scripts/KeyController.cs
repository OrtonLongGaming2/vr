using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class KeyController : IKey
{
    public NetworkVariable<int> AssignedDoor = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // key door number to display and use to check if correct key for door

    public TextMeshProUGUI numberText; // text component set via monobehavior

    public override KeyType type // if key can unlock every door or just the room number one
    {
        get { return KeyType.RoomNumber; }
    }

    private void Update() // update number text to match
    {
        if (numberText != null)
        {
            string textToSet;
            int absDoorNumber = Mathf.Abs(AssignedDoor.Value);

            //make number use 0s before digits so always 4 digits
            if (absDoorNumber > 999)
            {
                textToSet = absDoorNumber.ToString();
            }
            else if (absDoorNumber > 99)
            {
                textToSet = "0" + absDoorNumber.ToString();
            }
            else if (absDoorNumber > 9)
            {
                textToSet = "00" + absDoorNumber.ToString();
            }
            else
            {
                textToSet = "000" + absDoorNumber.ToString();
            }

            if (AssignedDoor.Value < 0)
            {
                textToSet = "-" + textToSet;
            }

            numberText.text = textToSet;
        }
    }

    public void SetRoomNumber(int value) // set the room number for this key
    {
        if (IsHost || IsServer)
        {
            SetRoomNumberServerRpc(value); // tell server to set network variable
        }
    }

    //set network variable
    [ServerRpc(RequireOwnership = false)]
    private void SetRoomNumberServerRpc(int value)
    {
        AssignedDoor.Value = value;
    }
}
