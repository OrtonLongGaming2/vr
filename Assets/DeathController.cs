using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Interaction.Toolkit;

public class DeathController : NetworkBehaviour
{
    public bool death = false;

    [SerializeField] private RigBuilder builder;
    [SerializeField] private IKTargetFollowVRRig follower;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject deathSound;
    private bool shownCorpse = false;

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

    void Update()
    {
        if (death)
        {
            DisableClientRpc(); //disable components for all clients - pls work            

            // kill for first time

            if (!shownCorpse)
            {
                shownCorpse = true; // prevent looping code

                Renderer[] renderers = GetComponentsInChildren<Renderer>();

                foreach (Renderer i in renderers) // show corpse
                {
                    i.enabled = true;
                }

                GameObject.Find("XR Origin").GetComponent<ActionBasedContinuousMoveProvider>().moveSpeed = 0; // prevent player movement

                deathSound.SetActive(true); // play death sound
            }
        }
    }

    [ClientRpc]
    public void DisableClientRpc()
    {
        //disable components that make ragdoll work

        builder.enabled = false;
        follower.enabled = false;
        animator.enabled = false;

        // allow ragdollin

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

    //call death from other components
    public void Die()
    {
        if (IsOwner)
        {
            death = true;
        }
    }
}
