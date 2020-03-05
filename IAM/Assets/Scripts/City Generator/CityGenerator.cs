#pragma warning disable 649, 414
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GeoUtil;
using Unity.Mathematics;
using System;

public class CityGenerator : MonoBehaviour
{
    [SerializeField] Vector2 normalExtents = default;
    [SerializeField] Vector2[] deformedPoints = new Vector2[4];
    [SerializeField] Vector2 pointTest = Vector2.zero;
    [SerializeField] bool test = false;
    [SerializeField] bool draw = false;
    [SerializeField] bool drawTriangulation = false;
    [SerializeField] Transform normalTransform = default;
    [SerializeField] PoissonTest poissson= default;
    [SerializeField] MeshFilter polygonMeshDisplay;

    Color c;

    private void OnValidate()
    {
        c = Color.black;
    }

    private void OnDrawGizmos()
    {
        if (draw)
        {
            var poly = new Polygon(deformedPoints);
            Gizmos.color = Color.black;
            for (int i = 0; i < deformedPoints.Length; i++)
            {
                var v0 = deformedPoints[i];
                var v1 = deformedPoints[(i + 1) % deformedPoints.Length];

                Gizmos.DrawLine(transform.TransformPoint(v0.XValY(1f)), transform.TransformPoint(v1.XValY(1f)));
            }

            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(poly.Centroid.x, 1f, poly.Centroid.y)), .035f);

            Gizmos.color = c;

            Gizmos.DrawSphere(transform.TransformPoint(pointTest.XValY(1f)), .025f);
        }
        else if (drawTriangulation)
        {
            Polygon p = new Polygon(deformedPoints);

            var triangles = GeoUtil.GeometryUtility.Triangulate(p);

            Gizmos.color = Color.black;
            for (int i = 0; i < triangles.Count-2; i+=3)
            {
                float3 v0 = new float3(p[i],1f);
                float3 v1 = new float3(p[i + 1], 1f);
                float3 v2 = new float3(p[i + 2], 1f);

                Gizmos.DrawLine(v0,v1);
                Gizmos.DrawLine(v1,v2);
                Gizmos.DrawLine(v2,v1);
            }
        }
    }


    public void CalculateBounding()
    {
        c = GeoUtil.GeometryUtility.PolygonContains(pointTest, new Polygon(deformedPoints)) ? Color.green : Color.red;
    }

    public void CheckSelfInterseciton()
    {
        Debug.Log(new Polygon(deformedPoints).HasSelfIntersection);
    }

    public void CheckConvexity()
    {
        Debug.Log(new Polygon(deformedPoints).IsConvex);
    }

    public void CheckOrientation()
    {
        Debug.Log(GeoUtil.GeometryUtility.GetWinding(new Polygon(deformedPoints)));
    }

    public void LogPolygon()
    {
        Debug.Log(new Polygon(deformedPoints).ToString());
    }

    public void ChangeOrientation()
    {
        var p = new Polygon(deformedPoints);
        var np = GeoUtil.GeometryUtility.ChangeOrientation(p,p.VertexWinding.Opposite());
        Debug.Log(np.ToString());
    }
}

[CustomEditor(typeof(CityGenerator))]
public class CityGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CityGenerator gen = target as CityGenerator;
        if (GUILayout.Button("Calculate bounds"))
        {
            gen.CalculateBounding();
        }

        if (GUILayout.Button("Ceheck Self Intersection"))
        {
            gen.CheckSelfInterseciton();
        }

        if (GUILayout.Button("Check Convex"))
        {
            gen.CheckConvexity();
        }

        if (GUILayout.Button("Check Orientation"))
        {
            gen.CheckOrientation();
        }

        if (GUILayout.Button("Change Orientation"))
        {
            gen.ChangeOrientation();
        }
    }
}
