using System;
using UnityEngine;

internal class UtilsClass
{

    internal static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }


    internal static bool Approximately(Quaternion quatA, Quaternion quatB, float acceptableRange)
    {
        return 1 - Mathf.Abs(Quaternion.Dot(quatA, quatB)) < acceptableRange;
    }
}