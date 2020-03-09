using Unity.Mathematics;
using GeoUtil.Vertex;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Polygons
{
    public struct Triangle : IPolygon
    {
        public float2 A { get; private set; }
        public float2 B { get; private set; }
        public float2 C { get; private set; }

        public float2 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return A;
                    case 1:
                        return B;
                    case 2:
                        return C;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }

        public int VertexCount => 3;

        public Bounds2D Bounds { get; private set; }

        public VertexWinding VertexWinding { get; private set; }

        public Triangle(float2 a, float2 b, float2 c) : this()
        {
            this.A = a;
            this.B = b;
            this.C = c;
            Bounds = GeometryUtility.CalculateBounds(this);
            VertexWinding = GeometryUtility.GetWinding(this);
        }
    }

}
