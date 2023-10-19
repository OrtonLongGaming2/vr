using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class KeyController : IKey
{
    public NetworkVariable<int> AssignedDoor = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public TextMeshProUGUI numberText;

    public override KeyType type
    {
        get { return KeyType.RoomNumber; }
    }

    private void Update()
    {
        //if (!IsOwner) return;

        if (numberText != null)
        {
            string textToSet;
            int absDoorNumber = Mathf.Abs(AssignedDoor.Value);

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

    public void SetRoomNumber(int value)
    {
        if (IsHost || IsServer)
        {
            SetRoomNumberServerRpc(value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRoomNumberServerRpc(int value)
    {
        AssignedDoor.Value = value;
    }
}
