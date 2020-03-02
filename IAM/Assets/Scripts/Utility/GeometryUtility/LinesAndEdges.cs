using Unity.Mathematics;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil
{
    public struct EdgePair<T> where T: IPolygonEdge 
    {
        public T e0;
        public T e1;

        public EdgePair(T e0, T e1)
        {
            this.e0 = e0;
            this.e1 = e1;
        }
    }

    public struct LineSegment
    {
        public float2 SPoint { get; private set; }
        public float2 EPoint { get; private set; }

        public float3 Line => GeometryUtility.CalcHomogeneousLine(this);

        public LineSegment(float2 sPoint, float2 ePoint) : this()
        {
            SPoint = sPoint;
            EPoint = ePoint;
        }
    }

    public struct Edge : IPolygonEdge
    {
        public LineSegment Line { get; private set; }
        public int SPointIDX { get; private set; }
        public int EPointIDX { get; private set; }

        public float2 SPoint => Line.SPoint;

        public float2 EPoint => Line.EPoint;

        public Edge(LineSegment line, int sPointIDX, int ePointIDX) : this()
        {
            Line = line;
            SPointIDX = sPointIDX;
            EPointIDX = ePointIDX;
        }

        public Edge(float2 sPoint, float2 ePoint, int sPointIDX, int ePointIDX)
        {
            Line = new LineSegment(sPoint,ePoint);
            SPointIDX = sPointIDX;
            EPointIDX = ePointIDX;
        }
    }

    public interface ILineSegmentContainer
    {
        LineSegment Line { get; }
    }

    public interface IPolygonEdge : ILineSegmentContainer
    {
        float2 SPoint { get; }
        float2 EPoint { get; }
        int SPointIDX { get; }
        int EPointIDX { get; }
    }

}
