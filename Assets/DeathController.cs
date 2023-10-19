using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations.Rigging;

public class DeathController : NetworkBehaviour
{
    public bool death = false;

    [SerializeField] private RigBuilder builder;
    [SerializeField] private IKTargetFollowVRRig follower;
    [SerializeField] private Animator animator;

    public List<Collider> col;
    public List<Rigidbody> rb;

    private void Awake()
    {
        foreach (Rigidbody i in rb)
        {
            i.isKinematic = true;
            i.detectCollisions = false;
        }
        foreach (Collider i in col)
        {
            i.enabled = false;
        }        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) // disable IK for other clients so it can be seen
        {
            builder.enabled = false;
            follower.enabled = false;
        }
    }

    void Update()
    {
        if (death)
        {
            builder.enabled = false;
            follower.enabled = false;
            animator.enabled = false;

            //animator.SetLayerWeight(2, 1);
            //animator.SetTrigger("Die");
            //transform.localPosition = new Vector3(transform.localPosition.x, 0.18f, transform.localPosition.z);

            foreach (Collider i in col)
            {
                i.enabled = true;
            }

            foreach (Rigidbody i in rb)
            {
                i.isKinematic = false;
                i.detectCollisions = true;
            }
        }
    }
}
