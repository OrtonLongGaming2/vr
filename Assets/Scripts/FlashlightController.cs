using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;

public class FlashlightController : NetworkBehaviour
{
    //public List<GameObject> enableDisableObjects;
    public Material neonMaterial;
    public GameObject batteryCanvas;
    public Slider batterySlider;

    private NetworkVariable<float> battery = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public float maxTime = 180f;
    private bool active = false;
    private ControllerVelocity ControllerVelocity = null;
    public bool ShakeLight = false;
    private List<GameObject> usedBatteryObjects = new List<GameObject>();
    public Color mainOnColor;
    public Color emissiveOnColor;

    private Animator _animator;

    public override void OnNetworkSpawn()
    {
        _animator = GetComponent<Animator>();

        active = false;

        batteryCanvas.SetActive(false);

        //foreach (GameObject i in enableDisableObjects)
        //{
        //    i.SetActive(false);
        //}
        _animator.SetBool("On", false);

        Debug.Log(neonMaterial.GetColor("_Color"));

        neonMaterial.SetColor("_Color", new Color(0, 0, 0));
        neonMaterial.SetColor("_EmissionColor", new Color(0, 0, 0));

        if (IsOwner)
        {
            SetBatteryServerRpc(Random.Range(0.4f, 1.0f));
        }
    }

    private void Update()
    {
        if (battery.Value <= 0f)
        {
            //foreach (GameObject i in enableDisableObjects)
            //{
            //    i.SetActive(false);
            //}
            if (_animator)
            {
                _animator.SetBool("On", false);
            }

            neonMaterial.SetColor("_Color", new Color(0, 0, 0));
            neonMaterial.SetColor("_EmissionColor", new Color(0, 0, 0));
        }

        if (!IsOwner) return;

        if (battery.Value <= 0f)
        {
            active = false;
        }

        batterySlider.value = battery.Value;

        if (active)
        {
            SetBatteryServerRpc(Mathf.Max(battery.Value - (Time.deltaTime / maxTime), 0f));
        }                

        if (ShakeLight && (ControllerVelocity != null))
        {
            if ((ControllerVelocity.Velocity.x > 1.5f) || (ControllerVelocity.Velocity.x < -1.5f) || (ControllerVelocity.Velocity.y > 1.5f) || (ControllerVelocity.Velocity.y < -1.5f) || (ControllerVelocity.Velocity.z > 1.5f) || (ControllerVelocity.Velocity.z < -1.5f))
            {
                SetBatteryServerRpc(Mathf.Min(1f, battery.Value + 0.05f));
            }
        }
    }

    public void OnInteractEnter()
    {
        active = true;
        if (battery.Value > 0f)
        {
            //foreach (GameObject i in enableDisableObjects)
            //{
            //    i.SetActive(true);
            //}
            _animator.SetBool("On", true);

            neonMaterial.SetColor("_Color", mainOnColor); //new Color(1f, 0.5803921f, 0.4941176f));
            neonMaterial.SetColor("_EmissionColor", new Color(emissiveOnColor.r * (Mathf.Pow(2, 1)), emissiveOnColor.g * (Mathf.Pow(2, 1)), emissiveOnColor.b * (Mathf.Pow(2, 1)))); // new Color(0.7490196f * (Mathf.Pow(2, 1)), 0.4352941f * (Mathf.Pow(2, 1)), 0.372549f * (Mathf.Pow(2, 1))));
        }
    }

    public void OnInteractExit()
    {
        active = false;

        //foreach (GameObject i in enableDisableObjects)
        //{
        //    i.SetActive(false);
        //}
        _animator.SetBool("On", false);

        neonMaterial.SetColor("_Color", new Color(0, 0, 0));
        neonMaterial.SetColor("_EmissionColor", new Color(0, 0, 0));
    }

    public void OnSelectEnter(SelectEnterEventArgs args)
    {
        batteryCanvas.SetActive(true);
        Debug.Log("selected");

        if (args.interactorObject.transform.gameObject.GetComponent<ControllerVelocity>() != null)
        {
            ControllerVelocity = args.interactorObject.transform.gameObject.GetComponent<ControllerVelocity>();
        }
        else
        {
            ControllerVelocity = null;
        }
    }

    public void OnSelectExit(SelectExitEventArgs args)
    {
        ControllerVelocity = null;
        Debug.Log("deselected");

        active = false;

        batteryCanvas.SetActive(false);

        //foreach (GameObject i in enableDisableObjects)
        //{
        //    i.SetActive(false);
        //}
        _animator.SetBool("On", false);

        neonMaterial.SetColor("_Color", new Color(0, 0, 0));
        neonMaterial.SetColor("_EmissionColor", new Color(0, 0, 0));
    }

    public void BatteryCollect(GameObject batteryObj)
    {
        if (usedBatteryObjects.Contains(batteryObj) == false)
        {
            usedBatteryObjects.Add(batteryObj);

            SetBatteryServerRpc(Mathf.Min(1, battery.Value + 0.3f));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBatteryServerRpc(float value)
    {
        battery.Value = value;
    }
}
