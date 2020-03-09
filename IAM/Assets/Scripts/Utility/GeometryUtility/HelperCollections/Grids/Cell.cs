namespace GeoUtil.HelperCollections.Grids
{
    public abstract class Cell<I, R>
    {
        public abstract R GetValue(I _in);
    }
}