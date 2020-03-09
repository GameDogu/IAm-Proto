using System;
/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Exceptions
{
    public class MalforemdPolygonException : Exception
    {
        public MalforemdPolygonException() : base()
        {
        }

        public MalforemdPolygonException(string malformationType) : base($"This polygon is malformed ({malformationType}), most algorithms wont work with it")
        {
        }
    }
}
