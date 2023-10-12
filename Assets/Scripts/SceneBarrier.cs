using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBarrier : MonoBehaviour
{
    public static SceneBarrier Singleton;

    private void Awake()
    {
        Singleton = this;
    }
}
