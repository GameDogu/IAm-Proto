/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Vertex
{
    public struct VertexTripleIndices
    {
        public int prevIdx;
        public int curIdx;
        public int nextIdx;

        public VertexTripleIndices(int prevIdx, int curIdx, int nextIdx)
        {
            this.prevIdx = prevIdx;
            this.curIdx = curIdx;
            this.nextIdx = nextIdx;
        }
    }

}
