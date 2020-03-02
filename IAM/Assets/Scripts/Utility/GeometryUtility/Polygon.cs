using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil
{
    public class Polygon
    {

        protected float2[] vertices;

        public Bounds2D Bounds { get; protected set; }

        public int VertexCount => vertices.Length;

        public float2 Centroid;

        protected List<Edge> edges;
        private bool edgesAreValid = false;
        public List<Edge> Edges
        {
            get
            {
                if (!edgesAreValid)
                    CalculateEdges();
                return edges;
            }
        }

        public bool HasSelfIntersection => GeometryUtility.CheckPolygonSelfIntersection(this);

        public bool IsConvex => GeometryUtility.CheckPolygonConvex(this);

        public Polygon(float2[] vertices)
        {
            this.vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));

            CalculateBounds();
            CalculateCentroid();
        }

        public Polygon(List<Vector2> vert)
        {
            vertices = new float2[vert.Count];
            for (int i = 0; i < vert.Count; i++)
            {
                vertices[i] = new float2(vert[i].x, vert[i].y);
            }
            CalculateBounds();
            CalculateCentroid();
        }

        public Polygon(Vector2[] vert)
        {
            vertices = new float2[vert.Length];
            for (int i = 0; i < vert.Length; i++)
            {
                vertices[i] = new float2(vert[i].x, vert[i].y);
            }
            CalculateBounds();
            CalculateCentroid();
        }

        protected Polygon()
        { }

        public Polygon(Polygon src)
        {
            this.vertices = new float2[src.VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                this.vertices[i] = src[i];
            }
            //CalculateBounds();
            //CalculateCentroid();
            Bounds = src.Bounds;
            Centroid = src.Centroid;
        }

        public float2 this[int i]
        {
            get { return vertices[i]; }
        }

        public void CalculateBounds()
        {
            Bounds = GeometryUtility.CalculateBounds(this);
        }

        protected void CalculateEdges()
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                int idxNext = (i + 1) % vertices.Length;
                var vCurrent = vertices[i];
                var vNext = vertices[idxNext];
                edges.Add(new Edge(vCurrent, vNext, i, idxNext));
            }
            edgesAreValid = true;
        }

        protected void InvalidateEdges()
        {
            edges.Clear();
            edgesAreValid = false;
        }

        protected void CalculateCentroid()
        {
            Centroid = float2.zero;
            for (int i = 0; i < VertexCount; i++)
            {
                Centroid += vertices[i];
            }
            Centroid /= (float)VertexCount;
        }

        public Mesh Triangulate()
        {
            throw new NotImplementedException();
        }
    }
}


