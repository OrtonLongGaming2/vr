using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomData : MonoBehaviour
{
    public int RoomIndex = -1;
    public List<RoomNetworkObject> roomNetworkObjs = new List<RoomNetworkObject>();
    //public Transform startPos;
    public Transform endPos;
    public List<Transform> rushPositions;
    [HideInInspector]
    public List<Transform> networkRushPositions = new List<Transform>();
}

[Serializable]
public class RoomNetworkObject
{
    public GameObject prefab;
    public Transform positioning;
    //public Vector3 pos;
    //public Vector3 eulerRot;
}
