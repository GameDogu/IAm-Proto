using System.Runtime.CompilerServices;
/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil.Linear
{
    public enum LinePosition
    {
        left = 1,
        on = 0,
        right = -1
    }

    public static class LinePositionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this LinePosition p)
        {
            return (int)p;
        }

        public static LinePosition ToLinePosition(this int i)
        {
            switch (i)
            {
                case int _ when i > 0:
                    return LinePosition.left;
                case int _ when i == 0:
                    return LinePosition.on;
                default:
                    return LinePosition.right;
            }
        }

    }
}



