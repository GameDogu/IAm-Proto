using Unity.Mathematics;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Linear
{
    public struct Edge : IPolygonEdge
    {
        public Line Line { get; private set; }
        public int SPointIDX { get; private set; }
        public int EPointIDX { get; private set; }

        public float2 SPoint => Line.v0;

        public float2 EPoint => Line.v1;

        public Edge(Line line, int sPointIDX, int ePointIDX) : this()
        {
            Line = line;
            SPointIDX = sPointIDX;
            EPointIDX = ePointIDX;
        }

        public Edge(float2 sPoint, float2 ePoint, int sPointIDX, int ePointIDX)
        {
            Line = new Line(sPoint,ePoint);
            SPointIDX = sPointIDX;
            EPointIDX = ePointIDX;
        }

        public static Edge Create(float2 sPoint, float2 ePoint, int sPointIDX, int ePointIDX)
        {
            return new Edge(sPoint, ePoint, sPointIDX, ePointIDX);
        }
    }

}
