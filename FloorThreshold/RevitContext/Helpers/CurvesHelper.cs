using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AXE.RevitContext.Helpers
{
    internal class CurvesHelper
    {


        public static List<XYZ> CustomSort(List<XYZ> list)
        {
            List<XYZ> newList = list.OrderBy(s => s.X).ToList();
            List<XYZ> result = new List<XYZ>();

            result.Add(newList[0]);
            for (int i = 1; i < list.Count; i++)
            {
                if (i % 2 != 0)
                {

                    result.Add(list.OrderBy(s => s.DistanceTo(list[i])).Skip(1).FirstOrDefault(s => s.X - list[i].X >0.01));
                }
                else
                {
                   result.Add( list.OrderBy(s => s.DistanceTo(list[i])).Skip(1).FirstOrDefault(s => s.Y - list[i].Y > 0.01));

                }

            }

            return result;
        }
        public static XYZ ProjectPointOnLine(XYZ P, Line line)
        {
            // Extract start and end points of the line
            XYZ A = line.GetEndPoint(0);
            XYZ B = line.GetEndPoint(1);

            // Direction vector of the line
            XYZ D = B - A;

            // Vector from A to P
            XYZ AP = P - A;

            // Projection scalar
            double t = (AP.DotProduct(D)) / (D.DotProduct(D));

            // Compute projected point
            XYZ P_prime = A + t * D;

            return P_prime;
        }


        public static CurveLoop CreateCurveLoop(List<XYZ> points)
        {
            CurveLoop cl = new CurveLoop();
            for (int i = 0; i < points.Count; i++)
            {
                if (!points[i].IsAlmostEqualTo(points[(i + 1) % points.Count]))
                    cl.Append(Line.CreateBound(points[i], points[(i + 1) % points.Count]));
            }
            return cl;
        }
        public static List<XYZ> SortXYZClockwise(List<XYZ> points, XYZ normal)
        {
            points = points
   .Select(p => new XYZ(
       Math.Round(p.X, 3),
       Math.Round(p.Y, 3),
       Math.Round(p.Z, 3)))
   .ToList();

            // Compute centroid (average of all points)
            XYZ centroid = new XYZ(
                points.Average(p => p.X),
                points.Average(p => p.Y),
                points.Average(p => p.Z)
            );

            // Choose reference axis
            XYZ referenceVector;
            if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
            {
                // Most likely a horizontal plane, project onto XY
                referenceVector = new XYZ(1, 0, 0);
            }
            else
            {
                // Otherwise, assume projection onto XZ or YZ
                referenceVector = new XYZ(0, 0, 1);
            }

            // Sort points by angle relative to centroid
            return points.OrderBy(p =>
            {
                XYZ vector = p - centroid; // Vector from centroid to point

                // Compute cross product for direction
                double cross = referenceVector.CrossProduct(vector).DotProduct(normal);

                // Compute atan2 for angle sorting
                return Math.Atan2(cross, vector.DotProduct(referenceVector));
            }).ToList();
        }
        public static List<XYZ> SortClockwise(List<XYZ> points)
        {
            // 1. Compute centroid
            double centerX = points.Average(p => p.X);
            double centerY = points.Average(p => p.Y);
            XYZ center = new XYZ(centerX, centerY, 0);

            return points

                .OrderBy(p => DistanceSquared(center, p)) // Sort collinear points by distance
                .ThenByDescending(p => Math.Atan2(p.Y - center.Y, p.X - center.X)) // Sort by angle
                .ToList();
        }
        public static List<XYZ> ComputeRectilinearHull(List<XYZ> points)
        {
            if (points.Count <= 1)
                return new List<XYZ>(points);

            // Get extreme coordinates
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            // Collect boundary points
            List<XYZ> top = points.Where(p => p.Y == maxY).OrderBy(p => p.X).ToList();
            List<XYZ> right = points.Where(p => p.X == maxX).OrderByDescending(p => p.Y).ToList();
            List<XYZ> bottom = points.Where(p => p.Y == minY).OrderByDescending(p => p.X).ToList();
            List<XYZ> left = points.Where(p => p.X == minX).OrderBy(p => p.Y).ToList();

            // Create the rectilinear path ensuring only horizontal and vertical moves
            List<XYZ> result = new List<XYZ>();

            // Connect the corners to ensure only right angles
            if (top.Count > 0) result.AddRange(top);
            if (right.Count > 0 && right[0] != result.LastOrDefault()) result.Add(right[0]);
            if (bottom.Count > 0) result.AddRange(bottom);
            if (left.Count > 0 && left[0] != result.LastOrDefault()) result.Add(left[0]);

            return result.Distinct().ToList();
        }
        public static double DistanceSquared(XYZ a, XYZ b)
        {
            return Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2);
        }
        private static double DoubleSignedArea(XYZ p1, XYZ p2, XYZ p3) =>
          (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
        public static List<XYZ> ConvexHull(IEnumerable<XYZ> source)
        {
            var p0 = source.OrderBy(p => p.Y).ThenBy(p => p.X).First();
            bool Clockwise(XYZ p1, XYZ p2, XYZ p3) =>
            DoubleSignedArea(p1, p2, p3) < 1e-9;

            double Cosine(XYZ pt)
            {
                double d = p0.DistanceTo(pt);
                return d == 0.0 ? 1.0 : Math.Round((pt.X - p0.X) / d, 9);
            }

            var pts = source.OrderByDescending(p => Cosine(p)).ThenBy(p => p0.DistanceTo(p)).ToList();
            for (int i = 1; i < pts.Count - 1; i++)
            {
                while (i > 0 && Clockwise(pts[i - 1], pts[i], pts[i + 1]))
                {
                    pts.RemoveAt(i);
                    i--;
                }
            }
            return pts;
        }
        public static List<XYZ> ComputeConvexHull(List<XYZ> points)
        {
            if (points.Count <= 1)
                return new List<XYZ>(points);

            // Sort points by X, then by Y
            points = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            List<XYZ> lower = new List<XYZ>();
            foreach (var p in points)
            {
                while (lower.Count >= 2 && CrossProduct(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            List<XYZ> upper = new List<XYZ>();
            for (int i = points.Count - 1; i >= 0; i--)
            {
                XYZ p = points[i];
                while (upper.Count >= 2 && CrossProduct(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            // Remove the last point from each half because it's repeated
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);

            // Combine the two halves
            lower.AddRange(upper);
            return lower;
        }

        public static double CrossProduct(XYZ a, XYZ b, XYZ c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }
        public static List<XYZ> SortByNearestNeighbor(List<XYZ> points)
        {
            if (points == null || points.Count == 0)
                return new List<XYZ>();

            List<XYZ> sortedPoints = new List<XYZ>();
            HashSet<XYZ> remaining = new HashSet<XYZ>(points);

            // Start with the first (or leftmost) point
            XYZ current = points[0];
            sortedPoints.Add(current);
            remaining.Remove(current);

            while (remaining.Count > 0)
            {
                // Find the closest point
                XYZ nearest = null;
                double minDist = double.MaxValue;

                foreach (XYZ p in remaining)
                {
                    double dist = current.DistanceTo(p);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = p;
                    }
                }

                // Move to nearest point
                if (nearest != null)
                {
                    sortedPoints.Add(nearest);
                    remaining.Remove(nearest);
                    current = nearest;
                }
            }

            return sortedPoints;
        }
        const double _inch = 1.0 / 12.0;
        const double _sixteenth = _inch / 16.0;
        static Curve CreateReversedCurve(Curve orig)
        {


            if (orig is Autodesk.Revit.DB.Line)
            {
                return orig.CreateReversed();
            }
            else if (orig is Arc)
            {
                return orig.CreateReversed();
            }
            else
            {
                throw new Exception(
                  "CreateReversedCurve - Unreachable");
            }
        }
        public static void SortCurvesContiguous(IList<Curve> curves)
        {
            double _precision1 = 1.0 / 12.0 / 16.0; // around 0.00520833
            double _precision2 = 0.001; // limit for CurveLoop.Create(...)

            int n = curves.Count;

            // Walk through each curve (after the first)
            // to match up the curves in order

            for (int i = 0; i < n; ++i)
            {
                //Curve curve = curves;

                XYZ beginPoint = curves[i].GetEndPoint(0);
                XYZ endPoint = curves[i].GetEndPoint(1);

                XYZ p, q;

                // Find curve with start point = end point

                bool found = (i + 1 >= n);

                for (int j = i + 1; j < n; ++j)
                {
                    p = curves[j].GetEndPoint(0);
                    q = curves[j].GetEndPoint(1);

                    // If there is a match end->start,
                    // this is the next curve
                    // double x= p.DistanceTo(endPoint);
                    if (p.DistanceTo(endPoint) < _precision1)
                    {
                        if (p.DistanceTo(endPoint) > _precision2)
                        {
                            XYZ intermediate = new XYZ((endPoint.X + p.X) / 2.0, (endPoint.Y + p.Y) / 2.0, (endPoint.Z + p.Z) / 2.0);

                            curves[i] = Line.CreateBound(beginPoint, intermediate);

                            curves[j] = Line.CreateBound(intermediate, q);
                        }

                        if (i + 1 != j)
                        {
                            Curve tmp = curves[i + 1];
                            curves[i + 1] = curves[j];
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }

                    // If there is a match end->end,
                    // reverse the next curve

                    if (q.DistanceTo(endPoint) < _precision1)
                    {
                        if (q.DistanceTo(endPoint) > _precision2)
                        {
                            XYZ intermediate = new XYZ((endPoint.X + q.X) / 2.0, (endPoint.Y + q.Y) / 2.0, (endPoint.Z + q.Z) / 2.0);

                            curves[i] = Line.CreateBound(beginPoint, intermediate);

                            curves[j] = Line.CreateBound(p, intermediate);
                        }

                        if (i + 1 == j)
                        {
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                        }
                        else
                        {
                            Curve tmp = curves[i + 1];
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new Exception("SortCurvesContiguous :" + " non-contiguous input curves");
                }
            }
        }

        public static List<XYZ> FindNearestNeighborPath(List<XYZ> points)
        {
            List<XYZ> path = new List<XYZ>();
            HashSet<XYZ> visited = new HashSet<XYZ>();

            // Start with the leftmost point
            XYZ current = points.OrderBy(p => p.X).ThenBy(p => p.Y).First();
            path.Add(current);
            visited.Add(current);

            while (visited.Count < points.Count)
            {
                XYZ nearest = null;
                double minDistance = int.MaxValue;

                // Find the closest unvisited point only in horizontal or vertical directions
                foreach (var p in points)
                {
                    if (visited.Contains(p)) continue;

                    if (p.X == current.X || p.Y == current.Y)  // Only horizontal or vertical movement
                    {
                        double distance = Math.Abs(p.X - current.X) + Math.Abs(p.Y - current.Y);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearest = p;
                        }
                    }
                }

                if (nearest == null) break;  // No more valid moves

                path.Add(nearest);
                visited.Add(nearest);
                current = nearest;
            }

            return path;
        }
    }
}
