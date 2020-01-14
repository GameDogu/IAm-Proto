using UnityEngine;

public static class TransformExtensions
{
    public static TransformLocalInfo GetLocalInfo(this Transform t)
    {
        return new TransformLocalInfo() { localPosition = t.localPosition, localRotation = t.localRotation, localScale = t.localScale };
    }

    public static void SetToTransformInfo(this Transform t, TransformLocalInfo info)
    {
        t.localPosition = info.localPosition;
        t.localRotation = info.localRotation;
        t.localScale = info.localScale;
    }

}
