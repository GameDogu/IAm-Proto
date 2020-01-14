#pragma warning disable 649
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformInformation : MonoBehaviour
{
    [System.Serializable]
    struct AxisInfo
    {
        [SerializeField] bool drawAxis;
        [SerializeField] Color color;
        [SerializeField] float length;
        public static implicit operator bool(AxisInfo info) => info.drawAxis;
        public static implicit operator Color(AxisInfo info) => info.color;
        public static implicit operator float(AxisInfo info) => info.length;
    }

    [SerializeField] AxisInfo right, up, forward;
    [SerializeField]Transform infoToDisplay = null;

    private void Reset()
    {
        if (infoToDisplay == null)
        {
            infoToDisplay = transform.parent;
        }
    }

    void DrawAxis(AxisInfo info, Vector3 axis)
    {
        Gizmos.color = info;
        Gizmos.DrawLine(infoToDisplay.localPosition, infoToDisplay.localPosition + axis * info);
    }

    private void OnDrawGizmos()
    {
        if (infoToDisplay == null)
            return;
        if (right)
        {
            DrawAxis(right, infoToDisplay.right);
        }
        if (up)
        {
            DrawAxis(up, infoToDisplay.up);
        }
        if (forward)
        {
            DrawAxis(forward, infoToDisplay.forward);
        }
    }
}
