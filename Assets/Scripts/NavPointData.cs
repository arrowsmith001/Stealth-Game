using System;
using UnityEngine;

public class NavPointData
{
    // List of times (in seconds) to go through
    public float[] times;

    // List of angles (relative to the sentry facing) to go through
    public float[] angles;

    // Initial direction this sentry is facing
    public Quaternion direction;

    public NavPointData(float[] times, float[] angles, Quaternion direction)
    {
        this.times = times;
        this.angles = angles;
        this.direction = direction;
    }

    internal NavPointSingle GetNavPoint(int i)
    {
        return new NavPointSingle(times[i], angles[i], direction);
    }
}

public class NavPointSingle
{
    public float time;

    public float angle;

    public Quaternion direction;

    public NavPointSingle(float time, float angle, Quaternion direction)
    {
        this.time = time;
        this.angle = angle;
        this.direction = direction;
    }
}