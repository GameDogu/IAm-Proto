using Unity.Mathematics;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Linear
{
    public interface IPolygonEdge : ILineContainer
    {
        float2 SPoint { get; }
        float2 EPoint { get; }
        int SPointIDX { get; }
        int EPointIDX { get; }
    }

}
