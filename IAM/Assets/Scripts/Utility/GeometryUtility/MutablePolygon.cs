using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil
{
    public class MutablePolygon : Polygon
    {
        public MutablePolygon(float2[] vertices) : base(vertices)
        {}

        public MutablePolygon(List<Vector2> vert) : base(vert)
        {}

        public MutablePolygon(Vector2[] vert) : base(vert)
        {}

        public MutablePolygon(int vertCount)
        {
            vertices = new float2[vertCount];
        }

        public MutablePolygon(Polygon src) : base(src) { }

        public new float2 this[int i]
        {
            get => vertices[i];
            set { vertices[i] = value; InvalidateEdges(); }
        }

        public void Reverse()
        {
            vertices = vertices.Reverse().ToArray();
            InvalidateEdges();
        }

        public void RemoveRange(int beginIdx, int count,bool recalculateBounds=true)
        {
            var vert = vertices.ToList();
            vert.RemoveRange(beginIdx, count);
            vertices = vert.ToArray();
            CalculateBounds();
            InvalidateEdges();
        }

        public Polygon MakeUnmutable()
        {
            return Pilfer(src:this,nullSrc:true);
        }

    }
}
