using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathfUtility
{
    public static float Smooth(float x, float pow)
    {
        return -Mathf.Pow(1 - x, pow) + 1;
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
}
