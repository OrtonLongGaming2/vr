using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ScreechController : NetworkBehaviour
{
    [SerializeField]
    private Transform screechRotator; // object to rotate screech around player

    [SerializeField]
    private Transform screechParent; // object screech is parented to

    [SerializeField]
    private GameObject Screech; // screech prefab

    [SerializeField]
    private Renderer ScreechBody; // screech body renderer for checking is on screen

    [SerializeField]
    private Animator ScreechAnimator; // screech animator for defeat/idle/jumpscare animations

    //camera - players headset position/rotation
    private Camera cam;

    //if is in a dark room
    private bool IsActive = false;

    //if screech has been activated and spawned around you
    private bool ScreechSpawned = false;

    //timers for spawning and killing player
    private SimpleTimer spawnTimer = new SimpleTimer();
    private SimpleTimer killTimer = new SimpleTimer();

    //amount used for how long player stared at screech to defeat
    private float defeatAmount = 0;

    //used to prevent functionality while waiting for screech to end animation
    private bool waitingOnDisable;
    private bool waitingOnDisableKill;

    //ragdoll player on death
    private DeathController death;

    private Quaternion rotationToSet = Quaternion.identity; // rotation to set screechRotator to to maintain constant rotation around player head

    public override void OnNetworkSpawn() // reparent to camera and get death
    {
        death = GetComponent<DeathController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        screechRotator.SetParent(cam.transform);
        screechRotator.transform.localPosition = Vector3.zero;
    }

    //set if in a dark room or not (active)
    public void SetActive(bool active)
    {
        IsActive = active;
    }

    private void Update()
    {
        if (!IsOwner) return; // dont run for all clients, only locally

        //if waiting for screech to end jumpscare animation
        if (waitingOnDisableKill)
        {
            screechRotator.rotation = cam.transform.rotation; // rotate to follow player vision for jumpscare
            return;
        }

        //if waiting for screech to end defeat animation
        if (waitingOnDisable)
        {
            screechRotator.rotation = rotationToSet; // keep random rotation around player constant despite character turning
            return;
        }

        //if currently active
        if (ScreechSpawned)
        {
            screechRotator.rotation = rotationToSet; // keep random rotation around player constant despite character turning
            screechParent.LookAt(cam.transform); // point Screech toward player

            //check if on screen!

            Vector3 screenPos = cam.WorldToScreenPoint(Screech.transform.position);
            bool onScreen = screenPos.x > 0f && screenPos.x < Screen.width && screenPos.y > 0f && screenPos.y < Screen.height;

            //if on screen, increment defeat timer - staring at screech defeats him!
            if (onScreen && ScreechBody.isVisible)
            {
                defeatAmount += Time.deltaTime;
                Debug.Log(defeatAmount);
            }

            //check if defeated
            if (defeatAmount >= 1f) // must stare at for one whole second to defeat
            {
                ScreechSpawned = false; // allow spawn functionality to run again and prevent spawned functionality
                ScreechAnimator.SetTrigger("Defeat"); // play defeat animation

                waitingOnDisable = true; // stop functionality during animation

                StartCoroutine(WaitAndDisable()); // wait 3 seconds before allowing functionality again

                return;
            }

            //check if ded
            if (killTimer.Started && killTimer.IsExpired()) // must be active for 5 seconds without being defeated to jumpscare and hurt player
            {
                ScreechSpawned = false; // allow spawn functionality to run again and prevent spawned functionality
                ScreechAnimator.SetTrigger("Jumpscare"); // play jumpscare animation

                waitingOnDisableKill = true; // stop functionality during animation

                StartCoroutine(WaitAndDisableKill()); // wait 3 seconds before allowing functionality again and killing player

                return;
            }

            return;
        }

        if (!IsActive) // if no longer a dark room
        {
            if (Screech.activeInHierarchy) // if screech is active, reset screech
            {
                ScreechSpawned = false; // allow spawn functionality to run again and prevent spawned functionality
                spawnTimer.Reset();
                killTimer.Reset();
                ScreechAnimator.ResetTrigger("Defeat");
                ScreechAnimator.ResetTrigger("Jumpscare");
                ScreechAnimator.SetTrigger("Reset");
                Screech.SetActive(false);
                waitingOnDisable = false;
                waitingOnDisableKill = false;
            }
            return;
        }

        if (!spawnTimer.Started) // if not spawned and havent started spawn timer, start it and wait for it to finish
        {
            spawnTimer.StartTimer(Random.Range(5f, 15f));
            return;
        }

        if (!spawnTimer.IsExpired()) // if still waiting to spawn, dont spawn
        {
            return;
        }

        // SPAWN NEW SCREECH

        Debug.Log("spawning new screech");

        //randomly rotate 
        rotationToSet = Random.rotation;
        screechRotator.rotation = rotationToSet;

        defeatAmount = 0; // reset look time to defeat

        Screech.SetActive(true); // show screech object

        ScreechAnimator.ResetTrigger("Reset"); // allow resetting on animator

        killTimer.StartTimer(5); // start timer until screech jumpscare player (if not defeated!)

        ScreechSpawned = true; // prevent spawn functionality running and allow screech spawned functionality
    }

    //wait 3 seconds (allow animation to finish) and then reset screech
    private IEnumerator WaitAndDisable()
    {
        yield return new WaitForSeconds(3);

        spawnTimer.Reset();
        killTimer.Reset();

        ScreechAnimator.ResetTrigger("Defeat");
        ScreechAnimator.ResetTrigger("Jumpscare");
        ScreechAnimator.SetTrigger("Reset");
        Screech.SetActive(false);
        waitingOnDisable = false;
        waitingOnDisableKill = false;

        yield return null;
    }

    //wait 3 seconds (allow animation to finish) and then reset screech and kill player
    private IEnumerator WaitAndDisableKill()
    {
        yield return new WaitForSeconds(3);

        spawnTimer.Reset();
        killTimer.Reset();

        ScreechAnimator.ResetTrigger("Defeat");
        ScreechAnimator.ResetTrigger("Jumpscare");
        ScreechAnimator.SetTrigger("Reset");
        Screech.SetActive(false);
        waitingOnDisable = false;
        waitingOnDisableKill = false;

        GetComponent<DeathController>().Die();

        yield return null;
    }
}
