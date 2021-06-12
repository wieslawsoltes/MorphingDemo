using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace PolyLineAnimation
{
    public static class Morph
    {
        public static List<PathGeometry> ToCache(PathGeometry source, PathGeometry target, double speed, IEasing easing)
        {
            int steps = (int) (1 / speed);
            double p = speed;
            var cache = new List<PathGeometry>(steps);

            var sourceFigure = source.Figures[0];
            var sourceSegment = sourceFigure.Segments[0] as PolyLineSegment;

            var targetFigure = target.Figures[0];
            var targetSegment = targetFigure.Segments[0] as PolyLineSegment;

            Debug.Assert(sourceSegment.Points.Count == targetSegment.Points.Count);

            cache.Add(source.ClonePathGeometry());

            for (int i = 0; i < steps; i++)
            {
                var clone = source.ClonePathGeometry();
                var progress = easing.Ease(p);

                var cloneFigure = clone.Figures[0];
                var cloneSegment = cloneFigure.Segments[0] as PolyLineSegment;

                cloneFigure.StartPoint = Interpolate(sourceFigure.StartPoint, targetFigure.StartPoint, progress);

                for (int j = 0; j < sourceSegment.Points.Count; j++)
                {
                    cloneSegment.Points[j] = Interpolate(sourceSegment.Points[j], targetSegment.Points[j], progress);
                }

                p += speed;

                cache.Add(clone);
            }

            cache.Add(target.ClonePathGeometry());

            return cache;
        }

        public static double Interpolate(double from, double to, double progress)
        {
            return from + (to - from) * progress;
        }

        public static Point Interpolate(Point from, Point to, double progress)
        {
            var x = Interpolate(from.X,  to.X, progress);
            var y = Interpolate(from.Y, to.Y, progress);
            return new Point(x, y);
        }

        public static PathGeometry CreatePathGeometry(IList<Point> sourcePoints)
        {
            var source = new PathGeometry
            {
                FillRule = FillRule.EvenOdd
            };

            var sourceFigure = new PathFigure()
            {
                IsClosed = false,
                IsFilled = false,
                StartPoint = sourcePoints.First()
            };
            source.Figures.Add(sourceFigure);

            var polylineSegment = new PolyLineSegment(sourcePoints.Skip(1));
            sourceFigure.Segments?.Add(polylineSegment);

            return source;
        }
    }
}