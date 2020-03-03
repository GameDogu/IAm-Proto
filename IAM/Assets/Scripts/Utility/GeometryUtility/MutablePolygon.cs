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
        bool manualNonSerializedDataUpdate;

        public MutablePolygon(float2[] vertices, bool manualNonSerializedDataUpdate = false) : base(vertices)
        {
            this.manualNonSerializedDataUpdate = manualNonSerializedDataUpdate;
        }

        public MutablePolygon(List<Vector2> vert, bool manualNonSerializedDataUpdate = false) : base(vert)
        {
            this.manualNonSerializedDataUpdate = manualNonSerializedDataUpdate;
        }

        public MutablePolygon(Vector2[] vert, bool manualNonSerializedDataUpdate = false) : base(vert)
        {
            this.manualNonSerializedDataUpdate = manualNonSerializedDataUpdate;
        }

        public MutablePolygon(int vertCount, bool manualNonSerializedDataUpdate = false)
        {
            vertices = new float2[vertCount];
            this.manualNonSerializedDataUpdate = manualNonSerializedDataUpdate;
        }

        public MutablePolygon(Polygon src, bool manualNonSerializedDataUpdate = false) : base(src)
        {
            this.manualNonSerializedDataUpdate = manualNonSerializedDataUpdate;
        }

        public new float2 this[int i]
        {
            get => vertices[i];
            set
            {
                vertices[i] = value;
                InvalidateEdges();
                TryUpdateDataU();
            }
        }

        private void TryUpdateDataU()
        {
            if (!manualNonSerializedDataUpdate)
                CalculateNonSerializedData();
        }

        public void Reverse()
        {
            vertices = vertices.Reverse().ToArray();
            InvalidateEdges();
            TryUpdateDataU();
        }

        public void RemoveRange(int beginIdx, int count,bool recalculateBounds=true)
        {
            var vert = vertices.ToList();
            vert.RemoveRange(beginIdx, count);
            vertices = vert.ToArray();
            TryUpdateDataU();
            InvalidateEdges();
        }

        public Polygon MakeUnmutable(bool updateNonSerializedData = true)
        {
            if (updateNonSerializedData)
                CalculateNonSerializedData();
            return Pilfer(src:this,nullSrc:true);
        }

    }
}
