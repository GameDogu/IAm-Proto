using System.Collections;
using System.Runtime.Serialization;
using Unity.Mathematics;
/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Vertex
{
    public enum VertexWinding
    {
        CW,
        CCW
    }

    public static class VertexWindingExtensions
    {
        public static VertexWinding Opposite(this VertexWinding w)
        {
            return (VertexWinding)(((int)w + 1) % 2);
        }
    }

}
