using Unity.Mathematics;

namespace GeoUtil.HelperCollections.Grids
{
    public abstract class RebasingHelperGrid<C, R> : Float2HelperGrid<C, R>
    where C : Cell<float2, R>
    {
        protected float2 origin;

        public RebasingHelperGrid(float2 origin, float resolution) : base(resolution)
        {
            this.origin = origin;
        }

        protected override int2 GetCellPosition(float2 v)
        {
            return base.GetCellPosition(v - origin);
        }
    }
}