using System;
using Unity.Mathematics;
namespace GeoUtil.HelperCollections.Trees
{
    class QTNode<Dt>
    {
        public float2 Position { get; protected set; }
        public Dt Data { get; protected set; }

        public QTNode(float2 pos, Dt data)
        {
            this.Position = pos;
            this.Data = data;
        }

        public QTNode()
        {
            Data = default(Dt);
        }
    }

    class QTQuad<Dt>
    {
        Bounds2D bounds;

        QTNode<Dt> node;

        QTQuad<Dt> bottomLeftTree;
        QTQuad<Dt> bottomRightTree;
        QTQuad<Dt> topLeftTree;
        QTQuad<Dt> topRightTree;

        protected QTQuad()
        {
            node = default;
            bottomLeftTree = default;
            bottomRightTree = default;
            topLeftTree = default;
            topRightTree = default;
        }

        public QTQuad(Bounds2D bounds, QTNode<Dt> node, QTTree<Dt> tree):this()
        {
            this.bounds = bounds;
            this.node = node ?? throw new ArgumentNullException(nameof(node));
        }

        public QTQuad(Bounds2D bounds):this()
        {
            this.bounds = bounds;            
        }

        public QTQuad(float2 min, float2 max):this(new Bounds2D(min,max))
        {}

        public void Insert(QTNode<Dt> insertNode)
        {
            if (insertNode == null)//
                return;

            if (!Contains(insertNode.Position))//not our concern
                return;

            //minimum divison reached
            if ((math.abs(bounds.Max - bounds.Min) <= 1).ElemtAnd())
            {
                //if no data yet: this is our data
                if (node == null)
                    node = insertNode;
                return;
            }

            if (bounds.Center.x >= insertNode.Position.x)
            {
                //on left
                if (bounds.Center.y >= insertNode.Position.y)
                {
                    if (bottomLeftTree == null)
                    {
                        bottomLeftTree = new QTQuad<Dt>(
                            bounds.Min, bounds.Center);
                    }
                    bottomLeftTree.Insert(insertNode);
                }
                else
                {
                    if (topLeftTree == null)
                    {
                        topLeftTree = new QTQuad<Dt>(
                            new float2(bounds.Min.x, bounds.Center.y),
                            new float2(bounds.Center.x, bounds.Max.y));
                    }
                    topLeftTree.Insert(insertNode);
                }
            }
            else
            {
                //on right
                if (bounds.Center.y >= insertNode.Position.y)
                {
                    if (bottomRightTree == null)
                    {
                        bottomRightTree = new QTQuad<Dt>(
                            new float2(bounds.Center.x, bounds.Min.y),
                            new float2(bounds.Max.x, bounds.Center.y));
                    }
                    bottomRightTree.Insert(insertNode);
                }
                else
                {
                    if (topRightTree == null)
                    {
                        topRightTree = new QTQuad<Dt>(bounds.Center, bounds.Max);
                    }
                    topRightTree.Insert(insertNode);
                }
            }
        }

        public QTNode<Dt> Search(float2 point)
        {
            if (!Contains(point))
                return null;
            //unit quad return data
            if (node != null)
                return node;

            if (bounds.Center.x > point.x)
            {
                //left
                if (bounds.Center.y >= point.y)
                {
                    if (bottomLeftTree == null)
                        return null;
                    return bottomLeftTree.Search(point);
                }
                else
                {
                    if (topLeftTree == null)
                        return null;
                    return topLeftTree.Search(point);
                }
            }
            else
            {
                //right
                if (bounds.Center.y >= point.y)
                {
                    if (bottomRightTree == null)
                        return null;
                    return bottomRightTree.Search(point);
                }
                else
                {
                    if (topRightTree == null)
                        return null;
                    return topRightTree.Search(point);
                }
            }
        }

        public bool Contains(float2 p)
        {
            return bounds.Contains(p);
        }
    }

    public class QTTree<Dt>
    {
        public float scale;
        QTQuad<Dt> root;

        public QTTree(Bounds2D bounds, float scale = 1f)
        {
            this.scale = scale;
            root = new QTQuad<Dt>(bounds.Scale(scale));
        }

        public void Insert(float2 position, Dt data)
        {
            root.Insert(new QTNode<Dt>(position * scale, data));
        }

        public (float2 pos, Dt data)? Search(float2 point)
        {
            var found = root.Search(point);
            if (found != null)
            {
                return (found.Position / scale, found.Data);
            }
            return null;
        }
    }

}
