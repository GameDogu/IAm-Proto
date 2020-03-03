using System.Collections.Generic;
using Unity.Mathematics;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil
{
    public interface IPolygon
    {
        float2 this[int i]
        {
            get;
        }

        int VertexCount { get; }

        Bounds2D Bounds { get; }

        VertexWinding VertexWinding { get; }

        List<IPolygonEdge> Edges { get; }
    }
}


