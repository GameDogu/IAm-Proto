using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Mathematics;
using GeoUtil.Vertex;
/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Polygons
{
    public class Polygon : IPolygon
    {
        protected float2[] vertices;

        public Bounds2D Bounds { get; protected set; }

        public int VertexCount => vertices.Length;

        public float2 Centroid { get; protected set; }

        public VertexWinding VertexWinding { get; protected set; }

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

        public Polygon(IPolygon src)
        {
            CopyVertices(src);
            //CalculateBounds();
            //CalculateCentroid();
            Bounds = src.Bounds;
            Centroid = GeometryUtility.CalculateCentroid(this);
            VertexWinding = src.VertexWinding;
        }

        protected Polygon Pilfer(Polygon src, bool nullSrc = false)
        {
            Polygon p = new Polygon();
            p.vertices = src.vertices;
            p.Bounds = src.Bounds;
            p.Centroid = src.Centroid;
            p.VertexWinding = src.VertexWinding;
            return p;
        }

        void CopyVertices(IPolygon src)
        {
            this.vertices = new float2[src.VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                this.vertices[i] = src[i];
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
            VertexWinding = GeometryUtility.GetWinding(this);
        }

        protected void CalculateCentroid()
        {
            Centroid = GeometryUtility.CalculateCentroid(this);
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


