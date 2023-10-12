using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BatteryController : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Flashlight"))
        {
            collision.collider.gameObject.GetComponent<FlashlightController>().BatteryCollect(gameObject);

            GetComponent<NetworkObject>().Despawn(true); // despawn from other clients and destroy
        }
    }
}
