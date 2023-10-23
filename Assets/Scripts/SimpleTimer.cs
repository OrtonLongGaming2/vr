using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTimer
{
    //FIELDS

    private const float Unset = 1f;

    private float _endTime = 1f;

    private bool _started;

    //PROPERTIES

    public bool Started
    {
        get { return _started; }
    }

    //METHODS

    public void StartTimer(float time)
    {
        _started = true;
        _endTime = Time.time + time;
    }

    public void Reset()
    {
        _started = false;
        _endTime = Unset;
    }

    public bool IsExpired()
    {
        if (Time.time >= _endTime)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(a: 0f, b: _endTime - Time.time);
    }

    public SimpleTimer()
    {
        _endTime = 1f;
    }
}
