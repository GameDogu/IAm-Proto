using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using Priority_Queue;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil
{

    public static class GeometryUtility
    {
        const float PriorityQueuePercision = 1000f;

        /// <summary>
        /// calculate point p is left right or on a line formed by lp0 to lp1
        /// </summary>
        /// <param name="lP0">the first point forming the line</param>
        /// <param name="lP1">the second point of the line</param>
        /// <param name="p">the point where whe want to know the relative position to the line of</param>
        /// <returns>the line position</returns>
        public static LinePosition CalculateLinePosition(float2 lP0, float2 lP1, float2 p)
        {
            float val = ((lP1.x - lP0.x) * (p.y - lP0.y) -
                            (p.x - lP0.x) * (lP1.y - lP0.y));

            return ((int)(Mathf.Sign(val) * Mathf.Ceil(Mathf.Abs(val)))).ToLinePosition();
        }

        /// <summary>
        /// calculate if the points form a counter clockwise angle
        /// </summary>
        /// <param name="p0">the first point forming the line</param>
        /// <param name="p1">the second point of the line</param>
        /// <param name="p3">the point where whe want to know the relative position to the line of</param>
        /// <returns>1 if ccw, 0 colinear, -1 if cw</returns>
        public static int CalculateCCW(float2 p0, float2 p1, float2 p3)
        {
            return CalculateLinePosition(p0, p1, p3).ToInt();
        }

        /// <summary>
        /// check if edge pair intersects
        /// </summary>
        /// <param name="p">the edge pair to check</param>
        /// <returns>true if intersect</returns>
        //https://algs4.cs.princeton.edu/91primitives/
        public static bool Intersects(EdgePair p)
        {
            if (CalculateCCW(p.e0.v0, p.e0.v1, p.e1.v0) * CalculateCCW(p.e0.v0, p.e0.v1, p.e1.v1) > 0) return false;
            if (CalculateCCW(p.e1.v0, p.e1.v1, p.e0.v0) * CalculateCCW(p.e1.v0, p.e1.v1, p.e0.v1) > 0) return false;
            return true;
        }

        /// <summary>
        /// checks if a point is in a certain relational position to a line
        /// </summary>
        /// <param name="lP0">line point 1</param>
        /// <param name="lP1">line pont 2</param>
        /// <param name="p">point to check</param>
        /// <param name="posToCheckAgains">the line position we want to check against</param>
        /// <returns>true if point hast that position</returns>
        private static bool CheckPointPositionAgainstLine(float2 lP0, float2 lP1, float2 p, LinePosition posToCheckAgains)
        {
            return CalculateLinePosition(lP0, lP1, p) == posToCheckAgains;
        }

        /// <summary>
        /// check if p is left of line
        /// </summary>
        /// <param name="lP0">line point 1</param>
        /// <param name="lP1">line pont 2</param>
        /// <param name="p">point to check</param>
        /// <returns>true if left of line</returns>
        public static bool IsLeftOfLine(float2 lP0, float2 lP1, float2 p)
        {
            return CheckPointPositionAgainstLine(lP0, lP1, p, LinePosition.left);
        }

        /// <summary>
        /// check if p is right of line
        /// </summary>
        /// <param name="lP0">line point 1</param>
        /// <param name="lP1">line pont 2</param>
        /// <param name="p">point to check</param>
        /// <returns>true if right of line</returns>
        public static bool IsRightOfLine(float2 lP0, float2 lP1, float2 p)
        {
            return CheckPointPositionAgainstLine(lP0, lP1, p, LinePosition.right);
        }

        /// <summary>
        /// check if p is on of line
        /// </summary>
        /// <param name="lP0">line point 1</param>
        /// <param name="lP1">line pont 2</param>
        /// <param name="p">point to check</param>
        /// <returns>true if on of line</returns>
        public static bool IsOnLine(float2 lP0, float2 lP1, float2 p)
        {
            return CheckPointPositionAgainstLine(lP0, lP1, p, LinePosition.on);
        }


        /// <summary>
        /// winding number point to polygon
        /// </summary>
        /// <param name="p">point to test</param>
        /// <param name="polygon">polygon to test against</param>
        /// <returns> == 0 iff outside</returns>
        public static int WindingNumberPointInPolygon(float2 p, Polygon polygon)
        {
            int windingNumber = 0;
            foreach (var (edgeVert0, edgeVert1) in polygon)
            {
                if (edgeVert0.y <= p.y && edgeVert1.y > p.y)
                {
                    //upward crossing
                    if (IsLeftOfLine(edgeVert0, edgeVert1, p))
                        windingNumber++;

                }
                else if (edgeVert1.y <= p.y)
                {
                    //downward crossing
                    if (IsRightOfLine(edgeVert0, edgeVert1, p))
                        windingNumber--;
                }
            }
            return windingNumber;
        }

        /// <summary>
        /// checks if a polygon contains a point
        /// </summary>
        /// <param name="p">the point</param>
        /// <param name="polygon">the polygon</param>
        /// <returns>true if contained</returns>
        public static bool PolygonContains(float2 p, Polygon polygon)
        {
            return polygon.Bounds.Contains(p) && WindingNumberPointInPolygon(p, polygon) != 0;
        }

        /// <summary>
        /// calculates bounds of polygon
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Bounds2D CalculateBounds(Polygon p)
        {
            var xBounds = (min: float.MaxValue, max: float.MinValue);
            var yBounds = (min: float.MaxValue, max: float.MinValue);

            for (int i = 0; i < p.VertexCount; i++)
            {
                var v = p[i];

                minMaxUpdate(v.x, ref xBounds);
                minMaxUpdate(v.y, ref yBounds);

            }

            return new Bounds2D(xBounds, yBounds);

            void minMaxUpdate(float value, ref (float min, float max) tracker)
            {
                if (value < tracker.min)
                    tracker.min = value;
                if (value > tracker.max)
                    tracker.max = value;
            }
        }

        /// <summary>
        /// checks a polygon for self intersections
        /// </summary>
        /// <param name="p">the poolygon to check</param>
        /// <returns></returns>
        public static bool CheckPolygonSelfIntersection(Polygon p)
        {
            return SelfIntersection(p) > 0 ? true : false;
        }

        /// <summary>
        /// triangulate via ear clipping
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static List<int> Triangulate(Polygon p)
        {
            var triIdxs = new List<int>();
            MutablePolygon muteablePoly = new MutablePolygon(p);

            while (muteablePoly.VertexCount > 3)
            {
                var ear = FindEar(muteablePoly);
                triIdxs.AddRange(ear);
                RemoveEar(muteablePoly, ear);
            }

            return triIdxs;
        }

        /// <summary>
        /// removes an ear from a mutable polygon
        /// </summary>
        /// <param name="poly">the polygon we remove the ear from</param>
        /// <param name="ear">the ear indices we want to remove</param>
        private static void RemoveEar(MutablePolygon poly, int[] ear)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// finds an ear of a polygon
        /// </summary>
        /// <param name="poly"></param>
        /// <returns>a list of indices of vertixces taht form an ear</returns>
        private static int[] FindEar(MutablePolygon poly)
        {
            int i = 0;
            int[] indices = default;
            bool earNotFound = true;
            while (earNotFound)
            {
                indices = l_CalcIndex(i);

                if (convexVertex(indices[0], indices[1], indices[2]))
                {
                    int prevOneOver = indices[0] - 1 < 0 ? poly.VertexCount - 1 : indices[0] - 1;
                    int nextOneOver = (indices[2] + 1) % poly.VertexCount;

                    if (!concaveVertex(prevOneOver, indices[0], indices[1]) &&
                       !concaveVertex(indices[0], indices[1], indices[2]) &&
                       !concaveVertex(indices[1], indices[2], nextOneOver))
                    {
                        earNotFound = false;
                    }
                }
                if (earNotFound)
                    i++;

            }
            return indices;
            int[] l_CalcIndex(int idx)
            {
                int prev = idx - 1 < 0 ? poly.VertexCount - 1 : idx - 1;
                int next = (idx + 1) % poly.VertexCount;
                return new int[] { prev, idx, next };
            }

            bool concaveVertex(int prev, int curr, int next)
            {
                Debug.Log("TODO");
                return false;
            }

            bool convexVertex(int prev, int curr, int next)
            {
                Debug.Log("TODO");
                return false;
            }

        }


        /// <summary>
        /// checks if polygon is convex
        /// </summary>
        /// <param name="polygon">the polygon to check</param>
        /// <returns>true if convex</returns>
        public static bool CheckPolygonConvex(Polygon polygon)
        {
            return IsConvex(polygon) > 0;
        }

        #region adapted from https://gist.github.com/KvanTTT/3855122
        /// <summary>
        /// adapted from https://gist.github.com/KvanTTT/3855122
        /// </summary>
        /// <param name="polygon">the polygon to check</param>
        /// <returns>-1 if no intersect 1 otherwise</returns>
        static int SelfIntersection(Polygon polygon)
        {
            if (polygon.VertexCount < 3)
                return 0;
            int High = polygon.VertexCount - 1;
            float2 O = new float2();
            int i;
            for (i = 0; i < High; i++)
            {
                for (int j = i + 2; j < High; j++)
                {
                    if (LineIntersect(polygon[i], polygon[i + 1],
                                      polygon[j], polygon[j + 1], ref O) == 1)
                        return 1;
                }
            }
            for (i = 1; i < High - 1; i++)
                if (LineIntersect(polygon[i], polygon[i + 1], polygon[High], polygon[0], ref O) == 1)
                    return 1;
            return -1;
        }

        /// <summary>
        /// adapted from https://gist.github.com/KvanTTT/3855122
        /// </summary>
        static float Square(Polygon polygon)
        {
            float S = 0;
            if (polygon.VertexCount >= 3)
            {
                for (int i = 0; i < polygon.VertexCount - 1; i++)
                    S += PMSquare(polygon[i], polygon[i + 1]);
                S += PMSquare(polygon[polygon.VertexCount - 1], polygon[0]);
            }
            return S;
        }

        /// <summary>
        /// adapted from https://gist.github.com/KvanTTT/3855122
        /// </summary>
        static int IsConvex(Polygon Polygon)
        {
            if (Polygon.VertexCount >= 3)
            {
                if (Square(Polygon) > 0)
                {
                    for (int i = 0; i < Polygon.VertexCount - 2; i++)
                        if (PMSquare(Polygon[i], Polygon[i + 1], Polygon[i + 2]) < 0)
                            return -1;
                    if (PMSquare(Polygon[Polygon.VertexCount - 2], Polygon[Polygon.VertexCount - 1], Polygon[0]) < 0)
                        return -1;
                    if (PMSquare(Polygon[Polygon.VertexCount - 1], Polygon[0], Polygon[1]) < 0)
                        return -1;
                }
                else
                {
                    for (int i = 0; i < Polygon.VertexCount - 2; i++)
                        if (PMSquare(Polygon[i], Polygon[i + 1], Polygon[i + 2]) > 0)
                            return -1;
                    if (PMSquare(Polygon[Polygon.VertexCount - 2], Polygon[Polygon.VertexCount - 1], Polygon[0]) > 0)
                        return -1;
                    if (PMSquare(Polygon[Polygon.VertexCount - 1], Polygon[0], Polygon[1]) > 0)
                        return -1;
                }
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// adapted from https://gist.github.com/KvanTTT/3855122
        /// </summary>
        static bool Intersect(Polygon polygon, int vertex1Ind, int vertex2Ind, int vertex3Ind)
        {
            float s1, s2, s3;
            for (int i = 0; i < polygon.VertexCount; i++)
            {
                if ((i == vertex1Ind) || (i == vertex2Ind) || (i == vertex3Ind))
                    continue;
                s1 = PMSquare(polygon[vertex1Ind], polygon[vertex2Ind], polygon[i]);
                s2 = PMSquare(polygon[vertex2Ind], polygon[vertex3Ind], polygon[i]);
                if (((s1 < 0) && (s2 > 0)) || ((s1 > 0) && (s2 < 0)))
                    continue;
                s3 = PMSquare(polygon[vertex3Ind], polygon[vertex1Ind], polygon[i]);
                if (((s3 >= 0) && (s2 >= 0)) || ((s3 <= 0) && (s2 <= 0)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// adapted from https://gist.github.com/KvanTTT/3855122
        /// </summary>
        static float PMSquare(float2 p1, float2 p2)
        {
            return (p2.x * p1.y - p1.x * p2.y);
        }

        /// <summary>
        /// adapted from https://gist.github.com/KvanTTT/3855122
        /// </summary>
        static float PMSquare(float2 p1, float2 p2, float2 p3)
        {
            return (p3.x - p1.x) * (p2.y - p1.y) - (p2.x - p1.x) * (p3.y - p1.y);
        }

        /// <summary>
        /// adapted from https://gist.github.com/KvanTTT/3855122
        /// </summary>
        static int LineIntersect(float2 A1, float2 A2, float2 B1, float2 B2, ref float2 O)
        {
            float a1 = A2.y - A1.y;
            float b1 = A1.x - A2.x;
            float d1 = -a1 * A1.x - b1 * A1.y;
            float a2 = B2.y - B1.y;
            float b2 = B1.x - B2.x;
            float d2 = -a2 * B1.x - b2 * B1.y;
            float t = a2 * b1 - a1 * b2;

            if (t == 0)
                return -1;

            O.y = (a1 * d2 - a2 * d1) / t;
            O.x = (b2 * d1 - b1 * d2) / t;

            if (A1.x > A2.x)
            {
                if ((O.x < A2.x) || (O.x > A1.x))
                    return 0;
            }
            else
            {
                if ((O.x < A1.x) || (O.x > A2.x))
                    return 0;
            }

            if (A1.y > A2.y)
            {
                if ((O.y < A2.y) || (O.y > A1.y))
                    return 0;
            }
            else
            {
                if ((O.y < A1.y) || (O.y > A2.y))
                    return 0;
            }

            if (B1.x > B2.x)
            {
                if ((O.x < B2.x) || (O.x > B1.x))
                    return 0;
            }
            else
            {
                if ((O.x < B1.x) || (O.x > B2.x))
                    return 0;
            }

            if (B1.y > B2.y)
            {
                if ((O.y < B2.y) || (O.y > B1.y))
                    return 0;
            }
            else
            {
                if ((O.y < B1.y) || (O.y > B2.y))
                    return 0;
            }

            return 1;
        }
        #endregion
    }

    public enum LinePosition
    {
        left = 1,
        on = 0,
        right = -1
    }

    public static class LinePositionExtensions
    {
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

    public class Polygon : IEnumerable<(float2 v0, float2 v1)>
    {
        protected float2[] vertices;

        public Bounds2D Bounds { get; protected set; }

        public int VertexCount => vertices.Length;

        public bool HasSelfIntersection => GeometryUtility.CheckPolygonSelfIntersection(this);

        public bool IsConvex => GeometryUtility.CheckPolygonConvex(this);

        public Polygon(float2[] vertices)
        {
            this.vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));

            CalculateBounds();
        }

        public Polygon(List<Vector2> vert)
        {
            vertices = new float2[vert.Count];
            for (int i = 0; i < vert.Count; i++)
            {
                vertices[i] = new float2(vert[i].x, vert[i].y);
            }
            CalculateBounds();
        }

        public Polygon(Vector2[] vert)
        {
            vertices = new float2[vert.Length];
            for (int i = 0; i < vert.Length; i++)
            {
                vertices[i] = new float2(vert[i].x, vert[i].y);
            }
            CalculateBounds();
        }

        protected Polygon()
        { }

        public Polygon(Polygon src)
        {
            this.vertices = new float2[src.VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                this.vertices[i] = src[i];
            }
        }

        public float2 this[int i]
        {
            get { return vertices[i]; }
        }

        public void CalculateBounds()
        {
            Bounds = GeometryUtility.CalculateBounds(this);
        }

        public IEnumerator<(float2 v0, float2 v1)> GetEnumerator()
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                var vCurrent = vertices[i];
                var vNext = vertices[(i + 1) % vertices.Length];
                yield return (v0: vCurrent, v1: vNext);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Mesh Triangulate()
        {
            throw new NotImplementedException();
        }
    }

    public readonly struct PolygonVertexTriple
    {
        public readonly float2 Current;
        public readonly int CurrentIdx;

        public readonly float2 Next;
        public readonly int NextIdx;

        public readonly float2 Prev;
        public readonly int PrevIndex;

        public PolygonVertexTriple(float2 current, int currentIdx, float2 next, int nextIdx, float2 prev, int prevIndex)
        {
            Current = current;
            CurrentIdx = currentIdx;
            Next = next;
            NextIdx = nextIdx;
            Prev = prev;
            PrevIndex = prevIndex;
        }
    }


    public class MutablePolygon : Polygon
    {
        public MutablePolygon(float2[] vertices) : base(vertices)
        {
        }

        public MutablePolygon(List<Vector2> vert) : base(vert)
        {
        }

        public MutablePolygon(Vector2[] vert) : base(vert)
        {
        }

        public MutablePolygon(int vertCount)
        {
            vertices = new float2[vertCount];
        }

        public MutablePolygon(Polygon src) : base(src) { }

        public new float2 this[int i]
        {
            get => vertices[i];
            set => vertices[i] = value;
        }

        public void Reverse()
        {
            vertices = vertices.Reverse().ToArray();
        }

        public void RemoveRange(int beginIdx, int count)
        {
            var vert = vertices.ToList();
            vert.RemoveRange(beginIdx, count);
            vertices = vert.ToArray();
        }
    }

    public struct EdgePair
    {
        public (float2 v0, float2 v1) e0;
        public (float2 v0, float2 v1) e1;
    }

    public struct Bounds2D
    {
        public (float min, float max) X { get; private set; }
        public (float min, float max) Y { get; private set; }

        public float2 Min => new float2(X.min, Y.min);

        public float2 Max => new float2(X.max, Y.max);

        public float2 Size { get; private set; }

        public float2 Center { get; private set; }

        public float2 Extents { get; private set; }

        public Bounds2D((float min, float max) x, (float min, float max) y) : this()
        {
            X = x;
            Y = y;
            Center = new float2(Mathf.Lerp(X.min, X.max, 0.5f), Mathf.Lerp(Y.min, Y.max, 0.5f));

            Size = new float2(X.max - X.min, Y.max - Y.min);
            Extents = Size * 0.5f;
        }

        public bool Contains(float2 p)
        {
            return (p >= Min).ElemtAnd() && (p <= Max).ElemtAnd();
        }

    }

    public static class Bool2Extension
    {
        public static bool ElemtAnd(this bool2 b)
        {
            return b.x && b.y;
        }

        public static bool ElemrOr(this bool2 b)
        {
            return b.x || b.y;
        }
    }
}


