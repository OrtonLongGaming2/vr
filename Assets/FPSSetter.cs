using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSSetter : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 120;
    }
}
