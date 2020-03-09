using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace GeoUtil.HelperCollections.Grids
{
    public abstract class Float2HelperGrid<C, R> : HelperGrid<C, float2, R>
        where C : Cell<float2, R>
    {
        protected Float2HelperGrid(float resolution) : base(resolution)
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual int2 GetCellPosition(float2 v)
        {
            return (int2)math.floor(v / resolution);
        }
    }
}