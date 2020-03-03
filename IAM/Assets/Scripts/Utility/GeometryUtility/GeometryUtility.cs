using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Priority_Queue;
using System.Runtime.CompilerServices;
using GeoUtil.Exceptions;

/// <summary>
/// some basic geometry utitltiy
/// polygon inclusion testing from: http://geomalgorithms.com/a03-_inclusion.html
/// triangulation https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
namespace GeoUtil
{

    public static class GeometryUtility
    {
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
        public static bool Intersects<T>(EdgePair<T> p) where T : IPolygonEdge
        {
            return Intersects(p.e0, p.e1);
        }

        /// <summary>
        /// check if line containers intersects
        /// </summary>
        /// <param name="p">the edge pair to check</param>
        /// <returns>true if intersect</returns>
        //https://algs4.cs.princeton.edu/91primitives/
        public static bool Intersects<T>(T l0, T l1) where T : ILineContainer
        {
            return Intersects(l0.Line, l1.Line);
        }

        /// <summary>
        /// check if edge pair intersects
        /// </summary>
        /// <param name="p">the edge pair to check</param>
        /// <returns>true if intersect</returns>
        //https://algs4.cs.princeton.edu/91primitives/
        public static bool Intersects(Line l0, Line l1) 
        {
            if (CalculateCCW(l0.SPoint, l0.EPoint, l1.SPoint) * CalculateCCW(l0.SPoint, l0.EPoint, l1.EPoint) > 0) return false;
            if (CalculateCCW(l1.SPoint, l1.EPoint, l0.SPoint) * CalculateCCW(l1.SPoint, l1.EPoint, l0.EPoint) > 0) return false;
            return true;
        }

        /// <summary>
        /// makes a list of edges for a given polygon
        /// </summary>
        /// <param name="polygon">polygon we want to get the edges</param>
        /// <returns>the list of edges for the polygon</returns>
        public static List<IPolygonEdge> GetEdgesForPolygon<T>(in IPolygon polygon,Func<float2, float2, int, int,T> generator) where T : IPolygonEdge, new()
        {
            List<IPolygonEdge> edges = new List<IPolygonEdge>();
            for (int i = 0; i < polygon.VertexCount; i++)
            {
                int idxNext = (i + 1) % polygon.VertexCount;
                var vCurrent = polygon[i];
                var vNext = polygon[idxNext];
                edges.Add(generator(vCurrent, vNext, i, idxNext));
            }
            return edges;
        }

        /// <summary>
        /// calculates the centroid for a polygon
        /// </summary>
        /// <param name="polygon">polygon the centroid is calculated of</param>
        /// <returns>the centroid of a polygon</returns>
        public static float2 CalculateCentroid(in IPolygon polygon)
        {
            float2 centroid = float2.zero;
            for (int i = 0; i < polygon.VertexCount; i++)
            {
                centroid += polygon[i];
            }
            centroid /= (float)polygon.VertexCount;
            return centroid;
        }

        /// <summary>
        /// returns the instersection point of to line segments
        /// </summary>
        /// <param name="l0">first line segment</param>
        /// <param name="l1">second line segment</param>
        /// <returns>the intersection point</returns>
        public static float2 IntersectUnknownIntersection(Line l0, Line l1)
        {
            if (Intersects(l0, l1))
            {
                var res = IntersectHomogenouseCoords(l0, l1);
                return res.xy / res.z;
            }
            throw new DivideByZeroException();
        }

        /// <summary>
        /// returns the instersection point of to line segments
        /// </summary>
        /// <param name="l0">first line segment</param>
        /// <param name="l1">second line segment</param>
        /// <returns>the intersection point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 IntersectKnownIntersection(Line l0, Line l1)
        {
            var res = IntersectHomogenouseCoords(l0, l1);
            return res.xy / res.z;
        }

        /// <summary>
        /// calculates lineintersection in homogneous coords from given line segments
        /// </summary>
        /// <param name="l0">first line segment</param>
        /// <param name="l1">second line segment</param>
        /// <returns>intersection point in hom coords</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 IntersectHomogenouseCoords(Line l0, Line l1)
        {
            return math.cross(l0.HomogenousLineCoords, l1.HomogenousLineCoords);
        }

        /// <summary>
        /// calculates the homogenoues cordinates line for a line segment
        /// </summary>
        /// <param name="l">the line segments</param>
        /// <returns>the line coords in porjective space</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 CalcHomogeneousLine(Line l)
        {
            return math.cross(new float3(l.SPoint,1), new float3(l.EPoint,1));
        }

        /// <summary>
        /// calculate polygon orientation
        /// may throw malformed polygon exception
        /// </summary>
        /// <param name="poly">the polygon we want the orientation of</param>
        /// <returns>the vertex orientation</returns>
        public static VertexWinding GetWinding(in IPolygon poly)
        {
            if (IsMalformed(poly, out string malformType))
                throw new MalforemdPolygonException(malformType);

            int curIdx = GetMinYVertexIndex(poly);

            int nextIdx = GetNextVertexIdx(poly,curIdx);

            if (poly[curIdx].x > poly[nextIdx].x)
                return VertexWinding.CW;
            else if (poly[curIdx].x < poly[nextIdx].x)
                return VertexWinding.CCW;
            else
            {
                int startIdx = curIdx;
                curIdx = nextIdx;
                nextIdx = GetNextVertexIdx(poly, curIdx);

                while (curIdx != startIdx)
                {
                    //inverted
                    if (poly[curIdx].x < poly[nextIdx].x)
                        return VertexWinding.CW;
                    else if (poly[curIdx].x > poly[nextIdx].x)
                        return VertexWinding.CCW;
                    else
                    {
                        curIdx = nextIdx;
                        nextIdx = GetNextVertexIdx(poly, curIdx);
                    }
                }
                throw new MalforemdPolygonException("Unorientable");
            }
            

        }

        /// <summary>
        /// gets the index of the first(if multiple had the same min y value) vertex with the minimum y value
        /// </summary>
        /// <param name="poly">the polygon</param>
        /// <returns>the index of the min y value vertex</returns>
        private static int GetMinYVertexIndex(in IPolygon poly)
        {
            float min = float.MaxValue;
            int idx = -1;
            for (int i = 0; i < poly.VertexCount; i++)
            {
                if (poly[i].y < min)
                {
                    min = poly[i].y;
                    idx = i;
                }
            }
            return idx;
        }

        /// <summary>
        /// calculates the next vertex index
        /// </summary>
        /// <param name="poly">polygon we are interessted in</param>
        /// <param name="curIdx">current vertex index</param>
        /// <returns>the next index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNextVertexIdx(in IPolygon poly,int curIdx)
        {
            return (curIdx + 1) % poly.VertexCount;
        }

        /// <summary>
        /// calculates the next vertex index
        /// </summary>
        /// <param name="poly">polygon we are interessted in</param>
        /// <param name="curIdx">current vertex index</param>
        /// <returns>the next index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetPrevVertexIdx(in IPolygon poly, int curIdx)
        {
            return curIdx - 1 < 0 ? poly.VertexCount - 1 : curIdx - 1;
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
        /// checks if a polygon is malformed in any way
        /// </summary>
        /// <param name="p">the polygon to check</param>
        /// <param name="malformedType">what malformation type(out param)</param>
        /// <returns>true if malformed</returns>
        public static bool IsMalformed(in IPolygon p,out string malformedType)
        {
            malformedType = "";
            //TODO are there more malformation types?
            bool malformed =  CheckPolygonSelfIntersection(p);
            if (malformed)
                malformedType = "SelfIntersection";
            return malformed;
        }

        /// <summary>
        /// orients the vertces in a polygon to a certain winding
        /// </summary>
        /// <param name="p">The polygon p</param>
        /// <param name="orientation">the winding wanted</param>
        /// <returns>a NEW polygon with the new winding</returns>
        public static Polygon ChangeOrientation(in IPolygon p, VertexWinding orientation = VertexWinding.CW)
        {
            if (orientation == p.VertexWinding)
                return new Polygon(p);

            int startIdx = GetMinYVertexIndex(p);
            var muteP = new MutablePolygon(p.VertexCount);

            muteP[0] = p[startIdx];

            int currentIdx = startIdx;
            int i = 1;
            //TODO
            do
            {
                currentIdx = GetPrevVertexIdx(p, currentIdx);
                muteP[i] = p[currentIdx];
                i++;

            } while (startIdx != GetPrevVertexIdx(p,currentIdx));
     

            return muteP.MakeUnmutable(updateNonSerializedData:true);

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
        /// check if p is left of line
        /// </summary>
        ///<param name="l">The line segement container</param>
        /// <param name="p">point to check</param>
        /// <returns>true if left of line</returns>
        public static bool IsLeftOfLine(ILineContainer l, float2 p)
        {
            return IsLeftOfLine(l.Line, p);
        }

        /// <summary>
        /// check if p is left of line
        /// </summary>
        ///<param name="l">The line segement</param>
        /// <param name="p">point to check</param>
        /// <returns>true if left of line</returns>
        public static bool IsLeftOfLine(Line l, float2 p)
        {
            return IsLeftOfLine(l.SPoint, l.EPoint, p);
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
        /// check if p is right of line
        /// </summary>
        ///<param name="l">The line segement container</param>
        /// <param name="p">point to check</param>
        /// <returns>true if right of line</returns>
        public static bool IsRightOfLine(ILineContainer l, float2 p)
        {
            return IsRightOfLine(l.Line.SPoint, l.Line.EPoint, p);
        }

        /// <summary>
        /// check if p is right of line
        /// </summary>
        ///<param name="l">The line segement</param>
        /// <param name="p">point to check</param>
        /// <returns>true if right of line</returns>
        public static bool IsRightOfLine(Line l, float2 p)
        {
            return IsRightOfLine(l.SPoint, l.EPoint, p);
        }

        /// <summary>
        /// check if p is on line
        /// </summary>
        /// <param name="lP0">line point 1</param>
        /// <param name="lP1">line pont 2</param>
        /// <param name="p">point to check</param>
        /// <returns>true if on line</returns>
        public static bool IsOnLine(float2 lP0, float2 lP1, float2 p)
        {
            return CheckPointPositionAgainstLine(lP0, lP1, p, LinePosition.on);
        }
        
        /// <summary>
        /// check if p is on line
        /// </summary>
        ///<param name="l">The line segement container</param>
        /// <param name="p">point to check</param>
        /// <returns>true if on line</returns>
        public static bool IsOnLine(ILineContainer l,float2 p)
        {
            return IsOnLine(l.Line.SPoint, l.Line.EPoint, p);
        }

        /// <summary>
        /// check if p is on line
        /// </summary>
        ///<param name="l">The line segement</param>
        /// <param name="p">point to check</param>
        /// <returns>true if left of line</returns>
        public static bool IsOnLine(Line l, float2 p)
        {
            return IsOnLine(l.SPoint, l.EPoint, p);
        }

        /// <summary>
        /// winding number point to polygon
        /// </summary>
        /// <param name="p">point to test</param>
        /// <param name="polygon">polygon to test against</param>
        /// <returns> == 0 iff outside</returns>
        public static int WindingNumberPointInPolygon(float2 p, in IPolygon polygon)
        {
            int windingNumber = 0;
            foreach (Edge edge in polygon.Edges)
            {
                if (edge.SPoint.y <= p.y && edge.EPoint.y > p.y)
                {
                    //upward crossing
                    if (IsLeftOfLine(edge.SPoint, edge.EPoint, p))
                        windingNumber++;

                }
                else if (edge.EPoint.y <= p.y)
                {
                    //downward crossing
                    if (IsRightOfLine(edge.SPoint, edge.EPoint, p))
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
        public static bool PolygonContains(float2 p, in IPolygon polygon)
        {
            return polygon.Bounds.Contains(p) && WindingNumberPointInPolygon(p, polygon) != 0;
        }

        /// <summary>
        /// calculates bounds of polygon
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Bounds2D CalculateBounds(in IPolygon p)
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
        public static bool CheckPolygonSelfIntersection(in IPolygon p)
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
                int prev = GetPrevVertexIdx(poly, idx);
                int next = GetNextVertexIdx(poly, idx);
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
        public static bool CheckPolygonConvex(in Polygon polygon)
        {
            return IsConvex(polygon) > 0;
        }

        #region adapted from https://gist.github.com/KvanTTT/3855122
        /// <summary>
        /// adapted from https://gist.github.com/KvanTTT/3855122
        /// </summary>
        /// <param name="polygon">the polygon to check</param>
        /// <returns>-1 if no intersect 1 otherwise</returns>
        static int SelfIntersection(in IPolygon polygon)
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
        static float Square(in IPolygon polygon)
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
        static int IsConvex(in IPolygon Polygon)
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
        static bool Intersect(in IPolygon polygon, int vertex1Ind, int vertex2Ind, int vertex3Ind)
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
        static float PMSquare(float2 p1,float2 p2)
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
}
