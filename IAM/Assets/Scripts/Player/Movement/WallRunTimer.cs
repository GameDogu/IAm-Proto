using System;
using UnityEngine;

[Serializable]  
public class WallRunTimer
{
    [SerializeField,Range(0f,10f)] float duration;
    [SerializeField,Range(0f,1f)] float halfTimeValue;
    [SerializeField] AnimationCurve curve = new AnimationCurve(
                                                    new Keyframe(0f, 1f), 
                                                    new Keyframe(0.5f, 0.5f), 
                                                    new Keyframe(1f, 0f));

    public float MinTimerValue => 0f;
    public float MaxTimerValue => duration;

    public WallRunTimer(float duration, float halfTimeValue)
    {
        this.duration = duration;
        this.halfTimeValue = halfTimeValue;
        CreateCurve();
    }

    public void OnValidate()
    {
        CreateCurve();
    }

    private void CreateCurve()
    {
        this.curve = new AnimationCurve();

        curve.AddKey(new Keyframe(0f, 1f));
        curve.AddKey(new Keyframe(duration * .5f, halfTimeValue));
        curve.AddKey(new Keyframe(duration, 0f));

        ClampKeyTangents(0);
        ClampKeyTangents(1);
        ClampKeyTangents(2);
    }

    void ClampKeyTangents(int idx)
    {
        UnityEditor.AnimationUtility.SetKeyLeftTangentMode(curve, idx, UnityEditor.AnimationUtility.TangentMode.ClampedAuto);
        UnityEditor.AnimationUtility.SetKeyRightTangentMode(curve, idx, UnityEditor.AnimationUtility.TangentMode.ClampedAuto);
    }

    public float Evaluate(float t)
    {
        return curve.Evaluate(t);
    }

}