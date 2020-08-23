using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavPointScript : MonoBehaviour
{
    // List of times (in seconds) to go through
    public float[] times;

    // List of angles (relative to the sentry facing) to go through
    public float[] angles;

    // Initial direction this sentry is facing
    private Quaternion rotation;

    public NavPointData GetNavData()
    {
        return new NavPointData(times, angles, rotation);
    }

    private void Awake()
    {
        this.rotation = transform.rotation;
    }



}
