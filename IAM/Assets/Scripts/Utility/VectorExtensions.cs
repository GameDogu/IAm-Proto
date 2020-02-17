using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public static Vector2 XY(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 YX(this Vector3 v)
    {
        return new Vector2(v.y, v.x);
    }

    public static Vector2 XZ(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector2 ZX(this Vector3 v)
    {
        return new Vector2(v.z, v.x);
    }

    public static Vector2 YZ(this Vector3 v)
    {
        return new Vector2(v.y, v.z);
    }

    public static Vector2 ZY(this Vector3 v)
    {
        return new Vector2(v.z, v.y);
    }

    public static Vector3 ValXY(this Vector2 v,float val)
    {
        return new Vector3(val,v.x, v.y);
    }

    public static Vector3 XValY(this Vector2 v, float val)
    {
        return new Vector3(v.x, val, v.y);
    }

    public static Vector3 XYVal(this Vector2 v, float val)
    {
        return new Vector3(v.x, v.y, val);
    }

    public static Vector3 ValYX(this Vector2 v, float val)
    {
        return new Vector3(val, v.y, v.x);
    }

    public static Vector3 YValX(this Vector2 v, float val)
    {
        return new Vector3(v.y, val, v.x);
    }

    public static Vector3 YXVal(this Vector2 v, float val)
    {
        return new Vector3(v.y, v.x, val);
    }

    public static float GetAngleDeg(this Vector2 v)
    {
        float angle = Mathf.Acos(v.y) * Mathf.Rad2Deg;
        return v.x < 0f ? 360f - angle : angle;
    }

}
