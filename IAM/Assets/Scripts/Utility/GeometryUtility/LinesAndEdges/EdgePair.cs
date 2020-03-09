/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Linear
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

}
