using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class ButtonVR : MonoBehaviour
{
    public GameObject Button;
    public UnityEvent OnPress;
    public UnityEvent OnRelease;

    public GameObject InstantiateObject;
    public Vector3 InstantiatePosition;

    private GameObject presser;
    private bool isPressed;
    private bool beenPressed = false;

    private void Start()
    {
        isPressed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("LeftHand") || other.transform.CompareTag("RightHand")) // prevent button being hit by random object - only hands work
        {
            if (!isPressed)
            {
                Button.transform.localPosition = new Vector3(0f, 0.0406f, 0f);
                presser = other.gameObject;
                OnPress.Invoke();
                isPressed = true;
                return;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == presser)
        {
            Button.transform.localPosition = new Vector3(0f, 0.058f, 0f);
            isPressed = false;
            OnRelease.Invoke();
            return;
        }
    }

    public void SpawnCube() // Button pressed test - spawn a grabbable cube
    {
        GameObject newCube = Instantiate(InstantiateObject);
        newCube.transform.position = InstantiatePosition;
    }

    public void ReadyUp() // Button pressed - tell NetworkGameManager player is ready
    {
        if (!beenPressed) // prevent pressing multiple times
        {
            beenPressed = true; // prevent pressing multiple times

            NetworkGameManager.Singleton.CompletedLoad(); // tell NetworkGameManager player is ready

            transform.parent.gameObject.SetActive(false); // hide button
        }
    }
}
