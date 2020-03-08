using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace GeoUtil.HelperCollections
{
    public class PolygonVertexIDXCell : Cell<float2, int>
    {
        const int ERROR_IDX = -1;
        public const float DistErrorRad = 0.0001f;
        List<(float2 v, int idx)> verticesInCell;
        float distanceErrorRadius;

        public PolygonVertexIDXCell(List<(float2 v, int idx)> verticesInCell, float distanceErrorRadius)
        {
            this.verticesInCell = verticesInCell ?? throw new ArgumentNullException(nameof(verticesInCell));
            this.distanceErrorRadius = distanceErrorRadius;
        }

        public PolygonVertexIDXCell(float distanceErrorRadius = DistErrorRad)
        {
            verticesInCell = new List<(float2 v, int idx)>();
            this.distanceErrorRadius = distanceErrorRadius;
        }

        public PolygonVertexIDXCell(float2 firstVert, int firstIDX, float distanceErrorRadius = DistErrorRad) : this(distanceErrorRadius)
        {
            Add(firstVert, firstIDX);
        }

        public override int GetValue(float2 _in)
        {
            return FindIDX(_in);
        }

        int FindIDX(float2 v)
        {
            for (int i = 0; i < verticesInCell.Count; i++)
            {
                var item = verticesInCell[i];
                if (math.distancesq(item.v, v) <= distanceErrorRadius)
                    return item.idx;
            }
            return ERROR_IDX;
        }

        public void Add(float2 vert, int idx)
        {
            verticesInCell.Add((vert, idx));
        }
    }

    public class PolygonVertexIDXHelperGrid : Float2HelperGrid<PolygonVertexIDXCell, int>
    {
        IPolygon poly;

        Dictionary<int2, PolygonVertexIDXCell> cells;

        public PolygonVertexIDXHelperGrid(IPolygon p, float resolution) : base(resolution)
        {
            poly = p;
            cells = new Dictionary<int2, PolygonVertexIDXCell>();
            BuildGrid();
        }

        private void BuildGrid()
        {
            for (int i = 0; i < poly.VertexCount; i++)
            {
                var vert = poly[i];
                var gridIDX = GetCellPosition(vert);
                if (cells.ContainsKey(gridIDX))
                {
                    cells[gridIDX].Add(vert, i);
                }
                else
                {
                    cells.Add(gridIDX, new PolygonVertexIDXCell(vert, i));
                }
            }
        }

        public override PolygonVertexIDXCell GetCell(float2 _in)
        {
            return cells[GetCellPosition(_in)];
        }

        public override int GetValue(float2 _in)
        {
            return GetCell(_in).GetValue(_in);
        }

    }
}
