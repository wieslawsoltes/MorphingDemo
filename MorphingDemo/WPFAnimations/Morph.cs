using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WPFAnimations
{
    public static class Morph
    {
        public static bool Collapse(PathGeometry sourceGeometry, double progress)
        {
            int count = sourceGeometry.Figures.Count;
            for (int i = 0; i < sourceGeometry.Figures.Count; i++)
            {
                count -= MorphCollapse(sourceGeometry.Figures[i], progress);
            }

            if (count <= 0) return true;

            return false;
        }

        private static void MoveFigure(PathFigure source, double p, double progress)
        {
            PolyLineSegment segment = (PolyLineSegment)source.Segments[0];

            for (int i = 0; i < segment.Points.Count; i++)
            {
                var fromX = segment.Points[i].X;
                var fromY = segment.Points[i].Y;

                var x = fromX + p;
                segment.Points[i] = new Point(x, fromY);
            }

            var newX = source.StartPoint.X + p;

            source.StartPoint = new Point(newX, source.StartPoint.Y);
        }

        private static bool DoFiguresOverlap(PathFigureCollection figures, int index0, int index1, int index2)
        {
            if (index2 < figures.Count && index0 >= 0)
            {
                PathGeometry g0 = new PathGeometry(new[] { figures[index2] });
                PathGeometry g1 = new PathGeometry(new[] { figures[index1] });
                PathGeometry g2 = new PathGeometry(new[] { figures[index0] });
                var result0 = g0.FillContainsWithDetail(g1);
                var result1 = g0.FillContainsWithDetail(g2);

                return
                    (result0 == IntersectionDetail.FullyContains ||
                        result0 == IntersectionDetail.FullyInside) &&
                    (result1 == IntersectionDetail.FullyContains ||
                        result1 == IntersectionDetail.FullyInside);
            }

            return false;
        }

        private static bool DoFiguresOverlap(PathFigureCollection figures, int index0, int index1)
        {
            if (index1 < figures.Count && index0 >= 0)
            {
                PathGeometry g1 = new PathGeometry(new[] { figures[index1] });
                PathGeometry g2 = new PathGeometry(new[] { figures[index0] });
                var result = g1.FillContainsWithDetail(g2);
                return result == IntersectionDetail.FullyContains || result == IntersectionDetail.FullyInside;
            }
            return false;
        }

        private static void CollapseFigure(PathFigure figure)
        {
            var points = ((PolyLineSegment)figure.Segments[0]).Points;
            var centroid = GetCentroid(points, points.Count);

            for (int p = 0; p < points.Count; p++)
            {
                points[p] = centroid;
            }

            figure.StartPoint = centroid;
        }

        public static void To(PathGeometry sourceGeometry, PathGeometry geometry, Range sourceRange, double progress)
        {
            int k = 0;
            for (int i = sourceRange.Start.Value; i < sourceRange.End.Value; i++)
            {
                MorphFigure(sourceGeometry.Figures[i], geometry.Figures[k], progress);
                k++;
            }
        }

        public static List<PathGeometry> ToCache(PathGeometry source, PathGeometry target, double speed)
        {
            PowerEase powerEase = new PowerEase();
            int steps = (int)(1 / speed);
            double p = speed;
            List<PathGeometry> cache = new List<PathGeometry>(steps);

            for (int i = 0; i < steps; i++)
            {
                var clone = source.Clone();
                var easeP = powerEase.Ease(p);

                To(clone, target, easeP);

                p += speed;

                cache.Add(clone);
            }

            return cache;
        }

        public static void To(PathGeometry source, PathGeometry target, double progress)
        {
            //
            // Clone figures.
            //
            if (source.Figures.Count < target.Figures.Count)
            {
                var last = source.Figures.Last();
                var toAdd = target.Figures.Count - source.Figures.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    var clone = last.Clone();
                    source.Figures.Add(clone);
                }
            }
            //
            // Contract the source, the problem here is that if we have a shape
            // like 'O' where we need to cut a hole in a shape we will butcher such character
            // since all excess shapes will be stored under this shape.
            //
            // We need to move and collapse them when moving.
            // So lets collapse then to a single point.
            //
            else if (source.Figures.Count > target.Figures.Count)
            {
                var toAdd = source.Figures.Count - target.Figures.Count;
                var lastIndex = target.Figures.Count - 1;

                for (int i = 0; i < toAdd; i++)
                {
                    var clone = target.Figures[lastIndex].Clone();
                    //var clone = target.Figures[(lastIndex - (i % (lastIndex + 1)))].Clone();

                    //
                    // This is a temp solution but it works well for now.
                    // We try to detect if our last shape has an overlapping geometry
                    // if it does then we will clone the previrous shape.
                    //
                    if (lastIndex > 0)
                    {
                        if (DoFiguresOverlap(target.Figures, lastIndex - 1, lastIndex))
                        {
                            if (DoFiguresOverlap(target.Figures, lastIndex - 2, lastIndex - 1, lastIndex))
                            {
                                clone = target.Figures[lastIndex - 3].Clone();
                            }
                            else if (lastIndex - 2 > 0)
                            {
                                clone = target.Figures[lastIndex - 2].Clone();
                            }
                            else
                            {
                                CollapseFigure(clone);
                            }
                        }
                    }
                    else
                    {
                        CollapseFigure(clone);
                    }

                    target.Figures.Add(clone);
                }
            }

            int[] map = new int[source.Figures.Count];
            for (int i = 0; i < map.Length; i++)
                map[i] = -1;

            //
            // Morph Closest Figures.
            //
            for (int i = 0; i < source.Figures.Count; i++)
            {
                double closest = double.MaxValue;
                int closestIndex = -1;

                for (int j = 0; j < target.Figures.Count; j++)
                {
                    if (map.Contains(j))
                        continue;
                   
                    var len = Point.Subtract(source.Figures[i].StartPoint, target.Figures[j].StartPoint).LengthSquared;
                    if (len < closest)
                    {
                        closest = len;
                        closestIndex = j;
                    }
                }

                map[i] = closestIndex;
            }

            for (int i = 0; i < source.Figures.Count; i++)
                MorphFigure(source.Figures[i], target.Figures[map[i]], progress);
        }

        public static void MorphFigure(PathFigure source, PathFigure target, double progress)
        {
            PolyLineSegment sourceSegment = (PolyLineSegment)source.Segments[0];
            PolyLineSegment targetSegment = (PolyLineSegment)target.Segments[0];

            if (sourceSegment.Points.Count < targetSegment.Points.Count)
            {
                //
                // Add points to segment.
                //
                var toAdd = targetSegment.Points.Count - sourceSegment.Points.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    sourceSegment.Points.Add(source.StartPoint);
                }
            }
            else if (sourceSegment.Points.Count > targetSegment.Points.Count)
            {
                //
                // Add points to segment.
                //
                var toAdd = sourceSegment.Points.Count - targetSegment.Points.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    targetSegment.Points.Add(target.StartPoint);
                }
            }

            //
            // Interpolate from source to target.
            //
            if (progress >= 1)
            {
                for (int i = 0; i < sourceSegment.Points.Count; i++)
                {
                    var toX = targetSegment.Points[i].X;
                    var toY = targetSegment.Points[i].Y;
                    sourceSegment.Points[i] = new Point(toX, toY);
                }
                source.StartPoint = new Point(target.StartPoint.X, target.StartPoint.Y);
            }
            else
            {
                for (int i = 0; i < sourceSegment.Points.Count; i++)
                {
                    var fromX = sourceSegment.Points[i].X;
                    var toX = targetSegment.Points[i].X;

                    var fromY = sourceSegment.Points[i].Y;
                    var toY = targetSegment.Points[i].Y;

                    if (fromX != toX || fromY != toY)
                    {
                        var x = Interpolate(fromX, toX, progress);
                        var y = Interpolate(fromY, toY, progress);
                        sourceSegment.Points[i] = new Point(x, y);
                    }
                }

                if (source.StartPoint.X != target.StartPoint.X || 
                    source.StartPoint.Y != target.StartPoint.Y)
                {
                    var newX = Interpolate(source.StartPoint.X, target.StartPoint.X, progress);
                    var newY = Interpolate(source.StartPoint.Y, target.StartPoint.Y, progress);
                    source.StartPoint = new Point(newX, newY);
                }
            }
        }

        public static int MorphCollapse(PathFigure source, double progress)
        {
            PolyLineSegment sourceSegment = (PolyLineSegment)source.Segments[0];

            //
            // Find Centroid
            //
            var centroid = GetCentroid(sourceSegment.Points, sourceSegment.Points.Count);
            for (int i = 0; i < sourceSegment.Points.Count; i++)
            {
                var fromX = sourceSegment.Points[i].X;
                var toX = centroid.X;

                var fromY = sourceSegment.Points[i].Y;
                var toY = centroid.Y;

                var x = Interpolate(fromX, toX, progress);
                var y = Interpolate(fromY, toY, progress);

                sourceSegment.Points[i] = new Point(x, y);
            }

            var newX = Interpolate(source.StartPoint.X, centroid.X, progress);
            var newY = Interpolate(source.StartPoint.Y, centroid.Y, progress);

            source.StartPoint = new Point(newX, newY);

            if (centroid.X - newX < 0.005)
            {
                return 1;
            }

            return 0;
        }

        public static Point GetCentroid(PointCollection nodes, int count)
        {
            double x = 0, y = 0, area = 0, k;
            Point a, b = nodes[count - 1];

            for (int i = 0; i < count; i++)
            {
                a = nodes[i];

                k = a.Y * b.X - a.X * b.Y;
                area += k;
                x += (a.X + b.X) * k;
                y += (a.Y + b.Y) * k;

                b = a;
            }
            area *= 3;

            return (area == 0) ? new Point() : new Point(x /= area, y /= area);
        }

        public static double Interpolate(double from, double to, double progress)
        {
            return from + (to - from) * progress;
        }
    }

}
