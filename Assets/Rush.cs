using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class Rush : NetworkBehaviour
{
    [SerializeField]
    private float _speedMetersPerSecond = 25f;

    private Vector3? _destination;
    private Vector3 _startPos;
    private float totalLerpDuration;
    private float elapsedLerpDuration;
    private Action completeCallback;

    private List<Transform> positions;

    //VARIABLES FOR TESTING/DEBUGGING

    [SerializeField]
    private Transform debugStartPosition;

    [SerializeField]
    private List<Transform> debugPositions;

    [SerializeField]
    private bool runDebug;

    [SerializeField]
    private bool isDebug = false;

    [SerializeField]
    private GameObject farAudio;

    [SerializeField]
    private GameObject nearAudio;

    //METHODS

    public void SetPositionAndStart(List<Transform> waypoints)
    {
        if (!IsHost && !isDebug) return;

        positions = waypoints;

        if (positions == null) return;
        if (positions.Count <= 0) return;

        transform.position = waypoints[0].position; // set position to start position

        transform.rotation = Quaternion.LookRotation(waypoints[1].position - transform.position); // point in movement

        transform.localPosition = new Vector3(transform.localPosition.x, 0f, transform.localPosition.z + 10); // add offset (build up!)

        Transform newPos = positions[0];
        positions.Remove(newPos); // remove waypoint from list

        StartCoroutine(WaitAndRush(newPos, waypoints)); // handle delay
    }

    private IEnumerator WaitAndRush(Transform newPos, List<Transform> waypoints)
    {
        yield return new WaitForSeconds(10); // add 10 second delay before rushing

        Debug.Log("rush activated");

        ActivateAudioClientRpc(); // activate audio emitters

        MoveTo(newPos, NextPoint); // begin movement

        yield return null;
    }

    [ClientRpc]
    public void ActivateAudioClientRpc()
    {
        farAudio.SetActive(true);
        nearAudio.SetActive(true);
    }

    // set enxt waypoint
    private void NextPoint()
    {
        if (positions == null) return;

        if (positions.Count <= 0) // if finished, hide rush
        {
            if (!isDebug)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
        else // if not done, move to next point
        {
            Transform newPos = positions[0];
            positions.Remove(newPos); // remove waypoint from list
            MoveTo(newPos, NextPoint);
        }
    }

    //initialize tween between waypoints
    public void MoveTo(Transform destination, Action onComplete)
    {
        Vector3 fixedDest = new Vector3(destination.position.x, 0f, destination.position.z);

        float distToNextPoint = Vector3.Distance(transform.position, fixedDest);
        totalLerpDuration = (distToNextPoint / _speedMetersPerSecond);

        _startPos = transform.position;
        _destination = fixedDest;
        elapsedLerpDuration = 0f;
        completeCallback = onComplete;
    }

    // manage debug and tween movement if intiialized
    private void Update()
    {
        if (!IsHost && !isDebug) return;

        if (runDebug)
        {
            runDebug = false;
            SetPositionAndStart(new List<Transform>(debugPositions));
        }

        if (_destination.HasValue == false) return;

        if (elapsedLerpDuration >= totalLerpDuration && totalLerpDuration > 0) return; // do not run if not set any command

        elapsedLerpDuration += Time.deltaTime;

        float percent = (elapsedLerpDuration / totalLerpDuration);

        Vector3 oldPosition = transform.position; // for rotation

        transform.position = Vector3.Lerp(_startPos, _destination.Value, percent); // move position of rush

        transform.rotation =  Quaternion.LookRotation(transform.position - oldPosition); // set rotation based on direction of movement

        if (elapsedLerpDuration >= totalLerpDuration)
        {
            completeCallback.Invoke();
        }
    }
}
