using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using GeoUtil.Polygons;
using GeoUtil.Vertex;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.HelperCollections.Enumerables
{
    public abstract class EnumerablePolygon< E> : IPolygon, IEnumerable<E>
    {
        protected IPolygon polygon;

        public EnumerablePolygon( IPolygon p)
        {
            polygon = p;
        }

        public float2 this[int i] => polygon[i];

        public int VertexCount => polygon.VertexCount;

        public Bounds2D Bounds => polygon.Bounds;

        public VertexWinding VertexWinding => polygon.VertexWinding;

        public abstract IEnumerator<E> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

}


