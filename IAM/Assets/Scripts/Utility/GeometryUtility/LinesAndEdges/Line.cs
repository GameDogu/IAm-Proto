using Unity.Mathematics;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Linear
{
    public struct Line
    {
        public float2 v0 { get; private set; }
        public float2 v1 { get; private set; }

        public float3 HomogenousLineCoords => GeometryUtility.CalcHomogeneousLine(this);

        public Line(float2 sPoint, float2 ePoint) : this()
        {
            v0 = sPoint;
            v1 = ePoint;
        }
    }

}
