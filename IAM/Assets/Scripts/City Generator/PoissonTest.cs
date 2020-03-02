using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonTest : MonoBehaviour {

    [SerializeField]float radius = 1f;
    [SerializeField] Vector3 RegionSize = Vector2.one;
    [Range(0,100)]public int RejectionSamples = 30;
    [SerializeField] bool draw = true;
    [SerializeField] bool generate = false;
    [SerializeField] [Range(0f, 1f)] float decimationPercent=0f;

    [SerializeField] float displayRadius => radius * 0.5f;

    public List<Vector2> points = null;

    private void OnValidate()
    {
        if(generate)
            points = PoissonDiscSampling.GeneratePoints(radius, RegionSize.XZ(), RejectionSamples, decimationPercent);
    }

    private void Start()
    {
        decimationPercent = Random.value;
    }

    private void OnDrawGizmos()
    {
        if (!draw)
            return;

        Gizmos.color = Color.white;

        Gizmos.DrawWireCube(RegionSize/2, RegionSize);

        if (points != null)
        {
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(point.XValY(RegionSize.y), displayRadius);
            }
        }

    }
}
