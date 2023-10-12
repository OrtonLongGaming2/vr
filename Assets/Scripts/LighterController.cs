using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LighterController : NetworkBehaviour
{
    public GameObject onObjects;
    public GameObject forcedOffObjects;
    public GameObject batteryCanvas;

    private bool activated;
    private bool forcedOffObjectsActive;
    private bool onObjectsActive;
    private Animator Animator;

    public NetworkVariable<float> battery = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public float maxTime = 60f;
    public Slider batterySliderA;
    public Slider batterySliderB;

    public override void OnNetworkSpawn()
    {
        batteryCanvas.SetActive(false);

        activated = false;

        Animator = GetComponent<Animator>();
        Animator.SetBool("Held", false);

        onObjects.SetActive(false);
        onObjectsActive = false;

        forcedOffObjects.SetActive(false);
        forcedOffObjectsActive = false;

        if (IsOwner)
        {
            SetBatteryServerRpc(Random.Range(0.4f, 1.0f));
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        batterySliderA.value = battery.Value;
        batterySliderB.value = battery.Value;

        if (Animator != null)
        {
            int flicks;

            if (battery.Value > 0.75)
            {
                flicks = 0;
            }
            else if (battery.Value > 0.50)
            {
                flicks = 1;
            }
            else if (battery.Value > 0.25)
            {
                flicks = 2;
            }
            else
            {
                flicks = 3;
            }

            Animator.SetInteger("Flicks", flicks);
        }

        if (activated)
        {
            SetBatteryServerRpc(Mathf.Max(battery.Value - (Time.deltaTime / maxTime), 0f));
        }

        if (battery.Value <= 0f)
        {
            if (!forcedOffObjectsActive)
            {
                forcedOffObjects.SetActive(true);
                forcedOffObjectsActive = true;
            }
            if (onObjectsActive)
            {
                onObjects.SetActive(false);
                onObjectsActive = false;
            }
        }
        else
        {
            if (activated) // activated and not forced off
            {
                if (!onObjectsActive && Animator.GetCurrentAnimatorStateInfo(0).IsTag("open"))
                {
                    onObjects.SetActive(true);
                    onObjectsActive = true;
                }
                if (forcedOffObjectsActive)
                {
                    forcedOffObjects.SetActive(false);
                    forcedOffObjectsActive = false;
                }
            }
            else // not activated or forced off
            {
                onObjects.SetActive(false);
                onObjectsActive = false;

                forcedOffObjects.SetActive(false);
                forcedOffObjectsActive = false;
            }
        }        
    }

    public void OnGrabEnter()
    {
        batteryCanvas.SetActive(true);
        Animator.SetBool("Held", true);
        Debug.Log("Grabbed");
    }

    public void OnGrabExit()
    {
        batteryCanvas.SetActive(false);
        activated = false;
        Animator.SetBool("Held", false);
    }

    public void OnTriggeredEnter()
    {
        activated = true;
        Animator.ResetTrigger("Close");
        Animator.SetTrigger("Open");
    }

    public void OnTriggeredExit()
    {
        activated = false;
        Animator.ResetTrigger("Open");
        Animator.SetTrigger("Close");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBatteryServerRpc(float value)
    {
        battery.Value = value;
    }
}
