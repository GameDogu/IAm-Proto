using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil
{
    public struct Bounds2D
    {
        public (float min, float max) X { get; private set; }
        public (float min, float max) Y { get; private set; }

        public float2 Min => new float2(X.min, Y.min);

        public float2 Max => new float2(X.max, Y.max);

        public float2 Size { get; private set; }

        public float2 Center { get; private set; }

        public float2 Extents { get; private set; }

        public Bounds2D((float min, float max) x, (float min, float max) y) : this()
        {
            X = x;
            Y = y;
            Center = new float2(Mathf.Lerp(X.min, X.max, 0.5f), Mathf.Lerp(Y.min, Y.max, 0.5f));

            Size = new float2(X.max - X.min, Y.max - Y.min);
            Extents = Size * 0.5f;
        }

        public bool Contains(float2 p)
        {
            return (p >= Min).ElemtAnd() && (p <= Max).ElemtAnd();
        }

    }    
}
