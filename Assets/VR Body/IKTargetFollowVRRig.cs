using UnityEngine;
using Unity.Netcode.Components;
using Unity.Netcode; 

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    public void Map()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class IKTargetFollowVRRig : NetworkBehaviour
{
    [Range(0,1)]
    public float turnSmoothness = 0.1f;
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;

    private bool Ready = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        head.vrTarget = GameObject.Find("Head Tracking VR Target").transform;
        leftHand.vrTarget = GameObject.Find("Left Hand Tracking VR Target").transform;
        rightHand.vrTarget = GameObject.Find("Right Hand Tracking VR Target").transform;

        Ready = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!IsOwner) return;

        if (!Ready) return;

        transform.position = head.ikTarget.position + headBodyPositionOffset;
        float yaw = head.vrTarget.eulerAngles.y;
        //transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z),turnSmoothness);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z);

        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
}
