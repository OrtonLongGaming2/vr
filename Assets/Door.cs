using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

public class Door : NetworkBehaviour
{
    [SerializeField]
    private List<TMPro.TextMeshProUGUI> numberTexts;

    [SerializeField]
    private Animator animator;

    public NetworkVariable<int> DoorIndex = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        animator.SetBool("HasLock", true);
    }

    public void SetRoomIndex(int room)
    {
        DoorIndex.Value = room;
    }

    public void SetLocked(bool locked)
    {
        animator.SetBool("HasLock", locked);
    }

    public void KeyInput(SelectEnterEventArgs args)
    {
        Debug.Log("input in lock socket");

        KeyController key = args.interactableObject.transform.gameObject.GetComponent<KeyController>();
        if (!key) return;

        Debug.Log("key input! type: " + key.type.ToString() + " - value: " + key.AssignedDoor.Value + " - door value: " + DoorIndex.Value);

        if (key.type == IKey.KeyType.Universal) // if key is universal, it can open any door. dont check value.
        {
            OpenDoor();
            return;
        }

        if (key.AssignedDoor.Value == DoorIndex.Value) // only open door if key value matches wanted value
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        animator.SetTrigger("Open");

        NetworkGameManager.Singleton.DoorOpened(DoorIndex.Value);
    }

    private void Update()
    {
        if (numberTexts != null)
        {
            string textToSet;
            int absDoorNumber = Mathf.Abs(DoorIndex.Value);

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

            if (DoorIndex.Value < 0)
            {
                textToSet = "-" + textToSet;
            }

            foreach (TMPro.TextMeshProUGUI i in numberTexts)
            {
                i.text = textToSet;
            }
        }
    }
}
