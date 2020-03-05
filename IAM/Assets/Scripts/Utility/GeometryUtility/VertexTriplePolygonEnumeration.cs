using System.Collections.Generic;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil
{
    public class VertexTriplePolygonEnumeration : EnumerablePolygon<VertexTripleIndices>
    {
        public VertexTriplePolygonEnumeration(IPolygon p) : base(p)
        {}

        public override IEnumerator<VertexTripleIndices> GetEnumerator()
        {
            for (int i = 0; i < polygon.VertexCount; i+=2)
            {
                int cur = i;
                int next = GeometryUtility.GetNextVertexIdx(polygon, cur);
                int prev = GeometryUtility.GetPrevVertexIdx(polygon, cur);
                yield return new VertexTripleIndices(cur, next, prev);
            }
        }
    }

}


