using System.Collections.Generic;
using System;

namespace GeoUtil.HelperCollections.Grids
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
}