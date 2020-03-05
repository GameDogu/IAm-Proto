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

    public struct Line
    {
        public float2 SPoint { get; private set; }
        public float2 EPoint { get; private set; }

        public float3 HomogenousLineCoords => GeometryUtility.CalcHomogeneousLine(this);

        public Line(float2 sPoint, float2 ePoint) : this()
        {
            SPoint = sPoint;
            EPoint = ePoint;
        }
    }

    public struct Edge : IPolygonEdge
    {
        public Line Line { get; private set; }
        public int SPointIDX { get; private set; }
        public int EPointIDX { get; private set; }

        public float2 SPoint => Line.SPoint;

        public float2 EPoint => Line.EPoint;

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

    public interface ILineContainer
    {
        Line Line { get; }
    }

    public interface IPolygonEdge : ILineContainer
    {
        float2 SPoint { get; }
        float2 EPoint { get; }
        int SPointIDX { get; }
        int EPointIDX { get; }
    }


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
