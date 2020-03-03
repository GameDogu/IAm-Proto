using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Text;

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

        public float2 Centroid { get; protected set; }

        public VertexWinding VertexWinding { get; protected set; }

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

            CalculateNonSerializedData();
        }

        public Polygon(List<Vector2> vert)
        {
            vertices = new float2[vert.Count];
            for (int i = 0; i < vert.Count; i++)
            {
                vertices[i] = new float2(vert[i].x, vert[i].y);
            }
            CalculateNonSerializedData();
        }

        public Polygon(Vector2[] vert)
        {
            vertices = new float2[vert.Length];
            for (int i = 0; i < vert.Length; i++)
            {
                vertices[i] = new float2(vert[i].x, vert[i].y);
            }
            CalculateNonSerializedData();
        }

        protected Polygon()
        { }

        public Polygon(Polygon src)
        {
            CopyVertices(src.vertices);
            //CalculateBounds();
            //CalculateCentroid();
            Bounds = src.Bounds;
            Centroid = src.Centroid;
            VertexWinding = src.VertexWinding;
        }

        protected Polygon Pilfer(Polygon src, bool nullSrc = false)
        {
            Polygon p = new Polygon();
            p.vertices = src.vertices;
            p.Bounds = src.Bounds;
            p.Centroid = src.Centroid;
            p.VertexWinding = src.VertexWinding;

            if (nullSrc)
            {
                src.vertices = null;
                src.edges = null;
            }

            return p;
        }

        void CopyVertices(float2[] srcVert)
        {
            this.vertices = new float2[srcVert.Length];
            for (int i = 0; i < VertexCount; i++)
            {
                this.vertices[i] = srcVert[i];
            }
        }

        public float2 this[int i]
        {
            get { return vertices[i]; }
        }

        public void CalculateBounds()
        {
            Bounds = GeometryUtility.CalculateBounds(this);
        }

        public void CalculateWinding()
        {
            VertexWinding = GeometryUtility.GetOrientation(this);
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
            if(edges != null)
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

        protected void CalculateNonSerializedData()
        {
            CalculateBounds();
            CalculateCentroid();
            CalculateWinding();
        }

        public Mesh Triangulate()
        {
            throw new NotImplementedException();
        }

        public void Log(ILogger l)
        {
            Stringify(l.Log);
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            Stringify((s)=> b.Append(s),addNewLine:true);
            return b.ToString();
        }

        private void Stringify(Action<string> stringAction,bool addNewLine = false)
        {
            string nl = addNewLine ? "\n" : "";
            stringAction("Polygon:"+ nl);
            stringAction($"Winding: {VertexWinding}" + nl);
            stringAction($"Centroid: {Centroid}" + nl);
            stringAction($"Bounds:\n\tCenter: {Bounds.Center}\n\tExtends: {Bounds.Extents}" + nl);
            stringAction("Vertices:" + nl);
            for (int i = 0; i < VertexCount; i++)
            {
                stringAction($"{i}: {vertices[i]}" + nl);
            }
        }

    }
}


