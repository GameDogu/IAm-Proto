using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace GeoUtil.HelperGrid
{
    public abstract class HelperGrid<C, I, R> where C : Cell<I, R>
    {
        protected float resolution;

        protected HelperGrid(float resolution)
        {
            this.resolution = resolution;
        }

        public abstract C GetCell(I _in);
        public abstract R GetValue(I _in);
    }

    public abstract class Cell<I, R>
    {
        public abstract R GetValue(I _in);
    }

    public abstract class Float2HelperGrid<C, R> : HelperGrid<C, float2, R>
        where C : Cell<float2, R>
    {
        protected Float2HelperGrid(float resolution) : base(resolution)
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int2 GetCellPosition(float2 v)
        {
            return (int2)math.floor(v / resolution);
        }
    }
   
}