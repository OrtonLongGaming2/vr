using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomData : MonoBehaviour
{
    public int RoomIndex = -1;
    public List<RoomNetworkObject> roomNetworkObjs = new List<RoomNetworkObject>();
    //public Transform startPos;
    //public Transform endPos;
}

[Serializable]
public class RoomNetworkObject
{
    public GameObject prefab;
    public Vector3 pos;
    public Vector3 eulerRot;
    public Vector3 scale;
}
