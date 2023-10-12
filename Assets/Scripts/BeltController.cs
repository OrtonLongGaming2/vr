using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    private void FixedUpdate()
    {
        transform.position = target.position + Vector3.up * offset.y
            + Vector3.ProjectOnPlane(target.right, Vector3.up).normalized * offset.x
            + Vector3.ProjectOnPlane(target.forward, Vector3.up).normalized * offset.z;
        transform.eulerAngles = new Vector3(0f, target.eulerAngles.y + 90, 0f);
    }
}
